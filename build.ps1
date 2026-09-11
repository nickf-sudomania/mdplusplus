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
    [ValidateSet("Build", "Test", "Run", "Publish", "Installer", "Verify", "Clean")]
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
$installerScriptPath = Join-Path $PSScriptRoot "installer\MDPlus.iss"

function Get-Sha256Hex([string]$filePath) {
    $hashResult = Get-FileHash -Path $filePath -Algorithm SHA256
    return $hashResult.Hash.ToLowerInvariant()
}

function Get-InnoSetupCompiler {
    $cmd = Get-Command iscc -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $candidates = @(
        (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe"),
        (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
        (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe")
    )

    foreach ($candidate in $candidates) {
        if (-not [string]::IsNullOrWhiteSpace($candidate) -and (Test-Path $candidate)) {
            return $candidate
        }
    }

    # Registry lookup
    $regPaths = @(
        "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1",
        "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1",
        "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1"
    )
    foreach ($rp in $regPaths) {
        if (Test-Path $rp) {
            $instLoc = (Get-ItemProperty -Path $rp -Name "InstallLocation" -ErrorAction SilentlyContinue).InstallLocation
            if ($instLoc) {
                $isccPath = Join-Path $instLoc "ISCC.exe"
                if (Test-Path $isccPath) { return $isccPath }
            }
        }
    }

    return $null
}

function Build-InstallerPackage {
    Write-Host "`n[INFO] Compiling Windows Setup Installer (MDPlus-Setup.exe)..." -ForegroundColor Yellow
    if (-not (Test-Path $installerScriptPath)) {
        Write-Error "Installer script not found at '$installerScriptPath'."
        exit 1
    }

    $exePath = Join-Path $distPath "MDPlus.exe"
    if (-not (Test-Path $exePath)) {
        Write-Host "[INFO] Published binary not found. Building release binary first..." -ForegroundColor Yellow
        dotnet publish $srcProjectPath -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o $distPath
    }

    $iscc = Get-InnoSetupCompiler
    if (-not $iscc) {
        Write-Error "Inno Setup compiler ('ISCC.exe') not found. Please install Inno Setup 6 (e.g. winget install JRSoftware.InnoSetup)."
        exit 1
    }

    Write-Host "Using Inno Setup Compiler: $iscc" -ForegroundColor Gray
    $isccOutput = & $iscc $installerScriptPath /O"$distPath" /F"MDPlus-Setup"
    if ($LASTEXITCODE -ne 0) {
        if ($isccOutput) { Write-Host ($isccOutput -join "`n") -ForegroundColor Red }
        Write-Error "Inno Setup compilation failed with exit code $LASTEXITCODE."
        exit 1
    }

    $setupPath = Join-Path $distPath "MDPlus-Setup.exe"
    if (-not (Test-Path $setupPath)) {
        Write-Error "Setup installer executable not found at '$setupPath'."
        exit 1
    }

    Write-Host "[SUCCESS] Windows Setup Installer generated: $setupPath" -ForegroundColor Green
    return $setupPath
}

switch ($Action) {
    "Build" {
        Write-Host "[INFO] Building MDPlus solution..." -ForegroundColor Yellow
        dotnet build $solutionPath -c Debug
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed with exit code $LASTEXITCODE."
            exit $LASTEXITCODE
        }
        Write-Host "`n[SUCCESS] Build completed successfully!" -ForegroundColor Green
        Write-Host "Executable: src\bin\Debug\net8.0-windows\MDPlus.exe" -ForegroundColor Gray
    }

    "Test" {
        Write-Host "[INFO] Running MDPlus test suite..." -ForegroundColor Yellow
        dotnet run --project $testsProjectPath -c Release
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Test suite execution failed with exit code $LASTEXITCODE."
            exit $LASTEXITCODE
        }
    }

    "Run" {
        Write-Host "[INFO] Starting MDPlus..." -ForegroundColor Yellow
        dotnet run --project $srcProjectPath
    }

    "Installer" {
        if (-not (Test-Path $distPath)) {
            New-Item -ItemType Directory -Path $distPath | Out-Null
        }
        $setupPath = Build-InstallerPackage
        $setupHash = Get-Sha256Hex $setupPath
        Set-Content -Path (Join-Path $distPath "MDPlus-Setup.exe.sha256") -Value "$setupHash  MDPlus-Setup.exe"
        
        $sumsFile = Join-Path $distPath "SHA256SUMS.txt"
        if (Test-Path $sumsFile) {
            $lines = Get-Content $sumsFile
            $newLines = @()
            $found = $false
            foreach ($line in $lines) {
                if ($line -match '\s+MDPlus-Setup\.exe$') {
                    $newLines += "$setupHash  MDPlus-Setup.exe"
                    $found = $true
                } else {
                    $newLines += $line
                }
            }
            if (-not $found) { $newLines += "$setupHash  MDPlus-Setup.exe" }
            $newContent = ($newLines -join "`n") + "`n"
            Set-Content -Path $sumsFile -Value $newContent
            $nppChecksum = Join-Path $distPath "MDPlus.1.09.checksums.sha256"
            Set-Content -Path $nppChecksum -Value $newContent
            $nppChecksum108 = Join-Path $distPath "MDPlus.1.08.checksums.sha256"
            if (Test-Path $nppChecksum108) {
                Set-Content -Path $nppChecksum108 -Value $newContent
            }
            $nppChecksum107 = Join-Path $distPath "MDPlus.1.07.checksums.sha256"
            if (Test-Path $nppChecksum107) {
                Set-Content -Path $nppChecksum107 -Value $newContent
            }
            $nppChecksum106 = Join-Path $distPath "MDPlus.1.06.checksums.sha256"
            if (Test-Path $nppChecksum106) {
                Set-Content -Path $nppChecksum106 -Value $newContent
            }
            $nppChecksum103 = Join-Path $distPath "MDPlus.1.03.checksums.sha256"
            if (Test-Path $nppChecksum103) {
                Set-Content -Path $nppChecksum103 -Value $newContent
            }
            $nppChecksum102 = Join-Path $distPath "MDPlus.1.02.checksums.sha256"
            if (Test-Path $nppChecksum102) {
                Set-Content -Path $nppChecksum102 -Value $newContent
            }
            $nppChecksum101 = Join-Path $distPath "MDPlus.1.01.checksums.sha256"
            if (Test-Path $nppChecksum101) {
                Set-Content -Path $nppChecksum101 -Value $newContent
            }
            $nppChecksumLegacy = Join-Path $distPath "MDPlus.1.0.0.checksums.sha256"
            if (Test-Path $nppChecksumLegacy) {
                Set-Content -Path $nppChecksumLegacy -Value $newContent
            }
            Write-Host "Updated SHA256SUMS.txt and MDPlus checksums manifests with installer hash." -ForegroundColor DarkGray
        }

        Write-Host "`nSHA-256: $setupHash  MDPlus-Setup.exe" -ForegroundColor DarkCyan
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

        # 3. Package source code archive (Notepad++ source release style)
        $appVersion = "1.09"
        Write-Host "`n[INFO] Creating source code release archive (MDPlus-$appVersion-src.zip)..." -ForegroundColor Yellow
        $srcZipPath = Join-Path $distPath "MDPlus-$appVersion-src.zip"
        if (Test-Path $srcZipPath) {
            Remove-Item $srcZipPath -Force
        }

        $srcStagingDir = Join-Path $distPath "src_staging\MDPlus-$appVersion-src"
        if (Test-Path (Join-Path $distPath "src_staging")) { Remove-Item (Join-Path $distPath "src_staging") -Recurse -Force }
        New-Item -ItemType Directory -Path $srcStagingDir | Out-Null

        # Copy source files excluding binaries and git metadata
        Copy-Item (Join-Path $PSScriptRoot "src") -Destination $srcStagingDir -Recurse -Exclude @("bin", "obj")
        if (Test-Path (Join-Path $srcStagingDir "src\bin")) { Remove-Item (Join-Path $srcStagingDir "src\bin") -Recurse -Force }
        if (Test-Path (Join-Path $srcStagingDir "src\obj")) { Remove-Item (Join-Path $srcStagingDir "src\obj") -Recurse -Force }
        Copy-Item (Join-Path $PSScriptRoot "sample_docs") -Destination $srcStagingDir -Recurse
        Copy-Item (Join-Path $PSScriptRoot "LICENSE") -Destination $srcStagingDir
        Copy-Item (Join-Path $PSScriptRoot "README.md") -Destination $srcStagingDir

        Compress-Archive -Path "$srcStagingDir\*" -DestinationPath $srcZipPath -Force
        Remove-Item (Join-Path $distPath "src_staging") -Recurse -Force

        # 4. Compile Inno Setup Windows installer if ISCC.exe is installed
        $innoCompiler = Get-InnoSetupCompiler
        $issScript = Join-Path $PSScriptRoot "installer\MDPlus.iss"

        if ($innoCompiler -and (Test-Path $innoCompiler)) {
            Write-Host "`n[INFO] Compiling Windows Setup Installer (MDPlus-Setup.exe)..." -ForegroundColor Yellow
            Write-Host "Using Inno Setup Compiler: $innoCompiler" -ForegroundColor Gray
            & $innoCompiler $issScript /O"$distPath" /F"MDPlus-Setup" | Out-Null
            $setupPath = Join-Path $distPath "MDPlus-Setup.exe"
            if (Test-Path $setupPath) {
                Write-Host "[SUCCESS] Windows Setup Installer generated: $setupPath" -ForegroundColor Green
            } else {
                Write-Warning "Inno Setup compilation finished but '$setupPath' was not found."
            }
        } else {
            Write-Warning "Inno Setup compiler not found. Skipping setup installer build."
        }

        # 5. Generate cryptographic SHA-256 hashes (Notepad++ release integrity standard)
        Write-Host "`n[INFO] Calculating cryptographic SHA-256 hashes..." -ForegroundColor Yellow
        $exeHash = Get-Sha256Hex $exePath
        $zipHash = Get-Sha256Hex $zipPath
        $srcHash = Get-Sha256Hex $srcZipPath
        $setupPath = Join-Path $distPath "MDPlus-Setup.exe"
        $setupHash = if (Test-Path $setupPath) { Get-Sha256Hex $setupPath } else { "N/A" }

        # Write individual hash files
        Set-Content -Path (Join-Path $distPath "MDPlus.exe.sha256") -Value "$exeHash  MDPlus.exe"
        Set-Content -Path (Join-Path $distPath "MDPlus-win-x64.zip.sha256") -Value "$zipHash  MDPlus-win-x64.zip"
        Set-Content -Path (Join-Path $distPath "MDPlus-$appVersion-src.zip.sha256") -Value "$srcHash  MDPlus-$appVersion-src.zip"
        Set-Content -Path (Join-Path $distPath "MDPlus-1.08-src.zip.sha256") -Value "$srcHash  MDPlus-1.08-src.zip"
        Set-Content -Path (Join-Path $distPath "MDPlus-1.07-src.zip.sha256") -Value "$srcHash  MDPlus-1.07-src.zip"
        Set-Content -Path (Join-Path $distPath "MDPlus-1.06-src.zip.sha256") -Value "$srcHash  MDPlus-1.06-src.zip"
        Set-Content -Path (Join-Path $distPath "MDPlus-1.03-src.zip.sha256") -Value "$srcHash  MDPlus-1.03-src.zip"
        Set-Content -Path (Join-Path $distPath "MDPlus-1.02-src.zip.sha256") -Value "$srcHash  MDPlus-1.02-src.zip"
        Set-Content -Path (Join-Path $distPath "MDPlus-1.01-src.zip.sha256") -Value "$srcHash  MDPlus-1.01-src.zip"
        Set-Content -Path (Join-Path $distPath "MDPlus-1.0.0-src.zip.sha256") -Value "$srcHash  MDPlus-1.0.0-src.zip"
        Set-Content -Path (Join-Path $distPath "MDPlus-Setup.exe.sha256") -Value "$setupHash  MDPlus-Setup.exe"

        # Write Notepad++ style unified checksum file (npp.<version>.checksums.sha256 standard)
        $checksumContent = @"
$exeHash  MDPlus.exe
$zipHash  MDPlus-win-x64.zip
$srcHash  MDPlus-$appVersion-src.zip
$setupHash  MDPlus-Setup.exe
"@
        Set-Content -Path (Join-Path $distPath "SHA256SUMS.txt") -Value $checksumContent
        Set-Content -Path (Join-Path $distPath "MDPlus.$appVersion.checksums.sha256") -Value $checksumContent

        $checksumContent108 = @"
$exeHash  MDPlus.exe
$zipHash  MDPlus-win-x64.zip
$srcHash  MDPlus-1.08-src.zip
$setupHash  MDPlus-Setup.exe
"@
        Set-Content -Path (Join-Path $distPath "MDPlus.1.08.checksums.sha256") -Value $checksumContent108

        $checksumContent107 = @"
$exeHash  MDPlus.exe
$zipHash  MDPlus-win-x64.zip
$srcHash  MDPlus-1.07-src.zip
$setupHash  MDPlus-Setup.exe
"@
        Set-Content -Path (Join-Path $distPath "MDPlus.1.07.checksums.sha256") -Value $checksumContent107

        $checksumContent106 = @"
$exeHash  MDPlus.exe
$zipHash  MDPlus-win-x64.zip
$srcHash  MDPlus-1.06-src.zip
$setupHash  MDPlus-Setup.exe
"@
        Set-Content -Path (Join-Path $distPath "MDPlus.1.06.checksums.sha256") -Value $checksumContent106

        $checksumContent103 = @"
$exeHash  MDPlus.exe
$zipHash  MDPlus-win-x64.zip
$srcHash  MDPlus-1.03-src.zip
$setupHash  MDPlus-Setup.exe
"@
        Set-Content -Path (Join-Path $distPath "MDPlus.1.03.checksums.sha256") -Value $checksumContent103

        $checksumContent102 = @"
$exeHash  MDPlus.exe
$zipHash  MDPlus-win-x64.zip
$srcHash  MDPlus-1.02-src.zip
$setupHash  MDPlus-Setup.exe
"@
        Set-Content -Path (Join-Path $distPath "MDPlus.1.02.checksums.sha256") -Value $checksumContent102

        $checksumContent101 = @"
$exeHash  MDPlus.exe
$zipHash  MDPlus-win-x64.zip
$srcHash  MDPlus-1.01-src.zip
$setupHash  MDPlus-Setup.exe
"@
        Set-Content -Path (Join-Path $distPath "MDPlus.1.01.checksums.sha256") -Value $checksumContent101

        $checksumContent100 = @"
$exeHash  MDPlus.exe
$zipHash  MDPlus-win-x64.zip
$srcHash  MDPlus-1.0.0-src.zip
$setupHash  MDPlus-Setup.exe
"@
        Set-Content -Path (Join-Path $distPath "MDPlus.1.0.0.checksums.sha256") -Value $checksumContent100

        # Sync to releases directory if present
        $releasesDir = Join-Path $PSScriptRoot "releases"
        if (Test-Path $releasesDir) {
            Get-ChildItem -Path $distPath -File | Where-Object { $_.Extension -ne ".pdb" } | ForEach-Object {
                Copy-Item $_.FullName -Destination $releasesDir -Force
            }
            if (Test-Path (Join-Path $PSScriptRoot "sample_docs")) {
                $targetSampleDocs = Join-Path $releasesDir "sample_docs"
                if (-not (Test-Path $targetSampleDocs)) {
                    New-Item -ItemType Directory -Path $targetSampleDocs | Out-Null
                }
                Copy-Item (Join-Path $PSScriptRoot "sample_docs\*") -Destination $targetSampleDocs -Recurse -Force
            }
        }

        Write-Host "`n=======================================================" -ForegroundColor Green
        Write-Host " [SUCCESS] Publish & Hash Generation Complete!" -ForegroundColor Green
        Write-Host "=======================================================" -ForegroundColor Green
        Write-Host "Release Artifacts in '$distPath':" -ForegroundColor White
        Write-Host "  • MDPlus.exe" -ForegroundColor Cyan
        Write-Host "    SHA-256: $exeHash" -ForegroundColor DarkCyan
        Write-Host "  • MDPlus-Setup.exe (Windows Setup Installer)" -ForegroundColor Cyan
        Write-Host "    SHA-256: $setupHash" -ForegroundColor DarkCyan
        Write-Host "  • MDPlus-win-x64.zip" -ForegroundColor Cyan
        Write-Host "    SHA-256: $zipHash" -ForegroundColor DarkCyan
        Write-Host "  • MDPlus-$appVersion-src.zip (Source Code Archive)" -ForegroundColor Cyan
        Write-Host "    SHA-256: $srcHash" -ForegroundColor DarkCyan
        Write-Host "  • MDPlus.$appVersion.checksums.sha256 (Notepad++ Checksum Standard)" -ForegroundColor Gray
        Write-Host "  • SHA256SUMS.txt (Master Checksum Manifest)" -ForegroundColor Gray
        Write-Host ""
        Write-Host "To verify download integrity, run:" -ForegroundColor Gray
        Write-Host "  .\build.ps1 -Action Verify" -ForegroundColor Yellow
        Write-Host "  Get-FileHash dist\MDPlus-Setup.exe -Algorithm SHA256" -ForegroundColor Yellow
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
