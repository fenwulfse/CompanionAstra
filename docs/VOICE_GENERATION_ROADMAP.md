# Voice Generation Roadmap - CompanionClaude
**Strategic Plan for AI Voice Implementation**

---

## Vision

Replace the current **vanilla voice file borrowing system** with **custom AI-generated voices** specifically for Claude, giving her a unique audio identity while maintaining professional quality and vanilla compatibility.

---

## Current State (v20)

✅ **What's Working:**
- 211 vanilla voice files mapped and integrated
- Multi-companion voice system (Piper, Cait, Preston voices)
- Complete voice coverage for all affinity scenes
- Trade functionality with voice
- Full 4-option dialogue wheel voiced

⚠️ **Limitations:**
- Using borrowed vanilla voices (limits creative freedom)
- Voice style doesn't match Claude's AI character perfectly
- Can't add new dialogue without finding matching vanilla voices
- Piper's voice dominates (most files from her)

---

## Goals

### Short-term (Proof of Concept)
1. Generate 5-10 test voice lines with AI
2. Verify .fuz file creation process works
3. Test in-game quality (audio + lip-sync)
4. Validate workflow feasibility

### Medium-term (Partial Integration)
1. Generate voices for Claude's unique dialogue
2. Keep vanilla voices for generic companion responses
3. Hybrid approach: Custom voices for personality, vanilla for common phrases

### Long-term (Full Custom Voice)
1. Generate all 211+ voice lines with consistent AI voice
2. Automate voice generation in build process
3. One-command build: ESP + Voices generated together
4. Release CompanionClaude with unique voice identity

---

## Phases

### Phase 0: Research & Documentation ✅ COMPLETE
**Status:** Done (2026-02-06)

**Deliverables:**
- ✅ VOICE_GENERATION_TOOLS.md created
- ✅ Tool stack documented (FallTalk, FaceFXWrapper, xVASynth, ElevenLabs)
- ✅ Workflow examples documented
- ✅ Technical requirements identified

**Next:** Move to Phase 1

---

### Phase 1: Tool Setup & Testing (2-3 days)
**Goal:** Get the tools working locally

**Tasks:**
1. **Download & Install Tools**
   - [ ] FaceFXWrapper from GitHub
   - [ ] ffmpeg (if not already installed)
   - [ ] FallTalk from Nexus (optional)
   - [ ] xVASynth (optional for local generation)

2. **Verify Installation**
   - [ ] Test FaceFXWrapper with sample WAV file
   - [ ] Confirm LIP file generation works
   - [ ] Test ffmpeg audio conversion

3. **Study .fuz Format**
   - [ ] Extract existing vanilla .fuz files
   - [ ] Examine structure (header, audio, LIP chunks)
   - [ ] Understand packing/unpacking process

4. **Find/Create Packing Tool**
   - [ ] Search for existing .fuz packer tool
   - [ ] OR: Implement custom packer in C#
   - [ ] Test packing process with sample files

**Success Criteria:**
- Can generate LIP from WAV ✓
- Can pack WAV + LIP into .fuz ✓
- Can place .fuz in mod directory and load in-game ✓

**Estimated Time:** 2-3 days (research + testing)

---

### Phase 2: Voice Selection & Test Generation (3-5 days)
**Goal:** Generate first test voices and validate quality

**Tasks:**
1. **Choose Voice Generation Method**
   - [ ] Option A: ElevenLabs API (fast, high quality, costs $)
   - [ ] Option B: xVASynth local (free, slower, requires model)
   - [ ] Option C: FallTalk integrated (simplest)
   - **Decision point:** Budget vs. quality vs. control

2. **Voice Character Design**
   - [ ] Define Claude's voice characteristics
     - Tone: Calm, analytical, warm?
     - Pitch: Medium? Lower for authority?
     - Pace: Measured, thoughtful?
     - Emotion range: Neutral to curious to fond
   - [ ] Find reference voices (existing game characters?)
   - [ ] Create voice profile

3. **Generate Test Lines**
   - [ ] Select 5-10 representative dialogue lines
   - [ ] Mix of: greeting, combat, friendship, romance
   - [ ] Generate with chosen tool
   - [ ] Create .fuz files
   - [ ] Name correctly (FormID_1.fuz)

4. **In-Game Testing**
   - [ ] Create test ESP with voice-mapped responses
   - [ ] Place .fuz files in correct directory structure
   - [ ] Test in-game
   - [ ] Evaluate:
     - Audio quality
     - Lip-sync accuracy
     - Character fit
     - Emotional tone

5. **Iteration**
   - [ ] Adjust voice parameters based on testing
   - [ ] Re-generate problematic lines
   - [ ] Get user feedback
   - [ ] Finalize voice profile

**Success Criteria:**
- Generated voices sound natural ✓
- Lip-sync matches dialogue ✓
- Voice fits Claude's character ✓
- User approves voice direction ✓

**Estimated Time:** 3-5 days (voice design + iteration)

---

### Phase 3: Batch Generation Script (5-7 days)
**Goal:** Automate voice generation for multiple lines

**Tasks:**
1. **Dialogue Export**
   - [ ] Extract all dialogue text from Program.cs
   - [ ] Create structured dialogue list (CSV/JSON)
   - [ ] Include: FormKey, Text, Scene, Emotion hint

2. **Batch Script Development**
   - [ ] Python or C# script for automation
   - [ ] Input: Dialogue list
   - [ ] Process:
     - Generate audio via API/xVASynth
     - Convert to proper format (ffmpeg)
     - Generate LIP (FaceFXWrapper)
     - Pack .fuz
     - Name with FormKey
     - Place in voice directory
   - [ ] Output: All .fuz files ready

3. **Error Handling**
   - [ ] Retry logic for API failures
   - [ ] Quality checks (audio length, silence detection)
   - [ ] Manual review list for problem lines

4. **Test Batch Processing**
   - [ ] Run on 20-30 lines
   - [ ] Verify all .fuz files generated correctly
   - [ ] Test multiple lines in-game
   - [ ] Check consistency across scenes

**Success Criteria:**
- Script generates 20+ voices without manual intervention ✓
- FormKey naming is accurate ✓
- Files load correctly in-game ✓
- Quality is consistent across batch ✓

**Estimated Time:** 5-7 days (scripting + testing)

---

### Phase 4: Full Voice Generation (3-5 days)
**Goal:** Generate all 211+ voice lines for CompanionClaude

**Tasks:**
1. **Preparation**
   - [ ] Complete dialogue list with all 211 lines
   - [ ] Categorize by emotion/tone for generation hints
   - [ ] Backup current vanilla voice files
   - [ ] Plan for overnight batch processing (if needed)

2. **Generation**
   - [ ] Run batch script for all 211 lines
   - [ ] Monitor for errors/failures
   - [ ] Log any problematic lines
   - [ ] Verify file count matches expected

3. **Quality Review**
   - [ ] Listen to samples from each scene
   - [ ] Check: pickup, dismiss, friendship, admiration, confidant, infatuation
   - [ ] Identify lines that need re-generation
   - [ ] Note: Awkward pacing, wrong emotion, technical issues

4. **Refinement**
   - [ ] Re-generate flagged lines with adjusted parameters
   - [ ] Iterate until quality threshold met
   - [ ] Get user approval on sample set

5. **Integration**
   - [ ] Replace vanilla voice files with AI-generated .fuz
   - [ ] Update voice map in Program.cs (if needed)
   - [ ] Rebuild ESP with new voices
   - [ ] Full in-game testing

**Success Criteria:**
- All 211 lines generated ✓
- 95%+ quality rate (minimal re-generation needed) ✓
- Voice consistency across all scenes ✓
- User approves final voice set ✓

**Estimated Time:** 3-5 days (generation + review + refinement)

---

### Phase 5: Build Integration (5-7 days)
**Goal:** Automate voice generation as part of build process

**Tasks:**
1. **Mutagen Integration Planning**
   - [ ] Study Mutagen extensibility points
   - [ ] Design: Generate voices AFTER ESP creation (FormKeys known)
   - [ ] OR: Generate voices in parallel, map to FormKeys after

2. **C# Voice Generation Wrapper**
   - [ ] Create C# library wrapping voice generation tools
   - [ ] Interface: `GenerateVoice(text, emotion) → .fuz file`
   - [ ] Call external tools (Python scripts, APIs, etc.)
   - [ ] Handle errors gracefully

3. **Build Process Modification**
   - [ ] Add voice generation step to Program.cs
   - [ ] After ESP created, iterate all DialogResponses
   - [ ] For each response: Generate voice if not exists
   - [ ] Copy .fuz to game voice directory

4. **Caching & Optimization**
   - [ ] Don't re-generate unchanged lines
   - [ ] Cache voices by dialogue text hash
   - [ ] Allow manual voice overrides
   - [ ] Skip generation flag for testing

5. **Testing & Validation**
   - [ ] Test full build process end-to-end
   - [ ] Verify: ESP + voices generated in one run
   - [ ] Validate all voices load in-game
   - [ ] Performance: Build time acceptable?

**Success Criteria:**
- `dotnet run` generates ESP + all voices automatically ✓
- Build time < 10 minutes ✓
- No manual voice copying needed ✓
- System is robust to failures ✓

**Estimated Time:** 5-7 days (integration + testing)

---

### Phase 6: Polish & Release (2-3 days)
**Goal:** Final quality pass and public release preparation

**Tasks:**
1. **Final Quality Review**
   - [ ] Full playthrough of all affinity scenes
   - [ ] Listen to every voice in context
   - [ ] Check lip-sync in various camera angles
   - [ ] Verify no audio glitches or pops

2. **Documentation Update**
   - [ ] Update README with custom voice info
   - [ ] Credit AI tools used
   - [ ] Document build process changes
   - [ ] Create CHANGELOG entry

3. **Mod Description**
   - [ ] Nexus Mods page creation/update
   - [ ] Screenshots/video showcase
   - [ ] Highlight: "First companion mod with fully AI-generated custom voice"
   - [ ] Installation instructions

4. **Release Build**
   - [ ] Create clean build with all voices
   - [ ] Package: ESP + Voice files + Scripts
   - [ ] Test installation on fresh FO4 install
   - [ ] Upload to Nexus Mods

5. **Community Feedback**
   - [ ] Monitor comments/bug reports
   - [ ] Address voice quality issues quickly
   - [ ] Iterate based on user feedback

**Success Criteria:**
- Mod released on Nexus with custom voices ✓
- Positive community reception ✓
- No major bugs reported ✓
- User can enjoy full CompanionClaude experience ✓

**Estimated Time:** 2-3 days (polish + release)

---

## Timeline Estimate

**Total: 20-30 days** (assuming part-time work, 2-4 hours/day)

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Phase 1: Tool Setup | 2-3 days | None |
| Phase 2: Test Generation | 3-5 days | Phase 1 complete |
| Phase 3: Batch Script | 5-7 days | Phase 2 complete |
| Phase 4: Full Generation | 3-5 days | Phase 3 complete |
| Phase 5: Build Integration | 5-7 days | Phase 4 complete |
| Phase 6: Polish & Release | 2-3 days | Phase 5 complete |

**Fast track (aggressive):** 20 days
**Realistic (with iterations):** 30 days
**Conservative (with delays):** 45 days

---

## Decision Points

### Decision 1: Voice Generation Method
**When:** Phase 2
**Options:**
- **ElevenLabs API:** Fast, high quality, $5-30/month, internet required
- **xVASynth Local:** Free, slower, requires GPU, model training
- **FallTalk:** Simplest, limited control, good for prototyping

**Recommendation:** Start with ElevenLabs for Phase 2-4, optionally switch to local for Phase 5+ to avoid recurring costs

---

### Decision 2: Voice Character
**When:** Phase 2
**Considerations:**
- Should Claude sound robotic/synthetic (AI theme)?
- Or warm/human to emphasize her learning humanity?
- Reference voices: GLaDOS? Cortana? EDI from Mass Effect?

**Recommendation:** Warm but slightly measured tone - shows AI origin but emphasizes emotional growth

---

### Decision 3: Integration Approach
**When:** Phase 5
**Options:**
- **Full automation:** Build generates all voices every time (slow but thorough)
- **On-demand:** Only generate voices that don't exist yet (fast, cached)
- **Manual trigger:** Separate command for voice generation (most flexible)

**Recommendation:** On-demand with cache - fast builds, re-generates only when dialogue changes

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Generated voices sound unnatural | Medium | High | Extensive Phase 2 testing, user feedback loop |
| Lip-sync quality poor | Low | Medium | FaceFXWrapper is proven tech, test early |
| .fuz packing fails | Low | High | Find existing packer tool, fallback to manual |
| API costs exceed budget | Medium | Low | Cap generation runs, use local tools as backup |
| Build process too slow | Medium | Medium | Implement caching, allow skip flag for testing |
| Community negative reaction | Low | Medium | Be transparent about AI use, offer traditional option |

---

## Success Metrics

**Technical:**
- ✅ All 211 lines generated with <5% re-generation rate
- ✅ Build process completes in <10 minutes
- ✅ No audio glitches or lip-sync issues in-game

**Quality:**
- ✅ Voice fits Claude's AI-learning-humanity character
- ✅ Emotional range appropriate for all scenes
- ✅ Consistent quality across all dialogue

**User Experience:**
- ✅ Users report positive feedback on voice
- ✅ No complaints about audio quality
- ✅ Voice enhances immersion rather than breaking it

---

## Future Enhancements (Post-Release)

### Dynamic Dialogue Generation
- Generate NEW dialogue lines on-the-fly
- Respond to mod updates without re-recording
- Community-contributed dialogue via AI voice

### Multi-Language Support
- Generate voices in other languages
- Expand CompanionClaude to non-English players

### Voice Modulation
- Vary Claude's voice based on affinity level
- Subtle changes: warmer tone at higher affinity
- More robotic at neutral, more human at romance

### Voice Packs
- Let users choose from multiple voice options
- "Professional," "Casual," "Sarcastic" variants
- Community-contributed voice models

---

## Budget Considerations

**One-time costs:**
- None (all tools are free or one-time purchase)

**Recurring costs (if using ElevenLabs):**
- Starter plan: $5/month (30K characters/month)
- Creator plan: $22/month (100K characters/month)
- Pro plan: $99/month (500K characters/month)

**CompanionClaude dialogue estimate:**
- 211 lines × ~50 characters avg = ~10,500 characters
- Fits easily in Starter plan ($5/month)
- Cancel after generation complete

**Alternative (free):**
- xVASynth: 100% free, requires local GPU
- Training time: 2-4 hours for voice model
- Generation: ~30 seconds per line
- Total cost: $0 (just time and electricity)

---

## Resources Needed

**Software:**
- Visual Studio / VS Code (C# development)
- Python 3.8+ (scripting)
- Git (version control)
- Audacity (audio review, optional)

**Hardware:**
- For ElevenLabs: Any PC with internet
- For xVASynth: GPU with 4GB+ VRAM recommended

**Time:**
- Developer: 20-30 days part-time
- User testing: 2-3 days
- Community feedback cycle: Ongoing

**Skills:**
- C# programming (Mutagen integration)
- Python scripting (batch processing)
- Audio editing basics (quality review)
- Mod testing (in-game validation)

---

## Conclusion

Voice generation for CompanionClaude is **technically feasible** with available tools. The roadmap provides a clear path from proof-of-concept to full integration.

**Key advantages:**
- Unique voice identity for Claude
- Unlimited dialogue expansion potential
- No dependency on vanilla voice files
- Professional-quality results achievable

**Next steps:**
1. Complete Phase 1 (Tool Setup)
2. Generate test voices (Phase 2)
3. Get user feedback before committing to full generation

**The future is exciting!** 🎤✨

---

*Last updated: 2026-02-06*
*Ready to begin: Phase 1 - Tool Setup & Testing*
