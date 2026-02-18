# Papyrus Workflow (Astra)

## Compiler Policy
- Use Bethesda Papyrus compiler (`PapyrusCompiler.exe`) for final artifact generation.
- Do not rely on Caprica for production deploy output paths.

## Why
- CK/Bethesda compiler output aligns with in-game expected fragment layout.
- Namespace and folder matching (`Fragments:Quests`) are enforced correctly.

## Required Paths
- Compiler:
  - `E:\SteamLibrary\steamapps\common\Fallout 4\Papyrus Compiler\PapyrusCompiler.exe`
- Flags:
  - `E:\SteamLibrary\steamapps\common\Fallout 4\Data\Scripts\Source\Base\Institute_Papyrus_Flags.flg`
- Source root:
  - `E:\FO4Projects\Tools\Papyrus`
- Output root:
  - `E:\FO4Projects\Tools\Papyrus\Output`

## Compile Command
```powershell
& "E:\SteamLibrary\steamapps\common\Fallout 4\Papyrus Compiler\PapyrusCompiler.exe" `
  "E:\FO4Projects\Tools\Papyrus" `
  -f="E:\SteamLibrary\steamapps\common\Fallout 4\Data\Scripts\Source\Base\Institute_Papyrus_Flags.flg" `
  -i="E:\FO4Projects\Tools\Papyrus;E:\SteamLibrary\steamapps\common\Fallout 4\Data\Scripts\Source\Base;E:\SteamLibrary\steamapps\common\Fallout 4\Data\Scripts\Source\User" `
  -o="E:\FO4Projects\Tools\Papyrus\Output" `
  -all
```

## Expected Result
- Root-level `QF_*.psc` often report filename mismatch (expected).
- `Fragments\Quests\QF_*.psc` compile to valid `.pex` (required artifacts).

## Piper Fragment Reference
- Loose vanilla source script:
  - `D:\SteamLibrary\steamapps\common\Fallout 4\Data\Scripts\Source\Base\Fragments\Quests\QF_COMPiper_000BBD96.psc`
- This is valid as a template for stage fragment logic, after renaming.

## Deploy Rule
- Deploy matched pairs for active test quest identity:
  - `QF_COMAstra_Test_<FORMID>.psc`
  - `QF_COMAstra_Test_<FORMID>.pex`
- Mismatch between ESP VMAD script name and deployed script filename will break stage testing.
- Deploy `.psc` sources to both CK paths:
  - `Data\Scripts\Source\User\QF_*.psc`
  - `Data\Scripts\Source\User\Fragments\Quests\QF_*.psc`

## Canonical Runtime Paths (Astra)
- Main quest runtime fragment binary:
  - `Data\Scripts\Fragments\Quests\QF_COMAstra_00000805.pex`
- Main quest source mirrors:
  - `Data\Scripts\Source\User\QF_COMAstra_00000805.psc`
  - `Data\Scripts\Source\User\Fragments\Quests\QF_COMAstra_00000805.psc`
- Do not deploy to nested paths like:
  - `Data\Scripts\Fragments\Quests\Fragments\Quests\...`
