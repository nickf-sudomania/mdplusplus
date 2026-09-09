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
echo 4. Publish Single-File Release Executable
echo 5. Clean Artifacts
echo.
set /p choice="Select an option (1-5, default=1): "

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
    echo [INFO] Publishing single-file release executable...
    dotnet publish src\MDPlus.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o dist\
    if %errorlevel% equ 0 (
        echo.
        echo [SUCCESS] Single-file executable ready:
        echo   dist\MDPlus.exe
    )
    goto end
)

if "%choice%"=="5" (
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
