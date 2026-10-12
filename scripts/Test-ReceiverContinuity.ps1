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
$effectiveCatalyst = $type.GetMethod('EffectiveCatalystId', $flags)
if ($null -eq $effectiveCatalyst) {
    throw 'Receiver effective-catalyst policy method was not found.'
}
$isLensed = $type.GetMethod('IsLensedSample', $flags)
if ($null -eq $isLensed) {
    throw 'Receiver lens policy method was not found.'
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

function Assert-EffectiveCatalyst {
    param(
        [string]$Name,
        [int]$CurrentId,
        [int]$CurrentPoints,
        [int]$BufferedId,
        [int]$BufferedCount,
        [int]$Expected
    )
    $actual = [int]$effectiveCatalyst.Invoke(
        $null,
        @($CurrentId, $CurrentPoints, $BufferedId, $BufferedCount)
    )
    if ($actual -ne $Expected) {
        throw "$Name returned $actual; expected $Expected."
    }
}

function Assert-Lensed {
    param(
        [string]$Name,
        [int]$ProductId,
        [int]$CurrentId,
        [int]$CurrentPoints,
        [int]$BufferedId,
        [int]$BufferedCount,
        [bool]$Expected
    )
    $actual = [bool]$isLensed.Invoke(
        $null,
        @(
            $ProductId,
            $CurrentId,
            $CurrentPoints,
            $BufferedId,
            $BufferedCount
        )
    )
    if ($actual -ne $Expected) {
        throw "$Name returned $actual; expected $Expected."
    }
}

Assert-EffectiveCatalyst 'Active lens with empty buffer' 1209 1200 0 0 1209
Assert-EffectiveCatalyst 'Buffered lens without active lens' 0 0 1209 2 1209
Assert-EffectiveCatalyst 'No active or buffered lens' 1209 0 1209 0 0
Assert-EffectiveCatalyst 'Active item takes native precedence' 9999 1200 1209 2 9999
Assert-Lensed 'Active lens is accepted' 1208 1209 1200 0 0 $true
Assert-Lensed 'Buffered lens is accepted' 1208 0 0 1209 2 $true
Assert-Lensed 'No lens is rejected' 1208 1209 0 1209 0 $false
Assert-Lensed 'Non-lens active item is rejected' 1208 9999 1200 1209 2 $false
Assert-Lensed 'Wrong receiver mode is rejected' 0 1209 1200 0 0 $false
Assert-Lensed 'Missing reflected evidence fails softly' 1208 0 0 0 0 $false

Write-Output 'Receiver lens and continuity tests passed.'
