[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$Sequence,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[0-9a-fA-F]{7,40}$')]
    [string]$Commit,

    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot),

    [string]$BuildVersionPath = (
        Join-Path $RepositoryRoot `
            'src\DspProgressionStatusExporter\BuildVersion.cs'
    ),

    [string]$BuildInfoPath = (
        Join-Path $RepositoryRoot 'artifacts\BUILD-INFO.txt'
    )
)

$ErrorActionPreference = 'Stop'

$versionPath = Join-Path $RepositoryRoot 'VERSION'
if (-not (Test-Path -LiteralPath $versionPath -PathType Leaf)) {
    throw "VERSION was not found at $versionPath."
}

$values = @{}
foreach ($line in Get-Content -LiteralPath $versionPath) {
    if ($line -match '^\s*(MAJOR|MINOR)\s*=\s*(\d+)\s*$') {
        $values[$Matches[1]] = [int]$Matches[2]
    }
    elseif (-not [string]::IsNullOrWhiteSpace($line)) {
        throw "Invalid VERSION line: '$line'. Expected MAJOR=<integer> or MINOR=<integer>."
    }
}

foreach ($requiredName in @('MAJOR', 'MINOR')) {
    if (-not $values.ContainsKey($requiredName)) {
        throw "VERSION is missing $requiredName."
    }
}

$shortCommit = $Commit.Substring(0, 7).ToLowerInvariant()
$releaseLabel = '{0}.{1}.{2}.{3}' -f (
    $values.MAJOR, $values.MINOR, $Sequence, $shortCommit
)
$semanticVersion = '{0}.{1}.{2}+{3}' -f (
    $values.MAJOR, $values.MINOR, $Sequence, $shortCommit
)
$assemblyVersion = '{0}.{1}.{2}.0' -f (
    $values.MAJOR, $values.MINOR, $Sequence
)

$buildVersionDirectory = Split-Path -Parent $BuildVersionPath
$buildInfoDirectory = Split-Path -Parent $BuildInfoPath
New-Item -ItemType Directory -Force -Path $buildVersionDirectory | Out-Null
New-Item -ItemType Directory -Force -Path $buildInfoDirectory | Out-Null

$buildVersionSource = @"
namespace DspProgressionStatusExporter
{
    internal static class BuildVersion
    {
        public const string BepInPluginVersion = "$assemblyVersion";
        public const string PluginVersion = "$semanticVersion";
        public const string ReleaseLabel = "$releaseLabel";
    }
}
"@
Set-Content -LiteralPath $BuildVersionPath -Value $buildVersionSource `
    -Encoding utf8

$buildInfo = @"
Release label: $releaseLabel
Semantic version: $semanticVersion
Assembly version: $assemblyVersion
Source commit: $($Commit.ToLowerInvariant())
Workflow sequence: $Sequence
"@
Set-Content -LiteralPath $BuildInfoPath -Value $buildInfo -Encoding utf8

$environmentValues = [ordered]@{
    RELEASE_LABEL = $releaseLabel
    SEMANTIC_VERSION = $semanticVersion
    ASSEMBLY_VERSION = $assemblyVersion
    SOURCE_COMMIT = $Commit.ToLowerInvariant()
    SHORT_COMMIT = $shortCommit
}

if ($env:GITHUB_ENV) {
    foreach ($entry in $environmentValues.GetEnumerator()) {
        Add-Content -LiteralPath $env:GITHUB_ENV `
            -Value "$($entry.Key)=$($entry.Value)"
    }
}

if ($env:GITHUB_OUTPUT) {
    foreach ($entry in $environmentValues.GetEnumerator()) {
        Add-Content -LiteralPath $env:GITHUB_OUTPUT `
            -Value "$($entry.Key.ToLowerInvariant())=$($entry.Value)"
    }
}

[pscustomobject]$environmentValues
