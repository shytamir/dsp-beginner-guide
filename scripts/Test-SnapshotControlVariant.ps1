[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$DllPath,

    [Parameter(Mandatory = $true)]
    [ValidateSet('diagnostic', 'public')]
    [string]$ExpectedVariant
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $DllPath -PathType Leaf)) {
    throw "Built DLL was not found: $DllPath."
}

$resolvedDllPath = (Resolve-Path -LiteralPath $DllPath).Path
$assembly = [Reflection.Assembly]::LoadFile($resolvedDllPath)
$features = $assembly.GetType(
    'DspProgressionStatusExporter.BuildFeatures',
    $true
)
$flags = [Reflection.BindingFlags]'Public,Static'
$variant = $features.GetField('Variant', $flags).GetRawConstantValue()
$snapshotControlEnabled = $features.GetField(
    'SnapshotControlEnabled',
    $flags
).GetRawConstantValue()
$expectedEnabled = $ExpectedVariant -ceq 'diagnostic'

if ($variant -cne $ExpectedVariant) {
    throw "Build variant mismatch. Expected '$ExpectedVariant'; found '$variant'."
}
if ($snapshotControlEnabled -ne $expectedEnabled) {
    throw (
        'Snapshot-control marker mismatch. Expected ' +
        "$expectedEnabled; found $snapshotControlEnabled."
    )
}

Write-Output (
    "Snapshot-control variant verification passed: $ExpectedVariant " +
    "(enabled=$snapshotControlEnabled)"
)
