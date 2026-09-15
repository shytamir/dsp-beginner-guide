#requires -Version 7.0
[CmdletBinding()]
param([Parameter(Mandatory)][string]$DllPath,[Parameter(Mandatory)][string]$ReportPath)
$ErrorActionPreference='Stop'
$hostExe=(Get-Process -Id $PID).Path
$lines=[Collections.Generic.List[string]]::new()
foreach ($suite in @('Test-ProductionRisk','Test-ReceiverContinuity','Test-IlsJourney','Test-PhotonReadiness','Test-ExpertMode')) {
    & $hostExe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot "$suite.ps1") -DllPath $DllPath
    if($LASTEXITCODE -ne 0) { throw "$suite failed ($LASTEXITCODE)." }
    $lines.Add("- $suite`: passed")
}
New-Item -ItemType Directory -Force -Path (Split-Path $ReportPath -Parent) | Out-Null
@('# Portable Guide 3.0 behavior checks','',"DLL: $DllPath",'','These use synthetic models; no Unity scene or player save was used.','') + $lines | Set-Content -LiteralPath $ReportPath
