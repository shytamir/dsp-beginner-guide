[CmdletBinding()]
param([string]$DllPath)
$ErrorActionPreference='Stop'
. "$PSScriptRoot/Test-PhotonOutcome.ps1" -DllPath $DllPath
$policyType = GetModelType 'GuidePresentationPolicy'
Assert (-not $policyType.GetField('DefaultExpertMode').GetRawConstantValue()) 'Expert mode default changed'
foreach ($expert in @($false,$true)) {
    $policy=[Activator]::CreateInstance($policyType,@($expert))
    Assert ($policy.ExpertMode -eq $expert -and $policy.GuidanceEnabled -eq (-not $expert)) 'Presentation policy mismatch'
    $buildFlag=(GetModelType 'BuildFeatures').GetField('SnapshotControlEnabled').GetRawConstantValue()
    Assert ($policy.SnapshotEnabled -eq ((-not $expert) -and $buildFlag)) 'Snapshot callback eligibility mismatch'
    $state=PhotonState
    foreach ($phase in @('blue','red','ils','yellow','purple','green','dyson','photon','white')) {
        $effectivePhase = if ($expert) { $policyType.GetField('ExpertPhaseId').GetRawConstantValue() } else { $phase }
        $a=PhasePanel $state $effectivePhase
        foreach ($refresh in 1..3) {
            $b=PhasePanel $state $effectivePhase
            Assert ((Field $a 'SourceGuideAnchor') -eq (Field $b 'SourceGuideAnchor')) 'Presentation policy changed guide link'
            Assert ((Field $a 'PhaseId') -eq $effectivePhase) 'Presentation used the wrong phase'
            if ($expert) {
                Assert ((Field $a 'SourceGuideAnchor') -eq 'white' -and (Field $a 'CubeRates').Count -eq 6) 'Expert mode omitted a Cube counter or used another guide anchor'
            }
            $aRates = @((Field $a 'CubeRates') | ForEach-Object { $_.RateText+':'+$_.Level }) -join ','
            $bRates = @((Field $b 'CubeRates') | ForEach-Object { $_.RateText+':'+$_.Level }) -join ','
            Assert ($aRates -eq $bRates) 'Presentation changed rates'
        }
    }
}
Write-Host 'Normal/Expert policy, default, callback eligibility, fixed WHITE counters and refresh parity passed.'
