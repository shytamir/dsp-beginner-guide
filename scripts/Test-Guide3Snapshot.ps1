#requires -Version 7.0
[CmdletBinding()]
param([string]$DllPath,[string]$GameRoot='C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program',[string]$ExpectedVersion,[Parameter(Mandatory)][string]$OutputPath)
$ErrorActionPreference='Stop'
foreach($name in @('UnityEngine.CoreModule','UnityEngine','UnityEngine.InputLegacyModule')) { [Reflection.Assembly]::LoadFrom((Join-Path $GameRoot "DSPGAME_Data/Managed/$name.dll")) | Out-Null }
[Reflection.Assembly]::LoadFrom((Join-Path $GameRoot 'BepInEx/core/BepInEx.dll')) | Out-Null
. "$PSScriptRoot/Test-IlsJourney.ps1" -DllPath $DllPath
. "$PSScriptRoot/Test-PhotonOutcome.ps1" -DllPath $DllPath
$attribute=(GetModelType 'Plugin').GetCustomAttributesData() | Where-Object { $_.AttributeType.FullName -eq 'BepInEx.BepInPlugin' }
Assert ($attribute.ConstructorArguments[0].Value -ceq 'local.dsp.progressionstatusexporter') 'BepInEx GUID changed'
Assert ($attribute.ConstructorArguments[1].Value -ceq 'DSP Guide Check') 'BepInEx display name changed'
Assert ($attribute.ConstructorArguments[2].Value -ceq $ExpectedVersion) 'Compiled BepInEx version mismatch'
$documents=[ordered]@{}; $maximumBytes=0
foreach($phase in @('blue','red','ils','yellow','purple','green','dyson','photon','white')) {
    $stages=if($phase -eq 'ils'){@(1,2,3)}else{@(1)}
    foreach($stage in $stages) {
        $state=if($phase -eq 'ils'){ReceiptState}else{PhotonState}
        if($phase -eq 'ils') { ObserveReceipt (New 'IlsReceiptTracker') ([object]::new()) $state | Out-Null }
        $irrelevant=New 'ObservedItemFlow'; SetField $irrelevant 'ItemId' 9999; SetField $irrelevant 'Name' ('UNRELATED-DATA-' * 20000); (Field $state 'ItemFlows')[9999]=$irrelevant
        foreach($i in 1..20) { (Field (Field $state 'Dyson') 'Receivers').Add((New 'ObservedReceiverState')) }
        $analysis=Call 'GuideAnalyzer' 'AnalyzeSelected' @($state,$phase,$stage)
        $panel=Call 'GuidePanelModelBuilder' 'Build' @($analysis,$state,$null,$null,$null)
        $empty=[Collections.Generic.Dictionary[string,object]]::new()
        $selection=(Selection "nav3;phase=$phase;ils=$stage;ilsOrigin=manual-control").Export('fixture')
        $snapshot=Call 'CompactSnapshotBuilder' 'Build' @('2.24','fixture',$empty,$empty,$empty,$empty,$state,$selection,$analysis,$panel.Export(),$empty,$false)
        $snapshot.Remove('exportedAtUtc') | Out-Null
        $json=Call 'Json' 'Stringify' (,$snapshot)
        $bytes=[Text.Encoding]::UTF8.GetByteCount($json); $maximumBytes=[Math]::Max($maximumBytes,$bytes)
        Assert ($bytes -le 262144) 'Compact snapshot exceeded 256 KiB'
        Assert (-not $json.Contains('UNRELATED-DATA-')) 'Unrelated data entered compact export'
        if($phase -eq 'ils') {
            Assert ($snapshot['guideSelection']['ilsStage'] -eq $stage) 'Selection provenance absent'
            Assert ($snapshot['evidence']['logistics']['stageEvidence']['homeDelivery'].ContainsKey('evidenceEpoch')) 'Receipt provenance absent'
            Assert ($snapshot['evidence']['logistics']['stageEvidence']['transportPackage'].ContainsKey('available')) 'Availability provenance absent'
        }
        if($phase -eq 'photon') { Assert ($snapshot['evidence']['photonReadiness'].Count -eq 6) 'Six-input provenance absent' }
        $documents["$phase-$stage"]=$snapshot
    }
}
New-Item -ItemType Directory -Force -Path (Split-Path $OutputPath -Parent) | Out-Null
$documents | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $OutputPath
"Snapshot fixtures passed: 11 cases; maximum $maximumBytes bytes; compiled GUID/name/version verified."
