#requires -Version 7.0
[CmdletBinding()]
param([string]$DllPath,[string]$GameRoot='C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program')
$ErrorActionPreference='Stop'
foreach ($name in @('UnityEngine.CoreModule','UnityEngine','UnityEngine.InputLegacyModule','UnityEngine.TextRenderingModule','UnityEngine.UIModule','UnityEngine.UI')) {
    [Reflection.Assembly]::LoadFrom((Join-Path $GameRoot "DSPGAME_Data/Managed/$name.dll")) | Out-Null
}
[Reflection.Assembly]::LoadFrom((Join-Path $GameRoot 'BepInEx/core/BepInEx.dll')) | Out-Null
. "$PSScriptRoot/Test-ExpertMode.ps1" -DllPath $DllPath
$fixtures=Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/guide3/config-fixtures'
New-Item -ItemType Directory -Path $fixtures -Force | Out-Null
foreach ($setting in @('absent','false','true')) {
    $path=Join-Path $fixtures (([Guid]::NewGuid().ToString())+'.cfg')
    if($setting -ne 'absent') { "[General]`nExpertMode = $setting" | Set-Content -LiteralPath $path }
    $config=[BepInEx.Configuration.ConfigFile]::new($path,$false)
    $config.SaveOnConfigSet=$false
    $actual=Call 'GuidePresentationPolicy' 'BindExpertMode' (,$config)
    Assert ($actual -eq ($setting -eq 'true')) "Config binding failed: $setting"
}
$controller=New 'GuidePanelController'
Assert ($controller.GuidanceEnabled -and -not $controller.IsVisible) 'Default controller mode/start visibility failed'
$controller.SetExpertMode($true)
Assert (-not $controller.GuidanceEnabled -and -not $controller.IsVisible) 'Expert controller started visible'
$script:navigations=0; $script:snapshots=0
$controller.SetNavigationAction([Action[string]]{param($command) $script:navigations++})
$instanceFlags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
foreach ($method in @('ToggleCollapsed','ScrollUp','ScrollDown')) { $controller.GetType().GetMethod($method,$instanceFlags).Invoke($controller,@()) | Out-Null }
$controller.GetType().GetMethod('Navigate',$instanceFlags).Invoke($controller,@('next')) | Out-Null
$snapshot=$controller.GetType().GetMethod('SaveSnapshot',$instanceFlags)
if($null -ne $snapshot) {
    $controller.SetSnapshotAction([Func[bool]]{ $script:snapshots++; return $true })
    $snapshot.Invoke($controller,@()) | Out-Null
}
Assert ($script:navigations -eq 0 -and $script:snapshots -eq 0) 'Omitted Expert callback remained active'
$controller.Hide(); $controller.UpdateModel((PhasePanel (PhotonState) 'photon')); $controller.Tick(1.0)
Assert (-not $controller.IsVisible) 'Hidden refresh reopened Expert overlay'
$controller.Destroy()
Write-Host 'BepInEx config binding, inert omitted callbacks and hidden lifecycle passed; Unity creation/layout requires workshop.'
