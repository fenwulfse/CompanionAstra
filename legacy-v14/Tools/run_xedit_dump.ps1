param(
    [string]$XEditPath = ""
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($XEditPath)) {
    $XEditPath = Join-Path $scriptDir "FO4Edit\\xFOEdit.exe"
}

$scriptName = "DumpDialogueToCSV_Astra"
if (!(Test-Path $XEditPath)) {
    Write-Host "xEdit not found at: $XEditPath"
    exit 1
}

Write-Host "Running xEdit dump script: $scriptName"
Write-Host "xEdit: $XEditPath"

& $XEditPath -nobuildrefs -nobackups -autoload -script:$scriptName

Write-Host "Done. Output: Astra_DialogueDump.csv (in xEdit folder)"
