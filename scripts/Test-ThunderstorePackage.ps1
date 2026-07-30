[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$ExpectedVersion,

    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot),

    [string]$ReportPath = (
        Join-Path $RepositoryRoot 'artifacts\PACKAGE-REPORT.md'
    )
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Read-ZipText {
    param(
        [System.IO.Compression.ZipArchiveEntry]$Entry
    )

    $stream = $Entry.Open()
    try {
        $encoding = New-Object System.Text.UTF8Encoding($false, $true)
        $reader = New-Object System.IO.StreamReader(
            $stream,
            $encoding,
            $true
        )
        try {
            return $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }
}

if (-not (Test-Path -LiteralPath $PackagePath -PathType Leaf)) {
    throw "Package was not found: $PackagePath"
}

$expectedEntries = @(
    'manifest.json',
    'README.md',
    'icon.png',
    'BepInEx/plugins/DSP-Guide-Check/DspGuideCheck.dll'
)
$expectedDescription = (
    'DSP Guide Check is an on-demand progression companion for Dyson Sphere ' +
    'Program. It reads the current save, evaluates the guide phase selected ' +
    'by the player, and presents stable objectives plus concise, phase-aware ' +
    'status.'
)
$expectedDependency = 'xiaoye97-BepInEx-5.4.17'
$expectedWebsite = 'https://github.com/shytamir/dsp-beginner-guide'

$archive = [System.IO.Compression.ZipFile]::OpenRead(
    (Resolve-Path -LiteralPath $PackagePath)
)
try {
    $fileEntries = @(
        $archive.Entries |
            Where-Object { -not $_.FullName.EndsWith('/') }
    )
    $entryNames = @($fileEntries | ForEach-Object {
        $_.FullName.Replace('\', '/')
    })

    if (@($fileEntries | Where-Object {
                $_.FullName.Contains('\')
            }).Count -gt 0) {
        throw 'Package contains non-portable backslash entry names.'
    }
    if ($entryNames.Count -ne $expectedEntries.Count) {
        throw "Package contains $($entryNames.Count) files; expected $($expectedEntries.Count)."
    }
    foreach ($expectedEntry in $expectedEntries) {
        if ($entryNames -cnotcontains $expectedEntry) {
            throw "Package entry is missing or incorrectly cased: $expectedEntry"
        }
    }
    if (($entryNames | Select-Object -Unique).Count -ne $entryNames.Count) {
        throw 'Package contains duplicate file entries.'
    }

    $manifestEntry = $fileEntries |
        Where-Object { $_.FullName -ceq 'manifest.json' } |
        Select-Object -First 1
    $manifest = (Read-ZipText -Entry $manifestEntry) | ConvertFrom-Json

    if ($manifest.name -cne 'DSPGuideCheck') {
        throw "Manifest name is invalid: $($manifest.name)"
    }
    if ($manifest.version_number -cne $ExpectedVersion) {
        throw "Manifest version is invalid: $($manifest.version_number)"
    }
    if ($manifest.website_url -cne $expectedWebsite) {
        throw "Manifest website is invalid: $($manifest.website_url)"
    }
    if ($manifest.description -cne $expectedDescription) {
        throw 'Manifest description does not match the package contract.'
    }
    if (@($manifest.dependencies).Count -ne 1 -or
        $manifest.dependencies[0] -cne $expectedDependency) {
        throw 'Manifest dependency list does not match the package contract.'
    }

    $readmeEntry = $fileEntries |
        Where-Object { $_.FullName -ceq 'README.md' } |
        Select-Object -First 1
    $readme = Read-ZipText -Entry $readmeEntry
    if ([string]::IsNullOrWhiteSpace($readme) -or
        -not $readme.StartsWith('# DSP Guide Check')) {
        throw 'Package README is empty or has an unexpected heading.'
    }

    Add-Type -AssemblyName System.Drawing
    $iconEntry = $fileEntries |
        Where-Object { $_.FullName -ceq 'icon.png' } |
        Select-Object -First 1
    $iconStream = $iconEntry.Open()
    try {
        $icon = [System.Drawing.Image]::FromStream($iconStream)
        try {
            if ($icon.Width -ne 256 -or $icon.Height -ne 256) {
                throw "Package icon is $($icon.Width)x$($icon.Height); expected 256x256."
            }
            if ($icon.RawFormat.Guid -ne [System.Drawing.Imaging.ImageFormat]::Png.Guid) {
                throw 'Package icon is not a PNG image.'
            }
        }
        finally {
            $icon.Dispose()
        }
    }
    finally {
        $iconStream.Dispose()
    }

    $dllEntry = $fileEntries |
        Where-Object {
            $_.FullName.Replace('\', '/') -ceq
                'BepInEx/plugins/DSP-Guide-Check/DspGuideCheck.dll'
        } |
        Select-Object -First 1
    if ($dllEntry.Length -le 0) {
        throw 'Packaged DLL is empty.'
    }
}
finally {
    $archive.Dispose()
}

$packageHash = (
    Get-FileHash -LiteralPath $PackagePath -Algorithm SHA256
).Hash.ToLowerInvariant()
$packageLength = (Get-Item -LiteralPath $PackagePath).Length
$reportDirectory = Split-Path -Parent $ReportPath
New-Item -ItemType Directory -Force -Path $reportDirectory | Out-Null

$entryList = ($expectedEntries | ForEach-Object { "- ``$_``" }) -join "`n"
$report = @"
# Thunderstore package verification

| Check | Result |
| --- | --- |
| Package | ``$PackagePath`` |
| Version | ``$ExpectedVersion`` |
| Manifest contract | Passed |
| Required root files | Passed |
| Install path | Passed |
| Icon format and dimensions | Passed |
| README UTF-8 and heading | Passed |
| File count | $($expectedEntries.Count) |
| Size | $packageLength bytes |
| SHA-256 | ``$packageHash`` |

## Package files

$entryList
"@
Set-Content -LiteralPath $ReportPath -Value $report -Encoding utf8

Write-Output "Thunderstore package verification passed: $ExpectedVersion"
Write-Output "Package SHA-256: $packageHash"
