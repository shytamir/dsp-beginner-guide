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
$plugin = [Runtime.Serialization.FormatterServices]::GetUninitializedObject((GetModelType 'Plugin'))
$instanceFlags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$plugin.GetType().GetField('guidePanel',$instanceFlags).SetValue($plugin,$controller)
$storedSelection = Selection 'nav3;phase=ils;ils=2;ilsOrigin=manual-control'
$plugin.GetType().GetField('activePhaseSelection',$instanceFlags).SetValue($plugin,$storedSelection)
$storedBefore = $storedSelection.Serialize()
$ensurePhase = $plugin.GetType().GetMethod('EnsurePhaseSelection',$instanceFlags)
foreach ($refresh in 1..3) {
    $effective = $ensurePhase.Invoke($plugin,@($null,$null))
    Assert ((Field $effective 'PhaseId') -eq 'white') 'Expert plugin did not pin collection/analysis to WHITE'
    Assert ((Field $effective 'PersistenceState') -eq 'not-persisted' -and $storedSelection.Serialize() -eq $storedBefore) 'Expert phase overwrote the normal selection'
}
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
$script:imports=0
$normalController=New 'GuidePanelController'
$normalController.SetBlueprintImportAction([Action]{ $script:imports++ })
$importCallback=$normalController.GetType().GetMethod('ImportBlueprints',$instanceFlags)
$importCallback.Invoke($normalController,@()) | Out-Null
Assert ($script:imports -eq 0) 'Normal-mode import callback remained active'
$controller.SetBlueprintImportAction([Action]{ $script:imports++ })
Assert ($script:imports -eq 0) 'Setting Expert import action triggered an import'
foreach ($click in 1..2) { $importCallback.Invoke($controller,@()) | Out-Null }
Assert ($script:imports -eq 2) 'Each Expert click must invoke import'

# A synthetic native path keeps this fixture away from the player's library.
Add-Type 'public static class GameConfig { public static string Folder; public static int Reads; public static string blueprintFolder { get { Reads++; return Folder; } } }'
$importRoot=Join-Path $fixtures ([Guid]::NewGuid().ToString())
[GameConfig]::Folder=$importRoot
$pluginImport=$plugin.GetType().GetMethod('ImportBlueprints',$instanceFlags)
$plugin.GetType().GetField('guidePanel',$instanceFlags).SetValue($plugin,$normalController)
$pluginImport.Invoke($plugin,@()) | Out-Null
Assert ([GameConfig]::Reads -eq 0 -and -not (Test-Path -LiteralPath $importRoot)) 'Normal plugin read/wrote the blueprint path'
$plugin.GetType().GetField('guidePanel',$instanceFlags).SetValue($plugin,$controller)
$pluginImport.Invoke($plugin,@()) | Out-Null
Assert ([GameConfig]::Reads -eq 1 -and (Test-Path -LiteralPath (Join-Path $importRoot 'Guide Check/README.md'))) 'Expert plugin did not use the native blueprint path'
[GameConfig]::Folder=$null
$pluginImport.Invoke($plugin,@()) | Out-Null
Assert ([GameConfig]::Reads -eq 2) 'Expert click did not resolve the current path or failed softly'
$controller.Hide(); $controller.UpdateModel((PhasePanel (PhotonState) 'photon')); $controller.Tick(1.0)
Assert (-not $controller.IsVisible) 'Hidden refresh reopened Expert overlay'
Assert ($script:imports -eq 2) 'Hidden refresh automatically imported blueprints'
$controller.Destroy()
Write-Host 'BepInEx config binding, Expert-only import callbacks/native path, soft failure, inert omitted callbacks and hidden lifecycle passed; Unity creation/layout requires workshop.'
