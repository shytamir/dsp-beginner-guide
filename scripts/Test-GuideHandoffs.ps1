[CmdletBinding()]
param([string]$DllPath)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Test-IlsStages.ps1" -DllPath $DllPath
function PhaseAnalysis($State,[string]$Phase) { Call 'GuideAnalyzer' 'AnalyzeSelected' @($State,$Phase,1) }
function PhaseCondition($State,[string]$Phase,[string]$Id) { (PhaseAnalysis $State $Phase)['progression']['gateEvaluations'][0]['conditions'] | Where-Object { $_['id'] -eq $Id } }
function PhasePanel($State,[string]$Phase) { Call 'GuidePanelModelBuilder' 'Build' @((PhaseAnalysis $State $Phase),$State,$null,$null,$null) }
function LabState([int]$Recipe,[int]$Item) {
    $s = New 'ObservedGameState'
    SetField $s 'RecipeTelemetryAvailable' $true
    SetField $s 'ProductionWindowReady' $true
    $r = New 'ObservedRecipeConfiguration'; SetField $r 'RecipeId' $Recipe; SetField $r 'ConfiguredMachineCount' 3
    (Field $s 'RecipeConfigurations').Add($r)
    $f = New 'ObservedItemFlow'; SetField $f 'ItemId' $Item; SetField $f 'OneMinuteAvailable' $true; SetField $f 'ProducedPerMinute' 20.0
    (Field $s 'ItemFlows')[$Item] = $f
    return $s
}
foreach ($case in @(@('yellow',27,6003),@('purple',55,6004))) {
    $phase,$recipe,$item = $case
    $s = LabState $recipe $item
    Assert ((PhaseCondition $s $phase "$phase-labs")['status'] -eq 'ready') 'Healthy three-Lab line failed without separate storage'
    Assert (@((PhaseAnalysis $s $phase)['progression']['gateEvaluations'][0]['conditions'] | Where-Object { $_['id'] -eq "$phase-inputs" }).Count -eq 0) 'Mandatory input-buffer objective remains'
    Assert ((Field (PhasePanel $s $phase) 'Pending').Count -eq 0) 'Healthy line creates a buffer task'
    SetField (Field $s 'ItemFlows')[$item] 'ProducedPerMinute' 0.0
    Assert ((PhaseCondition $s $phase "$phase-labs")['status'] -eq 'blocked') 'Stopped production completed phase'
    SetField (Field $s 'ItemFlows')[$item] 'ProducedPerMinute' 20.0
    SetField (Field $s 'RecipeConfigurations')[0] 'ConfiguredMachineCount' 2
    Assert ((PhaseCondition $s $phase "$phase-labs")['status'] -eq 'blocked') 'Two Labs completed a three-Lab phase'
    SetField $s 'RecipeTelemetryAvailable' $false
    Assert ((PhaseCondition $s $phase "$phase-labs")['status'] -eq 'unknown') 'Missing recipe evidence became blocked'
    SetField $s 'RecipeTelemetryAvailable' $true
    SetField (Field $s 'ItemFlows')[$item] 'OneMinuteAvailable' $false
    Assert ((PhaseCondition $s $phase "$phase-labs")['status'] -eq 'unknown') 'Missing item evidence became zero production'
}
Assert ((PhaseCondition (New 'ObservedGameState') 'green' 'green-inputs')['required']) 'GREEN storage requirement changed'
Write-Host 'YELLOW/PURPLE no-buffer readiness, stopped, incomplete and unavailable fixtures passed.'
function BridgeState {
    $s = LabState 74 1122
    foreach ($id in @(1504,1505,1506)) { (Field $s 'AvailableTechIds').Add($id) | Out-Null; (Field $s 'UnlockedTechIds').Add($id) | Out-Null }
    SetField $s 'ResearchQueueAvailable' $true
    foreach ($id in @(1208,1501)) {
        $f = New 'ObservedItemFlow'; SetField $f 'ItemId' $id; SetField $f 'OneMinuteAvailable' $true
        SetField $f 'ProducedPerMinute' 48.0; SetField $f 'ConsumedPerMinute' 48.0; (Field $s 'ItemFlows')[$id] = $f
    }
    (Field $s 'PlanetItemCounts')[101] = [Collections.Generic.Dictionary[int,long]]::new()
    (Field $s 'PlanetItemCounts')[101][1122] = 10L
    (Field $s 'AvailablePlanetInventories').Add(101) | Out-Null
    $dyson = Field $s 'Dyson'
    SetField $dyson 'SwarmSailCount' 100L
    SetField $dyson 'SwarmGenerationWatts' 1000000.0
    SetField $dyson 'ReceiverTelemetryAvailable' $true
    foreach ($name in @('ConfiguredPhotonReceiverCount','LensedPhotonReceiverCount','SustainedPhotonReceiverCount')) { SetField $dyson $name 4 }
    return $s
}
$s = BridgeState
Assert ((PhaseCondition $s 'dyson' 'dyson-conversion')['status'] -eq 'ready') 'Observed conversion failed'
Assert ((Field (PhasePanel $s 'dyson') 'SourceGuideAnchor') -eq 'receiver-antimatter-bridge') 'Ready swarm lacks bridge anchor'
$check = PhaseCondition $s 'dyson' 'dyson-handoff'
Assert ($check['required'] -and $check['status'] -eq 'unknown' -and $check['evidenceKind'] -eq 'player-check') 'Physical handoff was automatically completed'
foreach ($count in @(0,3,4)) {
    SetField (Field $s 'Dyson') 'SustainedPhotonReceiverCount' $count
    $expected = if ($count -eq 4) { 'ready' } else { 'blocked' }
    Assert ((PhaseCondition $s 'dyson' 'dyson-receivers')['status'] -eq $expected) 'Bridge receiver count policy failed'
}
foreach ($invalid in @('recipe','conversion','icarus-only','missing')) {
    $s = BridgeState
    switch ($invalid) {
        'recipe' { SetField (Field $s 'RecipeConfigurations')[0] 'RecipeId' 75 }
        'conversion' { SetField (Field $s 'ItemFlows')[1208] 'ConsumedPerMinute' 0.0 }
        'icarus-only' { (Field $s 'PlanetItemCounts')[101][1122] = 0L; (Field $s 'PlayerItemCounts')[1122] = 5000L; (Field $s 'OwnedItemCounts')[1122] = 5000L }
        'missing' { SetField (Field $s 'ItemFlows')[1208] 'OneMinuteAvailable' $false }
    }
    Assert ((PhaseCondition $s 'dyson' 'dyson-conversion')['status'] -ne 'ready') "Invalid conversion passed: $invalid"
}
$s = BridgeState
foreach ($id in @(1504,1505,1506)) { (Field $s 'UnlockedTechIds').Remove($id) | Out-Null }
foreach ($name in @('Ray Receiver','Planetary Ionosphere Utilization','Dirac Inversion Mechanism')) {
    $research = PhaseCondition $s 'dyson' 'dyson-bridge-research'
    Assert ($research['action'] -eq "Research $name.") 'Bridge research prerequisite order failed'
    $id = @{ 'Ray Receiver'=1504; 'Planetary Ionosphere Utilization'=1505; 'Dirac Inversion Mechanism'=1506 }[$name]
    (Field $s 'UnlockedTechIds').Add($id) | Out-Null
}
$a = @((PhaseAnalysis $s 'dyson')['progression']['gateEvaluations'][0]['conditions'] | ForEach-Object { $_['id'] }) -join ','
$b = @((PhaseAnalysis (New 'ObservedGameState') 'dyson')['progression']['gateEvaluations'][0]['conditions'] | ForEach-Object { $_['id'] }) -join ','
Assert ($a -eq $b) 'DYSON objectives changed with evidence'
Assert ((Field (PhasePanel (New 'ObservedGameState') 'dyson') 'SourceGuideAnchor') -eq 'dyson') 'Unready swarm skipped its guide anchor'
Write-Host 'DYSON bridge research, receivers, conversion, stationary stock, stable objectives and manual handoff passed.'
