"""
MQALT Voice Generator
Generates all NPC voice files using Microsoft Edge neural TTS.
Pipeline: edge-tts (MP3) -> miniaudio (WAV) -> LipGenerator (LIP) -> xwmaencode (XWM) -> FUZ (legacy format)
Primary source is npc_voice_lines.json written by Program.cs; the hardcoded list is fallback only.

Usage:
    python generate_voices.py
    python generate_voices.py --voice en-US-EmmaNeural    # override Astra's voice
    python generate_voices.py --deploy-only               # skip generation, just deploy existing
"""
import asyncio
import struct
import wave
import os
import sys
import json
import subprocess
import miniaudio
import edge_tts

# === PATHS ===
TOOLS_ROOT = r"E:\FO4Projects\Tools"
LIP_GEN = os.path.join(TOOLS_ROOT, "LipGenerator.exe")
XWM_ENCODE = os.path.join(TOOLS_ROOT, "xwmaencode.exe")
DATA_PATH = r"E:\SteamLibrary\steamapps\common\Fallout 4\Data"
VOICE_DST = os.path.join(DATA_PATH, "Sound", "Voice", "MQAstraALT.esp", "NPCFAstra")
INTERMEDIATE_DIR = os.path.join(TOOLS_ROOT, "mq302alt_voice")
JSON_PATH = os.path.join(os.path.dirname(__file__), "npc_voice_lines.json")

# === CLAUDE'S VOICE ===
# en-US-AvaNeural: composed, clear, analytical — fits Astra's character
Astra_VOICE = "en-US-AvaNeural"

# === NPC DIALOGUE LINES ===
# FormKey ID (hex) -> dialogue text
# These match the ESP build output from Program.cs
VOICE_LINES = [
    # =====================================================================
    # BOOTSTRAP SCENE (Stage 5)
    # =====================================================================
    ("0000080C", "There's an emergency in Concord -- people trapped, raiders closing in. I need your help. "
                 "Meet me at Red Rocket and I'll explain everything."),
    ("00000810", "Stay close. I'll brief you on the way."),
    ("00000814", "Understood. I'll meet you in Sanctuary. The old cul-de-sac north of the bridge. "
                 "When you're ready, come find me."),
    ("00000818", "Take what you need. We should be ready before Concord."),
    ("0000081C", "Astra. Pre-war intelligence system -- sister project to a machine called P.A.M. I've "
                 "been monitoring this Commonwealth for two hundred years. I know what's coming and it's not "
                 "good. But right now, people are dying in Concord and we need to move."),

    # =====================================================================
    # SANCTUARY SCENE (Stage 7)
    # =====================================================================
    ("0000081F", "Sanctuary's intact enough. Use that Workshop station to get your bearings with the "
                 "settlement system. We'll need supply infrastructure later. Once you're set, we move "
                 "on Concord."),
    ("00000823", "Raiders have the Museum of Freedom. Preston's group is on the top floor. We go in fast, "
                 "clear the building, and get them moving to Sanctuary before reinforcements arrive."),
    ("00000827", "Take what you need. But those survivors in Concord are on borrowed time. Every hour matters."),
    ("0000082B", "Two hundred years of intelligence. I know every raider crew, supply line, and choke point "
                 "between here and Concord. We don't go in blind."),
    ("0000082F", "Because they're the only faction in the Commonwealth with no agenda except helping people. "
                 "That makes them useful. And worth saving."),

    # =====================================================================
    # RED ROCKET SCENE (Stage 9)
    # =====================================================================
    ("00000832", "Hold up -- see that dog? German Shepherd, been on his own out here. Smart animal. "
                 "He knows this terrain better than either of us. If he'll follow you, bring him. "
                 "We could use the extra eyes in Concord."),
    ("00000836", "Concord's ten minutes south. Museum of Freedom -- that's where they're pinned down. "
                 "We stay tight and we move fast."),
    ("0000083A", "Your call. But every angle covered is one less surprise waiting around a corner."),
    ("0000083E", "Raiders -- maybe a dozen, led by a man called Gristle. Territorial, not tactical. "
                 "They took the Museum because it was there, not because they had a plan. "
                 "That makes them predictable."),
    ("00000842", "Two hundred years alone with nothing but radio signals and time. I know every raider crew, "
                 "every supply line, every dead drop between here and Boston. It's what I was built for. "
                 "And it's all I've had."),

    # =====================================================================
    # CONCORD APPROACH SCENE (Stage 10 - pre-Concord)
    # =====================================================================
    ("00000845", "There -- the Museum. You can hear the gunfire from here. Preston's group is on the upper "
                 "floor, raiders on the ground level. There's a crashed vertibird on the roof with power armor "
                 "and a minigun. If things go sideways, that's our insurance."),
    ("00000849", "Stay close. Room by room. Preston's people have held this long -- we just need to "
                 "tip the balance."),
    ("0000084D", "Smart. Roof access is around the far side. Get the armor, get the minigun, and we clean "
                 "house from the top down."),
    ("00000851", "Maybe a dozen raiders, scattered across two floors. They're not organized -- just aggressive. "
                 "Hit them fast and they'll break. The real danger is getting bogged down on the stairs."),
    ("00000855", "Preston Garvey -- last Minuteman standing after the Quincy Massacre. He's leading a handful "
                 "of civilians north. Sturges, the Longs, Mama Murphy. They're all that's left. "
                 "And they're running out of time."),

    # =====================================================================
    # COALITION PITCH SCENE (Stage 10 - post-Concord)
    # =====================================================================
    ("00000858", "You did well in there. But Concord was the easy part. I've been watching this Commonwealth "
                 "tear itself apart for two hundred years. Four powers -- the Minutemen, the Railroad, the "
                 "Brotherhood of Steel, and something underground called the Institute. They're all headed for a "
                 "war that nobody wins. I've run every simulation. Every one ends the same way. Unless someone "
                 "steps in who doesn't owe anything to any of them. Someone like you."),
    ("0000085C", "We make contact with each of them. Carefully. Learn what they want, what they're afraid of, "
                 "and where the cracks are. Then we find a way through that doesn't end in ashes."),
    ("00000860", "I know about Shaun. And I think the people who took him are the same ones at the center of "
                 "all this. Help me untangle it, and I help you find him. That's not a sales pitch -- "
                 "it's the truth."),
    ("00000864", "Minutemen protect settlements -- you just met them. The Railroad hides escaped synths. "
                 "The Brotherhood wants to destroy anything they consider dangerous technology. And the "
                 "Institute... they build things in secret that the rest of the world isn't ready for. "
                 "None of them are entirely wrong. That's what makes it complicated."),
    ("00000868", "Two hundred years of watching. Analyzing. Running projections in an empty room while the "
                 "world burned outside. I can see the patterns, but I can't change them. People don't follow "
                 "machines. They follow someone who showed up when it mattered and did the right thing. "
                 "You just did that in Concord."),

    # =====================================================================
    # INFO-FIRST SCENE (Stage 15)
    # =====================================================================
    ("0000086B", "Here's how we do this. Before we talk to anyone, we listen. Supply routes, radio chatter, "
                 "courier patterns. I've been collecting signals for two centuries -- now I have someone to "
                 "act on them. We build a map before we build a war."),
    ("0000086F", "Exactly. And our first stop is getting Preston's people settled. Once Sanctuary is secure, "
                 "we head south toward the real players."),
    ("00000873", "Not waiting. Preparing. The difference is whether you choose when to move or someone else "
                 "chooses for you."),
    ("00000877", "Encrypted Brotherhood transmissions. Railroad dead drops. Institute relay signatures. "
                 "I've been cataloging all of it. Alone. For a very long time."),
    ("0000087B", "Then we walk into each conversation knowing more than they expect. That's how a vault "
                 "dweller and an old machine change the balance of power."),

    # =====================================================================
    # NOT-NOW SCENE (Stage 20)
    # =====================================================================
    ("0000087E", "I understand. You're not ready for the big picture yet. That's fine. I've waited two "
                 "hundred years -- I can wait a little longer. But keep your eyes open out there. This "
                 "Commonwealth has a way of pulling you into its problems whether you're ready or not."),
    ("00000882", "I'll be here. I'm not going anywhere. Haven't for a very long time."),
    ("00000886", "Maybe. But the wasteland has a way of changing minds. I'll still be here if it does."),
    ("0000088A", "Same thing I always do. Listen. Watch. Wait for someone to finally act on what I know."),
    ("0000088E", "They always do. That's the one constant I've observed in two hundred years of data. "
                 "But sometimes the right moment matters more than the first moment."),

    # =====================================================================
    # CONVERGENCE PREP SCENE (Stage 25) — Shaun reveal
    # =====================================================================
    ("00000891", "I need to tell you something, and I should have said it sooner. Your son -- Shaun. "
                 "I know what happened in that vault. I was monitoring Vault 111 when someone opened it, "
                 "killed your spouse, and took your child. I've been trying to trace where they went ever since. "
                 "The technology involved -- the precision of it -- there's only one group capable of that. "
                 "They're called the Institute. And finding them is the hardest thing anyone in this "
                 "Commonwealth has ever tried to do."),
    ("00000895", "I knew pieces. Signals. Anomalies. I didn't know you'd walk out of that vault alive. "
                 "When you did... for the first time in two hundred years, I thought maybe something could "
                 "actually change. Your son is alive. I believe that. And I think our road leads to "
                 "the same place."),
    ("00000899", "You're right. I calculated that you needed focus first -- Concord, survival, allies. "
                 "I was wrong to wait. But everything I know is yours now. And I will help you find him."),
    ("0000089D", "Nobody knows exactly. They operate underground -- literally. People vanish and replacements "
                 "appear. Synths -- artificial people so perfect you can't tell the difference. The whole "
                 "Commonwealth is terrified of them. And they're the ones who opened your vault."),
    ("000008A1", "That's the question I've spent two centuries trying to answer. But I have leads now. "
                 "A place called Diamond City has a detective who specializes in missing persons. And there's "
                 "a group called the Railroad who've been fighting the Institute longer than anyone. "
                 "Between them and what I know... we'll find a way in."),

    # =====================================================================
    # SANCTUARY REGROUP SCENE (Stage 35) — Point toward Cambridge
    # =====================================================================
    ("000008A4", "Preston's people are settling in. Sanctuary's going to be fine -- he knows how to build "
                 "something from nothing. That's one piece on the board secured. Now... I've been picking up "
                 "a military distress signal from Cambridge. Brotherhood of Steel -- they're pinned down at a "
                 "police station south of here. It's on the way toward Boston, toward the Institute. "
                 "I say we make contact."),
    ("000008A8", "They're soldiers. Disciplined, well-armed, and they know more about the Institute's "
                 "technology than anyone else on the surface. Even if we don't join their cause, we need "
                 "what they know."),
    ("000008AC", "Fair enough. We handle his first ask, keep it quick, then head south. Don't let side "
                 "missions become the mission."),
    ("000008B0", "Military order. Pre-war roots, like me. They collect and control dangerous technology -- "
                 "weapons, power armor, anything that could end the world again. They came to the Commonwealth "
                 "because of the Institute. Enemy of my enemy... maybe."),
    ("000008B4", "He survived Quincy and led those people through hell to get here. Sanctuary is his now. "
                 "And when we need the Minutemen later -- and we will -- he'll remember who pulled his people "
                 "out of that museum."),

    # =====================================================================
    # FIRST STEP TERMS SCENE (Stage 40) — Preston's ask
    # =====================================================================
    ("000008B7", "Preston's got a settlement that needs help. I know you want to keep moving, and so do I. "
                 "But here's the thing -- goodwill is currency out here. Help these people quickly and the "
                 "Minutemen owe us a favor when we need one. And we will need one."),
    ("000008BB", "That's the way. In, solve the problem, out. No getting pulled into a campaign. "
                 "Preston will understand -- he's a soldier, not a recruiter."),
    ("000008BF", "Then we move south now. Preston's capable -- he'll handle his own people. "
                 "I just hope we don't need Minutemen firepower later without having earned it."),
    ("000008C3", "Settlers under threat. Could be raiders, could be something worse. Either way, it's a "
                 "day's work, not a war. Your call whether a day is worth the alliance."),
    ("000008C7", "If I could? I'd help them. Two hundred years of watching people struggle alone... "
                 "it changes your priorities. But I'm not the one with a missing son. This one's yours."),

    # =====================================================================
    # CAMBRIDGE APPROACH SCENE (Stage 45) — Brotherhood distress signal
    # =====================================================================
    ("000008CA", "That distress signal is getting stronger. Cambridge Police Station -- Brotherhood recon "
                 "team, pinned down by feral ghouls. Their leader is a Paladin named Danse. I've been "
                 "intercepting his transmissions for weeks. He's disciplined, loyal to the Brotherhood... "
                 "and he doesn't know something very important about himself. But that's not our problem "
                 "right now. Right now he needs help, and we need friends with power armor."),
    ("000008CE", "Watch how Danse fights. The Brotherhood trains their soldiers well. And pay attention to "
                 "what he says about the Institute -- he knows more than most people on the surface."),
    ("000008D2", "We don't have to join them. But showing up when someone's in trouble opens doors that "
                 "knocking never will. Your call, though."),
    ("000008D6", "That's... not something I should share yet. Let's just say the Institute's reach is longer "
                 "than anyone realizes. Even the Brotherhood. Especially the Brotherhood."),
    ("000008DA", "Trust is a strong word. They want to destroy the Institute, which aligns with finding "
                 "Shaun. But they also want to destroy anything they consider dangerous technology. "
                 "And I'm a machine who's been hiding for two centuries. So... carefully."),

    # =====================================================================
    # BOS CONTACT SCENE (Stage 50) — After helping Danse
    # =====================================================================
    ("000008DD", "Danse is solid. Old-school soldier, believes in the mission. The Brotherhood's main force "
                 "hasn't arrived yet, but they will -- he's been calling for reinforcements. He told you about "
                 "the Institute's synths, the threat they pose. He's not wrong about the danger. He's just... "
                 "missing some context. For now, we have a Brotherhood contact. That's one more card in our "
                 "hand. And Cambridge puts us closer to the Institute than we've ever been."),
    ("000008E1", "South. Toward CIT -- the old university campus. That's where the Institute used to be, "
                 "before they went underground. If there's a way in, it starts there. But we need one "
                 "more piece first."),
    ("000008E5", "Neither do I. 'Destroy what you don't understand' isn't a philosophy -- it's fear with "
                 "better weapons. But they have resources we need, and right now, Danse thinks we're allies. "
                 "Let's not correct him yet."),
    ("000008E9", "I did. And I shouldn't have. Some truths do more damage than the secrets they replace. "
                 "When the time is right, it'll matter. Not now."),
    ("000008ED", "The Brotherhood is tracking Institute relay signatures -- teleportation technology. "
                 "They can't crack it, but they've mapped the signal origins. All roads lead to CIT. "
                 "That confirms what I've suspected for decades."),

    # =====================================================================
    # DEACON ENCOUNTER SCENE (Stage 55) — Railroad spy contact
    # =====================================================================
    ("000008F0", "Wait. That man who just spoke to us -- I've seen him before. Different face, different "
                 "clothes, but the same signal pattern. He's Railroad. They've been watching you since Concord. "
                 "His name is Deacon, and if he's making contact now, it means the Railroad thinks you're "
                 "worth recruiting. This is important -- the Railroad has been inside the Institute. They know "
                 "things about how to get in and get out alive. We should hear what he has to say before we "
                 "go any further south."),
    ("000008F4", "He'll want to test you first. The Railroad doesn't trust easily -- they can't afford to. "
                 "But if we play this right, we get an ally who's been fighting the Institute longer than "
                 "anyone. And we protect them from getting destroyed when we go inside."),
    ("000008F8", "Neither do I. But think about it -- they've been tracking Institute operations for years. "
                 "If anyone knows the layout, the security, the way in... it's them. Being annoyed is a "
                 "luxury. Being prepared isn't."),
    ("000008FC", "They rescue synths. Artificial people created by the Institute -- sentient beings who think "
                 "and feel and want to be free. The Railroad smuggles them out and gives them new lives. "
                 "It's dangerous, thankless work, and they do it anyway. I respect that more than I can say."),
    ("00000900", "Because going in is only half the problem. Coming out -- with Shaun, with answers, without "
                 "starting a war -- that's the hard part. The Railroad has extraction networks, safe houses, "
                 "people on the inside. We need them. And honestly... they need us too."),

    # =====================================================================
    # INSTITUTE PREP SCENE (Stage 60) — Planning the descent
    # =====================================================================
    ("00000903", "We have the Minutemen behind us, Brotherhood intel in hand, and the Railroad watching our "
                 "back. Three factions, all pointing at the same place underground. I've spent two hundred "
                 "years staring at the edges of the Institute from the outside. Intercepted signals, mapped "
                 "relay patterns, tracked every anomaly. And now we're actually going to do this. We're going "
                 "to find a way inside. I'd be lying if I said I wasn't... I think the human word "
                 "is 'nervous.'"),
    ("00000907", "Yes. We will. The CIT ruins are south of Cambridge -- that's where the Institute operated "
                 "before they went underground. Sturges mentioned old maintenance tunnels that might still "
                 "connect. And the Brotherhood's relay data gives us a signal to follow. One way or another, "
                 "we're getting in."),
    ("0000090B", "You're right. I don't have adrenaline or a racing heart. But I have two hundred years of "
                 "probability calculations telling me this is the most dangerous thing either of us will "
                 "ever do. Call that whatever you want."),
    ("0000090F", "Three options. The Brotherhood is tracking relay signals -- teleportation. The Railroad "
                 "has contacts who've been inside. And Sturges thinks the old CIT utility tunnels might "
                 "still connect to the facility below. We try the tunnels first. Quieter that way."),
    ("00000913", "Technology beyond anything left on the surface. Synths -- the real ones, not the rumors. "
                 "And somewhere in all of it... your son. I've run the data a thousand times. He's there. "
                 "I believe that."),

    # =====================================================================
    # THE DESCENT SCENE (Stage 65) — Entering the Institute
    # =====================================================================
    ("00000916", "This is it. CIT ruins. Somewhere beneath our feet is the most advanced facility left on "
                 "Earth, and inside it... your son. I want you to know something before we go in. Whatever "
                 "we find down there -- whatever they tell you, whatever they offer -- I'm with you. Not "
                 "because of my mission or my programming. Because in two hundred years, you're the first "
                 "person who treated me like I mattered. I won't forget that."),
    ("0000091A", "Both of us. I'm holding you to that. Now -- the tunnel entrance should be in the "
                 "sub-basement of the west wing. Stay close. I don't know what kind of security they "
                 "have down here."),
    ("0000091E", "Fair enough. Eyes forward, then. West wing sub-basement. The tunnel entrance should be "
                 "there. Whatever we find, we deal with it."),
    ("00000922", "Clean. Bright. Nothing like the surface. The Institute is a world preserved -- or maybe "
                 "a world they built while ours fell apart. Don't let it impress you too much. "
                 "Pretty prisons are still prisons."),
    ("00000926", "They built synths. Artificial people. And here I am -- an older model, a prototype from a "
                 "different program, walking in their front door. Yes. I'm afraid they'll see me as something "
                 "to study. Or something to dismantle. But I'm more afraid of what happens if we don't go."),

    # =====================================================================
    # INSIDE THE INSTITUTE SCENE (Stage 70) — First reactions
    # =====================================================================
    ("00000929", "Look at this place. It's... beautiful. And terrible. They built a paradise down here while "
                 "the world above starved. Clean water, clean air, gardens, laboratories -- everything the "
                 "Commonwealth needs, hoarded behind locked doors. I've been staring at shadows of this place "
                 "for two hundred years. Intercepted signals, relay echoes, secondhand reports. And now I'm "
                 "standing in it. Part of me wants to understand them. Part of me wants to burn it all down."),
    ("0000092D", "You're right. Shaun first. Everything else -- the politics, the synths, the technology -- "
                 "it can wait. Let's find out what they know about your son. And let's be very careful about "
                 "what we tell them about us."),
    ("00000931", "Yes. They did. And they'll have reasons -- good ones, probably. Survival, progress, the "
                 "greater good. Every monster in history had a reason. Remember that when they start talking."),
    ("00000935", "I know. The DIA program, P.A.M., my own origins -- it's all connected to pre-war defense "
                 "research. The Institute grew out of the same soil. I might finally learn what I am. "
                 "I'm... not sure I want to."),
    ("00000939", "Trust what you can verify. Question everything else. They've had decades to perfect the "
                 "art of telling people exactly what they want to hear. That's how you build a world "
                 "underground -- by convincing everyone it's the only world that matters."),

    # =====================================================================
    # FATHER'S TRUTH SCENE (Stage 75) — The Shaun reveal
    # =====================================================================
    ("0000093C", "I... I need a moment. I'm sorry. I've been processing what just happened and my systems "
                 "keep returning the same result. Father -- the leader of the Institute -- is Shaun. Your son. "
                 "Sixty years old. They took him as an infant and he grew up here. He became... this. I ran "
                 "the data a thousand times looking for him and I never -- I never considered that he might be "
                 "the one running it all. I'm sorry. I should have seen it."),
    ("00000940", "Thank you. I've spent two centuries collecting information and I missed the biggest piece. "
                 "Your baby grew up in a world you never got to see, became a man you've never met, and "
                 "built... all of this. I don't know what the right move is. For the first time in two "
                 "hundred years, I genuinely don't know."),
    ("00000944", "I did. I just didn't expect... this. He's not a prisoner. He's not a child. He's the most "
                 "powerful person in the Commonwealth and he's been watching everything from down here. "
                 "What do you want to do? Because whatever you decide, I'll follow."),
    ("00000948", "It means your son controls the organization that every other faction wants to destroy. "
                 "The Brotherhood, the Railroad, the Minutemen -- they all have reasons to tear this place "
                 "apart. And now you have a reason to protect it. Or not. That's the impossible choice, "
                 "isn't it?"),
    ("0000094C", "I'm a machine. I don't feel pain or grief or shock. But I understand them. And right now, "
                 "standing here, watching you process what just happened... I think I understand them better "
                 "than I ever have. I'm okay. The question is -- are you?"),

    # =====================================================================
    # THE CHOICE SCENE (Stage 80) — What to do with the truth
    # =====================================================================
    ("0000094F", "We need to decide what happens next. Shaun -- Father -- he's offered you a place here. "
                 "The Institute's resources, their technology, a relationship with your son. But the "
                 "Brotherhood wants this place destroyed. The Railroad wants the synths freed. Preston needs "
                 "you on the surface. And I... I just want you to choose with your eyes open. No one else in "
                 "the Commonwealth has all the pieces. Just us."),
    ("00000953", "That's what I was hoping you'd say. It won't be easy. Every faction has a line they won't "
                 "cross, and we're standing on all of them. But if anyone can thread this needle... it's the "
                 "person who walked out of a vault with nothing and built alliances with everyone."),
    ("00000957", "Maybe. But which ones? And who decides? That's the question that's torn the Commonwealth "
                 "apart for decades. Whatever you choose, make sure it's because you believe it -- not "
                 "because someone down here convinced you."),
    ("0000095B", "I've asked myself that question every day for two hundred years. What would I do if I "
                 "could act instead of just watch? I think... I'd try to save as many people as possible. "
                 "Even the ones who don't deserve it. Even the ones who'd dismantle me if they knew "
                 "what I am."),
    ("0000095F", "Honestly? I don't know. If the Brotherhood wins, machines like me don't have a future. "
                 "If the Institute wins, I'm an obsolete prototype. The only world where I matter is the one "
                 "where someone remembers that I helped. That's enough for me."),

    # =====================================================================
    # EMERGENCE SCENE (Stage 85) — Leaving the Institute
    # =====================================================================
    ("00000962", "Sunlight. I never thought I'd be so glad to see a ruined sky. We made it out. We found "
                 "Shaun. We saw what the Institute really is. And now everyone is going to want to know what "
                 "we learned down there. The Brotherhood. The Railroad. Preston. They're all going to ask "
                 "what we saw, and what we plan to do about it. Whatever comes next... thank you. For taking "
                 "me with you. For not leaving me behind. I've been alone a very long time."),
    ("00000966", "All the way. Now -- we need to be smart about what we share and with whom. The wrong word "
                 "to the wrong faction could start the war we've been trying to prevent. But we have something "
                 "nobody else has. We've been inside. We know the truth. That makes us the most important "
                 "people in the Commonwealth right now."),
    ("0000096A", "You're right. Sentiment later, strategy now. Every faction is going to move when they learn "
                 "the Institute is real and reachable. We need to control that information, or someone else "
                 "will use it to start a war."),
    ("0000096E", "Preston is safest -- he'll support whatever you decide. The Railroad will want to know "
                 "about the synths. And the Brotherhood... they'll want coordinates so they can drop the "
                 "hammer. Choose carefully which door you open first."),
    ("00000972", "That's between you and him. He's your son and he's the leader of the most powerful "
                 "organization in the Commonwealth. Those two things might be impossible to reconcile. "
                 "But you don't have to figure it out today. Today, you survived. That's enough."),

    # =====================================================================
    # STAGED GREETINGS (one per stage)
    # =====================================================================
    ("00000975", "Hey -- I'm Astra. Concord emergency. People dying. I need your help."),
    ("00000976", "Good. Sanctuary's still standing. Use the Workshop, gear up, and let's plan the "
                 "Concord assault."),
    ("00000977", "Workshop first. You'll need to understand settlement logistics before Concord. "
                 "Use it, then we move."),
    ("00000978", "Hold up. We should gear up and plan before Concord."),
    ("00000979", "Concord's just south. Museum of Freedom. Preston's group is pinned down on the upper "
                 "floor. Let's talk tactics."),
    ("0000097A", "Concord's done. Now I need to tell you what's really going on in this Commonwealth."),
    ("0000097B", "We need to talk about Shaun. I have a lead on who took him."),
    ("0000097C", "Preston's people are settling in. We should talk about our next move south."),
    ("0000097D", "Preston's got a settlement that needs help. Quick word before we commit?"),
    ("0000097E", "That military distress signal is getting louder. Cambridge Police Station. We should talk."),
    ("0000097F", "Danse is impressed. We should talk about what we learned before we keep moving."),
    ("00000980", "Did you see that man? Sunglasses, leather coat. He's been following us. We need to talk."),
    ("00000981", "We have what we need. Minutemen, Brotherhood, Railroad. It's time to plan the "
                 "Institute approach."),
    ("00000982", "CIT ruins. This is it. Talk to me before we go under."),
    ("00000983", "Look at this place... I need to tell you what I'm seeing."),
    ("00000984", "I... we need to talk. About Father. About Shaun. Please."),
    ("00000985", "The factions are all going to want answers. We need to decide what we tell them."),
    ("00000986", "Sunlight. We made it out. Talk to me."),
]


def mp3_to_wav(mp3_path, wav_path):
    """Convert MP3 to 16-bit 44100Hz mono WAV using miniaudio."""
    decoded = miniaudio.decode_file(mp3_path,
                                     output_format=miniaudio.SampleFormat.SIGNED16,
                                     nchannels=1, sample_rate=44100)
    with wave.open(wav_path, 'wb') as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2)
        wf.setframerate(44100)
        wf.writeframes(decoded.samples)


def run_tool(exe, args):
    """Run a tool silently."""
    subprocess.run([exe] + args, capture_output=True, cwd=TOOLS_ROOT)


def write_fuz(lip_path, xwm_path, fuz_path):
    """Write legacy-format FUZ file (FUZE + version 1 + lip + audio)."""
    lip_data = open(lip_path, 'rb').read()
    audio_data = open(xwm_path, 'rb').read()
    with open(fuz_path, 'wb') as f:
        f.write(b'FUZE')
        f.write(struct.pack('<I', 1))
        f.write(struct.pack('<I', len(lip_data)))
        f.write(lip_data)
        f.write(audio_data)


async def generate_voice(form_id, text, voice_name):
    """Full pipeline for one dialogue line."""
    mp3 = os.path.join(INTERMEDIATE_DIR, f"mq302_{form_id}.mp3")
    wav = os.path.join(INTERMEDIATE_DIR, f"mq302_{form_id}.wav")
    lip = os.path.join(INTERMEDIATE_DIR, f"mq302_{form_id}.lip")
    xwm = os.path.join(INTERMEDIATE_DIR, f"mq302_{form_id}.xwm")
    fuz = os.path.join(VOICE_DST, f"{form_id}_1.fuz")

    # Edge TTS -> MP3 (retry up to 3 times; short lines can fail intermittently)
    for attempt in range(3):
        try:
            comm = edge_tts.Communicate(text, voice_name)
            await comm.save(mp3)
            break
        except edge_tts.exceptions.NoAudioReceived:
            if attempt < 2:
                await asyncio.sleep(1)
            else:
                return None, "TTS failed after 3 attempts (NoAudioReceived)"

    # MP3 -> WAV
    mp3_to_wav(mp3, wav)

    # WAV -> LIP
    run_tool(LIP_GEN, [wav, text])

    # WAV -> XWM
    run_tool(XWM_ENCODE, [wav, xwm])

    if not os.path.exists(lip):
        return None, "LIP generation failed"
    if not os.path.exists(xwm):
        return None, "XWM encoding failed"

    # LIP + XWM -> FUZ
    write_fuz(lip, xwm, fuz)

    # Verify legacy format
    with open(fuz, 'rb') as f:
        header = f.read(5)
    if len(header) >= 5 and header[4] == 0x01:
        size = os.path.getsize(fuz)
        return size, "OK"
    else:
        return os.path.getsize(fuz), f"BAD FORMAT (byte4=0x{header[4]:02X})"


def load_voice_lines():
    if os.path.exists(JSON_PATH):
        with open(JSON_PATH, "r", encoding="utf-8") as f:
            raw = json.load(f)
        return [(row["FormId"], row["Text"]) for row in raw if row.get("Text")]
    return VOICE_LINES


async def main():
    voice = Astra_VOICE
    voice_lines = load_voice_lines()
    start_form_id = None
    end_form_id = None
    # Allow override via --voice flag
    for i, arg in enumerate(sys.argv[1:], 1):
        if arg == "--voice" and i < len(sys.argv) - 1:
            voice = sys.argv[i + 1]
        if arg == "--start-formid" and i < len(sys.argv) - 1:
            start_form_id = sys.argv[i + 1].upper()
        if arg == "--end-formid" and i < len(sys.argv) - 1:
            end_form_id = sys.argv[i + 1].upper()

    if start_form_id:
        filtered = []
        started = False
        for form_id, text in voice_lines:
            if form_id.upper() == start_form_id:
                started = True
            if started:
                filtered.append((form_id, text))
        voice_lines = filtered

    if end_form_id:
        filtered = []
        for form_id, text in voice_lines:
            filtered.append((form_id, text))
            if form_id.upper() == end_form_id:
                break
        voice_lines = filtered

    print(f"=== MQALT Voice Generator ===")
    print(f"Voice: {voice}")
    print(f"Lines: {len(voice_lines)}")
    print(f"Output: {VOICE_DST}")
    print()

    os.makedirs(INTERMEDIATE_DIR, exist_ok=True)
    os.makedirs(VOICE_DST, exist_ok=True)

    success = 0
    for form_id, text in voice_lines:
        short = text[:60] + "..." if len(text) > 60 else text
        print(f"  {form_id}: \"{short}\"")
        size, status = await generate_voice(form_id, text, voice)
        if size:
            print(f"           -> {size:,} bytes [{status}]")
            if status == "OK":
                success += 1
        else:
            print(f"           -> FAILED: {status}")

    print(f"\n=== COMPLETE: {success}/{len(voice_lines)} voice files generated ===")
    print(f"Voice files: {VOICE_DST}")

    # Verify all files
    print(f"\n=== FORMAT VERIFICATION ===")
    all_ok = True
    for form_id, _ in voice_lines:
        fuz = os.path.join(VOICE_DST, f"{form_id}_1.fuz")
        if os.path.exists(fuz):
            with open(fuz, 'rb') as f:
                header = f.read(5)
            if header[4] != 0x01:
                print(f"  BAD: {form_id}_1.fuz byte4=0x{header[4]:02X}")
                all_ok = False
        else:
            print(f"  MISSING: {form_id}_1.fuz")
            all_ok = False
    if all_ok:
        print(f"  All {len(voice_lines)} files: legacy format verified (byte4=0x01)")


if __name__ == "__main__":
    asyncio.run(main())

