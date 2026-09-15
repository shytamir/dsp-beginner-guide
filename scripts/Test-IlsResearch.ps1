[CmdletBinding()]
param([string]$DllPath)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Test-IlsCargo.ps1" -DllPath $DllPath
$researchFixture = Get-Content -LiteralPath "$PSScriptRoot/IlsResearchFixture.json" -Raw | ConvertFrom-Json
function ResearchState {
    $s = CargoState
    SetField $s 'ResearchQueueAvailable' $true
    foreach ($row in $researchFixture.technologies) {
        $definition = New 'ObservedTechDefinition'
        SetField $definition 'Name' $row.name
        SetField $definition 'Required' ([int[]]$row.required)
        SetField $definition 'Implicit' ([int[]]$row.implicitRequired)
        (Field $s 'ResearchDefinitions').Add([int]$row.id,$definition)
        (Field $s 'AvailableTechIds').Add([int]$row.id) | Out-Null
        (Field $s 'UnlockedTechIds').Add([int]$row.id) | Out-Null
    }
    return $s
}
function Research($State,[int]$Stage) { Call 'IlsResearchPolicy' 'Evaluate' @($State,$Stage) }
foreach ($case in @(
    @(1,2902,'Drive Engine Lv2'), @(1,1413,'Titanium Smelting'),
    @(2,1131,'Applied Superconductor'), @(2,1302,'Processor'), @(2,1124,'Structure Matrix'),
    @(2,1703,'Magnetic Particle Trap'), @(2,1114,'Reinforced Thruster'),
    @(2,1603,'High-Efficiency Logistics System'), @(2,3701,'Vertical Construction Lv1'),
    @(2,1604,'Planetary Logistics System'), @(3,1414,'High-Strength Titanium Alloy'),
    @(3,1605,'Interstellar Logistics System')
)) {
    $s = ResearchState
    (Field $s 'UnlockedTechIds').Remove($case[1]) | Out-Null
    $condition = Research $s $case[0]
    Assert ((Field $condition 'Action') -eq ('Research ' + $case[2] + '.')) ('Wrong stage research: ' + $case[2])
    (Field $s 'QueuedTechIds').Add($case[1]) | Out-Null
    $condition = Research $s $case[0]
    Assert ((Field $condition 'Status') -eq 'watch' -and $null -eq (Field $condition 'Action')) 'Queued research generated a repeat action'
    (Field $s 'UnlockedTechIds').Add($case[1]) | Out-Null
    Assert ($null -eq (Field (Research $s $case[0]) 'Action')) 'Completed research generated a task'
}
$s = ResearchState
foreach ($id in @(1605,1604,1603,1702)) { (Field $s 'UnlockedTechIds').Remove($id) | Out-Null }
Assert ((Field (Research $s 3) 'Action') -eq 'Research Magnetic Levitation.') 'Inherited implicit prerequisite did not precede dependent'
$rows = @(Call 'IlsResearchPolicy' 'Export' @($s,3))
$ils = $rows | Where-Object { $_['id'] -eq 1605 }
$pls = $rows | Where-Object { $_['id'] -eq 1604 }
$mk3 = $rows | Where-Object { $_['id'] -eq 1603 }
Assert (($ils['required'] -join ',') -eq '1604,1414' -and ($ils['implicitRequired'] -join ',') -eq '1114') '1605 prerequisite contract changed'
Assert (($pls['required'] -join ',') -eq '1603' -and ($pls['implicitRequired'] -join ',') -eq '1113,3701') '1604 prerequisite contract changed'
Assert (($mk3['implicitRequired'] -join ',') -eq '1702') '1603 implicit prerequisite missing'
Assert (-not ($rows | Where-Object { $_['id'] -eq 2903 })) 'Drive Engine Lv3 entered the ILS dependency closure'

$batch = ResearchState
foreach ($id in @(1414,1605)) { (Field $batch 'UnlockedTechIds').Remove($id) | Out-Null }
Assert ((Field (Research $batch 3) 'Evidence') -match 'Guide batch: 200 Yellow Cubes') 'First batch reference missing'
(Field $batch 'QueuedTechIds').Add(1414) | Out-Null
Assert ((Field (Research $batch 3) 'Evidence') -notmatch '200 Yellow') 'Queued research still demands the original batch'
(Field $batch 'QueuedTechIds').Clear()
$progress = New 'ObservedTechProgress'
SetField $progress 'HashUploaded' 1L
(Field $batch 'TechProgress').Add(1414,$progress)
Assert ((Field (Research $batch 3) 'Evidence') -notmatch '200 Yellow') 'Partly consumed batch was demanded again'
$unknown = ResearchState
(Field $unknown 'UnlockedTechIds').Remove(1605) | Out-Null
(Field $unknown 'AvailableTechIds').Remove(1605) | Out-Null
Assert ((Field (Research $unknown 3) 'Status') -eq 'unknown' -and $null -eq (Field (Research $unknown 3) 'Action')) 'Unknown research generated an action'
(Field $unknown 'AvailableTechIds').Add(1605) | Out-Null
SetField $unknown 'ResearchQueueAvailable' $false
Assert ($null -eq (Field (Research $unknown 3) 'Action')) 'Unavailable queue generated a repeat-research risk'
$early = ResearchState
(Field $early 'UnlockedTechIds').Remove(1605) | Out-Null
$analysis = Call 'GuideAnalyzer' 'AnalyzeSelected' @($early,'ils',3)
$conditions = $analysis['progression']['gateEvaluations'][0]['conditions']
Assert (-not ($conditions | Where-Object { $_['action'] -match 'Activate|Store the missing' })) 'Early Automation offered premature hardware/route work'
Assert (@($analysis['ilsResearch'] | Where-Object { $_['id'] -eq 1605 }).Count -eq 1) 'Research evidence is not exported once'
$survey = ResearchState
(Field $survey 'UnlockedTechIds').Remove(4102) | Out-Null
$optional = Call 'IlsResearchPolicy' 'Survey' @($survey)
Assert (-not (Field $optional 'Required') -and (Field $optional 'Action') -eq 'Consider Cosmic Exploration Lv2.') 'Optional survey became a gate or lost rank'
$changed = ResearchState
(Field $changed 'UnlockedTechIds').Remove(1605) | Out-Null
(Field $changed 'UnlockedTechIds').Remove(1413) | Out-Null
SetField (Field $changed 'ResearchDefinitions')[1605] 'Required' ([int[]]@(1413))
Assert ((Field (Research $changed 3) 'Action') -eq 'Research Titanium Smelting.') 'Runtime prerequisite change was ignored'
(Field $changed 'ResearchDefinitions').Remove(1413) | Out-Null
Assert ((Field (Research $changed 3) 'Status') -eq 'unknown') 'Missing native prerequisite definition generated advice'
Assert (-not $analysis.ContainsKey('normalizedState')) 'Panel analysis materialized the full diagnostic state'
Write-Host 'ILS research branches, prerequisites, queue states, batch reference and early-stage eligibility passed.'
