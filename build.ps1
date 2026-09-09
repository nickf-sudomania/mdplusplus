<#
.SYNOPSIS
    Build, test, run, publish, and verify script for MDPlus Markdown Viewer.
    Implements Notepad++ release integrity standards (cryptographic SHA-256 checksums).

.PARAMETER Action
    The action to execute: Build, Test, Run, Publish, Verify, Clean. Default is Build.

.EXAMPLE
    .\build.ps1 -Action Build
    .\build.ps1 -Action Test
    .\build.ps1 -Action Run
    .\build.ps1 -Action Publish
    .\build.ps1 -Action Verify
#>

param(
    [ValidateSet("Build", "Test", "Run", "Publish", "Verify", "Clean")]
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

function Get-Sha256Hex([string]$filePath) {
    $hashResult = Get-FileHash -Path $filePath -Algorithm SHA256
    return $hashResult.Hash.ToLowerInvariant()
}

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
        
        # 1. Compile and publish single-file executable
        dotnet publish $srcProjectPath -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o $distPath
        $exePath = Join-Path $distPath "MDPlus.exe"

        if (-not (Test-Path $exePath)) {
            Write-Error "Published executable not found at '$exePath'."
            exit 1
        }

        # 2. Package portable zip archive (Notepad++ release style)
        Write-Host "`n[INFO] Creating portable release archive (MDPlus-win-x64.zip)..." -ForegroundColor Yellow
        $zipPath = Join-Path $distPath "MDPlus-win-x64.zip"
        if (Test-Path $zipPath) {
            Remove-Item $zipPath -Force
        }

        $stagingDir = Join-Path $distPath "staging"
        if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
        New-Item -ItemType Directory -Path $stagingDir | Out-Null
        Copy-Item $exePath -Destination $stagingDir
        Copy-Item (Join-Path $PSScriptRoot "README.md") -Destination $stagingDir
        Copy-Item (Join-Path $PSScriptRoot "LICENSE") -Destination $stagingDir
        if (Test-Path (Join-Path $PSScriptRoot "sample_docs")) {
            Copy-Item (Join-Path $PSScriptRoot "sample_docs") -Destination (Join-Path $stagingDir "sample_docs") -Recurse
        }
        Compress-Archive -Path "$stagingDir\*" -DestinationPath $zipPath -Force
        Remove-Item $stagingDir -Recurse -Force

        # 3. Generate cryptographic SHA-256 hashes (Notepad++ integrity standard)
        Write-Host "`n[INFO] Calculating cryptographic SHA-256 hashes..." -ForegroundColor Yellow
        $exeHash = Get-Sha256Hex $exePath
        $zipHash = Get-Sha256Hex $zipPath

        # Write individual hash files
        Set-Content -Path (Join-Path $distPath "MDPlus.exe.sha256") -Value "$exeHash  MDPlus.exe"
        Set-Content -Path (Join-Path $distPath "MDPlus-win-x64.zip.sha256") -Value "$zipHash  MDPlus-win-x64.zip"

        # Write master SHA256SUMS.txt file
        $checksumContent = @"
$exeHash  MDPlus.exe
$zipHash  MDPlus-win-x64.zip
"@
        Set-Content -Path (Join-Path $distPath "SHA256SUMS.txt") -Value $checksumContent

        Write-Host "`n=======================================================" -ForegroundColor Green
        Write-Host " [SUCCESS] Publish & Hash Generation Complete!" -ForegroundColor Green
        Write-Host "=======================================================" -ForegroundColor Green
        Write-Host "Release Artifacts in '$distPath':" -ForegroundColor White
        Write-Host "  • MDPlus.exe" -ForegroundColor Cyan
        Write-Host "    SHA-256: $exeHash" -ForegroundColor DarkCyan
        Write-Host "  • MDPlus-win-x64.zip" -ForegroundColor Cyan
        Write-Host "    SHA-256: $zipHash" -ForegroundColor DarkCyan
        Write-Host "  • SHA256SUMS.txt (Master Checksum Manifest)" -ForegroundColor Gray
        Write-Host ""
        Write-Host "To verify download integrity, run:" -ForegroundColor Gray
        Write-Host "  .\build.ps1 -Action Verify" -ForegroundColor Yellow
        Write-Host "  Get-FileHash dist\MDPlus.exe -Algorithm SHA256" -ForegroundColor Yellow
    }

    "Verify" {
        Write-Host "[INFO] Verifying release download integrity against SHA-256 digests..." -ForegroundColor Yellow
        $checksumFile = Join-Path $distPath "SHA256SUMS.txt"
        if (-not (Test-Path $checksumFile)) {
            Write-Error "Checksum manifest '$checksumFile' not found. Run '.\build.ps1 -Action Publish' first."
            exit 1
        }

        $lines = Get-Content $checksumFile
        $allPassed = $true

        foreach ($line in $lines) {
            $trimmed = $line.Trim()
            if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith("#")) { continue }

            $parts = $trimmed -split '\s+', 2
            if ($parts.Length -eq 2) {
                $expectedHash = $parts[0].ToLowerInvariant()
                $fileName = $parts[1].TrimStart('*')
                $targetFile = Join-Path $distPath $fileName

                if (-not (Test-Path $targetFile)) {
                    Write-Host "  [FAIL] $fileName - File not found!" -ForegroundColor Red
                    $allPassed = $false
                    continue
                }

                $actualHash = Get-Sha256Hex $targetFile
                if ($actualHash -eq $expectedHash) {
                    Write-Host "  [OK]   $fileName - SHA-256 verified!" -ForegroundColor Green
                    Write-Host "         $actualHash" -ForegroundColor DarkGray
                } else {
                    Write-Host "  [FAIL] $fileName - CHECKSUM MISMATCH!" -ForegroundColor Red
                    Write-Host "         Expected: $expectedHash" -ForegroundColor Yellow
                    Write-Host "         Actual:   $actualHash" -ForegroundColor Red
                    $allPassed = $false
                }
            }
        }

        if ($allPassed) {
            Write-Host "`n[VERIFIED] All release downloads are intact and authenticated!" -ForegroundColor Green
        } else {
            Write-Error "`n[SECURITY ALERT] One or more release artifacts failed SHA-256 verification!"
            exit 1
        }
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
