[CmdletBinding()]
param([string]$DllPath)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Test-IlsStages.ps1" -DllPath $DllPath
function Rates([double]$Value=40) {
    $r = [Collections.Generic.Dictionary[int,double]]::new()
    foreach ($id in @(6001,6002,6003,6004,6005,1122)) { $r[$id] = $Value }
    return ,$r
}
function SampleRates($Policy,[double]$Seconds,$Rates) { $Policy.Sample([long]($Seconds * 60),$Rates) }
foreach ($rate in @(0.0,39.99,40.0,48.0)) {
    $p = New 'PhotonReadiness'; $r = Rates $rate
    foreach ($sec in (0..24 | ForEach-Object { $_*5 })) { SampleRates $p $sec $r }
    $expected = if ($rate -ge 40) { 'ready' } else { 'below-target' }
    foreach ($id in @(6001,6002,6003,6004,6005,1122)) { Assert ($p.Evaluate($id).Reason -eq $expected) 'Rate threshold failed' }
    Assert ($p.Evaluate(6006).Reason -eq 'unavailable') 'White Cubes entered the six-input policy'
}
$p = New 'PhotonReadiness'; $r = Rates
foreach ($sec in (0..23 | ForEach-Object { $_*5 })) { SampleRates $p $sec $r }
SampleRates $p 119 $r
Assert ($p.Evaluate(6001).Reason -eq 'warming') '119 seconds passed'
SampleRates $p 120 $r
Assert ($p.Evaluate(6001).Reason -eq 'ready') '120 seconds failed'
foreach ($count in @(19,20)) {
    $p = New 'PhotonReadiness'
    for ($i=0; $i -lt $count; $i++) { SampleRates $p (120.0*$i/($count-1)) $r }
    $expected = if ($count -eq 20) { 'ready' } else { 'warming' }
    Assert ($p.Evaluate(6001).Reason -eq $expected) '20-sample boundary failed'
}
$p = New 'PhotonReadiness'; $r = Rates
$r[6001] = 39.99; SampleRates $p 0 $r; $r[6001] = 40
foreach ($sec in (1..24 | ForEach-Object { $_*5 })) { SampleRates $p $sec $r }
Assert ($p.Evaluate(6001).Reason -eq 'below-target') 'Isolated low boundary sample was forgotten'
SampleRates $p 125 $r
Assert ($p.Evaluate(6001).Reason -eq 'ready') 'Recovery failed after low sample aged out'
$r.Remove(6001) | Out-Null; SampleRates $p 130 $r
Assert ($p.Evaluate(6001).Reason -eq 'unavailable') 'Missing sample passed readiness'
$r[6001] = 40; SampleRates $p 135 $r
Assert ($p.Evaluate(6001).Reason -eq 'warming' -and $p.Evaluate(6001).SampleCount -eq 1) 'Unavailable sample did not break history'
$p = New 'PhotonReadiness'
foreach ($i in 0..99) { SampleRates $p 0 $r }
Assert ($p.Evaluate(6001).SampleCount -eq 1 -and $p.Evaluate(6001).Reason -eq 'warming') 'Pause filled the window'
foreach ($sec in 1..200) { SampleRates $p $sec $r; Assert ($p.Evaluate(6001).SampleCount -le 26) 'History exceeded fixed bound' }
$p.Clear()
Assert ($p.Evaluate(6001).Reason -eq 'unavailable') 'Session reset retained readiness'
Write-Host 'PHOTON time/sample/rate boundaries, low-sample recovery, missing data, pause and bounds passed.'
