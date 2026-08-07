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

$resources = $assembly.GetManifestResourceNames()
foreach ($resourceName in @(
        'DspGuideCheck.MatrixIcons.t-matrix.png',
        'DspGuideCheck.MatrixIcons.e-matrix.png',
        'DspGuideCheck.MatrixIcons.c-matrix.png',
        'DspGuideCheck.MatrixIcons.i-matrix.png',
        'DspGuideCheck.MatrixIcons.g-matrix.png',
        'DspGuideCheck.MatrixIcons.u-matrix.png',
        'DspGuideCheck.MatrixIcons.1605.png',
        'DspGuideCheck.MatrixIcons.solar-collector.png',
        'DspGuideCheck.MatrixIcons.photon-capacitor-full.png',
        'DspGuideCheck.MatrixIcons.signal-402.png',
        'DspGuideCheck.MatrixIcons.signal-404.png'
    )) {
    if ($resources -cnotcontains $resourceName) {
        throw "Matrix icon resource is missing: $resourceName"
    }
}

$richText = $assembly.GetType(
    'DspProgressionStatusExporter.GuidePanelController+GuideRichText', $true
)
$titleMethod = $richText.GetMethod('Title', $flags)
$phaseTitleColors = [ordered]@{
    blue = '5AB8FF'
    red = 'FF6B6B'
    ils = '4FD1C5'
    yellow = 'FFD166'
    purple = 'C792EA'
    green = '65D98C'
    dyson = 'DD6F5D'
    photon = 'E0AF68'
    white = 'E9EEF7'
}
foreach ($entry in $phaseTitleColors.GetEnumerator()) {
    $renderedTitle = $titleMethod.Invoke(
        $null,
        @($entry.Key, 'Presentation check')
    )
    $expectedTag = '<color=#' + $entry.Value + '>[' +
        $entry.Key.ToUpperInvariant() + ']</color>'
    if (-not $renderedTitle.StartsWith($expectedTag)) {
        throw "Phase title presentation mismatch for $($entry.Key)."
    }
}

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
$dysonPhase = $phases | Where-Object {
    $_.GetType().GetField('Id').GetValue($_) -eq 'dyson'
}
if ($dysonPhase.GetType().GetField(
        'Title'
    ).GetValue($dysonPhase) -ne 'Build the Photon swarm') {
    throw 'Guide analysis does not expose the guide 2.0 DYSON title.'
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
if ($phaseItems.Count -ne 9) {
    throw 'Snapshot evidence routing does not expose exactly nine phases.'
}
foreach ($phaseId in @(
        'blue', 'red', 'ils', 'yellow', 'purple',
        'green', 'dyson', 'photon', 'white'
    )) {
    if (-not $phaseItems.ContainsKey($phaseId)) {
        throw "Snapshot evidence routing is missing $phaseId."
    }
}
foreach ($removedPhaseId in @(
        'bootstrap', 'flight', 'titanium', 'sphere',
        'warp', 'logistics', 'complete'
    )) {
    if ($phaseItems.ContainsKey($removedPhaseId)) {
        throw "Snapshot evidence routing still exposes $removedPhaseId."
    }
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
$progressType = $assembly.GetType(
    'DspProgressionStatusExporter.ObservedTechProgress', $true
)
$panelBuilder = $assembly.GetType(
    'DspProgressionStatusExporter.GuidePanelModelBuilder', $true
)
$evaluatePhase = $gateEngine.GetMethod('EvaluatePhase', $flags)
$analyzeSelected = $analyzer.GetMethod('AnalyzeSelected', $flags)
$buildPanel = $panelBuilder.GetMethod('Build', $flags)

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
    Set-ObservedField $true 'OneMinuteAvailable' $flow
    Set-ObservedField $Produced 'ProducedPerMinute' $flow
    Set-ObservedField $Consumed 'ConsumedPerMinute' $flow
    Set-ObservedField $true 'TenMinuteAvailable' $flow
    Set-ObservedField $true 'TenMinuteReady' $flow
    Set-ObservedField $Produced 'TenMinuteProducedPerMinute' $flow
    Set-ObservedField $Consumed 'TenMinuteConsumedPerMinute' $flow
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

function Add-UnlockedTech {
    param($State, [int]$TechId)
    $values = $stateType.GetField(
        'UnlockedTechIds', $instanceFlags
    ).GetValue($State)
    $values.Add($TechId) | Out-Null
}

function Add-QueuedTech {
    param($State, [int]$TechId)
    $values = $stateType.GetField(
        'QueuedTechIds', $instanceFlags
    ).GetValue($State)
    $values.Add($TechId) | Out-Null
}

function Add-TechProgress {
    param(
        $State,
        [int]$TechId,
        [long]$HashUploaded,
        [long]$HashNeeded
    )
    $progress = [Activator]::CreateInstance($progressType, $true)
    Set-ObservedField $TechId 'TechId' $progress
    Set-ObservedField $HashUploaded 'HashUploaded' $progress
    Set-ObservedField $HashNeeded 'HashNeeded' $progress
    $values = $stateType.GetField(
        'TechProgress', $instanceFlags
    ).GetValue($State)
    $values.Add($TechId, $progress)
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

function Get-PanelModel {
    param([string]$PhaseId, $State)
    $analysis = $analyzeSelected.Invoke($null, @($State, $PhaseId))
    $buildPanel.Invoke($null, @($analysis, $State, $null, $null, $null))
}

function Get-PanelRow {
    param($Panel, [string]$Collection, [string]$RowId)
    $rows = $Panel.GetType().GetField(
        $Collection, $instanceFlags
    ).GetValue($Panel)
    foreach ($row in $rows) {
        if ($row.GetType().GetField(
                'Id', $instanceFlags
            ).GetValue($row) -eq $RowId) {
            return $row
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

$whiteUnresearched = New-ObservedState
$whitePanel = Get-PanelModel 'white' $whiteUnresearched
$whiteResearch = Get-PanelRow $whitePanel 'Objectives' 'tech-1507'
$whitePending = $whitePanel.GetType().GetField(
    'Pending', $instanceFlags
).GetValue($whitePanel)
if ($whiteResearch.GetType().GetField(
        'Label', $instanceFlags
    ).GetValue($whiteResearch) -ne 'White Cubes researched' -or
    $whitePending.Count -ne 1 -or
    $whitePending[0].GetType().GetField(
        'Label', $instanceFlags
    ).GetValue($whitePending[0]) -ne 'Complete Mission Completed research.') {
    throw 'WHITE unresearched presentation is not concise and single-action.'
}

$whiteState = New-ObservedState
Set-ObservedField $true 'ProductionWindowReady' $whiteState
Add-UnlockedTech $whiteState 1507
Add-QueuedTech $whiteState 1508
Add-TechProgress $whiteState 1508 370 1000
Add-ObservedFlow $whiteState 6006 40 0
Add-ObservedRecipe $whiteState 75 7
$whiteOwned = $stateType.GetField(
    'OwnedItemCounts', $instanceFlags
).GetValue($whiteState)
$whiteOwned.Add(6006, [long]1240)
$whitePanel = Get-PanelModel 'white' $whiteState
$whiteProduction = Get-PanelRow $whitePanel 'Objectives' 'white-production'
$mission = Get-PanelRow $whitePanel 'Objectives' 'mission-completed'
$productionLabel = $whiteProduction.GetType().GetField(
    'Label', $instanceFlags
).GetValue($whiteProduction)
$productionDetail = $whiteProduction.GetType().GetField(
    'Detail', $instanceFlags
).GetValue($whiteProduction)
$missionDetail = $mission.GetType().GetField(
    'Detail', $instanceFlags
).GetValue($mission)
if ($productionLabel -ne 'Ten labs sustain 40 White Cubes/min' -or
    $productionDetail -ne '7/10 labs configured; 1,240 White Cubes stored' -or
    $missionDetail -ne 'Mission Completed 37% done' -or
    $productionLabel -match 'Universe Matri' -or
    $productionDetail -match 'White Cubes/min') {
    throw 'WHITE lab, storage, or active research presentation is too verbose.'
}
$whiteExport = $whiteState.Export()
if ($whiteExport['modelVersion'] -ne '2.1' -or
    $whiteExport['techProgress'].Count -ne 1 -or
    $whiteExport['techProgress'][0]['techId'] -ne 1508 -or
    $whiteExport['techProgress'][0]['percent'] -ne 37) {
    throw 'Normalized WHITE research progress is not exported authoritatively.'
}

$whiteQueued = New-ObservedState
Add-UnlockedTech $whiteQueued 1507
Add-QueuedTech $whiteQueued 1508
$whiteQueuedPanel = Get-PanelModel 'white' $whiteQueued
$queuedMission = Get-PanelRow $whiteQueuedPanel 'Objectives' 'mission-completed'
if ($queuedMission.GetType().GetField(
        'Detail', $instanceFlags
    ).GetValue($queuedMission) -ne 'Mission Completed queued') {
    throw 'WHITE queued Mission Completed state is not concise.'
}

Add-UnlockedTech $whiteState 1508
$whiteCompletePanel = Get-PanelModel 'white' $whiteState
$completeMission = Get-PanelRow $whiteCompletePanel 'Objectives' 'mission-completed'
$completePending = $whiteCompletePanel.GetType().GetField(
    'Pending', $instanceFlags
).GetValue($whiteCompletePanel)
if ($completeMission.GetType().GetField(
        'Detail', $instanceFlags
    ).GetValue($completeMission) -ne 'Mission Completed complete' -or
    $completePending.Count -ne 0) {
    throw 'WHITE completed Mission Completed state is not concise and final.'
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

Assert-SingleDrainFinding 'purple' 1402 'production-risk-1402' 'Particle Broadband'
Assert-SingleDrainFinding 'green' 1305 'production-risk-1305' 'Quantum Chips'
Assert-SingleDrainFinding 'white' 6002 'production-risk-6002' 'Red Cubes'

$purpleItems = $phaseItems['purple']
foreach ($itemId in @(6004, 1303, 1124, 1402)) {
    if ($purpleItems -notcontains $itemId) {
        throw "PURPLE snapshot evidence is missing item $itemId."
    }
}

$panelSource = Get-Content -Raw -LiteralPath (
    Join-Path (Split-Path -Parent $PSScriptRoot) `
        'src\DspProgressionStatusExporter\GuidePanelModel.cs'
)
if (-not $panelSource.Contains('{ "contractVersion", "2.7" }')) {
    throw 'Panel model contract version is not 2.7.'
}
$controllerSource = Get-Content -Raw -LiteralPath (
    Join-Path (Split-Path -Parent $PSScriptRoot) `
        'src\DspProgressionStatusExporter\GuidePanelController.cs'
)
if ($controllerSource -notmatch
    '"SourceGuideLink",\s*cubeRateColumn\.transform,') {
    throw "DON'T PANIC is not parented to the fixed Cube-rate rail."
}
if (-not $controllerSource.Contains(
        'CubeRateSquareSize - DontPanicWidth')) {
    throw "DON'T PANIC is not right-aligned with the Cube-rate rail."
}
if (-not $controllerSource.Contains(
        'cubeRateViews.Count * (CubeRateSquareSize + CubeRateGap)')) {
    throw "DON'T PANIC is not placed after the last visible Cube rate."
}
if (-not $controllerSource.Contains(
        'UIRoot.instance.uiGame.veinDetail.nodePrefab.infoText') -or
    -not $controllerSource.Contains(
        'GetMember(veinDetail, "nodePrefab")') -or
    -not $controllerSource.Contains(
        'GetMember(nodePrefab, "infoText") as Text') -or
    -not $controllerSource.Contains(
        'infoText.GetComponent<Outline>()')) {
    throw 'Panel typography is not sourced from the native vein-label prefab.'
}
if (-not $controllerSource.Contains(
        'Native vein-label typography unavailable; using embedded fallback.') -or
    -not $controllerSource.Contains('nativeTextWarningLogged')) {
    throw 'Native typography does not have a bounded fallback warning.'
}
$snapshotSource = Get-Content -Raw -LiteralPath (
    Join-Path (Split-Path -Parent $PSScriptRoot) `
        'src\DspProgressionStatusExporter\CompactSnapshotBuilder.cs'
)
if (-not $snapshotSource.Contains('{ "presentation", presentation }')) {
    throw 'Compact snapshots do not expose focused typography diagnostics.'
}
$pluginSource = Get-Content -Raw -LiteralPath (
    Join-Path (Split-Path -Parent $PSScriptRoot) `
        'src\DspProgressionStatusExporter\Plugin.cs'
)
if (-not $pluginSource.Contains('SchemaVersion = "2.11"')) {
    throw 'Snapshot schema version is not 2.11.'
}
foreach ($obsoleteFindingId in @(
        'gas-giant-opportunity',
        'fire-ice-graphene-route',
        'fractionator-deuterium-route',
        'combat-investment',
        'dyson-route-choice',
        'phase-matrix-rate',
        'dyson-generation-shortfall'
    )) {
    if ($panelSource.Contains($obsoleteFindingId)) {
        throw "Panel presentation still translates $obsoleteFindingId."
    }
}

Write-Output 'Guide 2.0 phase contract test passed.'
