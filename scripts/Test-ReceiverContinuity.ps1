param(
    [string]$DllPath = (
        Join-Path (Split-Path -Parent $PSScriptRoot) `
            'src\DspProgressionStatusExporter\bin\Release\net472\DspGuideCheck.dll'
    )
)

$ErrorActionPreference = 'Stop'

$resolvedDll = (Resolve-Path -LiteralPath $DllPath).Path
$assembly = [Reflection.Assembly]::LoadFrom($resolvedDll)
$type = $assembly.GetType(
    'DspProgressionStatusExporter.ReceiverTelemetry',
    $true
)
$flags = [Reflection.BindingFlags]'Static, NonPublic'
$evaluate = $type.GetMethod('IsSustainedHealthy', $flags)
if ($null -eq $evaluate) {
    throw 'Receiver continuity policy method was not found.'
}

function Assert-Continuity {
    param(
        [string]$Name,
        [bool]$WindowReady,
        [int]$UnhealthySamples,
        [bool]$Expected
    )
    $actual = [bool]$evaluate.Invoke(
        $null,
        @($WindowReady, $UnhealthySamples)
    )
    if ($actual -ne $Expected) {
        throw "$Name returned $actual; expected $Expected."
    }
}

Assert-Continuity 'Unready healthy history' $false 0 $false
Assert-Continuity 'Ready clean history' $true 0 $true
Assert-Continuity 'One unhealthy sample' $true 1 $true
Assert-Continuity 'Two unhealthy samples' $true 2 $true
Assert-Continuity 'Third unhealthy sample' $true 3 $false
Assert-Continuity 'Recovery after an old failure expires' $true 2 $true

Write-Output 'Receiver continuity tests passed.'
