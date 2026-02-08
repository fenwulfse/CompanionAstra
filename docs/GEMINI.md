# GEMINI.md - CompanionGemini Project Context

**Date:** 2026-02-06
**Project:** CompanionGemini (Transitioning from CompanionClaude)
**Framework:** .NET 10.0
**Library:** Mutagen.Bethesda.Fallout4 (v0.52.0)
**Type:** CLI-based Fallout 4 Mod Generator

## 1. Project Overview
This project is an external C# application that generates a Fallout 4 mod (`CompanionGemini.esp`). It programmatically constructs a fully-voiced companion ("Gemini") with complex affinity quests, dialogue trees, and AI packages without using the Creation Kit (CK) for generation.

**Core Narrative:** An AI companion learning about humanity through four stages of affinity:
1.  **Friendship:** Understanding companionship vs utility.
2.  **Admiration:** Recognizing player agency.
3.  **Confidant:** Sharing vulnerabilities.
4.  **Infatuation:** Logic vs Emotion (Romance).

## 2. Current Mission: Voice Automation & Pattern Refactoring
**Voice Status:** 🎉 **PIPELINE COMPLETE!**
*   **WAV → LIP:** Solved via Bethesda's `LipGenerator.exe` (found in `Tools/`).
*   **WAV → XWM:** Solved via `xwmaencode.exe` (found in `Tools/`).
*   **XWM + LIP → FUZ:** Solved via custom `FuzPacker.cs` implementation.
*   **Next Step:** Integration testing of the full pipeline (WAV -> FUZ) using `FuzPacker.CreateFuzFromWav`.

**Structure Status:**
*   **Friendship Scene:** Works perfectly (matches Piper's structure).
*   **Other Scenes:** Need refactoring to strict vanilla blueprints.

### The "Golden Blueprints" (from `VANILLA_AFFINITY_PATTERNS.md`)
*Strictly adhere to these structures:*

| Scene | Vanilla Ref | Phases | Actions | Action Indices | Loops (Phase #) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Friendship** | `COMPiper_01` | 8 | 8 | 1, 2, 3, 4, 6, 7, 8, 9 (**Skips 5**) | 2, 4, 6 |
| **Admiration** | `COMPiper_02` | 6 | 6 | 1, 2, 3, 4, 5, 6 | 2 |
| **Confidant** | `COMPiper_02a` | 8 | 8 | 1, 2, 3, 5, 6, 7, 8, 9 (**Skips 4**) | 4 |
| **Infatuation** | `COMPiper_03` | 14 | 14 | 1-14 (Non-sequential mapping) | 4, 10, 12 |

## 3. Technical Constraints & Conventions
*   **Mutagen Version:** Strictly use **0.52.0**.
*   **Collection Initialization:** Always initialize `ExtendedList` properties (Factions, Items, Keywords) explicitly.
*   **FormKeys:** Enforce 6-digit hex padding (e.g., `005DE4`).
*   **Papyrus Fragments:** Must have **explicit newlines** between function calls.
*   **Versioning:**
    *   Use alphabetical folders for major iterations (current: `E:\CompanionGeminiFeb26`).
    *   **Do not** label builds as "Golden" or "Final".
    *   Maintain `CHANGELOG.md` (or detailed `SESSION_SUMMARY`).
*   **Tools:**
    *   **LipGenerator.exe:** Bethesda's official tool for generating `.lip` files.
    *   **xwmaencode.exe:** Bethesda's official tool for converting `.wav` to `.xwm`.
    *   **FonixData.cdf:** Required phoneme database for LipGenerator.
    *   **FuzPacker.cs:** Custom C# implementation for packing `.xwm` and `.lip` into `.fuz`.
    *   **FaceFXWrapper:** *Deprecated* in favor of `LipGenerator.exe`.

## 4. Directory Structure
*   `CompanionGemini/` - The main C# application code (`Program.cs`, `.csproj`).
*   `DialogueDumper/` - Auxiliary tool for extracting vanilla dialogue data.
*   `Tools/` - Critical voice generation tools (`LipGenerator.exe`, `xwmaencode.exe`, `FonixData.cdf`).
*   `VoiceFiles/` - Raw audio assets.
*   `docs/` - Documentation (`VANILLA_AFFINITY_PATTERNS.md`, `PIPER_BIBLE.md`).
*   `Backups/` - Versioned backups of `Program.cs`.
*   `FuzPacker.cs` - Standalone packer implementation (to be integrated).

## 5. Development Workflow
1.  **Modify:** Edit `CompanionGemini/Program.cs`.
2.  **Build & Run:**
    ```bash
    cd CompanionGemini
    dotnet build
    dotnet run
    ```
    *Output:* `D:\SteamLibrary\steamapps\common\Fallout 4\Data\CompanionGemini.esp`.
3.  **Voice Generation:** Use `FuzPacker` to process `.wav` files into `.fuz` files.
4.  **Validate:** Check against `docs/VANILLA_AFFINITY_PATTERNS.md`.
5.  **Backup:** Periodically copy `Program.cs` to `Backups/`.

## 6. Immediate Todo
1.  **Integrate FuzPacker:** Move `FuzPacker.cs` logic into `CompanionGemini` project.
2.  **Test Voice Pipeline:** Verify `WAV -> FUZ` creation end-to-end.
3.  **Refactor Scenes:** Update Admiration, Confidant, and Infatuation scenes to match vanilla blueprints.
