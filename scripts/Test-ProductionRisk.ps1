[CmdletBinding()]
param(
    [string]$DllPath
)

$ErrorActionPreference = 'Stop'
if ([String]::IsNullOrEmpty($DllPath)) {
    $repositoryRoot = Split-Path -Parent $PSScriptRoot
    $DllPath = Join-Path $repositoryRoot `
        'src\DspProgressionStatusExporter\bin\Release\net472\DspGuideCheck.dll'
}
if (-not (Test-Path -LiteralPath $DllPath -PathType Leaf)) {
    throw "Required assembly was not found: $DllPath"
}

$assembly = [Reflection.Assembly]::LoadFrom(
    (Resolve-Path -LiteralPath $DllPath)
)
$flags = [Reflection.BindingFlags]'Static,Instance,Public,NonPublic'
$inputType = $assembly.GetType(
    'DspProgressionStatusExporter.ProductionRiskInput', $true
)
$analyzerType = $assembly.GetType(
    'DspProgressionStatusExporter.ProductionRiskAnalyzer', $true
)
$evaluate = $analyzerType.GetMethod('Evaluate', $flags)

function Invoke-Risk {
    param([hashtable]$Values)

    $defaults = @{
        ItemId = 1305
        Name = 'Quantum Chips'
        Scope = 'fixture'
        OneMinuteAvailable = $true
        ProducedPerMinute = 100.0
        ConsumedPerMinute = 100.0
        TenMinuteAvailable = $true
        TenMinuteReady = $true
        TenMinuteProducedPerMinute = 100.0
        TenMinuteConsumedPerMinute = 100.0
        RunwayAvailable = $false
        RunwayMinutes = 0.0
        BackpressureStatus = 'unknown'
        ExactTargetPerMinute = 0.0
    }
    foreach ($entry in $Values.GetEnumerator()) {
        $defaults[$entry.Key] = $entry.Value
    }
    $input = [Activator]::CreateInstance($inputType, $true)
    foreach ($entry in $defaults.GetEnumerator()) {
        $inputType.GetField($entry.Key, $flags).SetValue(
            $input, $entry.Value
        )
    }
    return $evaluate.Invoke($null, (, $input))
}

function Assert-State {
    param(
        [string]$Name,
        [hashtable]$Values,
        [string]$ExpectedState,
        [bool]$ExpectedActionable
    )

    $result = Invoke-Risk $Values
    if ($result.State -cne $ExpectedState -or
        $result.Actionable -ne $ExpectedActionable) {
        throw "$Name expected $ExpectedState/actionable=$ExpectedActionable; found $($result.State)/actionable=$($result.Actionable)."
    }
    return $result
}

Assert-State 'Unavailable evidence' `
    @{ OneMinuteAvailable = $false } 'unknown' $false | Out-Null
Assert-State 'History warming' `
    @{ TenMinuteReady = $false; ProducedPerMinute = 0.0 } `
    'warming' $false | Out-Null
Assert-State 'Authoritative backpressure' `
    @{ BackpressureStatus = 'proven'; ProducedPerMinute = 0.0 } `
    'backpressured' $false | Out-Null
Assert-State 'Pulsed idle interval' `
    @{ ProducedPerMinute = 0.0; ConsumedPerMinute = 0.0;
       TenMinuteProducedPerMinute = 30.0 } `
    'balanced' $false | Out-Null
Assert-State 'Minor rate noise' `
    @{ ProducedPerMinute = 95.0; ConsumedPerMinute = 100.0 } `
    'balanced' $false | Out-Null
$chronic = Assert-State 'Stable chronic deficit' `
    @{ ProducedPerMinute = 50.0; ConsumedPerMinute = 100.0;
       TenMinuteProducedPerMinute = 50.0; RunwayAvailable = $true;
       RunwayMinutes = 10.0 } `
    'draining' $true
if (-not $chronic.DemandDeficit -or [double]$chronic.Score -ne 0.0) {
    throw 'A chronic deficit disappeared because its deterioration score was zero.'
}
$target = Assert-State 'Exact target deficit' `
    @{ ProducedPerMinute = 20.0; ConsumedPerMinute = 0.0;
       TenMinuteProducedPerMinute = 20.0;
       ExactTargetPerMinute = 40.0 } `
    'draining' $true
if (-not $target.TargetDeficit) {
    throw 'An exact guide target deficit was not retained separately.'
}
$draining = Assert-State 'Draining buffer' `
    @{ ProducedPerMinute = 40.0; ConsumedPerMinute = 80.0;
       TenMinuteProducedPerMinute = 100.0; RunwayAvailable = $true;
       RunwayMinutes = 0.25 } `
    'draining' $true
if ([double]$draining.Score -le 0.0 -or
    [double]$draining.Score -ge 0.7) {
    throw 'The draining fixture did not retain a moderate continuous score.'
}
$starved = Assert-State 'Actual starvation' `
    @{ ProducedPerMinute = 0.0; ConsumedPerMinute = 80.0;
       TenMinuteProducedPerMinute = 100.0; RunwayAvailable = $true;
       RunwayMinutes = 0.0 } `
    'starved' $true
if ([Math]::Abs([double]$starved.Score - 1.0) -gt 0.0001) {
    throw 'The starvation fixture did not produce the expected score.'
}

$repeatA = Invoke-Risk @{
    ProducedPerMinute = 40.0
    ConsumedPerMinute = 80.0
    TenMinuteProducedPerMinute = 100.0
    RunwayAvailable = $true
    RunwayMinutes = 0.25
}
$repeatB = Invoke-Risk @{
    ProducedPerMinute = 40.0
    ConsumedPerMinute = 80.0
    TenMinuteProducedPerMinute = 100.0
    RunwayAvailable = $true
    RunwayMinutes = 0.25
}
if (($repeatA.Export() | ConvertTo-Json -Compress) -cne
    ($repeatB.Export() | ConvertTo-Json -Compress)) {
    throw 'Identical normalized evidence produced different risk results.'
}

$stateType = $assembly.GetType(
    'DspProgressionStatusExporter.ObservedGameState', $true
)
$flowType = $assembly.GetType(
    'DspProgressionStatusExporter.ObservedItemFlow', $true
)
$factoryFlowType = $assembly.GetType(
    'DspProgressionStatusExporter.ObservedFactoryItemFlow', $true
)
$itemBufferType = $assembly.GetType(
    'DspProgressionStatusExporter.ObservedItemBufferEvidence', $true
)
$bufferScopeType = $assembly.GetType(
    'DspProgressionStatusExporter.ObservedBufferScopeEvidence', $true
)
$state = [Activator]::CreateInstance($stateType, $true)
$stateType.GetField('ProductionWindowReady', $flags).SetValue($state, $true)
$flow = [Activator]::CreateInstance($flowType, $true)
foreach ($entry in @{
        ItemId = 1305
        Name = 'Quantum Chips'
        OneMinuteAvailable = $true
        ProducedPerMinute = 100.0
        ConsumedPerMinute = 100.0
        TenMinuteAvailable = $true
        TenMinuteReady = $true
        TenMinuteProducedPerMinute = 100.0
        TenMinuteConsumedPerMinute = 100.0
    }.GetEnumerator()) {
    $flowType.GetField($entry.Key, $flags).SetValue($flow, $entry.Value)
}
$itemFlows = $stateType.GetField('ItemFlows', $flags).GetValue($state)
$itemFlows.Add(1305, $flow)
$factoryFlow = [Activator]::CreateInstance($factoryFlowType, $true)
foreach ($entry in @{
        FactoryIndex = 0
        PlanetId = 103
        PlanetName = 'Scope fixture'
        ItemId = 1305
        Name = 'Quantum Chips'
        OneMinuteAvailable = $true
        ProducedPerMinute = 0.0
        ConsumedPerMinute = 20.0
        TenMinuteAvailable = $true
        TenMinuteReady = $true
        TenMinuteProducedPerMinute = 40.0
        TenMinuteConsumedPerMinute = 20.0
    }.GetEnumerator()) {
    $factoryFlowType.GetField($entry.Key, $flags).SetValue(
        $factoryFlow, $entry.Value
    )
}
$factoryFlows = $stateType.GetField(
    'FactoryItemFlows', $flags
).GetValue($state)
$factoryFlows.Add($factoryFlow)
$scope = [Activator]::CreateInstance($bufferScopeType, $true)
foreach ($entry in @{
        PlanetId = 103
        PlanetName = 'Scope fixture'
        ItemId = 1305
        Name = 'Quantum Chips'
        AccessibleCount = [long]0
        AccessibleCapacity = [long]400
        DemandEvidenceAvailable = $true
        DemandPerMinute = 20.0
        RunwayAvailable = $true
        RunwayMinutes = 0.0
        BackpressureStatus = 'not-proven'
    }.GetEnumerator()) {
    $bufferScopeType.GetField($entry.Key, $flags).SetValue(
        $scope, $entry.Value
    )
}
$buffer = [Activator]::CreateInstance($itemBufferType, $true)
$itemBufferType.GetField('ItemId', $flags).SetValue($buffer, 1305)
$itemBufferType.GetField('Name', $flags).SetValue($buffer, 'Quantum Chips')
$itemBufferType.GetField(
    'BackpressureStatus', $flags
).SetValue($buffer, 'not-proven')
$itemBufferType.GetField('Scopes', $flags).GetValue($buffer).Add($scope)
$stateType.GetField('ItemBuffers', $flags).GetValue($state).Add(1305, $buffer)
$guideAnalyzer = $assembly.GetType(
    'DspProgressionStatusExporter.GuideAnalyzer', $true
)
$analyzeSelected = $guideAnalyzer.GetMethod('AnalyzeSelected', $flags)
$analysis = $analyzeSelected.Invoke($null, @($state, 'green'))
$riskSummary = $analysis['productionRisk']
if ($riskSummary['selected']['state'] -cne 'starved' -or
    $riskSummary['selected']['scope'] -cne 'planet-local' -or
    $analysis['findings'].Count -ne 1) {
    throw 'Selected-phase integration did not prefer the scope-matched local risk.'
}

$extraFinding = [Collections.Generic.Dictionary[string,object]]::new()
$extraFinding.Add('id', 'lower-priority-fixture')
$extraFinding.Add('status', 'ready')
$extraFinding.Add('claim', 'Lower priority context')
$extraFinding.Add('priority', 100)
$analysis['findings'].Add($extraFinding)
$panelBuilder = $assembly.GetType(
    'DspProgressionStatusExporter.GuidePanelModelBuilder', $true
)
$buildPanel = $panelBuilder.GetMethod('Build', $flags)
$panel = $buildPanel.Invoke($null, @($analysis, $state, $null, $null))
$context = $panel.GetType().GetField('Context', $flags).GetValue($panel)
if ($context.Count -ne 1) {
    throw 'Current Status rendered more than one conclusion.'
}
$riskSignalField = $panel.GetType().GetField('RiskSignal', $flags)
if ($riskSignalField.GetValue($panel).ToString() -cne 'Starved') {
    throw 'Starved production risk did not select the starved panel glyph.'
}
$selectedRisk = $riskSummary['selected']
$selectedRisk['state'] = 'draining'
$panel = $buildPanel.Invoke($null, @($analysis, $state, $null, $null))
if ($riskSignalField.GetValue($panel).ToString() -cne 'Draining') {
    throw 'Draining production risk did not select the draining panel glyph.'
}
$selectedRisk['actionable'] = $false
$panel = $buildPanel.Invoke($null, @($analysis, $state, $null, $null))
if ($riskSignalField.GetValue($panel).ToString() -cne 'None') {
    throw 'A non-actionable production risk selected a panel glyph.'
}

Write-Output 'Production risk tests passed.'
