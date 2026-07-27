@echo off
setlocal
set "DEFAULT_GAME_ROOT=C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program"

if "%~1"=="" (
  set "GAME_ROOT=%DEFAULT_GAME_ROOT%"
) else (
  set "GAME_ROOT=%~1"
)

pushd "%~dp0"
echo Building DSP Guide Check 1.15.0
echo Game root: %GAME_ROOT%
dotnet build "src\DspProgressionStatusExporter\DspProgressionStatusExporter.csproj" -c Release -p:GameRoot="%GAME_ROOT%"

if errorlevel 1 (
  echo.
  echo BUILD FAILED
  popd
  exit /b 1
)

echo.
echo BUILD SUCCEEDED
echo DLL: src\DspProgressionStatusExporter\bin\Release\net472\DspProgressionStatusExporter.dll
popd
