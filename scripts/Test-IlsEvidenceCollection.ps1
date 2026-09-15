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

# The panel consumes tank/station rows, not a second full factory diagnostic export.
Add-Type @"
public class PanelTank { public int fluidId; public long fluidCount; public long fluidCapacity; }
public class PanelEntity { public int id; public int protoId; }
public class PanelSlot { public int itemId = 1106; public long count = 30; public long max = 100; public string localLogic = "Supply"; public string remoteLogic = "Demand"; }
public class PanelStation { public int id = 1; public bool isStellar = true; public int idleShipCount = 2; public int workShipCount = 1; public object[] storage = new object[] { new PanelSlot() }; }
public class PanelStorage {
    public bool DepotsRead; public object[] Depots; public int storageCursor = 2;
    public object[] storagePool { get { DepotsRead = true; return Depots; } }
    public object[] tankPool; public int tankCursor = 3;
}
public class PanelPlanet { public int id = 101; public string displayName = "Home"; }
public class PanelFactory {
    public PanelPlanet planet = new PanelPlanet(); public PanelStorage factoryStorage = new PanelStorage();
    public object transport; public int entityCursor = 4;
    public bool EntitiesRead; public bool DiagnosticsRead;
    public object[] Entities;
    public object[] entityPool { get { EntitiesRead = true; return Entities; } }
    public object powerSystem { get { DiagnosticsRead = true; return null; } }
    public object factorySystem { get { DiagnosticsRead = true; return null; } }
    public object enemySystem { get { DiagnosticsRead = true; return null; } }
}
public class PanelWorld { public object[] factories; }
"@
$panelFactory = [PanelFactory]::new()
$panelFactory.transport = [CargoTransport]::new()
$depot = [CargoStorage]::new()
$depotGrid = [CargoGrid]::new(); $depotGrid.itemId = 1106; $depotGrid.count = 20
$depot.grids = @($depotGrid)
$panelFactory.factoryStorage.Depots = @($null,$depot)
$tankA = [PanelTank]::new(); $tankA.fluidId = 1114; $tankA.fluidCount = 50; $tankA.fluidCapacity = 100
$tankB = [PanelTank]::new(); $tankB.fluidId = 1114; $tankB.fluidCount = 20; $tankB.fluidCapacity = -1
$panelFactory.factoryStorage.tankPool = @($null,$tankA,$tankB)
$panelFactory.transport.stationPool = @($null,[PanelStation]::new()); $panelFactory.transport.stationCursor = 2
$labA = [PanelEntity]::new(); $labA.id = 1; $labA.protoId = 2901
$labB = [PanelEntity]::new(); $labB.id = 2; $labB.protoId = 2901
$deleted = [PanelEntity]::new(); $deleted.protoId = 2901
$panelFactory.Entities = @($null,$labA,$labB,$deleted,$labA)
$panelWorld = [PanelWorld]::new(); $panelWorld.factories = @($panelFactory,$null)
$factoryRows = @(Call 'Plugin' 'ExportFactories' @($panelWorld))
$panelEvidence = New 'ObservedGameState'
ReadEvidence $panelEvidence 'ReadTanks' $factoryRows
ReadEvidence $panelEvidence 'ReadStations' $factoryRows
$capacity = (Field $panelEvidence 'TankStorage')[1114]
Assert ((Field $capacity 'Count') -eq 70 -and (Field $capacity 'Capacity') -eq 100) 'Panel tank amount/capacity changed'
$station = (Field $panelEvidence 'Stations')[0]
Assert ((Field $station 'EvidenceAvailable') -and (Field $station 'PlanetId') -eq 101) 'Panel station evidence/identity changed'
Assert ((Field $station 'IdleShipCount') -eq 2 -and (Field $station 'WorkShipCount') -eq 1) 'Panel fleet counts changed'
$slot = (Field $station 'Slots')[0]
Assert ((Field $slot 'Count') -eq 30 -and (Field $slot 'Maximum') -eq 100 -and (Field $slot 'RemoteLogic') -eq 'Demand') 'Panel station storage changed'
Write-Host 'Panel tank, station, fleet and storage evidence preserved.'
Assert (-not $panelFactory.EntitiesRead) 'Factory rows still duplicate the entity count'
Assert (-not $panelFactory.factoryStorage.DepotsRead) 'Factory rows still scan discarded storage containers'
Assert (-not $panelFactory.DiagnosticsRead) 'Factory rows still read discarded diagnostics'
$progression = Call 'Plugin' 'ExportProgressionSummary' @($panelWorld)
ReadEvidence $panelEvidence 'ReadBuildingCounts' $progression
Assert ((Field $panelEvidence 'FactoryBuildingCounts')[2901] -eq 2) 'Building count lost live entities or included deleted/out-of-cursor slots'
$panelFactory.factoryStorage = $null
$panelFactory.transport = $null
$missingRows = @(Call 'Plugin' 'ExportFactories' @($panelWorld))
$missingEvidence = New 'ObservedGameState'
ReadEvidence $missingEvidence 'ReadTanks' $missingRows
ReadEvidence $missingEvidence 'ReadStations' $missingRows
Assert ((Field $missingEvidence 'TankStorage').Count -eq 0 -and (Field $missingEvidence 'AvailableStationPlanets').Count -eq 0) 'Missing tank/station pools did not fail softly'
Write-Host 'Panel collection skips discarded factory diagnostics and preserves building counts/missing evidence.'

Add-Type @"
public class PanelRecipe { public int id = 1; public int recipeId; }
public class PanelRecipeSystem {
    public bool AssemblersRead; public object[] Assemblers;
    public object[] assemblerPool { get { AssemblersRead = true; return Assemblers; } }
    public int assemblerCursor = 2; public object[] labPool; public int labCursor = 2;
}
public class PanelRecipeFactory { public object factorySystem; }
public class PanelNativePower {
    public long energyGenCurrentTick = 100; public long energyGenCurrentTick_Layers = 60;
    public long energyGenCurrentTick_Swarm = 40; public long energyReqCurrentTick = 50; public int rocketCount = 2;
    public bool NodesRead; public object layersIdBased { get { NodesRead = true; return null; } }
}
public class PanelTechState { public bool unlocked; public long hashUploaded = 25; public long hashNeeded = 100; }
public class PanelHistory { public System.Collections.IDictionary techStates = new System.Collections.Hashtable(); public int[] techQueue = new int[] { 1508 }; }
public class PanelGameMain { public static PanelHistory history = new PanelHistory(); }
"@
$recipeSystem = [PanelRecipeSystem]::new()
$matrix = [PanelRecipe]::new(); $matrix.recipeId = 75
$conversion = [PanelRecipe]::new(); $conversion.recipeId = 74
$recipeSystem.labPool = @($null,$matrix,$matrix)
$recipeSystem.Assemblers = @($null,$conversion,$conversion)
$recipeFactory = [PanelRecipeFactory]::new(); $recipeFactory.factorySystem = $recipeSystem
$recipeWorld = [PanelWorld]::new(); $recipeWorld.factories = @($recipeFactory)
$whiteRecipes = Call 'RecipeTelemetry' 'ExportForPanel' @($recipeWorld,'white')
Assert ($whiteRecipes['available'] -and -not $recipeSystem.AssemblersRead) 'WHITE queried unrelated assemblers'
Assert ($whiteRecipes['factories'][0]['recipes'][0]['configuredMachineCount'] -eq 1) 'Recipe collection ignored native cursor'
$dysonRecipes = Call 'RecipeTelemetry' 'ExportForPanel' @($recipeWorld,'dyson')
Assert ($recipeSystem.AssemblersRead -and $dysonRecipes['factories'][0]['recipes'].Count -eq 2) 'DYSON lost conversion recipe evidence'
$nativePower = [PanelNativePower]::new()
$power = Call 'Plugin' 'ExportNativeDysonPower' @($nativePower)
Assert ($power['available'] -and $power['energyGenCurrentTick_Swarm'] -eq 40 -and -not $nativePower.NodesRead) 'Live Dyson power traversed construction nodes or changed native fields'
$pluginType = GetModelType 'Plugin'
$mainTypeField = $pluginType.GetField('gameMainType',$static)
$oldMainType = $mainTypeField.GetValue($null)
$techNames = $pluginType.GetField('TechNames',$static).GetValue($null)
$savedNames = [Collections.Generic.Dictionary[int,string]]::new($techNames)
try {
    $mainTypeField.SetValue($null,[PanelGameMain]); $techNames.Clear()
    foreach ($id in @(1507,1508,2902)) { $techNames[$id] = "Technology $id" }
    $unlockedTech = [PanelTechState]::new(); $unlockedTech.unlocked = $true
    [PanelGameMain]::history.techStates[1507] = $unlockedTech
    [PanelGameMain]::history.techStates[1508] = [PanelTechState]::new()
    $nativeResearch = Call 'Plugin' 'ExportResearch' @()
    $researchState = New 'ObservedGameState'; ReadEvidence $researchState 'ReadResearch' $nativeResearch
    Assert ((Field $researchState 'UnlockedTechIds').Contains(1507)) 'Native unlocked flag was lost'
    Assert (-not (Field $researchState 'UnlockedTechIds').Contains(1508)) 'Incomplete research became complete'
    Assert ((Field $researchState 'AvailableTechIds').Contains(2902)) 'Absent native tech state lost TechUnlocked false semantics'
    Assert ((Field $researchState 'TechProgress')[1508].HashUploaded -eq 25) 'Mission hash progress changed'
    [PanelGameMain]::history.techStates = $null
    $missingResearch = New 'ObservedGameState'; ReadEvidence $missingResearch 'ReadResearch' (Call 'Plugin' 'ExportResearch' @())
    Assert ((Field $missingResearch 'AvailableTechIds').Count -eq 0) 'Missing native research was claimed known'
} finally {
    $mainTypeField.SetValue($null,$oldMainType); $techNames.Clear()
    foreach ($pair in $savedNames.GetEnumerator()) { $techNames[$pair.Key] = $pair.Value }
}
# Execute the actual installed native GetItemCount on an isolated, synthetic storage.
$gameAssembly = [Reflection.Assembly]::LoadFrom((Join-Path $GameRoot 'DSPGAME_Data/Managed/Assembly-CSharp.dll'))
$storageType = $gameAssembly.GetType('StorageComponent',$true)
$nativeStorage = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($storageType)
$gridsField = $storageType.GetField('grids')
$gridType = $gridsField.FieldType.GetElementType()
$nativeGrids = [Array]::CreateInstance($gridType,3)
foreach ($i in 0..2) {
    $nativeGrid = [Activator]::CreateInstance($gridType)
    $gridType.GetField('itemId').SetValue($nativeGrid, $(if($i -eq 1){1122}else{6006}))
    $gridType.GetField('count').SetValue($nativeGrid, $(if($i -eq 1){7}else{11}))
    $nativeGrids.SetValue($nativeGrid,$i)
}
$gridsField.SetValue($nativeStorage,$nativeGrids)
$storageType.GetField('size').SetValue($nativeStorage,3)
$allCounts = [Collections.Generic.Dictionary[int,long]]::new()
$whiteCounts = [Collections.Generic.Dictionary[int,long]]::new()
Assert (Call 'Plugin' 'MergeStorageCounts' @($allCounts,$nativeStorage)) 'Native fixture grids were unavailable'
Assert (Call 'Plugin' 'MergeSelectedStorageCount' @($whiteCounts,$nativeStorage,6006)) 'Native item count was unavailable'
Assert ($whiteCounts[6006] -eq $allCounts[6006] -and $whiteCounts[6006] -eq 22 -and $whiteCounts.Count -eq 1) 'Native item count changed storage scope/quantity'
$absentCounts = [Collections.Generic.Dictionary[int,long]]::new()
Assert (Call 'Plugin' 'MergeSelectedStorageCount' @($absentCounts,$nativeStorage,1106)) 'Known zero stock became missing'
Assert ($absentCounts.Count -eq 0) 'Native item count invented absent stock'
$fallbackCounts = [Collections.Generic.Dictionary[int,long]]::new()
Assert (Call 'Plugin' 'MergeSelectedStorageCount' @($fallbackCounts,$depot,1106)) 'Missing native method broke reflective fallback'
Assert ($fallbackCounts[1106] -eq 20) 'Reflective fallback count changed'
Write-Host 'Native storage method, research flags, bounded recipes and Dyson power fixtures passed.'
