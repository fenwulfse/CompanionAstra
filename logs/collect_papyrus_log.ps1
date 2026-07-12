# Collects the Fallout 4 Papyrus log after a test session, filters the
# Astra-related lines into logs/, and (with -Push) commits and pushes them
# so a Claude cloud session can read the results.
#
# Run this AFTER quitting Fallout 4 and BEFORE launching it again --
# the game rotates Papyrus.0.log -> Papyrus.1.log on every launch.

param(
    [switch]$Push
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$logsDir  = Join-Path $repoRoot 'logs'
$rawDir   = Join-Path $logsDir 'raw'
New-Item -ItemType Directory -Force -Path $rawDir | Out-Null

$docs = [Environment]::GetFolderPath('MyDocuments')

# --- 1. Make sure Papyrus logging is enabled in Fallout4Custom.ini ---------
$iniPath = Join-Path $docs 'My Games\Fallout4\Fallout4Custom.ini'
$managedKeys = 'bEnableLogging', 'bEnableTrace', 'bLoadDebugInformation'

if (-not (Test-Path $iniPath)) {
    New-Item -ItemType File -Force -Path $iniPath | Out-Null
}
$iniRaw = Get-Content $iniPath -Raw -ErrorAction SilentlyContinue
if ($null -eq $iniRaw) { $iniRaw = '' }

$needsUpdate = $false
foreach ($k in $managedKeys) {
    if ($iniRaw -notmatch "(?m)^\s*$k\s*=\s*1\s*$") { $needsUpdate = $true }
}

if ($needsUpdate) {
    Copy-Item $iniPath "$iniPath.bak" -Force
    # Drop any existing values for the keys we manage, then append a
    # [Papyrus] block at the end (last value wins in Bethesda INIs).
    $lines = @(Get-Content $iniPath | Where-Object {
        $_ -notmatch '^\s*(bEnableLogging|bEnableTrace|bLoadDebugInformation)\s*='
    })
    $lines += '', '[Papyrus]'
    foreach ($k in $managedKeys) { $lines += "$k=1" }
    Set-Content -Path $iniPath -Value $lines
    Write-Host "Enabled Papyrus logging in Fallout4Custom.ini (backup saved as Fallout4Custom.ini.bak)."
    Write-Host "If Fallout 4 is running, restart it -- logging takes effect on next launch."
}

# --- 2. Grab the current Papyrus log ---------------------------------------
$logPath = Join-Path $docs 'My Games\Fallout4\Logs\Script\Papyrus.0.log'
if (-not (Test-Path $logPath)) {
    Write-Host "No Papyrus log found at:"
    Write-Host "  $logPath"
    Write-Host "Logging was just enabled (or the game hasn't run since). Play a test session, quit, and run this again."
    exit 1
}

$stamp = Get-Date -Format 'yyyy-MM-dd_HHmm'
$rawCopy = Join-Path $rawDir "Papyrus_$stamp.log"
Copy-Item $logPath $rawCopy

# --- 3. Filter the Astra-related lines into the repo ------------------------
$rawLines = Get-Content $logPath
$astraLines = @($rawLines | Where-Object { $_ -match 'MQAstraALT|COMAstra|Astra' })

$outFile = Join-Path $logsDir "astra_$stamp.log"
$header = @(
    "# Astra-filtered Papyrus log",
    "# Collected: $stamp",
    "# Source: $logPath",
    "# Raw log lines: $($rawLines.Count) | Astra lines: $($astraLines.Count)",
    "# Full unfiltered copy kept locally at logs/raw/ (not committed)",
    ""
)
$header + $astraLines | Set-Content $outFile
Copy-Item $outFile (Join-Path $logsDir 'latest_astra.log') -Force

Write-Host ""
Write-Host "Collected $($astraLines.Count) Astra lines -> logs\astra_$stamp.log"

# --- 4. Optionally commit and push so Claude can read it --------------------
if ($Push) {
    Push-Location $repoRoot
    try {
        git add logs
        git commit -m "Add Papyrus test log $stamp"
        git pull --rebase --quiet
        git push
        Write-Host ""
        Write-Host "Pushed to GitHub. Tell Claude the log is up and it can read logs/latest_astra.log."
    } catch {
        Write-Host ""
        Write-Host "Log collected, but git commit/push failed: $_"
        Write-Host "You can push manually with: git add logs && git commit -m 'test log' && git push"
    } finally {
        Pop-Location
    }
} else {
    Write-Host "Run with -Push (or use collect_log.bat) to commit and push it to GitHub for Claude."
}
