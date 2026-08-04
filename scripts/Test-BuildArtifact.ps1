[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$DllPath,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedReleaseLabel,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedSemanticVersion,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedAssemblyVersion,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedBepInExReferenceVersion,

    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot),

    [string]$ReportPath = (
        Join-Path $RepositoryRoot 'artifacts\TEST-REPORT.md'
    )
)

$ErrorActionPreference = 'Stop'

function Assert-Equal {
    param(
        [string]$Name,
        [string]$Expected,
        [string]$Actual
    )

    if ($Expected -cne $Actual) {
        throw "$Name mismatch. Expected '$Expected'; found '$Actual'."
    }
}

if (-not (Test-Path -LiteralPath $DllPath -PathType Leaf)) {
    throw "Built DLL was not found at $DllPath."
}

if ((Get-Item -LiteralPath $DllPath).Length -le 0) {
    throw "Built DLL is empty: $DllPath."
}

if ($ExpectedReleaseLabel -notmatch '^\d+\.\d+\.\d+\.[0-9a-f]{7}$') {
    throw "Release label is invalid: $ExpectedReleaseLabel."
}

if ($ExpectedSemanticVersion -notmatch '^\d+\.\d+\.\d+$') {
    throw "Semantic version is invalid: $ExpectedSemanticVersion."
}

$versionSourcePath = Join-Path $RepositoryRoot `
    'src\DspProgressionStatusExporter\BuildVersion.cs'
$versionSource = Get-Content -Raw -LiteralPath $versionSourcePath
if (-not $versionSource.Contains(
        "BepInPluginVersion = `"$ExpectedSemanticVersion`"")) {
    throw 'Generated BepInEx plugin version does not match the public package version.'
}
if (-not $versionSource.Contains(
        "PluginVersion = `"$ExpectedSemanticVersion`"")) {
    throw 'Generated plugin version does not match the semantic version.'
}
if (-not $versionSource.Contains(
        "ReleaseLabel = `"$ExpectedReleaseLabel`"")) {
    throw 'Generated release label does not match the requested label.'
}

$parsedBepInPluginVersion = $null
if (-not [Version]::TryParse(
        $ExpectedSemanticVersion, [ref]$parsedBepInPluginVersion)) {
    throw "BepInEx plugin version is not a valid System.Version: $ExpectedSemanticVersion."
}

$pluginSourcePath = Join-Path $RepositoryRoot `
    'src\DspProgressionStatusExporter\Plugin.cs'
$pluginSource = Get-Content -Raw -LiteralPath $pluginSourcePath
if (-not $pluginSource.Contains(
        'BuildVersion.BepInPluginVersion)]')) {
    throw 'BepInPlugin does not use the numeric BepInPluginVersion.'
}

$assemblyName = [Reflection.AssemblyName]::GetAssemblyName(
    (Resolve-Path -LiteralPath $DllPath)
)
Assert-Equal 'Assembly version' $ExpectedAssemblyVersion `
    $assemblyName.Version.ToString()

$reflectionAssembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom(
    (Resolve-Path -LiteralPath $DllPath)
)
$bepInExReference = $reflectionAssembly.GetReferencedAssemblies() |
    Where-Object { $_.Name -ceq 'BepInEx' } |
    Select-Object -First 1
if ($null -eq $bepInExReference) {
    throw 'Built DLL does not reference BepInEx.'
}
Assert-Equal 'BepInEx assembly reference' $ExpectedBepInExReferenceVersion `
    $bepInExReference.Version.ToString()

$fileInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo(
    (Resolve-Path -LiteralPath $DllPath)
)
Assert-Equal 'File version' $ExpectedAssemblyVersion $fileInfo.FileVersion
Assert-Equal 'Product version' $ExpectedReleaseLabel $fileInfo.ProductVersion

$hash = (Get-FileHash -LiteralPath $DllPath -Algorithm SHA256).Hash.ToLowerInvariant()
$length = (Get-Item -LiteralPath $DllPath).Length
$reportDirectory = Split-Path -Parent $ReportPath
New-Item -ItemType Directory -Force -Path $reportDirectory | Out-Null

$report = @"
# Build artifact verification

| Check | Result |
| --- | --- |
| DLL | ``$DllPath`` |
| Release label | ``$ExpectedReleaseLabel`` |
| Semantic version | ``$ExpectedSemanticVersion`` |
| BepInEx plugin version | ``$ExpectedSemanticVersion`` |
| BepInEx assembly reference | ``$ExpectedBepInExReferenceVersion`` |
| Assembly/file version | ``$ExpectedAssemblyVersion`` |
| Size | $length bytes |
| SHA-256 | ``$hash`` |
| Compile | Passed before artifact verification |
| Version contract | Passed |
| BepInEx version parse | Passed |
| Artifact integrity | Passed |
"@
Set-Content -LiteralPath $ReportPath -Value $report -Encoding utf8

Write-Output "Artifact verification passed: $ExpectedReleaseLabel"
Write-Output "DLL SHA-256: $hash"
