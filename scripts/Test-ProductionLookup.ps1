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

if (-not ('ProductionLookupFixture' -as [type])) {
    Add-Type -TypeDefinition @'
public sealed class ProductionLookupFixture
{
    public object[] productPool;
    public int[] productIndices;
}

public sealed class ProductStatFixture
{
    public int itemId;
    public long[] total;
}
'@
}

function New-ProductStat {
    param(
        [int]$ItemId,
        [long]$Produced,
        [long]$Consumed,
        [long]$TenMinuteProduced = ($Produced * 10),
        [long]$TenMinuteConsumed = ($Consumed * 10)
    )

    $totals = [long[]]::new(14)
    $totals[1] = $Produced
    $totals[2] = $TenMinuteProduced
    $totals[8] = $Consumed
    $totals[9] = $TenMinuteConsumed
    $stat = [ProductStatFixture]::new()
    $stat.itemId = $ItemId
    $stat.total = $totals
    return $stat
}

$fixture = [ProductionLookupFixture]::new()
$fixture.productPool = [object[]]::new(6)
$fixture.productIndices = [int[]]::new(12000)
$fixture.productPool[1] = New-ProductStat 6004 48 80
$fixture.productPool[2] = New-ProductStat 1303 72 114
$fixture.productPool[3] = New-ProductStat 9999 1 1
$fixture.productPool[4] = New-ProductStat 6006 0 0 0 0
$oneMinuteOnly = New-ProductStat 6005 12 20
$oneMinuteOnly.total = [long[]]$oneMinuteOnly.total[0..8]
$fixture.productPool[5] = $oneMinuteOnly
$fixture.productIndices[6004] = 1
$fixture.productIndices[1303] = 2
$fixture.productIndices[1003] = 3
$fixture.productIndices[6006] = 4
$fixture.productIndices[6005] = 5

$assembly = [Reflection.Assembly]::LoadFrom(
    (Resolve-Path -LiteralPath $DllPath)
)
$type = $assembly.GetType(
    'DspProgressionStatusExporter.ProductionTelemetry',
    $true
)
$method = $type.GetMethod(
    'ReadFactoryAggregates',
    [Reflection.BindingFlags]'Static,NonPublic'
)
if ($null -eq $method) {
    throw 'ReadFactoryAggregates was not found.'
}

$result = $method.Invoke($null, @($fixture))
if ($result.Count -ne 4) {
    throw "Expected four watched rows; found $($result.Count)."
}
if (-not $result.ContainsKey(6004) -or -not $result.ContainsKey(1303)) {
    throw 'The native item-to-pool index mapping was not followed.'
}
if ($result.ContainsKey(1003)) {
    throw 'A mismatched ProductStat item ID was accepted.'
}

$zero = $result[6006]
if (-not $zero.OneMinuteAvailable -or -not $zero.TenMinuteAvailable) {
    throw 'A legitimate zero was confused with unavailable evidence.'
}
$oneMinute = $result[6005]
if (-not $oneMinute.OneMinuteAvailable -or $oneMinute.TenMinuteAvailable) {
    throw 'One-minute and ten-minute availability were not independent.'
}

$purple = $result[6004]
$processor = $result[1303]
if ($purple.Produced -ne 48 -or $purple.Consumed -ne 80) {
    throw 'Purple Cube native totals were read incorrectly.'
}
if ($processor.Produced -ne 72 -or $processor.Consumed -ne 114) {
    throw 'Processor native totals were read incorrectly.'
}
if ($purple.TenMinuteProduced -ne 480 -or
    $purple.TenMinuteConsumed -ne 800) {
    throw 'Purple Cube ten-minute native totals were read incorrectly.'
}

$normalize = $type.GetMethod(
    'NormalizePerMinute',
    [Reflection.BindingFlags]'Static,NonPublic'
)
if ($null -eq $normalize) {
    throw 'NormalizePerMinute was not found.'
}
$normalized = $normalize.Invoke($null, @([long]480, [double]10))
if ([Math]::Abs([double]$normalized - 48.0) -gt 0.0001) {
    throw 'Ten-minute totals were not normalized to items per minute.'
}

Write-Output 'Production lookup test passed.'
