# Astra watcher: leave this running on the gaming PC and the test loop
# becomes hands-off.
#
#   - While Fallout 4 is NOT running: every few minutes it pulls new
#     commits from GitHub. If code changed, it rebuilds the ESP, compiles
#     Papyrus, and deploys both to the game's Data folder.
#   - When you quit Fallout 4: it automatically collects the Papyrus log
#     and pushes the Astra-filtered lines to GitHub for Claude to read.
#
# Start it with start_watcher.bat and leave the window open.

param(
    [switch]$NoBuild   # pull + collect logs only, never build/deploy
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

# --- Paths (from docs/BUILD_GUIDE.md; edit here if your install moves) ------
$gameDir   = 'E:\SteamLibrary\steamapps\common\Fallout 4'
$dataDir   = Join-Path $gameDir 'Data'
$compiler  = Join-Path $gameDir 'Papyrus Compiler\PapyrusCompiler.exe'
$flagsFile = Join-Path $dataDir 'Scripts\Source\Base\Institute_Papyrus_Flags.flg'
$vanillaSrc = (Join-Path $dataDir 'Scripts\Source\User') + ';' + (Join-Path $dataDir 'Scripts\Source\Base')

$espName = 'MQAstraALT.esp'
$pexRel  = 'Fragments\Quests\QF_MQAstraALT_0000080A.pex'

$syncIntervalSec = 300   # check GitHub every 5 minutes while game is closed
$pollSec         = 15

function Get-GameRunning {
    [bool](Get-Process -Name 'Fallout4' -ErrorAction SilentlyContinue)
}

# Pull from GitHub; returns list of changed files, or $null if up to date.
function Sync-Repo {
    git -C $repoRoot fetch --quiet 2>$null
    $local  = (git -C $repoRoot rev-parse HEAD).Trim()
    $remote = git -C $repoRoot rev-parse '@{u}' 2>$null
    if (-not $remote) { return $null }
    $remote = "$remote".Trim()
    if ($local -eq $remote) { return $null }
    git -C $repoRoot pull --rebase --quiet
    return @(git -C $repoRoot diff --name-only $local $remote)
}

function Build-Mod {
    Write-Host "[$(Get-Date -Format HH:mm)] Building ESP (dotnet run)..."
    Push-Location $repoRoot
    try {
        dotnet run
        if ($LASTEXITCODE) { throw "dotnet run failed (exit $LASTEXITCODE)" }

        Write-Host "[$(Get-Date -Format HH:mm)] Compiling Papyrus..."
        & $compiler (Join-Path $repoRoot 'Source') `
            "-i=$(Join-Path $repoRoot 'Source');$vanillaSrc" `
            "-o=$(Join-Path $repoRoot 'out')" `
            "-f=$flagsFile" -all
        if ($LASTEXITCODE) { throw "PapyrusCompiler failed (exit $LASTEXITCODE)" }

        Copy-Item (Join-Path $repoRoot $espName) (Join-Path $dataDir $espName) -Force
        $pexDstDir = Join-Path $dataDir ('Scripts\' + (Split-Path -Parent $pexRel))
        New-Item -ItemType Directory -Force -Path $pexDstDir | Out-Null
        Copy-Item (Join-Path $repoRoot "out\$pexRel") (Join-Path $dataDir "Scripts\$pexRel") -Force

        Write-Host "[$(Get-Date -Format HH:mm)] Deployed $espName + PEX to Data. Ready to test."
        Write-Host "  (Remember: ESP changes need a New Game to take effect.)"
    } finally {
        Pop-Location
    }
}

Write-Host "=== Astra watcher running ==="
Write-Host "Repo: $repoRoot (branch: $((git -C $repoRoot rev-parse --abbrev-ref HEAD).Trim()))"
Write-Host "Leave this window open. Play Fallout 4 normally; logs upload when you quit."
Write-Host ""

$lastSync   = [datetime]::MinValue
$wasRunning = Get-GameRunning

while ($true) {
    $running = Get-GameRunning

    # Game just exited -> collect and push the Papyrus log
    if ($wasRunning -and -not $running) {
        Write-Host "[$(Get-Date -Format HH:mm)] Fallout 4 exited; collecting Papyrus log..."
        Start-Sleep -Seconds 5
        try {
            & (Join-Path $PSScriptRoot 'collect_papyrus_log.ps1') -Push
        } catch {
            Write-Host "Log collection failed: $_"
        }
    }
    if (-not $wasRunning -and $running) {
        Write-Host "[$(Get-Date -Format HH:mm)] Fallout 4 started. Have a good test session."
    }

    # While the game is closed, periodically pull (and build) new work
    if (-not $running -and ((Get-Date) - $lastSync).TotalSeconds -ge $syncIntervalSec) {
        $lastSync = Get-Date
        try {
            $changed = Sync-Repo
            if ($changed) {
                Write-Host "[$(Get-Date -Format HH:mm)] Pulled updates from GitHub:"
                $changed | ForEach-Object { Write-Host "    $_" }

                $needsBuild = $changed | Where-Object { $_ -match '\.(cs|csproj|psc)$' -or $_ -eq 'stable_formkeys.json' }
                if ($needsBuild -and -not $NoBuild) { Build-Mod }

                if ($changed | Where-Object { $_ -match 'voice_lines\.json|generate_.*voices\.py' }) {
                    Write-Host "  *** VOICE LINES CHANGED -- run 'python generate_voices.py' before testing. ***"
                }
            }
        } catch {
            Write-Host "[$(Get-Date -Format HH:mm)] Sync/build problem: $_"
            Write-Host "  Watcher keeps running; will retry in $([int]($syncIntervalSec/60)) minutes."
        }
    }

    $wasRunning = $running
    Start-Sleep -Seconds $pollSec
}
