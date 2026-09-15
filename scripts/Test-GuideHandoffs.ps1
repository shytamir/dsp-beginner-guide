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
