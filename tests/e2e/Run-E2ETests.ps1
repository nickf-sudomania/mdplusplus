<#
.SYNOPSIS
    Automated Unattended E2E Test Runner for MDPlus
.DESCRIPTION
    Runs the complete 4-tier opaque-box E2E test suite for MDPlus.
    Configures .NET 8 SDK PATH, compiles necessary targets, executes MDPlus.E2E,
    and returns exit code 0 on success, non-zero on failure.
#>

$ErrorActionPreference = "Stop"

# 1. Configure .NET 8 SDK path
$dotnetPath = "$env:LOCALAPPDATA\Microsoft\dotnet"
if (Test-Path $dotnetPath) {
    $env:PATH = "$dotnetPath;" + $env:PATH
}

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "       MDPlus Automated E2E Test Suite Runner     " -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Dotnet SDK: $(dotnet --version)" -ForegroundColor Gray
Write-Host "Working Dir: $PSScriptRoot" -ForegroundColor Gray
Write-Host ""

$repoRoot = Resolve-Path "$PSScriptRoot\..\.."
$e2eProj = Join-Path $PSScriptRoot "MDPlus.E2E.csproj"

# 2. Build MDPlus and E2E Test Project
Write-Host "[1/3] Building MDPlus solution..." -ForegroundColor Yellow
$buildOutput = dotnet build "$repoRoot\src\MDPlus.csproj" -c Debug 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed for MDPlus.csproj" -ForegroundColor Red
    Write-Host $buildOutput
    exit 1
}

Write-Host "[2/3] Building MDPlus.E2E test suite..." -ForegroundColor Yellow
$e2eBuild = dotnet build "$e2eProj" -c Debug 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed for MDPlus.E2E.csproj" -ForegroundColor Red
    Write-Host $e2eBuild
    exit 1
}

# 3. Execute E2E Tests
Write-Host "[3/3] Executing E2E Test Harness (Tiers 1-4)..." -ForegroundColor Yellow
Write-Host ""

dotnet run --project "$e2eProj" -c Debug --no-build

$testExitCode = $LASTEXITCODE

if ($testExitCode -eq 0) {
    Write-Host "`nAll E2E tests PASSED successfully." -ForegroundColor Green
    exit 0
} else {
    Write-Host "`nE2E test suite reported FAILURES. Exit code: $testExitCode" -ForegroundColor Red
    exit $testExitCode
}
