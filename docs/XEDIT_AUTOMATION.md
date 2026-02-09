# xEdit Automation (Astra)

This workflow dumps all INFO records from `CompanionAstra.esp` to CSV so you can verify text/IDs without CK.

## One-Command Dump
From PowerShell:
```
E:\CompanionGeminiFeb26\Tools\run_xedit_dump.ps1
```

Output:
```
E:\CompanionGeminiFeb26\Tools\FO4Edit\Astra_DialogueDump.csv
```

## Notes
- Uses `xFOEdit.exe` from `E:\CompanionGeminiFeb26\Tools\FO4Edit`
- Script lives at:
  `E:\CompanionGeminiFeb26\Tools\FO4Edit\Edit Scripts\DumpDialogueToCSV_Astra.pas`
- If you keep FO4Edit elsewhere, pass a custom path:
```
E:\CompanionGeminiFeb26\Tools\run_xedit_dump.ps1 -XEditPath "D:\Path\To\xFOEdit.exe"
```

## What This Gives You
- INFO FormID
- Topic (DIAL) name
- Text

This helps catch mismatched text/audio and supports automated diffing.
