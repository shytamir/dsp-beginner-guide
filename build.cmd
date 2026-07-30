@echo off
setlocal
set "DEFAULT_GAME_ROOT=C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program"

if "%~1"=="" (
  set "GAME_ROOT=%DEFAULT_GAME_ROOT%"
) else (
  set "GAME_ROOT=%~1"
)

pushd "%~dp0"
for /f "tokens=1,2 delims==" %%A in (VERSION) do set "%%A=%%B"
echo Building DSP Guide Check %MAJOR%.%MINOR%.0
echo Game root: %GAME_ROOT%
dotnet build "src\DspProgressionStatusExporter\DspProgressionStatusExporter.csproj" -c Release -p:GameRoot="%GAME_ROOT%"

if errorlevel 1 (
  echo.
  echo BUILD FAILED
  popd
  exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\Test-ProductionLookup.ps1" -GameRoot "%GAME_ROOT%"

if errorlevel 1 (
  echo.
  echo TEST FAILED
  popd
  exit /b 1
)

echo.
echo BUILD SUCCEEDED
echo DLL: src\DspProgressionStatusExporter\bin\Release\net472\DspProgressionStatusExporter.dll
popd
