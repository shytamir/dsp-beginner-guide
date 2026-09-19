#requires -Version 7.0
[CmdletBinding()]
param(
    [string]$GameRoot='C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program',
    [ValidateRange(1,65534)][int]$Sequence=1,
    [string]$OutputDirectory,
    [switch]$AllowWorkingTree
)
$ErrorActionPreference='Stop'
$repo=(Resolve-Path (Split-Path $PSScriptRoot -Parent)).Path
$gitRepo=$repo.Replace('\','/')
$source=(& git -c "safe.directory=$gitRepo" -C $repo rev-parse HEAD).Trim()
if($LASTEXITCODE -ne 0) { throw 'Cannot resolve source revision.' }
$dirty=@(& git -c "safe.directory=$gitRepo" -C $repo status --porcelain)
if($LASTEXITCODE -ne 0) { throw 'Cannot inspect source state.' }
if($dirty.Count -gt 0 -and -not $AllowWorkingTree) { throw 'Commit the candidate source first, or use -AllowWorkingTree for a preflight that is not workshop-ready.' }
if([string]::IsNullOrWhiteSpace($OutputDirectory)) { $OutputDirectory=Join-Path $repo ("artifacts/guide3/candidate-{0}-{1}" -f $source.Substring(0,7),$Sequence) }
$output=[IO.Path]::GetFullPath($OutputDirectory)
$artifactRoot=[IO.Path]::GetFullPath((Join-Path $repo 'artifacts'))+[IO.Path]::DirectorySeparatorChar
if(-not $output.StartsWith($artifactRoot,[StringComparison]::OrdinalIgnoreCase)) { throw 'Candidate output must be under this repository artifacts directory.' }
foreach($path in @('BepInEx/core/BepInEx.dll','DSPGAME_Data/Managed/Assembly-CSharp.dll','DSPGAME_Data/Managed/UnityEngine.UI.dll')) {
    if(-not (Test-Path -LiteralPath (Join-Path $GameRoot $path))) { throw "Game reference missing: $path. Supply -GameRoot." }
}
New-Item -ItemType Directory -Force -Path $output | Out-Null
$reportRoot=Join-Path $output 'reports'; New-Item -ItemType Directory -Force -Path $reportRoot | Out-Null
$hostExe=(Get-Process -Id $PID).Path
function Check([string]$Name,[string[]]$Arguments) {
    & $hostExe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot "$Name.ps1") @Arguments 2>&1 | Tee-Object -FilePath (Join-Path $reportRoot "$Name-$script:variant.log")
    if($LASTEXITCODE -ne 0) { throw "$Name failed ($LASTEXITCODE)." }
}
$generated=Join-Path $output 'BuildVersion.cs'
$version=& (Join-Path $PSScriptRoot 'Set-BuildVersion.ps1') -Sequence $Sequence -Commit $source -BuildVersionPath $generated -BuildInfoPath (Join-Path $output 'BUILD-INFO.txt')
$project=Join-Path $repo 'src/DspProgressionStatusExporter/DspProgressionStatusExporter.csproj'
foreach($script:variant in @('diagnostic','public')) {
    $dir=Join-Path $output $variant
    $snapshotControl=if($variant -eq 'diagnostic'){'true'}else{'false'}
    & dotnet build $project -c Release "-p:GameRoot=$GameRoot" "-p:GeneratedBuildVersion=$generated" "-p:IncludeSnapshotControl=$snapshotControl" "-p:Version=$($version.SEMANTIC_VERSION)" "-p:AssemblyVersion=$($version.ASSEMBLY_VERSION)" "-p:FileVersion=$($version.ASSEMBLY_VERSION)" "-p:InformationalVersion=$($version.RELEASE_LABEL)" -p:IncludeSourceRevisionInInformationalVersion=false -o $dir 2>&1 | Tee-Object -FilePath (Join-Path $reportRoot "build-$variant.log")
    if($LASTEXITCODE -ne 0) { throw "$variant build failed." }
    $dll=Join-Path $dir 'DspGuideCheck.dll'
    Check 'Test-Guide3Portable' @('-DllPath',$dll,'-ReportPath',(Join-Path $reportRoot "portable-$variant.md"))
    foreach($suite in @('Test-ProductionLookup','Test-Guide2PhaseContract','Test-PhotonCollection','Test-ExpertConfiguration')) { Check $suite @('-DllPath',$dll,'-GameRoot',$GameRoot) }
    Check 'Test-SnapshotControlVariant' @('-DllPath',$dll,'-ExpectedVariant',$variant)
    Check 'Test-Guide3Snapshot' @('-DllPath',$dll,'-GameRoot',$GameRoot,'-ExpectedVersion',$version.SEMANTIC_VERSION,'-OutputPath',(Join-Path $reportRoot "snapshots-$variant.json"))
    Check 'Test-BuildArtifact' @('-DllPath',$dll,'-BuildVersionPath',$generated,'-ExpectedReleaseLabel',$version.RELEASE_LABEL,'-ExpectedSemanticVersion',$version.SEMANTIC_VERSION,'-ExpectedAssemblyVersion',$version.ASSEMBLY_VERSION,'-ExpectedBepInExReferenceVersion','5.4.17.0','-ReportPath',(Join-Path $reportRoot "artifact-$variant.md"))
}
$a=(Get-FileHash -LiteralPath (Join-Path $reportRoot 'snapshots-diagnostic.json')).Hash
$b=(Get-FileHash -LiteralPath (Join-Path $reportRoot 'snapshots-public.json')).Hash
if($a -cne $b) { throw 'Diagnostic/public snapshot fixture parity failed.' }
$publicDll=Join-Path $output 'public/DspGuideCheck.dll'
$packages=Join-Path $output 'packages'
Check 'New-ThunderstorePackage' @('-DllPath',$publicDll,'-VersionNumber',$version.PACKAGE_VERSION,'-OutputDirectory',$packages)
$package=Join-Path $packages "DSPGuideCheck-$($version.PACKAGE_VERSION).zip"
Check 'Test-ThunderstorePackage' @('-PackagePath',$package,'-ExpectedVersion',$version.PACKAGE_VERSION,'-ExpectedDllPath',$publicDll,'-ReportPath',(Join-Path $reportRoot 'package.md'))
$manifest=[ordered]@{ sourceRevision=$source; sourceClean=($dirty.Count -eq 0); workingTreeChanges=$dirty; packageVersion=$version.PACKAGE_VERSION; releaseLabel=$version.RELEASE_LABEL; powerShell=$PSVersionTable.PSVersion.ToString(); gameRoot=$GameRoot; runtimeValidation='automated checks only; owner evidence is recorded in WORKSHOP.md'; snapshotParity='passed'; artifacts=@() }
foreach($file in @((Join-Path $output 'diagnostic/DspGuideCheck.dll'),$publicDll,$package)) {
    $manifest.artifacts += [ordered]@{path=$file;bytes=(Get-Item -LiteralPath $file).Length;sha256=(Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant()}
}
if($dirty.Count -eq 0) {
    & git -c "safe.directory=$gitRepo" -C $repo archive --format=zip "--output=$(Join-Path $output 'source.zip')" HEAD
    if($LASTEXITCODE -ne 0) { throw 'Source archive failed.' }
}
Copy-Item -LiteralPath (Join-Path $repo 'docs/management/GUIDE3-WORKSHOP.md') -Destination (Join-Path $output 'WORKSHOP.md')
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $output 'candidate.json')
Write-Host "Candidate verification passed: $output"
if($dirty.Count -gt 0) { Write-Host 'Preflight only: uncommitted source is not a workshop candidate.' }
