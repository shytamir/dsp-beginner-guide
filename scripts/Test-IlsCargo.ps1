[CmdletBinding()]
param([string]$DllPath)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Test-IlsStages.ps1" -DllPath $DllPath
function CargoState {
    $s = New 'ObservedGameState'
    SetField $s 'StarterPlanetId' 101
    SetField $s 'PlayerPlanetId' 102
    SetField $s 'PlayerLocationAvailable' $true
    SetField $s 'PlayerInventoryAvailable' $true
    foreach ($planet in @(101,102)) {
        (Field $s 'PlanetItemCounts').Add($planet, [Collections.Generic.Dictionary[int,long]]::new())
        (Field $s 'AvailablePlanetInventories').Add($planet) | Out-Null
    }
    foreach ($id in @(1105,1106)) {
        $flow = New 'ObservedFactoryItemFlow'
        SetField $flow 'PlanetId' 102
        SetField $flow 'ItemId' $id
        SetField $flow 'OneMinuteAvailable' $true
        SetField $flow 'ProducedPerMinute' 30.0
        (Field $s 'FactoryItemFlows').Add($flow)
    }
    return $s
}
function CargoCondition($State, [string]$Id, [int]$Stage = 2) {
    $analysis = Call 'GuideAnalyzer' 'AnalyzeSelected' @($State, 'ils', $Stage)
    $analysis['progression']['gateEvaluations'][0]['conditions'] | Where-Object { $_['id'] -eq $Id }
}
$s = CargoState
(Field $s 'PlanetItemCounts')[102][1106] = 860L
(Field $s 'PlanetItemCounts')[102][1105] = 520L
Assert ((CargoCondition $s 'ils-expedition-production')['status'] -eq 'ready') 'Finished smelting not ready'
Assert ((CargoCondition $s 'ils-expedition-cargo')['status'] -eq 'blocked') 'Remote stock claimed cargo aboard'
Assert ((CargoCondition $s 'ils-expedition-cargo')['action'] -match '860 Titanium Ingots and 520 High-Purity Silicon') 'Missing amounts not actionable'
foreach ($titanium in @(400L,860L)) {
    (Field $s 'PlayerItemCounts')[1106] = $titanium
    (Field $s 'PlayerItemCounts')[1105] = 520L
    $expected = if ($titanium -eq 860L) { 'ready' } else { 'blocked' }
    Assert ((CargoCondition $s 'ils-expedition-cargo')['status'] -eq $expected) 'Aboard threshold boundary failed'
}
Assert ((CargoCondition $s 'ils-expedition-home')['action'] -eq 'Bring the cargo home.') 'Full aboard cargo lacks return task'
SetField $s 'PlayerPlanetId' 0
Assert ((CargoCondition $s 'ils-expedition-cargo')['status'] -eq 'ready') 'Transit lost aboard cargo'
SetField $s 'PlayerPlanetId' 101
foreach ($unloaded in @(0L,300L,860L)) {
    (Field $s 'PlayerItemCounts')[1106] = 860L - $unloaded
    (Field $s 'PlanetItemCounts')[101][1106] = $unloaded
    Assert ((CargoCondition $s 'ils-expedition-home')['status'] -eq 'ready') 'Partial unloading lost secured cargo'
    Assert ((CargoCondition $s 'ils-expedition-cargo')['evidence'] -match '860/860') 'Unloading counted cargo twice'
}
(Field $s 'PlanetItemCounts')[101][1106] = 0L
Assert ((CargoCondition $s 'ils-expedition-home')['status'] -eq 'blocked') 'Spent stock incorrectly proves historical delivery'
Assert ((CargoCondition $s 'ils-expedition-cargo')['action'] -match '860 Titanium') 'Spent cargo shortfall omitted'

foreach ($flag in @('PlayerInventoryAvailable','PlayerLocationAvailable')) {
    $missing = CargoState
    SetField $missing $flag $false
    $cargo = CargoCondition $missing 'ils-expedition-cargo'
    Assert ($cargo['status'] -eq 'unknown' -and $null -eq $cargo['action']) 'Unavailable evidence became a restocking order'
}
$missingHome = CargoState
SetField $missingHome 'PlayerPlanetId' 101
(Field $missingHome 'AvailablePlanetInventories').Remove(101) | Out-Null
Assert ((CargoCondition $missingHome 'ils-expedition-cargo')['status'] -eq 'unknown') 'Missing home inventory became zero stock'
$ore = CargoState
foreach ($flow in (Field $ore 'FactoryItemFlows')) { SetField $flow 'ItemId' ((Field $flow 'ItemId') - 102) }
Assert ((CargoCondition $ore 'ils-expedition-production')['status'] -ne 'ready') 'Ore passed finished smelting'
$homeState = CargoState
SetField $homeState 'PlayerPlanetId' 101
foreach ($flow in (Field $homeState 'FactoryItemFlows')) { SetField $flow 'PlanetId' 101 }
Assert ((Call 'GuideGateEngine' 'FindExpeditionPlanet' @($homeState)) -eq 0) 'Home Silicon became an outpost'
$unknown = CargoState
SetField $unknown 'StarterPlanetId' 0
Assert ((Call 'GuideGateEngine' 'FindExpeditionPlanet' @($unknown)) -eq 0) 'Unknown birth identity proved an outpost'
$multi = CargoState
SetField $multi 'PlayerPlanetId' 101
$extra = New 'ObservedFactoryItemFlow'
SetField $extra 'PlanetId' 103
SetField $extra 'ItemId' 1105
SetField $extra 'OneMinuteAvailable' $true
SetField $extra 'ProducedPerMinute' 60.0
(Field $multi 'FactoryItemFlows').Insert(0,$extra)
Assert ((Call 'GuideGateEngine' 'FindExpeditionPlanet' @($multi)) -eq 102) 'Outpost choice is not deterministic'
SetField $multi 'PlayerPlanetId' 103
Assert ((Call 'GuideGateEngine' 'FindExpeditionPlanet' @($multi)) -eq 103) 'Known current outpost not preferred'
$zero = CargoState
foreach ($flow in (Field $zero 'FactoryItemFlows')) { SetField $flow 'ProducedPerMinute' 0.0 }
Assert ((CargoCondition $zero 'ils-expedition-production')['status'] -eq 'blocked') 'Observed zero production not distinguished from missing'
SetField (Field $zero 'FactoryItemFlows')[0] 'OneMinuteAvailable' $false
Assert ((CargoCondition $zero 'ils-expedition-production')['status'] -eq 'unknown') 'Unavailable rate became zero'
Assert ((CargoCondition (New 'ObservedGameState') 'ils-preparation' 1)['status'] -eq 'unknown') 'Missing departure evidence became absent equipment'
Write-Host 'ILS finished production, cargo scope, location, availability and unloading tests passed.'
