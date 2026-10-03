#requires -Version 7.0
[CmdletBinding()]
param([Parameter(Mandatory)][string]$DllPath)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression
function Assert([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
$repo = Split-Path $PSScriptRoot -Parent
$fixture = Join-Path $repo ('artifacts/blueprint-import-fixtures/' + [Guid]::NewGuid().ToString())
$blueprints = Join-Path $fixture 'Custom Documents/Blueprint'
New-Item -ItemType Directory -Force -Path $blueprints | Out-Null
$assembly = [Reflection.Assembly]::LoadFile((Resolve-Path -LiteralPath $DllPath).Path)
$type = $assembly.GetType('DspProgressionStatusExporter.BlueprintImporter', $true)
$import = $type.GetMethod('Import', [Reflection.BindingFlags]'Static,NonPublic')
function Import-Pack([string]$Root) { $import.Invoke($null, @($Root)) | Out-Null }
function Assert-Rejected([string]$Root) {
    $rejected = $false
    try { Import-Pack $Root } catch {
        $cause = $_.Exception.GetBaseException()
        if ($cause -isnot [IO.IOException] -and $cause -isnot [InvalidOperationException]) { throw }
        $rejected = $true
    }
    Assert $rejected "Invalid or unwritable destination was accepted: $Root"
}
Assert-Rejected $null
Assert-Rejected 'relative-blueprint-path'
$resource = $assembly.GetManifestResourceStream('DspGuideCheck.Blueprints.DSP-Guide-Complete-Playthrough.zip')
Assert ($null -ne $resource) 'Bundled archive missing'
$sha = [Security.Cryptography.SHA256]::Create()
try {
    $embeddedHash = [BitConverter]::ToString($sha.ComputeHash($resource)).Replace('-', '').ToLowerInvariant()
    $suppliedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $repo 'src/DspProgressionStatusExporter/Assets/Blueprints/DSP-Guide-Complete-Playthrough.zip')).Hash.ToLowerInvariant()
    Assert ($embeddedHash -ceq $suppliedHash) 'Embedded archive differs from the supplied package'
    $resource.Position = 0
    $archive = [IO.Compression.ZipArchive]::new($resource, [IO.Compression.ZipArchiveMode]::Read, $true)
    try {
        $neighbor = Join-Path $blueprints 'player-blueprint.txt'
        [IO.File]::WriteAllText($neighbor, 'outside Guide Check')
        Import-Pack $blueprints
        $root = Join-Path $blueprints 'Guide Check'
        $entries = @($archive.Entries | Where-Object { $_.Name.Length -gt 0 })
        Assert (@(Get-ChildItem -LiteralPath $root -File -Recurse).Count -eq $entries.Count) 'Wrong extracted file count'
        foreach ($entry in $entries) {
            $path = Join-Path $root $entry.FullName
            Assert ([IO.Path]::GetFullPath($path).StartsWith([IO.Path]::GetFullPath($root) + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) 'Archive path escapes Guide Check'
            $stream = $entry.Open()
            try { $expected = [BitConverter]::ToString($sha.ComputeHash($stream)) } finally { $stream.Dispose() }
            $actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash
            Assert ($expected.Replace('-', '') -ceq $actual) "Extracted bytes differ: $($entry.FullName)"
            [IO.File]::SetLastWriteTimeUtc($path, [DateTime]::new(2000, 1, 1))
        }
        $first = Join-Path $root ($entries | Where-Object FullName -like '*.txt' | Select-Object -First 1).FullName
        $original = [IO.File]::ReadAllBytes($first)
        $changed = [byte[]]$original.Clone(); $changed[0] = $changed[0] -bxor 1
        [IO.File]::WriteAllBytes($first, $changed)
        $extra = Join-Path $root 'player-notes.txt'
        [IO.File]::WriteAllText($extra, 'keep unrelated file')
        Import-Pack $blueprints
        Assert ([Convert]::ToBase64String([IO.File]::ReadAllBytes($first)) -ceq [Convert]::ToBase64String($original)) 'Same-size modification was not overwritten'
        foreach ($entry in $entries) {
            Assert ([IO.File]::GetLastWriteTimeUtc((Join-Path $root $entry.FullName)).Year -gt 2000) "Unchanged file was skipped: $($entry.FullName)"
        }
        [IO.File]::SetLastWriteTimeUtc($first, [DateTime]::new(2000, 1, 1))
        Import-Pack $blueprints
        Assert ([IO.File]::GetLastWriteTimeUtc($first).Year -gt 2000) 'Identical repeated click skipped a write'
        Assert ([IO.File]::ReadAllText($extra) -ceq 'keep unrelated file') 'Unrelated Guide Check file was deleted'
        Assert ([IO.File]::ReadAllText($neighbor) -ceq 'outside Guide Check') 'Neighboring blueprint was modified'

        $locked = [IO.File]::Open($first, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
        try { Assert-Rejected $blueprints } finally { $locked.Dispose() }
        Import-Pack $blueprints

        $redirectRoot = Join-Path $fixture 'Redirected/Blueprint'
        $outside = Join-Path $fixture 'Outside'
        New-Item -ItemType Directory -Force -Path $redirectRoot, $outside | Out-Null
        New-Item -ItemType Junction -Path (Join-Path $redirectRoot 'Guide Check') -Target $outside | Out-Null
        Assert-Rejected $redirectRoot
        Assert (@(Get-ChildItem -LiteralPath $outside -Force).Count -eq 0) 'Import followed a redirected Guide Check folder'
        Write-Host "Blueprint import passed: $($entries.Count) exact files, nested custom root, repeat/overwrite, no skipped files, neighboring files preserved, invalid path, redirected folder, locked-file failure and recovery."
    } finally { $archive.Dispose() }
} finally { $sha.Dispose(); $resource.Dispose() }
