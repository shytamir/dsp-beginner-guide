[CmdletBinding()]
param([string]$DllPath)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Test-IlsReceipt.ps1" -DllPath $DllPath
function JourneyPanel($State,[int]$Stage) {
    $a = Call 'GuideAnalyzer' 'AnalyzeSelected' @($State,'ils',$Stage)
    return Call 'GuidePanelModelBuilder' 'Build' @($a,$State,$null,$null,$null)
}
function Tasks($Panel) { @((Field $Panel 'Pending') | ForEach-Object { $_.Id }) -join ',' }
$s = ResearchState
(Field $s 'UnlockedTechIds').Remove(2902) | Out-Null
$p = JourneyPanel $s 1
Assert ((Tasks $p) -match 'departure-research.*preparation') 'Departure actions not ordered'
(Field $s 'QueuedTechIds').Add(2902) | Out-Null
Assert ((Tasks (JourneyPanel $s 1)) -notmatch 'departure-research') 'Queued departure repeated research'
$s = ResearchState
foreach ($flow in (Field $s 'FactoryItemFlows')) { SetField $flow 'ProducedPerMinute' 0.0 }
$p = JourneyPanel $s 2
Assert ((Tasks $p) -match 'expedition-production' -and (Tasks $p) -notmatch 'expedition-cargo') 'Load task preceded usable source material'
foreach ($flow in (Field $s 'FactoryItemFlows')) { SetField $flow 'ProducedPerMinute' 30.0 }
Assert ((Tasks (JourneyPanel $s 2)) -match 'expedition-cargo') 'Ready smelting lacks loading task'
(Field $s 'PlayerItemCounts')[1106] = 860L
(Field $s 'PlayerItemCounts')[1105] = 520L
Assert ((Tasks (JourneyPanel $s 2)) -eq 'pending-ils-expedition-home') 'Full cargo lacks a single return task'
SetField $s 'PlayerPlanetId' 101
Assert ((Field (JourneyPanel $s 2) 'Pending').Count -eq 0) 'Returned cargo still creates a task'
$s = HardwareState
(Field $s 'UnlockedTechIds').Remove(1605) | Out-Null
Assert ((Tasks (JourneyPanel $s 3)) -eq 'pending-ils-rush-tech') 'Early Automation exposed unusable hardware tasks'
(Field $s 'QueuedTechIds').Add(1605) | Out-Null
Assert ((Field (JourneyPanel $s 3) 'Pending').Count -eq 0) 'Queued Automation repeats tasks'
(Field $s 'UnlockedTechIds').Add(1605) | Out-Null
Assert ((Tasks (JourneyPanel $s 3)) -eq 'pending-ils-rush-hardware') 'Researched Automation lacks finished-package task'
(Field $s 'PlayerItemCounts')[2104] = 1L
(Field $s 'PlayerItemCounts')[5002] = 4L
Assert ((Tasks (JourneyPanel $s 3)) -eq 'pending-ils-rush-hardware') 'Partial package bypasses hardware'
(Field $s 'PlayerItemCounts')[2104] = 2L
(Field $s 'PlayerItemCounts')[5002] = 5L
Assert ((Tasks (JourneyPanel $s 3)) -eq 'pending-ils-rush-deployment') 'Finished package lacks configuration task'
$s = ReceiptState; $tracker = New 'IlsReceiptTracker'; $data = [object]::new()
ObserveReceipt $tracker $data $s | Out-Null
Assert ((Field (JourneyPanel $s 3) 'Pending').Count -eq 0) 'Configured waiting route repeats activation'
SetField $s 'TrafficSampleTick' 300L
foreach ($id in @(1106,1105)) { (Field $s 'FinishedInputTotals')[101][$id] = 200L }
ObserveReceipt $tracker $data $s | Out-Null
Assert ((Field (JourneyPanel $s 3) 'Pending').Count -eq 0) 'Confirmed receipt creates repeat work'
foreach ($stage in @(1,2,3)) {
    $a = JourneyPanel $s $stage; $b = JourneyPanel $s $stage
    Assert ((Tasks $a) -eq (Tasks $b)) 'Identical evidence reordered actions'
    Assert ((Field $a 'Pending').Count -le 3) 'More than three Pending tasks'
    $labels = @((Field $a 'Pending') | ForEach-Object { $_.Label })
    Assert (@($labels | Select-Object -Unique).Count -eq $labels.Count) 'Duplicate Pending labels'
    Assert ((Field $a 'SourceGuideAnchor') -eq @('flight','titanium','ils-automate')[$stage-1]) 'Journey changed source anchor'
}
Write-Host 'Complete ILS fixture journey, task eligibility, limits and stable order passed.'
