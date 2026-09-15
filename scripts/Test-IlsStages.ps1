[CmdletBinding()]
param([string]$DllPath)
$ErrorActionPreference = 'Stop'
if ([String]::IsNullOrEmpty($DllPath)) { $DllPath = Join-Path $PSScriptRoot '..\src\DspProgressionStatusExporter\bin\Release\net472\DspGuideCheck.dll' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $DllPath))
$static = [Reflection.BindingFlags]'Public,NonPublic,Static'
function GetModelType([string]$Name) { $assembly.GetType('DspProgressionStatusExporter.' + $Name, $true) }
function New([string]$Name) { [Activator]::CreateInstance((GetModelType $Name), $true) }
function Field($Target, [string]$Name) { ,($Target.GetType().GetField($Name).GetValue($Target)) }
function SetField($Target, [string]$Name, $Value) { $Target.GetType().GetField($Name).SetValue($Target, $Value) }
function Call([string]$TypeName, [string]$Method, [object[]]$Arguments) {
    (GetModelType $TypeName).GetMethod($Method, $static).Invoke($null, $Arguments)
}
function Assert($Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
function Selection([string]$Text) { Call 'ManualPhaseSelection' 'Parse' @($Text) }
function SeedStage($Selection, $State) { Call 'ManualPhaseNavigator' 'EnsureIlsStage' @($Selection, $State) }
function Command($Selection, [string]$Command) { Call 'ManualPhaseNavigator' 'ApplyCommand' @($Selection, $Command) }

foreach ($version in @('nav1','nav2','nav3')) {
    $selection = Selection ($version + ';phase=ils;seed=stored;ils=invalid')
    Assert ((Field $selection 'PhaseId') -eq 'ils') 'Migration changed phase'
    Assert ((Field $selection 'IlsStage') -eq 0) 'Invalid stage did not remain uninitialized'
    Assert ($selection.Serialize().StartsWith('nav3;')) 'Serialization did not migrate'
}
foreach ($stage in @(0,4,-1)) {
    Assert ((Field (Selection "nav3;phase=yellow;ils=$stage") 'IlsStage') -eq 0) 'Out-of-range stage accepted'
}
$state = New 'ObservedGameState'
$selection = Selection 'nav2;phase=ils'
Assert (SeedStage $selection $state) 'First entry was not seeded'
Assert ((Field $selection 'IlsStage') -eq 1) 'Missing evidence did not seed Departure'
SetField $state 'StarterPlanetId' 101
SetField $state 'PlayerPlanetId' 102
foreach ($id in @(1604,2903,1414)) { (Field $state 'UnlockedTechIds').Add($id) | Out-Null }
(Field $state 'QueuedTechIds').Add(1605) | Out-Null
(Field $state 'PlayerItemCounts')[1106] = 860L
(Field $state 'PlayerItemCounts')[1105] = 520L
Assert (-not (SeedStage $selection $state)) 'Changing evidence overwrote the stored stage'
Assert ((Field $selection 'IlsStage') -eq 1) 'R1-R3 changed selection'
$away = Selection 'nav2;phase=ils'
SeedStage $away $state | Out-Null
Assert ((Field $away 'IlsStage') -eq 2) 'Known remote location did not seed Haulback'
(Field $state 'UnlockedTechIds').Add(1605) | Out-Null
$researched = Selection 'nav2;phase=ils'
SeedStage $researched $state | Out-Null
Assert ((Field $researched 'IlsStage') -eq 3) 'ILS research did not seed Automation'

foreach ($item in @(1003,1004,1105,1106)) {
    $sample = New 'ObservedGameState'
    SetField $sample 'StarterPlanetId' 101
    SetField $sample 'PlayerPlanetId' 101
    $flow = New 'ObservedFactoryItemFlow'
    SetField $flow 'PlanetId' 102
    SetField $flow 'ItemId' $item
    SetField $flow 'OneMinuteAvailable' $true
    SetField $flow 'ProducedPerMinute' 30.0
    (Field $sample 'FactoryItemFlows').Add($flow)
    $first = Selection 'nav2;phase=ils'
    SeedStage $first $sample | Out-Null
    $expected = if ($item -ge 1105) { 2 } else { 1 }
    Assert ((Field $first 'IlsStage') -eq $expected) 'Finished-material seed policy failed'
    SetField $sample 'StarterPlanetId' 0
    $unknown = Selection 'nav2;phase=ils'
    SeedStage $unknown $sample | Out-Null
    Assert ((Field $unknown 'IlsStage') -eq 1) 'Unknown birth planet proved an outpost'
}

$selection = Selection 'nav3;phase=ils;ils=3;ilsOrigin=stored'
foreach ($stage in @(1,2,3)) {
    Command $selection ('ils-' + $stage) | Out-Null
    $selection = Selection ($selection.Serialize())
    Assert ((Field $selection 'IlsStage') -eq $stage) 'Stage did not survive serialization'
    Assert ((Field $selection 'PhaseId') -eq 'ils') 'Stage control changed phase'
    $analysis = Call 'GuideAnalyzer' 'AnalyzeSelected' @($state, 'ils', $stage)
    $panel = Call 'GuidePanelModelBuilder' 'Build' @($analysis, $state, $null, $null, $null)
    Assert ((Field $panel 'IlsStage') -eq $stage) 'Panel selected state mismatch'
    Assert ($panel.ShowIlsStages) 'ILS selector missing'
    Assert ((Field $panel 'SourceGuideAnchor') -eq @('flight','titanium','ils-automate')[$stage-1]) 'Stage guide anchor mismatch'
    Assert ($selection.Export('test')['ilsStageOrigin'] -eq 'manual-control') 'Stage provenance missing'
    $before = @($analysis['progression']['gateEvaluations'][0]['conditions'] | ForEach-Object { $_['id'] }) -join ','
    $empty = Call 'GuideAnalyzer' 'AnalyzeSelected' @((New 'ObservedGameState'), 'ils', $stage)
    $after = @($empty['progression']['gateEvaluations'][0]['conditions'] | ForEach-Object { $_['id'] }) -join ','
    Assert ($before -eq $after) 'Evidence changed objective IDs within selected stage'
}
Command $selection 'next' | Out-Null
Assert (-not (Command $selection 'ils-1')) 'ILS control acted outside ILS'
Command $selection 'previous' | Out-Null
Assert ((Field $selection 'IlsStage') -eq 3) 'Leaving ILS lost its stage'
Assert ((Field $away 'IlsStage') -eq 2) 'Independent selection was overwritten'
$identityA = Call 'PhaseSaveIdentity' 'Build' @('123','42','64','false','first-slot')
$renamed = Call 'PhaseSaveIdentity' 'Build' @('123','42','64','false','renamed-slot')
$identityB = Call 'PhaseSaveIdentity' 'Build' @('456','42','64','false','first-slot')
Assert ((Field $identityA 'SaveKey') -eq (Field $renamed 'SaveKey')) 'Slot rename changed playthrough identity'
Assert ((Field $identityA 'SaveKey') -ne (Field $identityB 'SaveKey')) 'Independent playthroughs share a key'
$other = Call 'GuideAnalyzer' 'AnalyzeSelected' @($state, 'blue', 3)
$otherPanel = Call 'GuidePanelModelBuilder' 'Build' @($other, $state, $null, $null, $null)
Assert (-not $otherPanel.ShowIlsStages) 'Selector leaked into another phase'
Assert ((Field $otherPanel 'SourceGuideAnchor') -eq 'blue') 'Other phase link changed'
Write-Host 'ILS stage selection, migration, provenance, commands and panel policy passed.'
