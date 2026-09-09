@echo off
setlocal enabledelayedexpansion

echo =======================================================
echo          MDPlus - Native Windows Markdown Viewer       
echo =======================================================
echo.

if exist "%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe" set "PATH=%LOCALAPPDATA%\Microsoft\dotnet;%PATH%"

where dotnet >nul 2>nul
if errorlevel 1 (
    echo [ERROR] .NET SDK 'dotnet' not found in PATH.
    echo Please install the .NET 8.0 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo 1. Build Solution [Debug]
echo 2. Run Comprehensive Unit Tests
echo 3. Launch MDPlus Viewer
echo 4. Publish Full Release [Binaries, Windows Installer, Notepad++ Hashes]
echo 5. Build Windows Setup Installer [MDPlus-Setup.exe]
echo 6. Verify Download Checksums [SHA-256]
echo 7. Clean Artifacts
echo.
set "choice=%~1"
if /i "%choice%"=="build" set choice=1
if /i "%choice%"=="test" set choice=2
if /i "%choice%"=="run" set choice=3
if /i "%choice%"=="publish" set choice=4
if /i "%choice%"=="installer" set choice=5
if /i "%choice%"=="verify" set choice=6
if /i "%choice%"=="clean" set choice=7

if "%choice%"=="" (
    set /p choice="Select an option (1-7, default=1): "
)
if "%choice%"=="" set choice=1

set "EXITCODE=0"

if "%choice%"=="1" (
    echo.
    echo [INFO] Building MDPlus solution...
    dotnet build MDPlus.sln -c Debug
    set "EXITCODE=!errorlevel!"
    if !EXITCODE! equ 0 (
        echo [SUCCESS] Build succeeded! Executable located in:
        echo   src\bin\Debug\net8.0-windows\MDPlus.exe
    )
    goto end
)

if "%choice%"=="2" (
    echo.
    echo [INFO] Building and running unit tests...
    dotnet run --project tests\MDPlus.Tests.csproj -c Release
    set "EXITCODE=!errorlevel!"
    goto end
)

if "%choice%"=="3" (
    echo.
    echo [INFO] Launching MDPlus...
    dotnet run --project src\MDPlus.csproj
    set "EXITCODE=!errorlevel!"
    goto end
)

if "%choice%"=="4" (
    echo.
    echo [INFO] Publishing release executable, installer, source code, and generating SHA-256 hashes...
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1" -Action Publish
    set "EXITCODE=!errorlevel!"
    goto end
)

if "%choice%"=="5" (
    echo.
    echo [INFO] Building MDPlus Windows Setup Installer...
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1" -Action Installer
    set "EXITCODE=!errorlevel!"
    goto end
)

if "%choice%"=="6" (
    echo.
    echo [INFO] Verifying release download integrity against SHA256SUMS.txt...
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1" -Action Verify
    set "EXITCODE=!errorlevel!"
    goto end
)

if "%choice%"=="7" (
    echo.
    echo [INFO] Cleaning build artifacts...
    dotnet clean MDPlus.sln
    set "EXITCODE=!errorlevel!"
    if exist dist rmdir /s /q dist
    echo [SUCCESS] Clean completed.
    goto end
)

:end
echo.
if "%~1"=="" pause
exit /b !EXITCODE!
