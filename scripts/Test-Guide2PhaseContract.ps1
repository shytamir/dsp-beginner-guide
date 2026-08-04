[CmdletBinding()]
param(
    [string]$DllPath,

    [string]$GameRoot = (
        'C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program'
    )
)

$ErrorActionPreference = 'Stop'
if ([String]::IsNullOrEmpty($DllPath)) {
    $repositoryRoot = Split-Path -Parent $PSScriptRoot
    $DllPath = Join-Path $repositoryRoot `
        'src\DspProgressionStatusExporter\bin\Release\net472\DspGuideCheck.dll'
}

function Load-Assembly {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required assembly was not found: $Path"
    }
    [Reflection.Assembly]::LoadFrom(
        (Resolve-Path -LiteralPath $Path)
    ) | Out-Null
}

$managed = Join-Path $GameRoot 'DSPGAME_Data\Managed'
foreach ($name in @(
        'UnityEngine.CoreModule.dll',
        'UnityEngine.dll',
        'UnityEngine.InputLegacyModule.dll',
        'UnityEngine.TextRenderingModule.dll',
        'UnityEngine.UIModule.dll',
        'UnityEngine.UI.dll'
    )) {
    Load-Assembly (Join-Path $managed $name)
}
Load-Assembly (Join-Path $GameRoot 'BepInEx\core\BepInEx.dll')
Load-Assembly $DllPath

$assembly = [Reflection.Assembly]::LoadFrom(
    (Resolve-Path -LiteralPath $DllPath)
)
$flags = [Reflection.BindingFlags]'Static,Public,NonPublic'

$navigator = $assembly.GetType(
    'DspProgressionStatusExporter.ManualPhaseNavigator', $true
)
$normalize = $navigator.GetMethod('NormalizePhase', $flags)
$previous = $navigator.GetMethod('Previous', $flags)
$next = $navigator.GetMethod('Next', $flags)
$seed = $navigator.GetMethod('Seed', $flags)

if ($normalize.Invoke($null, @('bootstrap')) -ne 'blue') {
    throw 'A stored BOOTSTRAP selection did not normalize to BLUE.'
}
if ($normalize.Invoke($null, @('unknown-phase')) -ne 'blue') {
    throw 'The invalid-selection fallback is not BLUE.'
}
if ($previous.Invoke($null, @('blue')) -ne 'blue' -or
    $next.Invoke($null, @('blue')) -ne 'red') {
    throw 'BLUE is not the first phase in manual navigation.'
}

$selectionType = $assembly.GetType(
    'DspProgressionStatusExporter.ManualPhaseSelection', $true
)
$parse = $selectionType.GetMethod('Parse', $flags)
$storedSelection = $parse.Invoke(
    $null, @('nav2;phase=bootstrap;seed=stored')
)
if ($selectionType.GetField('PhaseId').GetValue($storedSelection) -ne 'blue' -or
    $storedSelection.Serialize() -notmatch 'phase=blue') {
    throw 'A persisted BOOTSTRAP selection did not migrate to BLUE.'
}

$unlocked = [System.Collections.Generic.HashSet[int]]::new()
$seeded = $seed.Invoke($null, (, $unlocked))
if ($seeded.GetType().GetField('PhaseId').GetValue($seeded) -ne 'blue') {
    throw 'A new playthrough without Cube research did not seed BLUE.'
}

$analyzer = $assembly.GetType(
    'DspProgressionStatusExporter.GuideAnalyzer', $true
)
$phases = $analyzer.GetField('Phases', $flags).GetValue($null)
if ($phases[0].GetType().GetField('Id').GetValue($phases[0]) -ne 'blue' -or
    $phases.Count -ne 9) {
    throw 'Guide analysis does not expose the nine-phase BLUE-first contract.'
}

$gateEngine = $assembly.GetType(
    'DspProgressionStatusExporter.GuideGateEngine', $true
)
$gates = $gateEngine.GetField('Gates', $flags).GetValue($null)
if ($gates[0].GetType().GetField('Id').GetValue($gates[0]) -ne 'blue' -or
    $gates.Count -ne 9) {
    throw 'Progression evaluation does not expose the nine-phase BLUE-first contract.'
}

$snapshotBuilder = $assembly.GetType(
    'DspProgressionStatusExporter.CompactSnapshotBuilder', $true
)
$phaseItems = $snapshotBuilder.GetField('PhaseItems', $flags).GetValue($null)
if ($phaseItems.ContainsKey('bootstrap') -or
    -not $phaseItems.ContainsKey('blue')) {
    throw 'Snapshot evidence routing still exposes BOOTSTRAP.'
}
$blueItems = $phaseItems['blue']
foreach ($itemId in @(1101, 1104, 1202, 1301, 2001, 6001)) {
    if ($blueItems -notcontains $itemId) {
        throw "BLUE snapshot evidence is missing item $itemId."
    }
}

$instanceFlags = [Reflection.BindingFlags]'Instance,Public,NonPublic'
$stateType = $assembly.GetType(
    'DspProgressionStatusExporter.ObservedGameState', $true
)
$flowType = $assembly.GetType(
    'DspProgressionStatusExporter.ObservedItemFlow', $true
)
$recipeType = $assembly.GetType(
    'DspProgressionStatusExporter.ObservedRecipeConfiguration', $true
)
$evaluatePhase = $gateEngine.GetMethod('EvaluatePhase', $flags)
$analyzeSelected = $analyzer.GetMethod('AnalyzeSelected', $flags)

function New-ObservedState {
    [Activator]::CreateInstance($stateType, $true)
}

function Set-ObservedField {
    param($Value, [string]$Name, $Target)
    $Target.GetType().GetField($Name, $instanceFlags).SetValue($Target, $Value)
}

function Add-ObservedFlow {
    param($State, [int]$ItemId, [double]$Produced, [double]$Consumed)
    $flow = [Activator]::CreateInstance($flowType, $true)
    Set-ObservedField $ItemId 'ItemId' $flow
    Set-ObservedField $Produced 'ProducedPerMinute' $flow
    Set-ObservedField $Consumed 'ConsumedPerMinute' $flow
    $flows = $stateType.GetField('ItemFlows', $instanceFlags).GetValue($State)
    $flows.Add($ItemId, $flow)
}

function Add-ObservedRecipe {
    param($State, [int]$RecipeId, [int]$MachineCount)
    $recipe = [Activator]::CreateInstance($recipeType, $true)
    Set-ObservedField $RecipeId 'RecipeId' $recipe
    Set-ObservedField $MachineCount 'ConfiguredMachineCount' $recipe
    $recipes = $stateType.GetField(
        'RecipeConfigurations', $instanceFlags
    ).GetValue($State)
    $recipes.Add($recipe)
}

function Get-SelectedGate {
    param([string]$PhaseId, $State)
    $evaluation = $evaluatePhase.Invoke($null, @($PhaseId, $State))
    $evaluation.GetType().GetField(
        'Gates', $instanceFlags
    ).GetValue($evaluation)[0]
}

function Get-GateCondition {
    param($Gate, [string]$ConditionId)
    $conditions = $Gate.GetType().GetField(
        'Conditions', $instanceFlags
    ).GetValue($Gate)
    foreach ($condition in $conditions) {
        if ($condition.GetType().GetField(
                'Id', $instanceFlags
            ).GetValue($condition) -eq $ConditionId) {
            return $condition
        }
    }
    return $null
}

$redState = New-ObservedState
Set-ObservedField $true 'ProductionWindowReady' $redState
Add-ObservedFlow $redState 6002 20 0
Add-ObservedRecipe $redState 18 2
$redGate = Get-SelectedGate 'red' $redState
$redCondition = Get-GateCondition $redGate 'red-loop'
if ($null -eq $redCondition -or
    $redCondition.GetType().GetField(
        'Status', $instanceFlags
    ).GetValue($redCondition) -ne 'ready') {
    throw 'RED still requires refinery-output rates as a hard objective.'
}

$dysonGate = Get-SelectedGate 'dyson' (New-ObservedState)
if ($dysonGate.GetType().GetField(
        'Title', $instanceFlags
    ).GetValue($dysonGate) -ne 'Build the Photon swarm') {
    throw 'DYSON does not expose the guide 2.0 Photon-swarm title.'
}

$photonState = New-ObservedState
Set-ObservedField $true 'ProductionWindowReady' $photonState
Add-ObservedFlow $photonState 1208 48 0
Add-ObservedFlow $photonState 1122 48 0
$owned = $stateType.GetField(
    'OwnedItemCounts', $instanceFlags
).GetValue($photonState)
$owned.Add(1122, [long]2000)
$dyson = $stateType.GetField('Dyson', $instanceFlags).GetValue($photonState)
Set-ObservedField $true 'ReceiverTelemetryAvailable' $dyson
Set-ObservedField 4 'ConfiguredPhotonReceiverCount' $dyson
Set-ObservedField 4 'LensedPhotonReceiverCount' $dyson
Set-ObservedField 4 'SustainedPhotonReceiverCount' $dyson
$photonGate = Get-SelectedGate 'photon' $photonState
$receiverCondition = Get-GateCondition $photonGate 'photon-receivers'
if ($null -eq $receiverCondition -or
    $receiverCondition.GetType().GetField(
        'Status', $instanceFlags
    ).GetValue($receiverCondition) -ne 'ready') {
    throw 'PHOTON does not accept four sustained lensed receivers.'
}
Set-ObservedField 3 'SustainedPhotonReceiverCount' $dyson
$photonGate = Get-SelectedGate 'photon' $photonState
$receiverCondition = Get-GateCondition $photonGate 'photon-receivers'
if ($receiverCondition.GetType().GetField(
        'Status', $instanceFlags
    ).GetValue($receiverCondition) -ne 'blocked') {
    throw 'PHOTON does not retain four-receiver continuity as a hard objective.'
}

function Assert-SingleDrainFinding {
    param(
        [string]$PhaseId,
        [int]$WeakItemId,
        [string]$ExpectedFindingId,
        [string]$ExpectedText
    )
    $state = New-ObservedState
    Set-ObservedField $true 'ProductionWindowReady' $state
    Add-ObservedFlow $state $WeakItemId 5 10
    $analysis = $analyzeSelected.Invoke($null, @($state, $PhaseId))
    $findings = $analysis['findings']
    if ($findings.Count -ne 1 -or
        $findings[0]['id'] -ne $ExpectedFindingId -or
        $findings[0]['claim'] -notmatch $ExpectedText) {
        throw "$PhaseId did not emit one focused draining-input finding."
    }
}

Assert-SingleDrainFinding 'purple' 1402 'purple-support-drain' 'Particle Broadband'
Assert-SingleDrainFinding 'green' 1305 'green-support-drain' 'Quantum Chips'
Assert-SingleDrainFinding 'white' 6002 'white-feeder-drain' 'Red Cubes'

$purpleItems = $phaseItems['purple']
foreach ($itemId in @(6004, 1303, 1124, 1402)) {
    if ($purpleItems -notcontains $itemId) {
        throw "PURPLE snapshot evidence is missing item $itemId."
    }
}

Write-Output 'Guide 2.0 phase contract test passed.'
