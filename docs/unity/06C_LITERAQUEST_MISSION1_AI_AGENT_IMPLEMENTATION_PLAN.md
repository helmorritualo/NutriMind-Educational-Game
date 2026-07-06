# AI Agent Implementation Plan — Grade 5 Area 1 MVP

## Purpose

This document tells the AI coding agent what to implement after the project owner has built the **Grade 5 Area 1 Parade Meadow MVP scene** and designed the needed Canvas UI panels.

This is not a phase plan. It is a focused checklist for one playable gameplay-loop proof.

```text
Current task:
Wire Grade 5 Mission 1 Area 1 only.
```

Do not implement the full mission yet.

---

# Current target

```text
Scene: LiteraQuest_Term1_Mission1_MVP
Mission: Grade 5 — Festival Storybook Rescue
Area: Area 1 — Parade Meadow
```

The runtime should prove this loop:

```text
Interact with Farmer Lira
→ read dialogue
→ inspect parade clues / Knowledge Lock
→ answer 3 questions
→ receive hint on wrong answer
→ unlock Story Map Fragment
→ pick up Story Map Fragment
→ inventory updates to 1/1
→ gate opens
→ Area Complete panel appears
```

---

# Do not begin until

The project owner has placed or approved the following scene objects:

```text
__PLAYER__/Player
__PLAYER__/PlayerSpawnPoint
__CAMERA__/Main Camera
__ENVIRONMENT__/Ground_ParadeMeadow
__G5_AREA01_PARADE_MEADOW__/G5_NPC_FarmerLira
__G5_AREA01_PARADE_MEADOW__/G5_Interactable_ParadeClueSet
__G5_AREA01_PARADE_MEADOW__/G5_KnowledgeLock_ParadeMeadow
__G5_AREA01_PARADE_MEADOW__/G5_Collectible_StoryMapFragment
__G5_AREA01_PARADE_MEADOW__/G5_Gate_To_Area02
__UI_CANVAS__/Canvas_GameplayHUD
__UI_CANVAS__/Canvas_MissionPanels
```

The UI does not need final art polish, but it must have the required TextMeshPro fields and buttons listed in `06B_LITERAQUEST_MISSION1_SCENE_OBJECT_AND_ASSET_CHECKLIST.md`.

---

# Required implementation scope

Implement only the Area 1 logic.

Required systems or equivalent components:

```text
MissionArea01RuntimeController
MissionArea01DataLoader
PlayerInteractionController
InteractableMissionTrigger
MissionDialoguePanelBinder
KnowledgeLockQuestionController
MissionFeedbackPanelBinder
MissionObjectiveTrackerBinder
MissionInventoryCounterBinder
MissionCollectiblePickup
MissionGateUnlocker
AreaCompletePanelBinder
MissionMvpResetTool
```

These may be implemented as reusable generic mission classes, but the current wiring must stop at Area 1 completion.

---

# Data model required for MVP

Use ScriptableObject or local JSON. Do not hardcode all content inside MonoBehaviours.

Minimum required data:

```text
mission_id
area_id
npc_id
interactable_id
knowledge_lock_id
question_id
question text
answer options
correct answer
wrong-answer hint
collectible_id
gate_id
objective text steps
area complete title/body
```

Recommended IDs:

```text
mission_id: literaquest_t1_m01_g5_area01_mvp
full_mission_id: literaquest_t1_m01_g5
area_id: g5_a01_parade_meadow
npc_id: npc_g5_farmer_lira
interaction_id: interactable_g5_a01_parade_object_cluster
knowledge_lock_id: g5_lock_story_parts
collectible_id: g5_story_map_fragment
gate_id: gate_g5_area01_to_area02_stub
```

---

# Step-by-step AI implementation

## 1. Inspect the project

Read:

```text
AGENTS.md
.cursor/rules/*
UNITY_AI_ASSISTANT_CUSTOM_INSTRUCTIONS.txt
README.md
docs/UNITY_REQUIREMENTS.md
docs/unity/06_GAMEPLAY_MISSIONS_CURRENT_PLAN.md
docs/unity/06A_LITERAQUEST_TERM1_MISSION1_PLAN.md
docs/unity/06B_LITERAQUEST_MISSION1_SCENE_OBJECT_AND_ASSET_CHECKLIST.md
docs/unity/06C_LITERAQUEST_MISSION1_AI_AGENT_IMPLEMENTATION_PLAN.md
docs/unity/examples/literaquest-term1-mission1-demo-data.json
```

Inspect existing movement, camera, input, UI, and scene-management code before adding new code.

## 2. Preserve existing work

Do not delete quiz work, app scenes, login, profile, settings, provider infrastructure, generated quiz assets, or existing useful tests.

Do not continue quiz implementation in this session.

## 3. Bind player and camera

Implement or reuse:

```text
basic player movement
player interaction sensor
camera follow / static camera framing
```

The player must be able to walk to Farmer Lira, the Parade Clue Set, the Knowledge Lock, the Story Map Fragment, and the gate.

## 4. Implement interaction prompt

When the player enters an NPC/object trigger:

```text
show InteractPrompt
store current interactable
allow keyboard interaction for editor testing
allow UI/tap interaction for Android testing
hide prompt when leaving trigger
```

## 5. Implement Farmer Lira dialogue

Interaction with `G5_NPC_FarmerLira` should:

```text
open DialoguePanel
show NPCNameText = Farmer Lira
show dynamic dialogue body
on Continue, close dialogue
update objective to inspect parade clues
```

## 6. Implement Parade Clue / Knowledge Lock interaction

Interaction with `G5_Interactable_ParadeClueSet` or `G5_KnowledgeLock_ParadeMeadow` should:

```text
open KnowledgeLockIntroPanel
show challenge title and instructions
on StartQuestionsButton, open QuestionPanel
```

## 7. Implement question flow

QuestionPanel must support:

```text
3 questions
4 answer buttons
single correct answer
wrong answer hint
retry until correct
question counter text
next question transition
lock-complete callback after all questions are correct
```

Wrong answers must not fail the mission.

## 8. Unlock Story Map Fragment

After all three questions are correct:

```text
mark Knowledge Lock complete
activate G5_Collectible_StoryMapFragment or enable pickup state
show ItemUnlockedPanel
update objective to pick up Story Map Fragment
```

The collectible must not be pickupable before this moment.

## 9. Implement pickup

When the player touches/interacts with `G5_Collectible_StoryMapFragment` after it is unlocked:

```text
add g5_story_map_fragment to local mission state
hide or mark collectible as collected
update InventoryCounterPanel from 0/1 to 1/1
update objective to go through the opened gate
```

## 10. Open gate

After Story Map Fragment pickup:

```text
disable GateBlockerCollider
hide or change GateClosedVisual
show GateOpenVisual if present
mark gate_g5_area01_to_area02_stub open
```

The gate does not need a real Area 2 behind it yet.

## 11. Show Area Complete

After the gate opens or after the player reaches `G5_Area01_EndMarker`:

```text
show AreaCompletePanel
show title: Parade Meadow Complete
show body: Story Map Fragment collected. Sign Repair Barn is locked for later.
```

Do not show full mission complete yet.

## 12. Add reset/debug support

Add a simple editor/demo reset action that restores:

```text
questions incomplete
collectible hidden/not pickupable
inventory count 0/1
gate closed
objective reset
all panels hidden
```

---

# Tests and checks to run

Run what is available in the project and manually test the scene.

Minimum test route:

```text
1. Open LiteraQuest_Term1_Mission1_MVP.
2. Press Play.
3. Move to Farmer Lira.
4. Confirm InteractPrompt appears.
5. Interact and read dialogue.
6. Confirm objective changes.
7. Interact with Parade Clue Set or Knowledge Lock.
8. Answer one question wrong.
9. Confirm hint appears and retry is allowed.
10. Answer all questions correctly.
11. Confirm Story Map Fragment appears/unlocks.
12. Pick up Story Map Fragment.
13. Confirm inventory changes to 1/1.
14. Confirm gate opens and blocker collider disables.
15. Confirm Area Complete panel appears.
16. Check Console for errors.
```

Report exact checks run and exact results. Do not claim a test passed unless it actually ran.

---

# Explicitly deferred

Do not implement these in the Area 1 MVP session:

```text
Grade 5 Area 2
Grade 5 Area 3
Grade 5 final restoration
Grade 6 Mission 1
Mission 2
server mission APIs
audio
VFX
reward economy
shop
EXP/coins
inventory economy
drag-and-drop minigames
```

---

# Completion report format

At the end, report:

```text
completed MVP scope
files changed
scenes changed
prefabs changed
data files changed
UI references wired
scene objects wired
placeholders created
owner tasks still needed
tests/checks run
exact results
Console errors/warnings
remaining gaps
next recommended step
```

Stop after reporting.
