[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$DllPath,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$VersionNumber,

    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot),

    [string]$ManifestTemplatePath = (
        Join-Path $RepositoryRoot 'packaging\manifest.template.json'
    ),

    [string]$ReadmePath = (
        Join-Path $RepositoryRoot 'packaging\README.md'
    ),

    [string]$IconPath = (
        Join-Path $RepositoryRoot 'packaging\icon.png'
    ),

    [string]$FontLicensePath = (
        Join-Path $RepositoryRoot (
            'src\DspProgressionStatusExporter\Assets\Fonts\Basic-OFL.txt'
        )
    ),

    [string]$OutputDirectory = (
        Join-Path $RepositoryRoot 'artifacts\packages'
    )
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

foreach ($requiredPath in @(
        $DllPath,
        $ManifestTemplatePath,
        $ReadmePath,
        $IconPath,
        $FontLicensePath
    )) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required package input was not found: $requiredPath"
    }
}

$template = Get-Content -Raw -LiteralPath $ManifestTemplatePath
$placeholder = '{{VERSION_NUMBER}}'
if (([regex]::Matches(
            $template,
            [regex]::Escape($placeholder)
        )).Count -ne 1) {
    throw "Manifest template must contain exactly one $placeholder placeholder."
}

$manifestText = $template.Replace($placeholder, $VersionNumber)
$manifest = $manifestText | ConvertFrom-Json
if ($manifest.version_number -cne $VersionNumber) {
    throw 'Manifest version replacement failed.'
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$packagePath = Join-Path $OutputDirectory "DSPGuideCheck-$VersionNumber.zip"

if (Test-Path -LiteralPath $packagePath) {
    Remove-Item -LiteralPath $packagePath -Force
}

$archive = [System.IO.Compression.ZipFile]::Open(
    $packagePath,
    [System.IO.Compression.ZipArchiveMode]::Create
)
try {
    $manifestEntry = $archive.CreateEntry(
        'manifest.json',
        [System.IO.Compression.CompressionLevel]::Optimal
    )
    $manifestStream = $manifestEntry.Open()
    try {
        $encoding = New-Object System.Text.UTF8Encoding($false)
        $writer = New-Object System.IO.StreamWriter(
            $manifestStream,
            $encoding
        )
        try {
            $writer.Write($manifestText)
        }
        finally {
            $writer.Dispose()
        }
    }
    finally {
        $manifestStream.Dispose()
    }

    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
        $archive,
        $ReadmePath,
        'README.md',
        [System.IO.Compression.CompressionLevel]::Optimal
    ) | Out-Null
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
        $archive,
        $IconPath,
        'icon.png',
        [System.IO.Compression.CompressionLevel]::Optimal
    ) | Out-Null
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
        $archive,
        $FontLicensePath,
        'Basic-OFL.txt',
        [System.IO.Compression.CompressionLevel]::Optimal
    ) | Out-Null
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
        $archive,
        $DllPath,
        'BepInEx/plugins/DSP-Guide-Check/DspGuideCheck.dll',
        [System.IO.Compression.CompressionLevel]::Optimal
    ) | Out-Null
}
finally {
    $archive.Dispose()
}

Write-Output "Thunderstore package created: $packagePath"
Write-Output "Package version: $VersionNumber"
