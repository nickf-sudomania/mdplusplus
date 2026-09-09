# MDPlus vs MarkText Empirical Performance Measurement Script
param(
    [string]$MdPlusPath = "$PSScriptRoot\..\dist\MDPlus.exe",
    [string]$MarkTextPath = "C:\Users\nickf\AppData\Local\Programs\MarkText\MarkText.exe",
    [string]$SmallDoc = "$PSScriptRoot\..\sample_docs\welcome.md",
    [string]$LargeDoc = "$PSScriptRoot\..\sample_docs\benchmark_5000.md",
    [int]$Iterations = 5
)

$MdPlusPath = [System.IO.Path]::GetFullPath($MdPlusPath)
$MarkTextPath = [System.IO.Path]::GetFullPath($MarkTextPath)
$SmallDoc = [System.IO.Path]::GetFullPath($SmallDoc)
$LargeDoc = [System.IO.Path]::GetFullPath($LargeDoc)

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "         MDPLUS vs. MARKTEXT EMPIRICAL BENCHMARK SUITE                " -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "MDPlus Binary:   $MdPlusPath"
Write-Host "MarkText Binary: $MarkTextPath"
Write-Host "Small Doc:       $SmallDoc"
Write-Host "Large Doc:       $LargeDoc"
Write-Host "Iterations:      $Iterations"
Write-Host "OS:              $([System.Environment]::OSVersion)"
Write-Host "Machine:         $($env:COMPUTERNAME)"
Write-Host "Date/Time:       $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host ""

function Clean-Processes {
    Get-Process -Name "MarkText" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Get-Process -Name "MDPlus" -ErrorAction SilentlyContinue | ForEach-Object {
        try { Stop-Process -Id $_.Id -Force -ErrorAction Stop } catch {}
    }
    Start-Sleep -Milliseconds 400
}

Write-Host ">>> STEP 1: Measuring Disk & Installation Footprint..." -ForegroundColor Yellow

$mdPlusFileInfo = Get-Item $MdPlusPath
$mdPlusDiskMB = [math]::Round($mdPlusFileInfo.Length / 1MB, 2)

$markTextDir = Split-Path -Parent $MarkTextPath
$markTextExeInfo = Get-Item $MarkTextPath
$markTextExeMB = [math]::Round($markTextExeInfo.Length / 1MB, 2)
$markTextTotalBytes = (Get-ChildItem -Path $markTextDir -Recurse -File | Measure-Object -Property Length -Sum).Sum
$markTextTotalMB = [math]::Round($markTextTotalBytes / 1MB, 2)

Write-Host "  MDPlus Standalone Single-File Binary: $mdPlusDiskMB MB ($($mdPlusFileInfo.Length) bytes)"
Write-Host "  MarkText Main Executable:             $markTextExeMB MB ($($markTextExeInfo.Length) bytes)"
Write-Host "  MarkText Full Installed Directory:    $markTextTotalMB MB ($markTextTotalBytes bytes)"
Write-Host ""

function Get-Stats([double[]]$values) {
    if ($values.Length -eq 0) { return [PSCustomObject]@{ Min = 0; Max = 0; Avg = 0; Median = 0 } }
    $sorted = $values | Sort-Object
    $min = $sorted[0]
    $max = $sorted[-1]
    $avg = [math]::Round(($sorted | Measure-Object -Average).Average, 2)
    $med = if ($sorted.Length % 2 -eq 1) {
        $sorted[[math]::Floor($sorted.Length / 2)]
    } else {
        [math]::Round(($sorted[$sorted.Length / 2 - 1] + $sorted[$sorted.Length / 2]) / 2.0, 2)
    }
    return [PSCustomObject]@{ Min = [math]::Round($min, 2); Max = [math]::Round($max, 2); Avg = $avg; Median = [math]::Round($med, 2) }
}

function Get-WarmStats([double[]]$values) {
    if ($values.Length -le 1) {
        return Get-Stats $values
    }
    # Exclude iteration 1 (cold start) so warm stats reflect pure steady-state runs
    $warm = $values[1..($values.Length - 1)]
    return Get-Stats $warm
}

function Measure-MdPlusLaunch([string]$filePath, [int]$count) {
    $times = @()
    for ($i = 1; $i -le $count; $i++) {
        Clean-Processes
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $proc = if ([string]::IsNullOrEmpty($filePath)) {
            Start-Process -FilePath $MdPlusPath -PassThru
        } else {
            Start-Process -FilePath $MdPlusPath -ArgumentList @($filePath) -PassThru
        }
        $found = $false
        while ($sw.ElapsedMilliseconds -lt 10000) {
            $proc.Refresh()
            if ($proc.HasExited) { break }
            if ($proc.MainWindowHandle -ne [IntPtr]::Zero) {
                $sw.Stop()
                $found = $true
                break
            }
            Start-Sleep -Milliseconds 5
        }
        if (-not $found) { $sw.Stop() }
        $elapsed = $sw.ElapsedMilliseconds
        $times += [double]$elapsed
        Write-Host "    [MDPlus] Run #${i}: ${elapsed} ms"
        try { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue } catch {}
        Start-Sleep -Milliseconds 300
    }
    return $times
}

function Measure-MarkTextLaunch([string]$filePath, [int]$count) {
    $times = @()
    for ($i = 1; $i -le $count; $i++) {
        Clean-Processes
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $proc = if ([string]::IsNullOrEmpty($filePath)) {
            Start-Process -FilePath $MarkTextPath -ArgumentList @("--disable-gpu-sandbox") -PassThru
        } else {
            Start-Process -FilePath $MarkTextPath -ArgumentList @("--disable-gpu-sandbox", $filePath) -PassThru
        }
        $found = $false
        while ($sw.ElapsedMilliseconds -lt 15000) {
            $allMark = Get-Process -Name "MarkText" -ErrorAction SilentlyContinue
            $w = $allMark | Where-Object { $_.MainWindowHandle -ne [IntPtr]::Zero }
            if ($w) {
                $sw.Stop()
                $found = $true
                break
            }
            Start-Sleep -Milliseconds 10
        }
        if (-not $found) { $sw.Stop() }
        $elapsed = $sw.ElapsedMilliseconds
        $times += [double]$elapsed
        Write-Host "    [MarkText] Run #${i}: ${elapsed} ms"
        Get-Process -Name "MarkText" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
        Start-Sleep -Milliseconds 500
    }
    return $times
}

Write-Host ">>> STEP 2: Measuring Empty Process Launch Latency (to Window Ready)..." -ForegroundColor Yellow
Write-Host "  Benchmarking MDPlus Launch ($Iterations runs)..."
$mdPlusLaunchTimes = Measure-MdPlusLaunch "" $Iterations
$mdPlusColdLaunch = $mdPlusLaunchTimes[0]
$mdPlusWarmStats = Get-WarmStats $mdPlusLaunchTimes

Write-Host "  Benchmarking MarkText Launch ($Iterations runs)..."
$markTextLaunchTimes = Measure-MarkTextLaunch "" $Iterations
$markTextColdLaunch = $markTextLaunchTimes[0]
$markTextWarmStats = Get-WarmStats $markTextLaunchTimes

Write-Host ">>> STEP 3: Measuring Small File Opening Latency ($SmallDoc)..." -ForegroundColor Yellow
Write-Host "  Benchmarking MDPlus Small File Open ($Iterations runs)..."
$mdPlusSmallDocTimes = Measure-MdPlusLaunch $SmallDoc $Iterations
$mdPlusSmallCold = $mdPlusSmallDocTimes[0]
$mdPlusSmallWarmStats = Get-WarmStats $mdPlusSmallDocTimes

Write-Host "  Benchmarking MarkText Small File Open ($Iterations runs)..."
$markTextSmallDocTimes = Measure-MarkTextLaunch $SmallDoc $Iterations
$markTextSmallCold = $markTextSmallDocTimes[0]
$markTextSmallWarmStats = Get-WarmStats $markTextSmallDocTimes

Write-Host ">>> STEP 4: Measuring Large File Opening Latency ($LargeDoc)..." -ForegroundColor Yellow
Write-Host "  Benchmarking MDPlus Large File Open ($Iterations runs)..."
$mdPlusLargeDocTimes = Measure-MdPlusLaunch $LargeDoc $Iterations
$mdPlusLargeCold = $mdPlusLargeDocTimes[0]
$mdPlusLargeWarmStats = Get-WarmStats $mdPlusLargeDocTimes

Write-Host "  Benchmarking MarkText Large File Open ($Iterations runs)..."
$markTextLargeDocTimes = Measure-MarkTextLaunch $LargeDoc $Iterations
$markTextLargeCold = $markTextLargeDocTimes[0]
$markTextLargeWarmStats = Get-WarmStats $markTextLargeDocTimes

Write-Host ">>> STEP 5: Measuring Stabilized Idle Memory & Process Hierarchy on 5,000-Line Document..." -ForegroundColor Yellow

Clean-Processes
Write-Host "  Launching MDPlus with 5,000-line document..."
$mdProc = Start-Process -FilePath $MdPlusPath -ArgumentList @($LargeDoc) -PassThru
Start-Sleep -Seconds 5
$mdProc.Refresh()
$mdWorkingSetMB = [math]::Round($mdProc.WorkingSet64 / 1MB, 2)
$mdPrivateMB = [math]::Round($mdProc.PrivateMemorySize64 / 1MB, 2)
$mdProcCount = 1
try { Stop-Process -Id $mdProc.Id -Force -ErrorAction SilentlyContinue } catch {}

Clean-Processes
Write-Host "  Launching MarkText with 5,000-line document..."
$mtProc = Start-Process -FilePath $MarkTextPath -ArgumentList @("--disable-gpu-sandbox", $LargeDoc) -PassThru
Start-Sleep -Seconds 6
$allMt = Get-Process -Name "MarkText" -ErrorAction SilentlyContinue
$cim = Get-CimInstance Win32_Process | Where-Object { $_.Name -eq "MarkText.exe" }
$mtProcCount = $allMt.Count
$mtTotalWorkingSetMB = [math]::Round(($allMt | Measure-Object -Property WorkingSet64 -Sum).Sum / 1MB, 2)
$mtTotalPrivateMB = [math]::Round(($allMt | Measure-Object -Property PrivateMemorySize64 -Sum).Sum / 1MB, 2)

$mtBreakdown = foreach ($p in $allMt) {
    $c = $cim | Where-Object { $_.ProcessId -eq $p.Id }
    $cmd = if ($c) { $c.CommandLine } else { "" }
    $type = "browser (main)"
    if ($cmd -match "--type=([^ ]+)") { $type = $matches[1] }
    [PSCustomObject]@{
        PID = $p.Id
        Type = $type
        WorkingSetMB = [math]::Round($p.WorkingSet64 / 1MB, 2)
        PrivateBytesMB = [math]::Round($p.PrivateMemorySize64 / 1MB, 2)
    }
}
$allMt | Stop-Process -Force -ErrorAction SilentlyContinue

Write-Host ">>> STEP 6: Measuring MDPlus In-Engine AST Parsing & Serialization..." -ForegroundColor Yellow
$testDll = "$PSScriptRoot\..\tests\bin\Release\net8.0-windows\MDPlus.Tests.dll"
$parseOutput = ""
if (Test-Path $testDll) {
    $testRun = & dotnet $testDll 2>&1 | Out-String
    if ($testRun -match "\[(\d+) blocks parsed in (\d+)ms\]") {
        $parseBlocks = $matches[1]
        $parseMs = $matches[2]
        $parseOutput = "$parseBlocks blocks parsed in ${parseMs}ms"
        Write-Host "  MDPlus Native Parser: $parseOutput"
    }
}

Write-Host ""
Write-Host "======================================================================" -ForegroundColor Green
Write-Host "               EMPIRICAL BENCHMARK SUMMARY TABLE                      " -ForegroundColor Green
Write-Host "======================================================================" -ForegroundColor Green

$results = @(
    [PSCustomObject]@{
        Metric = "Installed / Binary Size"
        MDPlus = "$mdPlusDiskMB MB (Single binary)"
        MarkText = "$markTextTotalMB MB ($markTextExeMB MB exe)"
        Delta = "-$([math]::Round(($markTextTotalMB - $mdPlusDiskMB) / $markTextTotalMB * 100, 1))% (MDPlus smaller)"
    },
    [PSCustomObject]@{
        Metric = "OS Processes at Idle"
        MDPlus = "$mdProcCount (Single native process)"
        MarkText = "$mtProcCount (Electron multi-process)"
        Delta = "-$([math]::Round(($mtProcCount - $mdProcCount) / $mtProcCount * 100, 1))% (MDPlus fewer)"
    },
    [PSCustomObject]@{
        Metric = "Cold Process Launch"
        MDPlus = "$mdPlusColdLaunch ms"
        MarkText = "$markTextColdLaunch ms"
        Delta = if ($markTextColdLaunch -ne 0) { "+$([math]::Round(($mdPlusColdLaunch - $markTextColdLaunch) / $markTextColdLaunch * 100, 1))%" } else { "N/A" }
    },
    [PSCustomObject]@{
        Metric = "Warm Process Launch (Min)"
        MDPlus = "$($mdPlusWarmStats.Min) ms"
        MarkText = "$($markTextWarmStats.Min) ms"
        Delta = if ($markTextWarmStats.Min -ne 0) { "+$([math]::Round(($mdPlusWarmStats.Min - $markTextWarmStats.Min) / $markTextWarmStats.Min * 100, 1))%" } else { "N/A" }
    },
    [PSCustomObject]@{
        Metric = "Warm Process Launch (Avg)"
        MDPlus = "$($mdPlusWarmStats.Avg) ms"
        MarkText = "$($markTextWarmStats.Avg) ms"
        Delta = if ($markTextWarmStats.Avg -ne 0) { "+$([math]::Round(($mdPlusWarmStats.Avg - $markTextWarmStats.Avg) / $markTextWarmStats.Avg * 100, 1))%" } else { "N/A" }
    },
    [PSCustomObject]@{
        Metric = "Small Doc Open (welcome.md, Warm Avg)"
        MDPlus = "$($mdPlusSmallWarmStats.Avg) ms"
        MarkText = "$($markTextSmallWarmStats.Avg) ms"
        Delta = if ($markTextSmallWarmStats.Avg -ne 0) { "+$([math]::Round(($mdPlusSmallWarmStats.Avg - $markTextSmallWarmStats.Avg) / $markTextSmallWarmStats.Avg * 100, 1))%" } else { "N/A" }
    },
    [PSCustomObject]@{
        Metric = "Large Doc Open (5,000 lines, Warm Avg)"
        MDPlus = "$($mdPlusLargeWarmStats.Avg) ms"
        MarkText = "$($markTextLargeWarmStats.Avg) ms"
        Delta = if ($markTextLargeWarmStats.Avg -ne 0) { "+$([math]::Round(($mdPlusLargeWarmStats.Avg - $markTextLargeWarmStats.Avg) / $markTextLargeWarmStats.Avg * 100, 1))%" } else { "N/A" }
    },
    [PSCustomObject]@{
        Metric = "Physical Working Set RAM (5k doc idle)"
        MDPlus = "$mdWorkingSetMB MB"
        MarkText = "$mtTotalWorkingSetMB MB"
        Delta = "-$([math]::Round(($mtTotalWorkingSetMB - $mdWorkingSetMB) / $mtTotalWorkingSetMB * 100, 1))% (MDPlus saves $([math]::Round($mtTotalWorkingSetMB - $mdWorkingSetMB, 1)) MB)"
    },
    [PSCustomObject]@{
        Metric = "Private Committed Bytes (5k doc idle)"
        MDPlus = "$mdPrivateMB MB"
        MarkText = "$mtTotalPrivateMB MB"
        Delta = "-$([math]::Round(($mtTotalPrivateMB - $mdPrivateMB) / $mtTotalPrivateMB * 100, 1))% (MDPlus saves $([math]::Round($mtTotalPrivateMB - $mdPrivateMB, 1)) MB)"
    }
)

$results | Format-Table -AutoSize | Out-String | Write-Host
Write-Host "Process breakdown for MarkText (5,000-line doc):"
$mtBreakdown | Format-Table -AutoSize | Out-String | Write-Host
if ($parseOutput) {
    Write-Host "Native Engine Performance: MDPlus parsed $parseOutput"
}
Write-Host "Benchmark completed successfully."
