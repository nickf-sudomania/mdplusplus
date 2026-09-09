@echo off
setlocal enabledelayedexpansion

echo =======================================================
echo          MDPlus - Native Windows Markdown Viewer       
echo =======================================================
echo.

if exist "%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe" (
    set "PATH=%LOCALAPPDATA%\Microsoft\dotnet;%PATH%"
)

where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK (dotnet) not found in PATH.
    echo Please install the .NET 8.0 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo 1. Build Solution (Debug)
echo 2. Run Comprehensive Unit Tests
echo 3. Launch MDPlus Viewer
echo 4. Publish Release & Generate SHA-256 Hashes (Notepad++ Standard)
echo 5. Verify Download Checksums (SHA-256)
echo 6. Clean Artifacts
echo.
set /p choice="Select an option (1-6, default=1): "

if "%choice%"=="" set choice=1

if "%choice%"=="1" (
    echo.
    echo [INFO] Building MDPlus solution...
    dotnet build MDPlus.sln -c Debug
    if %errorlevel% equ 0 (
        echo [SUCCESS] Build succeeded! Executable located in:
        echo   src\bin\Debug\net8.0-windows\MDPlus.exe
    )
    goto end
)

if "%choice%"=="2" (
    echo.
    echo [INFO] Building and running unit tests...
    dotnet run --project tests\MDPlus.Tests.csproj -c Release
    goto end
)

if "%choice%"=="3" (
    echo.
    echo [INFO] Launching MDPlus...
    dotnet run --project src\MDPlus.csproj
    goto end
)

if "%choice%"=="4" (
    echo.
    echo [INFO] Publishing release executable, zip archive, and generating SHA-256 hashes...
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1" -Action Publish
    goto end
)

if "%choice%"=="5" (
    echo.
    echo [INFO] Verifying release download integrity against SHA256SUMS.txt...
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1" -Action Verify
    goto end
)

if "%choice%"=="6" (
    echo.
    echo [INFO] Cleaning build artifacts...
    dotnet clean MDPlus.sln
    if exist dist rmdir /s /q dist
    echo [SUCCESS] Clean completed.
    goto end
)

:end
echo.
pause
