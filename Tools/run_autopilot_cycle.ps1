param(
    [string]$ProjectRoot = "",
    [string]$Fo4Data = "",
    [string]$ToolsRoot = "",
    [switch]$IncludeBackup,
    [switch]$IncludeVoiceInBackup,
    [string]$BackupRoot = "",
    [switch]$GenerateFriendshipTts,
    [switch]$GenerateAdmirationTts,
    [switch]$GenerateConfidantTts,
    [switch]$GenerateInfatuationTts,
    [switch]$GenerateDisdainTts,
    [switch]$GenerateHatredTts,
    [switch]$GenerateRecoveryTts,
    [switch]$GenerateMurderTts,
    [switch]$EnableGitCommit,
    [string]$CommitMessage = "build: automated local deploy"
)

$ErrorActionPreference = "Stop"

function Resolve-Fo4DataPath {
    param([string]$ExplicitFo4Data)

    if (-not [string]::IsNullOrWhiteSpace($ExplicitFo4Data) -and (Test-Path $ExplicitFo4Data)) {
        return $ExplicitFo4Data
    }

    if (-not [string]::IsNullOrWhiteSpace($env:FO4_DATA) -and (Test-Path $env:FO4_DATA)) {
        return $env:FO4_DATA
    }

    $candidates = @(
        "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Fallout 4\\Data",
        "C:\\Program Files\\Steam\\steamapps\\common\\Fallout 4\\Data"
    )

    foreach ($drive in [System.IO.DriveInfo]::GetDrives() | Where-Object { $_.DriveType -eq 'Fixed' -and $_.IsReady }) {
        $candidates += (Join-Path $drive.RootDirectory.FullName "SteamLibrary\\steamapps\\common\\Fallout 4\\Data")
    }

    return ($candidates | Select-Object -Unique | Where-Object { Test-Path $_ } | Select-Object -First 1)
}

function Step([string]$Message)
{
    Write-Host ""
    Write-Host "=== $Message ===" -ForegroundColor Cyan
}

function RequirePath([string]$Path, [string]$Label)
{
    if (-not (Test-Path $Path))
    {
        throw "$Label not found: $Path"
    }
}

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

if ([string]::IsNullOrWhiteSpace($Fo4Data)) {
    $Fo4Data = Resolve-Fo4DataPath -ExplicitFo4Data ""
}

if ([string]::IsNullOrWhiteSpace($ToolsRoot)) {
    if (-not [string]::IsNullOrWhiteSpace($env:FO4_TOOLS_ROOT) -and (Test-Path $env:FO4_TOOLS_ROOT)) {
        $ToolsRoot = $env:FO4_TOOLS_ROOT
    }
    else {
        $workspaceRoot = Split-Path $ProjectRoot -Parent
        $candidate = Join-Path $workspaceRoot "Tools"
        if (Test-Path $candidate) { $ToolsRoot = $candidate }
    }
}

if ([string]::IsNullOrWhiteSpace($BackupRoot)) {
    $workspaceRoot = Split-Path $ProjectRoot -Parent
    $BackupRoot = Join-Path $workspaceRoot "Backups"
}

$csprojPath = Join-Path $ProjectRoot "CompanionAstra_LockedIDs\CompanionClaude_v13.csproj"
$espSourcePath = Join-Path $ProjectRoot "CompanionAstra.esp"
$espDestPath = Join-Path $Fo4Data "CompanionAstra.esp"
$backupScriptPath = Join-Path $ProjectRoot "Tools\create_disaster_backup.ps1"
$reportPath = Join-Path $ProjectRoot "docs\AUTOPILOT_LAST_RUN.md"

RequirePath -Path $ProjectRoot -Label "Project root"
RequirePath -Path $Fo4Data -Label "Fallout 4 Data folder"
RequirePath -Path $csprojPath -Label "Generator project"
RequirePath -Path $ToolsRoot -Label "Tools root"

Step "Build"
& dotnet build $csprojPath

Step "Generate ESP + Voice"
$runArgs = @(
    "run",
    "--project", $csprojPath,
    "--",
    "--tools-root", $ToolsRoot,
    "--enable-greeting-tts",
    "--enable-dismiss-tts"
)

if ($GenerateFriendshipTts) { $runArgs += "--enable-friendship-tts" }
if ($GenerateAdmirationTts) { $runArgs += "--enable-admiration-tts" }
if ($GenerateConfidantTts) { $runArgs += "--enable-confidant-tts" }
if ($GenerateInfatuationTts) { $runArgs += "--enable-infatuation-tts" }
if ($GenerateDisdainTts) { $runArgs += "--enable-disdain-tts" }
if ($GenerateHatredTts) { $runArgs += "--enable-hatred-tts" }
if ($GenerateRecoveryTts) { $runArgs += "--enable-recovery-tts" }
if ($GenerateMurderTts) { $runArgs += "--enable-murder-tts" }

& dotnet @runArgs

Step "Deploy ESP"
RequirePath -Path $espSourcePath -Label "Generated ESP"
Copy-Item -Path $espSourcePath -Destination $espDestPath -Force
Write-Host "Deployed: $espDestPath"

if ($IncludeBackup)
{
    Step "Backup"
    RequirePath -Path $backupScriptPath -Label "Backup script"
    $backupArgs = @(
        "-ExecutionPolicy", "Bypass",
        "-File", $backupScriptPath,
        "-ProjectRoot", $ProjectRoot,
        "-BackupRoot", $BackupRoot,
        "-Fo4Data", $Fo4Data
    )
    if ($IncludeVoiceInBackup)
    {
        $backupArgs += "-IncludeVoice"
    }

    & powershell @backupArgs
}

Step "Write Report"
$runUtc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'")
$espInfo = Get-Item $espSourcePath
$deployedInfo = Get-Item $espDestPath
$statusLines = @()
try
{
    $statusLines = git -C $ProjectRoot status --short
}
catch
{
    $statusLines = @("(git status unavailable)")
}
if (-not $statusLines -or $statusLines.Count -eq 0)
{
    $statusLines = @("(working tree clean)")
}

$reportLines = @(
    "# Autopilot Last Run",
    "",
    "- Run UTC: $runUtc",
    "- Project root: $ProjectRoot",
    "- FO4 Data: $Fo4Data",
    "- Tools root: $ToolsRoot",
    "- ESP source: $($espInfo.FullName)",
    "- ESP source timestamp: $($espInfo.LastWriteTime.ToString('yyyy-MM-dd HH:mm:ss'))",
    "- ESP deployed: $($deployedInfo.FullName)",
    "- ESP deployed timestamp: $($deployedInfo.LastWriteTime.ToString('yyyy-MM-dd HH:mm:ss'))",
    "",
    "## Git Status",
    '```text'
)
$reportLines += $statusLines
$reportLines += @(
    '```',
    ""
)

Set-Content -Path $reportPath -Value $reportLines -Encoding UTF8
Write-Host "Wrote report: $reportPath"

if ($EnableGitCommit)
{
    Step "Git Commit"
    $trackedCandidates = @(
        "CompanionAstra_LockedIDs/Program.cs",
        "CompanionAstra.esp",
        "HANDOVER_CODEX.md",
        "docs/AUTOPILOT_LAST_RUN.md",
        "docs/AUTONOMY_ROADMAP.md",
        "docs/TESTER_GUIDE.md",
        "docs/BUG_REPORT_TEMPLATE.md",
        "docs/RECRUITMENT_POST_PACK.md",
        "Tools/run_autopilot_cycle.ps1"
    )

    foreach ($candidate in $trackedCandidates)
    {
        $full = Join-Path $ProjectRoot $candidate
        if (Test-Path $full)
        {
            & git -C $ProjectRoot add $candidate
        }
    }

    $dirty = git -C $ProjectRoot status --porcelain
    if ($dirty -and $dirty.Count -gt 0)
    {
        & git -C $ProjectRoot commit -m $CommitMessage
        Write-Host "Committed local changes."
    }
    else
    {
        Write-Host "No changes to commit."
    }
}

Step "Done"
Write-Host "Autopilot cycle completed."
