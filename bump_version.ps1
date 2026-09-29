$files = Get-ChildItem -Path . -Recurse -Include "*.md","*.ps1","*.iss","*.cs","*.csproj","*.xaml" -Exclude "bin","obj",".git","dist","releases"

foreach ($f in $files) {
    if ($f.FullName -match "\\bin\\" -or $f.FullName -match "\\obj\\" -or $f.FullName -match "\\.git\\" -or $f.FullName -match "\\releases\\") { continue }
    $content = Get-Content $f.FullName -Raw
    if ($content -match "1\.14\.4") {
        # special handling for build.ps1
        if ($f.Name -eq "build.ps1") {
            # Add 1.14.5 keeping 1.14.5
            $content = $content -replace "`$nppChecksum1144 = Join-Path `$distPath `"MDPlus.1.14.5.checksums.sha256`"\r?\n\s*Set-Content -Path `$nppChecksum1144 -Value `$newContent", "`$nppChecksum1145 = Join-Path `$distPath `"MDPlus.1.14.5.checksums.sha256`"`r`n            Set-Content -Path `$nppChecksum1145 -Value `$newContent`r`n            `$nppChecksum1144 = Join-Path `$distPath `"MDPlus.1.14.5.checksums.sha256`"`r`n            Set-Content -Path `$nppChecksum1144 -Value `$newContent"
            $content = $content -replace "`$appVersion = `"1.14.5`"", "`$appVersion = `"1.14.5`""
            $content = $content -replace "Set-Content -Path \(Join-Path `$distPath `"MDPlus-1.14.5-src.zip.sha256`"\) -Value `"`$srcHash  MDPlus-1.14.5-src.zip`"", "Set-Content -Path (Join-Path `$distPath `"MDPlus-1.14.5-src.zip.sha256`") -Value `"`$srcHash  MDPlus-1.14.5-src.zip`"`r`n        Set-Content -Path (Join-Path `$distPath `"MDPlus-1.14.5-src.zip.sha256`") -Value `"`$srcHash  MDPlus-1.14.5-src.zip`""
            $content = $content -replace "Set-Content -Path \(Join-Path `$distPath `"MDPlus.1.14.5.checksums.sha256`"\) -Value `$checksumContent", "Set-Content -Path (Join-Path `$distPath `"MDPlus.1.14.5.checksums.sha256`") -Value `$checksumContent`r`n        Set-Content -Path (Join-Path `$distPath `"MDPlus.1.14.5.checksums.sha256`") -Value `$checksumContent"
            Set-Content -Path $f.FullName -Value $content -NoNewline
        }
        else {
            $newContent = $content -replace "1\.14\.4", "1.14.5"
            Set-Content -Path $f.FullName -Value $newContent -NoNewline
        }
    }
}
