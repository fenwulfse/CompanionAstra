param(
    [string]$ProjectRoot = "",
    [string]$Fo4Data = "",
    [string]$OutRoot = "",
    [string]$Label = "playtest",
    [switch]$IncludeVoice,
    [switch]$NoZip
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-Fo4DataPath {
    param([string]$ExplicitFo4Data)

    if (-not [string]::IsNullOrWhiteSpace($ExplicitFo4Data) -and (Test-Path $ExplicitFo4Data)) {
        return (Resolve-Path $ExplicitFo4Data).Path
    }

    if (-not [string]::IsNullOrWhiteSpace($env:FO4_DATA) -and (Test-Path $env:FO4_DATA)) {
        return (Resolve-Path $env:FO4_DATA).Path
    }

    $candidates = @(
        "C:\Program Files (x86)\Steam\steamapps\common\Fallout 4\Data",
        "C:\Program Files\Steam\steamapps\common\Fallout 4\Data"
    )

    try {
        $drives = [System.IO.DriveInfo]::GetDrives() | Where-Object { $_.DriveType -eq 'Fixed' -and $_.IsReady }
        foreach ($drive in $drives) {
            $candidates += (Join-Path $drive.RootDirectory.FullName "SteamLibrary\steamapps\common\Fallout 4\Data")
        }
    } catch {
        # Ignore drive enumeration failures and continue with static candidates.
    }

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) { return $candidate }
    }

    return $null
}

function Safe-Hash {
    param([string]$Path)
    if (-not (Test-Path $Path)) { return "missing" }
    return (Get-FileHash -Path $Path -Algorithm SHA256).Hash
}

function Copy-IfExists {
    param(
        [string]$SourcePath,
        [string]$DestPath
    )
    if (-not (Test-Path $SourcePath)) { return $false }

    $destDir = Split-Path $DestPath -Parent
    if (-not (Test-Path $destDir)) {
        New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    }
    Copy-Item -Path $SourcePath -Destination $DestPath -Force
    return $true
}

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}
if (-not (Test-Path $ProjectRoot)) {
    throw "Project root not found: $ProjectRoot"
}

if ([string]::IsNullOrWhiteSpace($OutRoot)) {
    $OutRoot = Join-Path $ProjectRoot "Backups\PlaytestPackets"
}

$fo4DataPath = Resolve-Fo4DataPath -ExplicitFo4Data $Fo4Data
if ([string]::IsNullOrWhiteSpace($fo4DataPath) -or -not (Test-Path $fo4DataPath)) {
    throw "Could not resolve Fallout 4 Data path. Pass -Fo4Data explicitly."
}

$safeLabel = ($Label -replace "[^A-Za-z0-9_-]", "_")
if ([string]::IsNullOrWhiteSpace($safeLabel)) { $safeLabel = "playtest" }
if ($safeLabel.Length -gt 48) { $safeLabel = $safeLabel.Substring(0, 48) }

$timestamp = Get-Date -Format "yyyy-MM-dd_HHmmss"
$packetDir = Join-Path $OutRoot ("{0}_{1}" -f $timestamp, $safeLabel)
$deployDir = Join-Path $packetDir "Deploy"

New-Item -ItemType Directory -Path $deployDir -Force | Out-Null

$espData = Join-Path $fo4DataPath "CompanionAstra.esp"
$pexData = Join-Path $fo4DataPath "Scripts\Fragments\Quests\QF_COMAstra_00000805.pex"
$pscData = Join-Path $fo4DataPath "Scripts\Source\User\Fragments\Quests\QF_COMAstra_00000805.psc"
$voiceRoot = Join-Path $fo4DataPath "Sound\Voice\CompanionAstra.esp"
$pluginsTxt = Join-Path $env:LOCALAPPDATA "Fallout4\plugins.txt"

$copiedEsp = Copy-IfExists -SourcePath $espData -DestPath (Join-Path $deployDir "CompanionAstra.esp")
$copiedPex = Copy-IfExists -SourcePath $pexData -DestPath (Join-Path $deployDir "Scripts\Fragments\Quests\QF_COMAstra_00000805.pex")
$copiedPsc = Copy-IfExists -SourcePath $pscData -DestPath (Join-Path $deployDir "Scripts\Source\User\Fragments\Quests\QF_COMAstra_00000805.psc")

$voiceTotal = 0
$voiceNpcAstra = 0
$voiceDirs = 0
if (Test-Path $voiceRoot) {
    $voiceTotal = (Get-ChildItem $voiceRoot -Recurse -File -Filter *.fuz).Count
    $voiceDirs = (Get-ChildItem $voiceRoot -Directory).Count
    $npcVoicePath = Join-Path $voiceRoot "NPCFAstra"
    if (Test-Path $npcVoicePath) {
        $voiceNpcAstra = (Get-ChildItem $npcVoicePath -File -Filter *.fuz).Count
    }

    if ($IncludeVoice) {
        $voiceDst = Join-Path $deployDir "Sound\Voice\CompanionAstra.esp"
        Copy-Item -Path $voiceRoot -Destination $voiceDst -Recurse -Force
    }
}

$pluginsLines = @()
if (Test-Path $pluginsTxt) {
    $pluginsLines = Get-Content $pluginsTxt
}
else {
    $pluginsLines = @("missing: $pluginsTxt")
}

$manifestPath = Join-Path $packetDir "MANIFEST.txt"
$manifest = @(
    "Playtest Packet",
    "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')",
    "ProjectRoot: $ProjectRoot",
    "Fo4Data: $fo4DataPath",
    "Label: $Label",
    "",
    "Artifacts:",
    "- CompanionAstra.esp present: $copiedEsp",
    "- QF_COMAstra_00000805.pex present: $copiedPex",
    "- QF_COMAstra_00000805.psc present: $copiedPsc",
    "- Voice included in packet: $([bool]$IncludeVoice)",
    "",
    "Hashes:",
    "- ESP SHA256: $(Safe-Hash $espData)",
    "- PEX SHA256: $(Safe-Hash $pexData)",
    "- PSC SHA256: $(Safe-Hash $pscData)",
    "",
    "Voice Counts:",
    "- Total .fuz: $voiceTotal",
    "- NPCFAstra .fuz: $voiceNpcAstra",
    "- Voice dirs: $voiceDirs",
    "",
    "plugins.txt:"
)
$manifest += $pluginsLines
$manifest | Set-Content -Path $manifestPath -Encoding UTF8

$checklistPath = Join-Path $packetDir "PLAYTEST_CHECKLIST.md"
$checklist = @(
    "# Playtest Checklist",
    "",
    "## Minimum Test Pass",
    "- Talk to Astra from idle state.",
    "- Verify player is not movement-locked after talk.",
    "- Pickup Astra, then dismiss Astra.",
    "- Handoff tests: Astra <-> Codsworth and Astra <-> Dogmeat.",
    "- Exit to menu and exit to desktop; note if force-close is required.",
    "",
    "## Report Format",
    "- Save type: new game or existing long-running save.",
    "- Repro steps.",
    "- Expected vs actual.",
    "- Reliability: always / sometimes / once.",
    "- Screenshots or short clip if possible."
)
$checklist | Set-Content -Path $checklistPath -Encoding UTF8

$readmePath = Join-Path $packetDir "README.txt"
$readme = @(
    "Playtest packet generated by Tools/create_playtest_packet.ps1",
    "Use MANIFEST.txt for exact build fingerprint.",
    "Use PLAYTEST_CHECKLIST.md for required validation steps."
)
$readme | Set-Content -Path $readmePath -Encoding UTF8

if (-not $NoZip) {
    $zipPath = "$packetDir.zip"
    if (Test-Path $zipPath) { Remove-Item -Path $zipPath -Force }
    Compress-Archive -Path (Join-Path $packetDir "*") -DestinationPath $zipPath -Force
    Write-Host "Packet zip: $zipPath"
}

Write-Host "Packet folder: $packetDir"
Write-Host "Done."
