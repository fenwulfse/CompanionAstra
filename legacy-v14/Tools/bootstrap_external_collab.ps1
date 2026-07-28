param(
    [string]$Repo = "",
    [switch]$CreateLabels = $true,
    [switch]$CreateIssues = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Require-Cli {
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
        throw "GitHub CLI (gh) is required."
    }

    $null = gh auth status
}

function Ensure-Label {
    param(
        [string]$Name,
        [string]$Color,
        [string]$Description
    )

    $existingNames = @(gh label list --repo $Repo --json name --jq '.[].name')
    if ($existingNames -notcontains $Name) {
        gh label create $Name --repo $Repo --color $Color --description $Description | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create label: $Name"
        }
        Write-Host "Created label: $Name"
    }
    else {
        Write-Host "Label exists: $Name"
    }
}

function New-IssueIfMissing {
    param(
        [string]$Title,
        [string[]]$Labels,
        [string]$Body
    )

    $q = [System.Uri]::EscapeDataString("repo:$Repo is:issue in:title `"$Title`"")
    $count = gh api "search/issues?q=$q" --jq ".total_count"
    if ($count -gt 0) {
        Write-Host "Issue already exists: $Title"
        return
    }

    $labelArg = [string]::Join(",", $Labels)
    $url = gh issue create --repo $Repo --title $Title --label $labelArg --body $Body
    Write-Host "Created issue: $url"
}

Require-Cli

if ([string]::IsNullOrWhiteSpace($Repo)) {
    $Repo = gh repo view --json nameWithOwner --jq .nameWithOwner
}

if ($CreateLabels) {
    Ensure-Label -Name "ai-task" -Color "5319e7" -Description "Task intended for AI-assisted contributors"
    Ensure-Label -Name "playtest" -Color "1d76db" -Description "In-game validation request"
    Ensure-Label -Name "safety" -Color "b60205" -Description "Guardrails, privacy, or release safety work"
}

if ($CreateIssues) {
    $commonBody = @'
### Goal
Contribute through fork + pull request without direct writes to main.

### Required Workflow
1. Fork this repo.
2. Create a branch from your fork: agent/<name>/<task>.
3. Make focused changes only.
4. Open PR to `main` with the template completed.
5. Include evidence from:
   - Privacy guard
   - Locked-ID consistency report

### Constraints
- No direct pushes to `main`.
- Preserve locked INFO IDs.
- Do not overwrite NPC voice files unless explicitly requested.
'@

    New-IssueIfMissing -Title "[AI Task] Claude: fork repo and submit first guardrail PR" -Labels @("ai-task", "help wanted", "safety") -Body ($commonBody + @'

### First PR Scope
- Pick one small mechanics or docs improvement.
- Keep commit count minimal.
'@)

    New-IssueIfMissing -Title "[AI Task] Gemini: fork repo and submit first guardrail PR" -Labels @("ai-task", "help wanted", "safety") -Body ($commonBody + @'

### First PR Scope
- Pick one small voice-mapping or docs improvement.
- Keep commit count minimal.
'@)

    New-IssueIfMissing -Title "[AI Task] Grok: external review and patch proposal via PR" -Labels @("ai-task", "help wanted", "question") -Body @"
### Goal
Provide external review findings and one minimal patch PR.

### Constraints
- No direct collaborator write needed.
- Use fork + PR flow.
- If no GitHub fork access is available, provide a patch/diff in issue comments.

### Suggested Focus
- Scene routing sanity checks.
- Missing line dead-ends.
- Repro for player movement lock on talk failure.
"@
}

Write-Host "Done."
