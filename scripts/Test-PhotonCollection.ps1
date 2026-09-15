[CmdletBinding()]
param([string]$DllPath,[string]$GameRoot='C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program')
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Test-IlsEvidenceCollection.ps1" -DllPath $DllPath -GameRoot $GameRoot
Add-Type @"
public class GcProduct { public int itemId; public long[] total = new long[14]; }
public class GcProductFactory { public int[] productIndices = new int[7000]; public GcProduct[] productPool = new GcProduct[7]; }
public class GcProduction { public GcProductFactory[] factoryStatPool; }
public class GcProductionStatistics { public GcProduction production = new GcProduction(); }
public class GcProductionData { public GcProductionStatistics statistics = new GcProductionStatistics(); public object[] factories = new object[0]; }
"@
$data = [GcProductionData]::new(); $factory = [GcProductFactory]::new(); $index=1
foreach ($id in @(6001,6002,6003,6004,6005,1122)) {
    $item = [GcProduct]::new(); $item.itemId = $id; $item.total[1]=40; $item.total[2]=400
    $factory.productIndices[$id]=$index; $factory.productPool[$index]=$item; $index++
}
$data.statistics.production.factoryStatPool=@($factory)
$collector = New 'ProductionTelemetry'
foreach ($tick in (0..24 | ForEach-Object { [long]($_*300) })) { $collector.Sample($data,$tick) }
$export = $collector.Export()
Assert ($export['photonReadiness'].Count -eq 6) 'Collector did not emit exactly six inputs'
Assert (@($export['photonReadiness'] | Where-Object { $_['reason'] -eq 'ready' }).Count -eq 6) 'Existing native aggregates did not populate policy'
$s = New 'ObservedGameState'; ReadEvidence $s 'ReadProduction' $export
Assert ((Field $s 'PhotonInputs')[6001].Reason -eq 'ready') 'Readiness normalization failed'
foreach ($tick in (25..100 | ForEach-Object { [long]($_*300) })) { $collector.Sample($data,$tick) }
Assert (@($collector.Export()['photonReadiness'] | Where-Object { $_['sampleCount'] -gt 26 }).Count -eq 0) 'Collector exceeded memory bound'
$newData = [GcProductionData]::new(); $newData.statistics.production.factoryStatPool=@($factory)
$collector.Sample($newData,30000L)
Assert ($collector.Export()['photonReadiness'][0]['reason'] -eq 'warming') 'Game-data replacement kept history'
$newData.statistics.production.factoryStatPool=$null
$collector.Sample($newData,30300L)
Assert ($collector.Export()['photonReadiness'][0]['reason'] -eq 'unavailable') 'Collection failure retained readiness'
Write-Host 'PHOTON native sampler connection, normalization, bounds and game replacement passed.'
