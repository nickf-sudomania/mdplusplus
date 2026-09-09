<#
.SYNOPSIS
    Build, test, run, and publish script for MDPlus Markdown Viewer.

.PARAMETER Action
    The action to execute: Build, Test, Run, Publish, Clean. Default is Build.

.EXAMPLE
    .\build.ps1 -Action Build
    .\build.ps1 -Action Test
    .\build.ps1 -Action Run
    .\build.ps1 -Action Publish
#>

param(
    [ValidateSet("Build", "Test", "Run", "Publish", "Clean")]
    [string]$Action = "Build"
)

$ErrorActionPreference = "Stop"

Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host "         MDPlus - Native Windows Markdown Viewer       " -ForegroundColor Cyan
Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host ""

# Check dotnet presence and add user local dotnet if available
$localDotnet = Join-Path $env:LocalAppData "Microsoft\dotnet"
if (Test-Path (Join-Path $localDotnet "dotnet.exe")) {
    $env:PATH = "$localDotnet;" + $env:PATH
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error ".NET SDK ('dotnet') not found in PATH. Please install .NET 8.0 SDK."
    exit 1
}

$solutionPath = Join-Path $PSScriptRoot "MDPlus.sln"
$srcProjectPath = Join-Path $PSScriptRoot "src\MDPlus.csproj"
$testsProjectPath = Join-Path $PSScriptRoot "tests\MDPlus.Tests.csproj"
$distPath = Join-Path $PSScriptRoot "dist"

switch ($Action) {
    "Build" {
        Write-Host "[INFO] Building MDPlus solution..." -ForegroundColor Yellow
        dotnet build $solutionPath -c Debug
        Write-Host "`n[SUCCESS] Build completed successfully!" -ForegroundColor Green
        Write-Host "Executable: src\bin\Debug\net8.0-windows\MDPlus.exe" -ForegroundColor Gray
    }

    "Test" {
        Write-Host "[INFO] Running MDPlus test suite..." -ForegroundColor Yellow
        dotnet run --project $testsProjectPath -c Release
    }

    "Run" {
        Write-Host "[INFO] Starting MDPlus..." -ForegroundColor Yellow
        dotnet run --project $srcProjectPath
    }

    "Publish" {
        Write-Host "[INFO] Publishing standalone single-file binary to '$distPath'..." -ForegroundColor Yellow
        if (-not (Test-Path $distPath)) {
            New-Item -ItemType Directory -Path $distPath | Out-Null
        }
        dotnet publish $srcProjectPath -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o $distPath
        Write-Host "`n[SUCCESS] Publish complete! Standalone executable: dist\MDPlus.exe" -ForegroundColor Green
    }

    "Clean" {
        Write-Host "[INFO] Cleaning build directories..." -ForegroundColor Yellow
        dotnet clean $solutionPath
        if (Test-Path $distPath) {
            Remove-Item -Path $distPath -Recurse -Force
        }
        Write-Host "[SUCCESS] Clean completed." -ForegroundColor Green
    }
}
