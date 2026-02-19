$toolsDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Generating CK checklist..."
python (Join-Path $toolsDir "generate_ck_checklist.py")

Write-Host "Generating player voice audit..."
python (Join-Path $toolsDir "player_voice_audit.py")

Write-Host "Reports written:"
Write-Host " - $(Join-Path $toolsDir 'CK_Checklist_Astra.txt')"
Write-Host " - $(Join-Path $toolsDir 'PlayerVoice_Audit.txt')"
