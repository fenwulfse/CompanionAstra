Write-Host "Generating CK checklist..."
python E:\CompanionGeminiFeb26\Tools\generate_ck_checklist.py

Write-Host "Generating player voice audit..."
python E:\CompanionGeminiFeb26\Tools\player_voice_audit.py

Write-Host "Reports written:"
Write-Host " - E:\CompanionGeminiFeb26\Tools\CK_Checklist_Astra.txt"
Write-Host " - E:\CompanionGeminiFeb26\Tools\PlayerVoice_Audit.txt"
