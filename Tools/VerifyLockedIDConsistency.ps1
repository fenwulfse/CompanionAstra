param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [switch]$IncludeAllDocs,
    [switch]$FailOnDrift
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Normalize-HexId {
    param([string]$Hex)
    $trimmed = $Hex.Trim()

    if ($trimmed -match "^[0-9A-Fa-f]{1,8}$") {
        $trimmed = "0x$trimmed"
    }

    if ($trimmed -notmatch "^0x[0-9A-Fa-f]{1,8}$") {
        return $null
    }

    $value = [Convert]::ToUInt32($trimmed.Substring(2), 16)
    return ("0x{0:X8}" -f $value)
}

function Get-HexIdsFromText {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return @()
    }

    # Match:
    # - explicit 0x-prefixed ids from code
    # - plain hex ids that start with 0 (common in locked-id docs, e.g. 0000F420 / 00F400)
    $matches = [regex]::Matches($Text, "0x[0-9A-Fa-f]{1,8}|\b0[0-9A-Fa-f]{5,7}\b")
    $ids = foreach ($m in $matches) {
        Normalize-HexId -Hex $m.Value
    }

    return $ids | Where-Object { $_ } | Sort-Object -Unique
}

$programPath = Join-Path $RepoRoot "CompanionAstra_LockedIDs\Program.cs"
$docsPath = Join-Path $RepoRoot "docs"

if (-not (Test-Path $programPath)) {
    throw "Program.cs not found: $programPath"
}
if (-not (Test-Path $docsPath)) {
    throw "Docs folder not found: $docsPath"
}

$docFilter = if ($IncludeAllDocs) { "*.md" } else { "*LOCKED_IDS*.md" }
$docFiles = Get-ChildItem -Path $docsPath -File -Filter $docFilter | Sort-Object Name

if (-not $docFiles) {
    throw "No documentation files found with filter '$docFilter' in $docsPath"
}

$programText = Get-Content -Path $programPath -Raw
$programIds = @(Get-HexIdsFromText -Text $programText)

$docIds = @()
foreach ($file in $docFiles) {
    $text = Get-Content -Path $file.FullName -Raw
    $docIds += Get-HexIdsFromText -Text $text
}
$docIds = @($docIds | Sort-Object -Unique)

$programOnly = @(Compare-Object -ReferenceObject @($programIds) -DifferenceObject @($docIds) -PassThru |
    Where-Object { $_ -in $programIds } |
    Sort-Object -Unique)
$docsOnly = @(Compare-Object -ReferenceObject @($docIds) -DifferenceObject @($programIds) -PassThru |
    Where-Object { $_ -in $docIds } |
    Sort-Object -Unique)

Write-Host "Locked-ID Consistency Report"
Write-Host "RepoRoot: $RepoRoot"
Write-Host "Program.cs IDs: $($programIds.Count)"
Write-Host "Doc IDs: $($docIds.Count)"
Write-Host "Doc files scanned: $($docFiles.Count)"
foreach ($f in $docFiles) {
    Write-Host "  - $($f.Name)"
}
Write-Host ""

if ((@($programOnly).Count -eq 0) -and (@($docsOnly).Count -eq 0)) {
    Write-Host "PASS: No drift detected between Program.cs and docs."
    exit 0
}

Write-Host "DRIFT DETECTED"
Write-Host ""

if (@($programOnly).Count -gt 0) {
    Write-Host "In Program.cs but not docs:"
    $programOnly | ForEach-Object { Write-Host "  $_" }
    Write-Host ""
}

if (@($docsOnly).Count -gt 0) {
    Write-Host "In docs but not Program.cs:"
    $docsOnly | ForEach-Object { Write-Host "  $_" }
    Write-Host ""
}

if ($FailOnDrift) {
    exit 2
}
