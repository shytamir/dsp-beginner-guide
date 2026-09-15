[CmdletBinding()]
param([string]$DllPath)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Test-IlsResearch.ps1" -DllPath $DllPath
function HardwareState {
    $s = ResearchState
    SetField $s 'PlayerPlanetId' 101
    foreach ($id in @(101,102)) { (Field $s 'AvailableStationPlanets').Add($id) | Out-Null }
    return $s
}
function Station($State,[int]$Planet,[int]$Id,[int]$Idle=0,[int]$Working=0) {
    $station = New 'ObservedStationState'
    SetField $station 'PlanetId' $Planet
    SetField $station 'StationId' $Id
    SetField $station 'IsStellar' $true
    SetField $station 'EvidenceAvailable' $true
    SetField $station 'IdleShipCount' $Idle
    SetField $station 'WorkShipCount' $Working
    (Field $State 'Stations').Add($station)
    return $station
}
function Slot($State,[int]$Planet,[int]$Id,[int]$Item,[string]$Policy,[long]$Count=0) {
    $slot = New 'ObservedStationSlot'
    SetField $slot 'PlanetId' $Planet
    SetField $slot 'StationId' $Id
    SetField $slot 'ItemId' $Item
    SetField $slot 'RemoteLogic' $Policy
    SetField $slot 'IsStellar' $true
    SetField $slot 'Count' $Count
    (Field $State 'StationSlots').Add($slot)
    $owner = (Field $State 'Stations') | Where-Object { $_.PlanetId -eq $Planet -and $_.StationId -eq $Id }
    (Field $owner 'Slots').Add($slot)
}
function Transport($State) { Call 'IlsTransportEvidence' 'Build' @($State) }
$s = HardwareState
foreach ($towers in @(0L,1L,2L)) {
    (Field $s 'PlayerItemCounts')[2104] = $towers
    foreach ($vessels in @(4L,5L)) {
        (Field $s 'PlayerItemCounts')[5002] = $vessels
        $evidence = Transport $s
        Assert ($evidence.HardwareReady -eq ($towers -eq 2L -and $vessels -eq 5L)) 'Finished package boundary failed'
        Assert (-not $evidence.DeploymentReady) 'Undeployed hardware passed deployment'
    }
}
$s = HardwareState
foreach ($id in @(1103,1106,1303,1206,1107,1203,6003)) { (Field $s 'PlayerItemCounts')[$id] = 99999L }
Assert (-not (Transport $s).HardwareReady) 'Raw reserve passed the finished-package checkpoint'
$s = HardwareState
(Field $s 'PlayerItemCounts')[2104] = 1L
(Field $s 'PlayerItemCounts')[5002] = 5L
$homeStation = Station $s 101 1
Assert ((Transport $s).HardwareReady) 'Partial deployment lost hardware'
(Field $s 'PlayerItemCounts')[2104] = 0L
(Field $s 'PlayerItemCounts')[5002] = 0L
$sourceStation = Station $s 102 1
SetField $homeStation 'IdleShipCount' 2
SetField $homeStation 'WorkShipCount' 3
Assert ((Transport $s).HardwareReady) 'Idle/working Vessels were not counted once'
Assert (-not (Transport $s).DeploymentReady) 'Unconfigured stations passed deployment'
foreach ($id in @(1106,1105)) { Slot $s 101 1 $id 'Demand'; Slot $s 102 1 $id 'Supply' }
Assert ((Transport $s).DeploymentReady) 'Configured home fleet and endpoints did not pass deployment'
Station $s 203 1 100 | Out-Null
Assert ((Field (Transport $s) 'AssignedVessels') -eq 5) 'Unrelated off-world fleet inflated the package'
$abroad = HardwareState
(Field $abroad 'PlayerItemCounts')[2104] = 2L
(Field $abroad 'PlayerItemCounts')[5002] = 5L
SetField $abroad 'PlayerPlanetId' 102
Assert (-not (Transport $abroad).HardwareReady) 'Off-world Icarus stock counted as home stock'
foreach ($flag in @('PlayerInventoryAvailable','PlayerLocationAvailable')) {
    $missing = HardwareState
    SetField $missing $flag $false
    Assert (-not (Transport $missing).Available) 'Missing inventory/location evidence became complete'
}
$missing = HardwareState
(Field $missing 'AvailableStationPlanets').Remove(101) | Out-Null
Assert ((CargoCondition $missing 'ils-rush-hardware' 3)['status'] -eq 'unknown') 'Missing station evidence became missing hardware'
$ready = HardwareState
(Field $ready 'PlayerItemCounts')[2104] = 2L
(Field $ready 'PlayerItemCounts')[5002] = 5L
Assert ($null -eq (CargoCondition $ready 'ils-rush-hardware' 3)['action']) 'Completed package still asks for components'
Assert ((CargoCondition $ready 'ils-rush-deployment' 3)['action'] -match 'Remote Demand') 'Completed package lacks deployment task'
(Field $ready 'UnlockedTechIds').Remove(1605) | Out-Null
Assert ($null -eq (CargoCondition $ready 'ils-rush-deployment' 3)['action']) 'Deployment preceded ILS research'
Write-Host 'ILS finished hardware, endpoint scope, deployment and availability tests passed.'
