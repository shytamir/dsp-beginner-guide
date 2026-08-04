[CmdletBinding()]
param(
    [string]$DllPath,

    [string]$GameRoot = (
        'C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program'
    )
)

$ErrorActionPreference = 'Stop'
if ([String]::IsNullOrEmpty($DllPath)) {
    $repositoryRoot = Split-Path -Parent $PSScriptRoot
    $DllPath = Join-Path $repositoryRoot `
        'src\DspProgressionStatusExporter\bin\Release\net472\DspGuideCheck.dll'
}

function Load-Assembly {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required assembly was not found: $Path"
    }
    [Reflection.Assembly]::LoadFrom(
        (Resolve-Path -LiteralPath $Path)
    ) | Out-Null
}

$managed = Join-Path $GameRoot 'DSPGAME_Data\Managed'
foreach ($name in @(
        'UnityEngine.CoreModule.dll',
        'UnityEngine.dll',
        'UnityEngine.InputLegacyModule.dll',
        'UnityEngine.TextRenderingModule.dll',
        'UnityEngine.UIModule.dll',
        'UnityEngine.UI.dll'
    )) {
    Load-Assembly (Join-Path $managed $name)
}
Load-Assembly (Join-Path $GameRoot 'BepInEx\core\BepInEx.dll')
Load-Assembly $DllPath

$assembly = [Reflection.Assembly]::LoadFrom(
    (Resolve-Path -LiteralPath $DllPath)
)
$flags = [Reflection.BindingFlags]'Static,Public,NonPublic'

$navigator = $assembly.GetType(
    'DspProgressionStatusExporter.ManualPhaseNavigator', $true
)
$normalize = $navigator.GetMethod('NormalizePhase', $flags)
$previous = $navigator.GetMethod('Previous', $flags)
$next = $navigator.GetMethod('Next', $flags)
$seed = $navigator.GetMethod('Seed', $flags)

if ($normalize.Invoke($null, @('bootstrap')) -ne 'blue') {
    throw 'A stored BOOTSTRAP selection did not normalize to BLUE.'
}
if ($normalize.Invoke($null, @('unknown-phase')) -ne 'blue') {
    throw 'The invalid-selection fallback is not BLUE.'
}
if ($previous.Invoke($null, @('blue')) -ne 'blue' -or
    $next.Invoke($null, @('blue')) -ne 'red') {
    throw 'BLUE is not the first phase in manual navigation.'
}

$selectionType = $assembly.GetType(
    'DspProgressionStatusExporter.ManualPhaseSelection', $true
)
$parse = $selectionType.GetMethod('Parse', $flags)
$storedSelection = $parse.Invoke(
    $null, @('nav2;phase=bootstrap;seed=stored')
)
if ($selectionType.GetField('PhaseId').GetValue($storedSelection) -ne 'blue' -or
    $storedSelection.Serialize() -notmatch 'phase=blue') {
    throw 'A persisted BOOTSTRAP selection did not migrate to BLUE.'
}

$unlocked = [System.Collections.Generic.HashSet[int]]::new()
$seeded = $seed.Invoke($null, (, $unlocked))
if ($seeded.GetType().GetField('PhaseId').GetValue($seeded) -ne 'blue') {
    throw 'A new playthrough without Cube research did not seed BLUE.'
}

$analyzer = $assembly.GetType(
    'DspProgressionStatusExporter.GuideAnalyzer', $true
)
$phases = $analyzer.GetField('Phases', $flags).GetValue($null)
if ($phases[0].GetType().GetField('Id').GetValue($phases[0]) -ne 'blue' -or
    $phases.Count -ne 9) {
    throw 'Guide analysis does not expose the nine-phase BLUE-first contract.'
}

$gateEngine = $assembly.GetType(
    'DspProgressionStatusExporter.GuideGateEngine', $true
)
$gates = $gateEngine.GetField('Gates', $flags).GetValue($null)
if ($gates[0].GetType().GetField('Id').GetValue($gates[0]) -ne 'blue' -or
    $gates.Count -ne 9) {
    throw 'Progression evaluation does not expose the nine-phase BLUE-first contract.'
}

$snapshotBuilder = $assembly.GetType(
    'DspProgressionStatusExporter.CompactSnapshotBuilder', $true
)
$phaseItems = $snapshotBuilder.GetField('PhaseItems', $flags).GetValue($null)
if ($phaseItems.ContainsKey('bootstrap') -or
    -not $phaseItems.ContainsKey('blue')) {
    throw 'Snapshot evidence routing still exposes BOOTSTRAP.'
}
$blueItems = $phaseItems['blue']
foreach ($itemId in @(1101, 1104, 1202, 1301, 2001, 6001)) {
    if ($blueItems -notcontains $itemId) {
        throw "BLUE snapshot evidence is missing item $itemId."
    }
}

Write-Output 'Guide 2.0 phase contract test passed.'
