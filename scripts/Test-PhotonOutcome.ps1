[CmdletBinding()]
param([string]$DllPath)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Test-GuideHandoffs.ps1" -DllPath $DllPath
function PhotonState {
    $s = BridgeState
    (Field $s 'PlanetItemCounts')[101][1122] = 2000L
    foreach ($id in @(6001,6002,6003,6004,6005,1122)) {
        $input = New 'SustainedInputReadiness'; SetField $input 'ItemId' $id; SetField $input 'Reason' 'ready'
        SetField $input 'ElapsedGameSeconds' 120.0; SetField $input 'SampleCount' 25; SetField $input 'MinimumRate' 40.0
        (Field $s 'PhotonInputs')[$id] = $input
        $flow = New 'ObservedItemFlow'; SetField $flow 'ItemId' $id; SetField $flow 'OneMinuteAvailable' $true; SetField $flow 'ProducedPerMinute' 40.0
        (Field $s 'ItemFlows')[$id]=$flow
    }
    return $s
}
$s = PhotonState
$conditions = (PhaseAnalysis $s 'photon')['progression']['gateEvaluations'][0]['conditions']
Assert ($conditions.Count -eq 2 -and @($conditions | Where-Object { $_['status'] -ne 'ready' }).Count -eq 0) 'Six inputs and stationary reserve did not pass'
Assert ((Field (PhasePanel $s 'photon') 'Pending').Count -eq 0) 'Healthy PHOTON creates upgrade tasks'
$names = @('Blue Cubes','Red Cubes','Yellow Cubes','Purple Cubes','Green Cubes','Antimatter'); $ids=@(6001,6002,6003,6004,6005,1122)
for ($i=0; $i -lt $ids.Count; $i++) {
    $s = PhotonState; SetField (Field $s 'PhotonInputs')[$ids[$i]] 'Reason' 'below-target'; SetField (Field $s 'ItemFlows')[$ids[$i]] 'ProducedPerMinute' 39.99
    $condition = PhaseCondition $s 'photon' 'photon-inputs'
    Assert ($condition['status'] -eq 'blocked' -and $condition['action'] -eq "Bring $($names[$i]) production to at least 40/min.") 'Individual shortage not actionable'
}
$s = PhotonState
foreach ($id in @(6001,6003,1122)) { SetField (Field $s 'PhotonInputs')[$id] 'Reason' 'below-target'; SetField (Field $s 'ItemFlows')[$id] 'ProducedPerMinute' 20.0 }
Assert ((PhaseCondition $s 'photon' 'photon-inputs')['action'] -match 'Blue Cubes') 'First-shortage order changed'
foreach ($reason in @('warming','unavailable','below-target')) {
    $s = PhotonState; SetField (Field $s 'PhotonInputs')[6001] 'Reason' $reason
    Assert ($null -eq (PhaseCondition $s 'photon' 'photon-inputs')['action']) 'Healthy current rate gets a repeat upgrade during warmup/recovery'
}
foreach ($stock in @(1999L,2000L)) {
    $s = PhotonState; (Field $s 'PlanetItemCounts')[101][1122]=$stock
    $expected=if($stock -eq 2000){'ready'}else{'blocked'}
    Assert ((PhaseCondition $s 'photon' 'antimatter-stock')['status'] -eq $expected) 'Stationary reserve boundary failed'
}
$s = PhotonState; (Field $s 'PlanetItemCounts')[101][1122]=0L; (Field $s 'PlayerItemCounts')[1122]=2000L; (Field $s 'OwnedItemCounts')[1122]=2000L
Assert ((PhaseCondition $s 'photon' 'antimatter-stock')['status'] -eq 'blocked') 'Icarus stock passed stationary reserve'
foreach ($rate in @(20.0,39.99,40.0,48.0)) {
    $s=PhotonState; foreach ($id in $ids) { SetField (Field $s 'ItemFlows')[$id] 'ProducedPerMinute' $rate }
    $rates = Field (PhasePanel $s 'photon') 'CubeRates'; $expected=if($rate -ge 40){'Comfortable'}else{'BelowMinimum'}
    Assert ($rates.Count -eq 5) 'PHOTON Cube set changed'
    foreach ($r in $rates) { Assert ($r.Level.ToString() -eq $expected) 'PHOTON color threshold failed' }
}
$s=PhotonState; SetField (Field $s 'ItemFlows')[6002] 'OneMinuteAvailable' $false
Assert ((Field (PhasePanel $s 'photon') 'CubeRates')[1].Level.ToString() -eq 'Unknown') 'Unknown rate rendered as healthy'
$s=PhotonState; SetField (Field $s 'ItemFlows')[6002] 'ProducedPerMinute' 20.0
Assert ((Field (PhasePanel $s 'red') 'CubeRates')[1].Level.ToString() -eq 'Comfortable') 'Other phase color bands changed'
$s=BridgeState
Assert ((PhaseCondition $s 'photon' 'photon-inputs')['status'] -ne 'ready') 'R6 receiver/stock-only state still passes'
Assert ($null -eq (PhaseCondition $s 'photon' 'photon-receivers')) 'Receiver construction still gates PHOTON'
Write-Host 'PHOTON six-input outcome, first shortage, stock scope, current colors and R6 fixtures passed.'
$s=PhotonState
SetField (Field $s 'PhotonInputs')[6001] 'Reason' 'below-target'
SetField (Field $s 'ItemFlows')[6001] 'ProducedPerMinute' 20.0
$a=PhaseAnalysis $s 'photon'
$risks=[Collections.Generic.List[object]]::new()
foreach ($id in @(6001,6002,6003,6004)) {
    $risk=[Collections.Generic.Dictionary[string,object]]::new(); $risk['itemId']=$id; $risk['name']="Input $id"; $risk['state']='draining'; $risk['actionable']=$true; $risks.Add($risk)
}
$a['productionRisk']['actionable']=$risks
$p=Call 'GuidePanelModelBuilder' 'Build' @($a,$s,$null,$null,$null)
Assert ((Field $p 'Context').Count -eq 3 -and (Field $p 'Pending').Count -eq 1) 'PHOTON Pending disturbed bounded Current Status risks'
Write-Host 'PHOTON Pending and maximum-three risk projection coexist.'
