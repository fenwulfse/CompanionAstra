# xEdit Automation (Astra)

This workflow dumps all INFO records from `CompanionAstra.esp` to CSV so you can verify text/IDs without CK.

## One-Command Dump
From PowerShell:
```
<WORKSPACE_ROOT>\Tools\run_xedit_dump.ps1
```

Output:
```
<WORKSPACE_ROOT>\Tools\FO4Edit\Astra_DialogueDump.csv
```

## Notes
- Uses `xFOEdit.exe` from `<WORKSPACE_ROOT>\Tools\FO4Edit`
- Script lives at:
  `<WORKSPACE_ROOT>\Tools\FO4Edit\Edit Scripts\DumpDialogueToCSV_Astra.pas`
- If you keep FO4Edit elsewhere, pass a custom path:
```
<WORKSPACE_ROOT>\Tools\run_xedit_dump.ps1 -XEditPath "<PATH_TO_XEDIT>\xFOEdit.exe"
```

## What This Gives You
- INFO FormID
- Topic (DIAL) name
- Text

This helps catch mismatched text/audio and supports automated diffing.

## Generate CK Checklist + Player Voice Audit
After the dump, run:
```
<WORKSPACE_ROOT>\Tools\run_reports.ps1
```

Outputs:
- `<WORKSPACE_ROOT>\Tools\CK_Checklist_Astra.txt`
- `<WORKSPACE_ROOT>\Tools\PlayerVoice_Audit.txt`
