[CmdletBinding()]
param([string]$DllPath)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Test-IlsHardware.ps1" -DllPath $DllPath
function ReceiptState {
    $s = HardwareState
    Station $s 101 2 5 | Out-Null
    Station $s 102 2 | Out-Null
    foreach ($id in @(1106,1105)) { Slot $s 101 2 $id 'Demand'; Slot $s 102 2 $id 'Supply' 50 }
    (Field $s 'FinishedInputTotals')[101] = [Collections.Generic.Dictionary[int,long]]::new()
    foreach ($id in @(1106,1105)) { (Field $s 'FinishedInputTotals')[101][$id] = 100L }
    return $s
}
function ObserveReceipt($Tracker,$Data,$State) {
    $Tracker.GetType().GetMethod('Observe').Invoke($Tracker,@($Data,$State)) | Out-Null
    return Field $State 'IlsReceipt'
}
$tracker = New 'IlsReceiptTracker'
$data = [object]::new()
$s = ReceiptState
Assert ((ObserveReceipt $tracker $data $s).Status -eq 'awaiting-receipt') 'Old imports passed a newly observed configuration'
SetField $s 'TrafficSampleTick' 300L
(Field $s 'FinishedInputTotals')[101][1106] = 150L
$r = ObserveReceipt $tracker $data $s
Assert ($r.TitaniumReceived -and -not $r.SiliconReceived -and $r.Status -eq 'awaiting-receipt') 'One item completed both deliveries'
SetField $s 'TrafficSampleTick' 600L
(Field $s 'FinishedInputTotals')[101][1105] = 150L
Assert ((ObserveReceipt $tracker $data $s).Status -eq 'confirmed') 'Both home imports were not confirmed'
SetField $s 'TrafficSampleTick' 900L
Assert ((ObserveReceipt $tracker $data $s).Status -eq 'confirmed') 'Quiet interval erased receipts'
Station $s 101 1 5 | Out-Null
Station $s 102 1 | Out-Null
foreach ($id in @(1106,1105)) { Slot $s 101 1 $id 'Demand'; Slot $s 102 1 $id 'Supply' 50 }
Assert ((ObserveReceipt $tracker $data $s).Status -eq 'confirmed') 'Equally suitable endpoint replaced the current pair'
Assert ((Field (Field $s 'IlsTransport') 'Home').StationId -eq 2) 'Preferred receiver identity changed'
foreach ($reset in @('reload','counter','epoch','unavailable','policy')) {
    $t = New 'IlsReceiptTracker'; $x = ReceiptState; $d = [object]::new()
    ObserveReceipt $t $d $x | Out-Null
    SetField $x 'TrafficSampleTick' 300L
    foreach ($id in @(1106,1105)) { (Field $x 'FinishedInputTotals')[101][$id] = 200L }
    Assert ((ObserveReceipt $t $d $x).Status -eq 'confirmed') 'Receipt setup failed'
    switch ($reset) {
        'reload' { $d = [object]::new() }
        'counter' { (Field $x 'FinishedInputTotals')[101][1106] = 0L }
        'epoch' { SetField $x 'TrafficEvidenceEpoch' 1L }
        'unavailable' { (Field $x 'FinishedInputTotals').Clear() }
        'policy' { SetField (Field $x 'StationSlots')[0] 'RemoteLogic' 'Supply' }
    }
    $r = ObserveReceipt $t $d $x
    Assert (-not $r.TitaniumReceived -and -not $r.SiliconReceived) "Reset failed: $reset"
}
foreach ($invalid in @('other-destination','wrong-policy','wrong-fleet','ore-and-internal')) {
    $t = New 'IlsReceiptTracker'; $x = ReceiptState; $d = [object]::new()
    ObserveReceipt $t $d $x | Out-Null
    SetField $x 'TrafficSampleTick' 300L
    switch ($invalid) {
        'other-destination' { (Field $x 'FinishedInputTotals')[203] = [Collections.Generic.Dictionary[int,long]]::new(); (Field $x 'FinishedInputTotals')[203][1106] = 500L }
        'wrong-policy' { SetField (Field $x 'StationSlots')[0] 'RemoteLogic' 'Supply' }
        'wrong-fleet' { SetField (Field $x 'Stations')[0] 'IdleShipCount' 0; SetField (Field $x 'Stations')[1] 'IdleShipCount' 5 }
        'ore-and-internal' { $flow = New 'ObservedTrafficFlow'; SetField $flow 'PlanetId' 101; SetField $flow 'ItemId' 1004; SetField $flow 'InputPerMinute' 900.0; SetField $flow 'InternalPerMinute' 900.0; (Field $x 'TrafficFlows').Add($flow) }
    }
    Assert ((ObserveReceipt $t $d $x).Status -ne 'confirmed') "False home delivery: $invalid"
}
$x = ReceiptState
foreach ($slot in (Field $x 'StationSlots')) { SetField $slot 'Count' 0L }
foreach ($flow in (Field $x 'FactoryItemFlows')) { SetField $flow 'ProducedPerMinute' 0.0 }
Assert ((ObserveReceipt (New 'IlsReceiptTracker') ([object]::new()) $x).Status -eq 'source-empty') 'Known empty source lost its task'
Write-Host 'ILS home receipt, endpoint preference, source, reset and false-positive fixtures passed.'
