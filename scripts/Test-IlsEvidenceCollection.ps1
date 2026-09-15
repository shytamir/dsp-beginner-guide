[CmdletBinding()]
param(
    [string]$DllPath,
    [string]$GameRoot = 'C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program'
)
$ErrorActionPreference = 'Stop'
foreach ($name in @('UnityEngine.CoreModule.dll','UnityEngine.dll','UnityEngine.InputLegacyModule.dll')) {
    [Reflection.Assembly]::LoadFrom((Join-Path $GameRoot "DSPGAME_Data\Managed\$name")) | Out-Null
}
[Reflection.Assembly]::LoadFrom((Join-Path $GameRoot 'BepInEx\core\BepInEx.dll')) | Out-Null
. "$PSScriptRoot/Test-IlsCargo.ps1" -DllPath $DllPath
Add-Type -TypeDefinition @'
public class CargoGrid { public int itemId; public long count; }
public class CargoStorage { public int id = 1; public object[] grids; }
public class CargoStorageSystem { public object[] storagePool; public int storageCursor; public object[] tankPool; public int tankCursor; }
public class CargoTransport { public object[] stationPool; public int stationCursor; }
public class CargoFactory { public CargoStorageSystem factoryStorage; public CargoTransport transport; }
'@
$counts = [Collections.Generic.Dictionary[int,long]]::new()
$storage = [CargoStorage]::new()
$storage.grids = @()
Assert (Call 'Plugin' 'MergeStorageCounts' @($counts,$storage)) 'Empty valid package not available'
$storage.grids = $null
Assert (-not (Call 'Plugin' 'MergeStorageCounts' @($counts,$storage))) 'Missing grids became an empty package'
$grid = [CargoGrid]::new()
$grid.itemId = 1106
$grid.count = 860
$storage.grids = @($grid)
Assert (Call 'Plugin' 'MergeStorageCounts' @($counts,$storage)) 'Valid package unavailable'
Assert ($counts[1106] -eq 860) 'Package count incorrect'
$storage.grids = @([object]::new())
Assert (-not (Call 'Plugin' 'MergeStorageCounts' @($counts,$storage))) 'Missing grid fields passed availability'
$factory = [CargoFactory]::new()
Assert (-not (Call 'Plugin' 'MergeOwnedStorageCounts' @($counts,$factory))) 'Missing storage system became empty stock'
$factory.factoryStorage = [CargoStorageSystem]::new()
$factory.factoryStorage.storagePool = @($null)
$factory.factoryStorage.tankPool = @($null)
Assert (Call 'Plugin' 'MergeOwnedStorageCounts' @($counts,$factory)) 'Valid empty stationary storage unavailable'
$factory.factoryStorage.storagePool = @($null,[object]::new())
Assert (-not (Call 'Plugin' 'MergeOwnedStorageCounts' @($counts,$factory))) 'Missing component identity passed availability'
Assert (-not (Call 'Plugin' 'MergeLogisticsStorageCounts' @($counts,$factory))) 'Missing station pool became empty stock'
$factory.transport = [CargoTransport]::new()
$factory.transport.stationPool = @($null)
Assert (Call 'Plugin' 'MergeLogisticsStorageCounts' @($counts,$factory)) 'Valid zero stations unavailable'
$factory.transport.stationPool = @($null,[object]::new())
Assert (-not (Call 'Plugin' 'MergeLogisticsStorageCounts' @($counts,$factory))) 'Missing station identity passed availability'

function ReadEvidence($Target,[string]$Method,$InputData) {
    $Target.GetType().GetMethod($Method,[Reflection.BindingFlags]'Instance,NonPublic').Invoke($Target,(,$InputData))
}
$normalized = New 'ObservedGameState'
$summary = [Collections.Generic.Dictionary[string,object]]::new()
$summary['playerInventoryAvailable'] = $true
$summary['playerInventoryItems'] = @()
ReadEvidence $normalized 'ReadOwnedItems' $summary
Assert (Field $normalized 'PlayerInventoryAvailable') 'Empty available inventory lost in normalization'
$summary['playerInventoryAvailable'] = $false
ReadEvidence $normalized 'ReadOwnedItems' $summary
Assert (-not (Field $normalized 'PlayerInventoryAvailable')) 'Unavailable package became available in normalization'
$location = [Collections.Generic.Dictionary[string,object]]::new()
$location['playerPlanetId'] = 0
ReadEvidence $normalized 'ReadLocation' $location
Assert (Field $normalized 'PlayerLocationAvailable') 'Known space location became missing'
$location['playerPlanetId'] = $null
ReadEvidence $normalized 'ReadLocation' $location
Assert (-not (Field $normalized 'PlayerLocationAvailable')) 'Missing location became observed space'
$research = [Collections.Generic.Dictionary[string,object]]::new()
$knownTech = [Collections.Generic.Dictionary[string,object]]::new()
$knownTech['id'] = 2902
$knownTech['unlocked'] = $false
$unknownTech = [Collections.Generic.Dictionary[string,object]]::new()
$unknownTech['id'] = 1413
$unknownTech['unlocked'] = $null
$research['technologies'] = @($knownTech,$unknownTech)
ReadEvidence $normalized 'ReadResearch' $research
Assert ((Field $normalized 'AvailableTechIds').Contains(2902)) 'Locked known research became unknown'
Assert (-not (Field $normalized 'AvailableTechIds').Contains(1413)) 'Missing research result became known'
$planetRow = [Collections.Generic.Dictionary[string,object]]::new()
$planetRow['id'] = 101
$logistics = [Collections.Generic.Dictionary[string,object]]::new()
$logistics['available'] = $true
$logistics['stations'] = @()
$factoryRow = [Collections.Generic.Dictionary[string,object]]::new()
$factoryRow['planet'] = $planetRow
$factoryRow['logistics'] = $logistics
ReadEvidence $normalized 'ReadStations' @($factoryRow)
Assert ((Field $normalized 'AvailableStationPlanets').Contains(101)) 'Valid empty station collection became unavailable'
$stationRow = [Collections.Generic.Dictionary[string,object]]::new()
$stationRow['id'] = 1
$stationRow['isStellar'] = $true
$stationRow['available'] = $false
$logistics['stations'] = @($stationRow)
ReadEvidence $normalized 'ReadStations' @($factoryRow)
Assert (-not (Field (Field $normalized 'Stations')[0] 'EvidenceAvailable')) 'Missing station fields became available'
Write-Host 'ILS collection and normalization availability tests passed.'
Add-Type @"
public class GcTrafficItem { public int itemId; public long[] total = new long[21]; }
public class GcTrafficFactory { public GcTrafficItem[] trafficPool; }
public class GcTrafficPool { public GcTrafficFactory[] factoryTrafficPool; }
public class GcTrafficStatistics { public GcTrafficPool traffic; }
public class GcTrafficPlanet { public int id = 101; }
public class GcTrafficWorld { public GcTrafficPlanet planet = new GcTrafficPlanet(); }
public class GcTrafficData { public GcTrafficStatistics statistics; public GcTrafficWorld[] factories = new[] { new GcTrafficWorld() }; }
"@
$trafficCollector = New 'TrafficTelemetry'
$trafficData = [GcTrafficData]::new()
$trafficData.statistics = [GcTrafficStatistics]::new()
$trafficData.statistics.traffic = [GcTrafficPool]::new()
$trafficFactory = [GcTrafficFactory]::new()
$trafficFactory.trafficPool = @()
$trafficData.statistics.traffic.factoryTrafficPool = @($trafficFactory)
$trafficCollector.GetType().GetMethod('SampleNow').Invoke($trafficCollector,@($trafficData,0L)) | Out-Null
$export = $trafficCollector.Export()
Assert ($export['factories'][0]['inputCountersAvailable']) 'Known zero traffic counters were unavailable'
$trafficItem = [GcTrafficItem]::new(); $trafficItem.itemId = 1106; $trafficItem.total[6] = 40L; $trafficItem.total[20] = 100L
$trafficFactory.trafficPool = @($trafficItem)
$trafficCollector.GetType().GetMethod('SampleNow').Invoke($trafficCollector,@($trafficData,300L)) | Out-Null
$export = $trafficCollector.Export()
Assert ($export['factories'][0]['finishedInputTotals']['1106'] -eq 40L) 'Input counter included internal traffic'
$trafficState = New 'ObservedGameState'
ReadEvidence $trafficState 'ReadTraffic' $export
Assert ((Field $trafficState 'FinishedInputTotals')[101][1106] -eq 40L) 'Input counter lost in normalization'
$oldEpoch = $export['evidenceEpoch']
$trafficItem.total[6] = 0L
$trafficCollector.GetType().GetMethod('SampleNow').Invoke($trafficCollector,@($trafficData,600L)) | Out-Null
Assert ($trafficCollector.Export()['evidenceEpoch'] -gt $oldEpoch) 'Counter reset not exposed'
$trafficData.statistics.traffic.factoryTrafficPool = $null
$trafficCollector.GetType().GetMethod('SampleNow').Invoke($trafficCollector,@($trafficData,900L)) | Out-Null
Assert (-not $trafficCollector.Export()['available']) 'Stale traffic reported available after collection failure'
Write-Host 'ILS native-counter collection, normalization, zero, reset and failure fixtures passed.'
Add-Type @"
public class GcRecipeSystem { public object[] assemblerPool; public object[] labPool; }
public class GcRecipeFactory { public GcRecipeSystem factorySystem = new GcRecipeSystem(); }
public class GcRecipeData { public GcRecipeFactory[] factories = new[] { new GcRecipeFactory() }; }
"@
$recipeData = [GcRecipeData]::new()
Assert (-not (Call 'RecipeTelemetry' 'Export' @($recipeData))['available']) 'Missing recipe pools became known zero machines'
$recipeData.factories[0].factorySystem.assemblerPool = @()
$recipeData.factories[0].factorySystem.labPool = @()
Assert ((Call 'RecipeTelemetry' 'Export' @($recipeData))['available']) 'Valid empty recipe pools became unavailable'
Write-Host 'Recipe collection distinguishes unavailable pools from known zero machines.'

Add-Type @"
public class GcTechProto {
    public int ID; public string name; public int Level;
    public int[] PreTechs = new int[0]; public int[] PreTechsImplicit = new int[0];
}
public class GcTechSet {
    public System.Collections.Generic.Dictionary<int,GcTechProto> Values = new System.Collections.Generic.Dictionary<int,GcTechProto>();
    public GcTechProto Select(int id) { GcTechProto value; return Values.TryGetValue(id, out value) ? value : null; }
}
"@
$nativeTechs = [GcTechSet]::new()
$rootTech = [GcTechProto]::new(); $rootTech.ID = 2902; $rootTech.name = 'Drive Engine'; $rootTech.Level = 2
$rootTech.PreTechs = @(2901); $rootTech.PreTechsImplicit = @(2102)
$previousTech = [GcTechProto]::new(); $previousTech.ID = 2901; $previousTech.name = 'Drive Engine'; $previousTech.Level = 1
$coreTech = [GcTechProto]::new(); $coreTech.ID = 2102; $coreTech.name = 'Mecha Core'; $coreTech.Level = 2
foreach ($proto in @($rootTech,$previousTech,$coreTech)) { $nativeTechs.Values.Add($proto.ID,$proto) }
$definitions = Field (New 'ObservedGameState') 'ResearchDefinitions'
Call 'Plugin' 'ReadResearchDefinition' @($nativeTechs,2902,$definitions)
Assert ($definitions.Count -eq 3) 'Native prerequisite closure missing explicit or implicit dependencies'
Assert ($definitions[2902].Name -eq 'Drive Engine Lv2') 'Native upgrade rank lost its established wording'
Assert ([object]::ReferenceEquals($definitions[2902].Required,$rootTech.PreTechs)) 'Native prerequisite array was copied or reconstructed'
Assert ([object]::ReferenceEquals($definitions[2902].Implicit,$rootTech.PreTechsImplicit)) 'Implicit native prerequisite array was copied'
$coreTech.PreTechs = $null
$missingDefinitions = Field (New 'ObservedGameState') 'ResearchDefinitions'
Call 'Plugin' 'ReadResearchDefinition' @($nativeTechs,2902,$missingDefinitions)
Assert (-not $missingDefinitions.ContainsKey(2102)) 'Missing native prerequisite metadata became an empty known prerequisite list'
$research['definitions'] = $definitions
ReadEvidence $normalized 'ReadResearch' $research
Assert ([object]::ReferenceEquals((Field $normalized 'ResearchDefinitions'),$definitions)) 'Normalization rebuilt native research definitions'

$stationRow['available'] = $true
$fleetRow = [Collections.Generic.Dictionary[string,object]]::new()
$fleetRow['idleShipCount'] = 5; $fleetRow['workShipCount'] = 0
$stationRow['fleet'] = $fleetRow
$nativeSlotRow = [Collections.Generic.Dictionary[string,object]]::new()
$nativeSlotRow['itemId'] = 1106; $nativeSlotRow['count'] = 30L; $nativeSlotRow['remoteLogic'] = 'Demand'
$stationRow['storage'] = @($nativeSlotRow)
$stationState = New 'ObservedGameState'
ReadEvidence $stationState 'ReadStations' @($factoryRow)
$ownedSlot = (Field (Field $stationState 'Stations')[0] 'Slots')[0]
Assert ([object]::ReferenceEquals($ownedSlot,(Field $stationState 'StationSlots')[0])) 'Station ownership was discarded or its slot copied'
Assert ($ownedSlot.ItemId -eq 1106 -and $ownedSlot.Count -eq 30) 'Native station slot contents were lost'
$unrelatedSlot = New 'ObservedStationSlot'; SetField $unrelatedSlot 'ItemId' 1105; SetField $unrelatedSlot 'RemoteLogic' 'Demand'
(Field $stationState 'StationSlots').Add($unrelatedSlot)
Assert ((Call 'IlsTransportEvidence' 'CountPolicies' @((Field $stationState 'Stations')[0],'Demand')) -eq 1) 'Endpoint policy lookup searched unrelated global slots'
Write-Host 'Native research definitions, missing metadata, station ownership and slot isolation passed.'
