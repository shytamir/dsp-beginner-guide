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
    foreach ($phase in @('photon','white','ils')) {
        $a=PhasePanel $state $phase
        $selection=Selection "nav3;phase=$phase;ils=2;ilsOrigin=manual-control"
        foreach ($refresh in 1..3) {
            $b=PhasePanel $state $phase
            Assert ((Field $a 'SourceGuideAnchor') -eq (Field $b 'SourceGuideAnchor')) 'Presentation policy changed guide link'
            Assert ((Field $a 'PhaseId') -eq (Field $selection 'PhaseId')) 'Presentation changed stored phase'
            $aRates = @((Field $a 'CubeRates') | ForEach-Object { $_.RateText+':'+$_.Level }) -join ','
            $bRates = @((Field $b 'CubeRates') | ForEach-Object { $_.RateText+':'+$_.Level }) -join ','
            Assert ($aRates -eq $bRates) 'Presentation changed rates'
        }
    }
}
Write-Host 'Normal/Expert policy, default, callback eligibility and model rate/link parity passed.'
