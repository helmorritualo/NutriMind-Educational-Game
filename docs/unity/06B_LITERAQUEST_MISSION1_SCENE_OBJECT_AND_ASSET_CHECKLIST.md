# Grade 5 Area 1 MVP Scene Object and Asset Checklist

## Purpose

This checklist tells the project owner exactly what to create in the Unity scene for the **Grade 5 LiteraQuest Mission 1 Area 1 MVP** and tells the AI coding agent what objects and UI references it should wire.

The current goal is not the full mission. The current goal is to prove the core gameplay loop in one small area.

```text
Current MVP:
Grade 5 Mission 1 — Festival Storybook Rescue
Area 1 only — Parade Meadow
```

Quiz Portal / Assessment Room work is temporarily on hold. Do not continue quiz implementation during this mission MVP task.

---

# Current MVP decision

Build only **Area 1: Parade Meadow** first.

This area must prove:

```text
Walk/explore
→ interact with NPC/object
→ read dialogue/situation
→ answer Knowledge Lock questions
→ wrong answer shows hint
→ correct answers unlock collectible
→ pick up Story Map Fragment
→ gate/path opens
→ objective and inventory UI update
→ Area Complete panel appears
```

After this loop works, Area 2, Area 3, the final stage, and Grade 6 can reuse the same logic.

---

# What not to create for this MVP

Do not create or require these yet:

```text
Grade 5 Area 2: Sign Repair Barn
Grade 5 Area 3: Main Idea Garden
Grade 5 final Festival Storybook Stage
Grade 6 Mission 1 areas
Mission 2 or future mission areas
Health/PE mission objects
Science mission objects
server mission tracking objects
shop/reward economy objects
audio objects
VFX objects
cutscene system
drag-and-drop objects
```

Audio and VFX are optional later. They are not required for this MVP.

---

# Responsibility split

## Project owner responsibilities

The project owner creates and places:

```text
Parade Meadow environment
player visual asset or placeholder
Farmer Lira NPC model or placeholder
Parade Clue Set prop group
Knowledge Lock pedestal prop
Story Map Fragment collectible prop
closed/open gate visual states
boundary colliders
owner-designed Canvas UI panels
TextMeshPro fields and Button objects inside the UI
```

The project owner decides the visual design and object placement.

## AI coding agent responsibilities

The AI coding agent implements and wires:

```text
player movement if missing
interaction trigger behavior
Interact prompt behavior
Farmer Lira dialogue flow
Knowledge Lock question flow
answer validation
wrong-answer hint and retry
Story Map Fragment unlock
Story Map Fragment pickup
gate opening
objective tracker update
inventory counter update
Area Complete panel
local reset/testing support
```

The AI agent must not redesign the environment or UI. It should wire logic to the owner-created objects and report missing references.

---

# Required asset types for the Area 1 MVP

## 1. Player asset type

Required:

```text
one player character model or placeholder capsule
player collider or CharacterController
player spawn point
interaction sensor trigger
camera target child object
```

Generated asset placement:

```text
Player character asset -> __PLAYER__/Player/PlayerModel
```

Notes:

- The player only needs movement and interaction for this MVP.
- Advanced animation is not required yet.

## 2. Camera asset type

Required:

```text
one Main Camera
one camera follow target or camera target on the player
simple camera framing for Android landscape
```

Notes:

- The camera should show the player, Farmer Lira, the parade clue area, and the gate clearly.
- Camera bounds can be simple colliders or empty markers.

## 3. Environment asset types

Required:

```text
Parade Meadow ground or terrain
walkable path from spawn to NPC
walkable path from NPC to clue object
walkable path from clue object to gate
simple fences/trees/props to guide the route
invisible boundary colliders
```

Recommended environment style:

```text
stylized low-poly festival farm village meadow
```

For this MVP, the environment can be small and linear.

## 4. NPC asset type

Required:

```text
Farmer Lira 3D model or placeholder
NPC collider
NPC interaction trigger
NPC interaction marker child object
NPC dialogue anchor child object
```

Generated asset placement:

```text
Farmer Lira asset -> __G5_AREA01_PARADE_MEADOW__/G5_NPC_FarmerLira/NPC_Model
Interaction marker asset -> __G5_AREA01_PARADE_MEADOW__/G5_NPC_FarmerLira/NPC_InteractionMarker
```

Notes:

- Farmer Lira can be static.
- Dialogue text must be dynamic in the Canvas UI, not baked into the model.

## 5. Interactable clue asset type

Required:

```text
Parade Clue Set prop group
collider
interaction trigger
interaction marker child object
question anchor child object
```

Recommended visual contents:

```text
festival flag
scattered banners
rain-cloud clue
children/banner clue props
parade decoration bundle
```

Generated asset placement:

```text
Parade meadow story object set -> __G5_AREA01_PARADE_MEADOW__/G5_Interactable_ParadeClueSet/ParadeClueSet_Model
Interaction marker asset -> __G5_AREA01_PARADE_MEADOW__/G5_Interactable_ParadeClueSet/ParadeClueSet_InteractionMarker
```

Notes:

- Do not bake story text into the 3D prop.
- The AI agent opens the Knowledge Lock UI from this interactable or from the Knowledge Lock pedestal.

## 6. Knowledge Lock asset type

Required:

```text
Knowledge Lock pedestal model or placeholder
collider
interaction trigger
stable object name
```

Generated asset placement:

```text
Knowledge Lock pedestal asset -> __G5_AREA01_PARADE_MEADOW__/G5_KnowledgeLock_ParadeMeadow/KnowledgeLock_Model
```

Notes:

- This object represents the question challenge.
- The player should not receive the collectible until this lock is completed.

## 7. Collectible asset type

Required:

```text
Story Map Fragment model
pickup trigger collider
inactive or hidden starting state
visible/unlocked state after questions
```

Generated asset placement:

```text
Story Map Fragment asset -> __G5_AREA01_PARADE_MEADOW__/G5_Collectible_StoryMapFragment/StoryMapFragment_Model
```

Starting state:

```text
G5_Collectible_StoryMapFragment inactive or pickup disabled
```

Notes:

- The collectible must not be pickupable before the Knowledge Lock is complete.
- Inventory text must be dynamic: `0/1` then `1/1`.

## 8. Gate/path blocker asset type

Required:

```text
closed gate visual
open gate visual or opened state
blocking collider
```

Generated asset placement:

```text
Closed path gate asset -> __G5_AREA01_PARADE_MEADOW__/G5_Gate_To_Area02/GateClosedVisual
Open path gate asset -> __G5_AREA01_PARADE_MEADOW__/G5_Gate_To_Area02/GateOpenVisual
```

Starting state:

```text
GateClosedVisual active
GateOpenVisual inactive
GateBlockerCollider enabled
```

After Story Map Fragment pickup:

```text
GateClosedVisual inactive or changed
GateOpenVisual active
GateBlockerCollider disabled
```

Notes:

- The gate does not need to lead to a real Area 2 yet.
- The purpose is to prove the area-unlock mechanic.

## 9. Dynamic Canvas UI asset types

The project owner designs the UI. The AI agent wires logic and dynamic data to it.

Required UI groups:

```text
Canvas_GameplayHUD
Canvas_MissionPanels
```

Required HUD panels:

```text
MissionHeaderPanel
ObjectiveTrackerPanel
InventoryCounterPanel
InteractPrompt
```

Required mission panels:

```text
DialoguePanel
KnowledgeLockIntroPanel
QuestionPanel
FeedbackPanel
ItemUnlockedPanel
AreaCompletePanel
```

Required TextMeshPro dynamic text fields:

```text
MissionTitleText
GradeSubjectTermText
CurrentObjectiveText
ObjectiveProgressText
CollectibleCountText
InteractText
NPCNameText
DialogueBodyText
ChallengeTitleText
ChallengeInstructionText
QuestionCounterText
QuestionText
AnswerText_A
AnswerText_B
AnswerText_C
AnswerText_D
FeedbackTitleText
FeedbackBodyText
ItemNameText
ItemDescriptionText
AreaCompleteTitleText
AreaCompleteBodyText
```

Required Button references:

```text
ContinueButton
CloseButton
StartQuestionsButton
AnswerButton_A
AnswerButton_B
AnswerButton_C
AnswerButton_D
NextQuestionButton
TryAgainButton
ItemUnlockedContinueButton
AreaCompleteContinueButton
```

UI content that must stay dynamic:

```text
mission title
grade/subject/term
objectives
NPC name
NPC dialogue
question text
answer choices
feedback/hint text
collectible count
item name and description
area complete title/body
```

Fixed button labels may be baked into sprites only if they never change, such as:

```text
Continue
Retry
Close
```

---

# Simple hierarchy to create for the Area 1 MVP

Use this hierarchy as the build guide. The bracket tells you the object type.

```text
LiteraQuest_Term1_Mission1_MVP
│
├── __SYSTEMS__                                      [Empty GameObject]
│   ├── MissionRuntimeController                     [Empty GameObject / script holder]
│   ├── MissionDataLoader                            [Empty GameObject / script holder]
│   ├── MissionObjectiveController                   [Empty GameObject / script holder]
│   ├── InteractionController                        [Empty GameObject / script holder]
│   ├── DialogueController                           [Empty GameObject / script holder]
│   ├── KnowledgeLockController                      [Empty GameObject / script holder]
│   ├── QuestionController                           [Empty GameObject / script holder]
│   ├── FeedbackController                           [Empty GameObject / script holder]
│   ├── CollectibleController                        [Empty GameObject / script holder]
│   └── GateUnlockController                         [Empty GameObject / script holder]
│
├── __CAMERA__                                       [Empty GameObject]
│   ├── Main Camera                                  [Camera]
│   └── CameraFollowTarget                           [Empty GameObject]
│
├── __PLAYER__                                       [Empty GameObject]
│   ├── Player                                       [Player prefab root]
│   │   ├── PlayerModel                              [3D model or placeholder]
│   │   ├── PlayerController                         [Script holder]
│   │   ├── PlayerCollider                           [CharacterController or CapsuleCollider]
│   │   ├── PlayerInteractionSensor                  [Trigger Collider]
│   │   └── CameraTarget                             [Empty GameObject]
│   └── PlayerSpawnPoint                             [Empty GameObject]
│
├── __ENVIRONMENT__                                  [Empty GameObject]
│   ├── Ground_ParadeMeadow                          [Terrain / Plane / Mesh]
│   ├── WalkablePath_Entrance_To_Meadow              [Ground/path mesh]
│   ├── WalkablePath_Meadow_To_Gate                  [Ground/path mesh]
│   ├── BoundaryColliders                            [Empty GameObject]
│   │   ├── Boundary_Left                            [Invisible BoxCollider]
│   │   ├── Boundary_Right                           [Invisible BoxCollider]
│   │   ├── Boundary_Back                            [Invisible BoxCollider]
│   │   └── Boundary_Front                           [Invisible BoxCollider]
│   └── StaticProps                                  [Empty GameObject]
│       ├── Trees                                    [3D environment props]
│       ├── GrassPatches                             [3D environment props]
│       ├── Fences                                   [3D environment props]
│       ├── FestivalFlags                            [3D props]
│       ├── Banners                                  [3D props]
│       ├── Crates_Barrels                           [3D props]
│       └── ParadeDecorations                        [3D props]
│
├── __G5_AREA01_PARADE_MEADOW__                      [Empty GameObject]
│   ├── G5_Area01_Bounds                             [Empty GameObject / area marker]
│   ├── G5_Area01_CameraAnchor                       [Empty GameObject / optional]
│   │
│   ├── G5_NPC_FarmerLira                            [NPC root]
│   │   ├── NPC_Model                                [3D NPC model]
│   │   ├── NPC_Collider                             [CapsuleCollider or BoxCollider]
│   │   ├── NPC_InteractTrigger                      [Trigger Collider]
│   │   ├── NPC_InteractionMarker                    [3D marker/icon, hidden by default]
│   │   └── NPC_DialogueAnchor                       [Empty GameObject]
│   │
│   ├── G5_Interactable_ParadeClueSet                [Interactable root]
│   │   ├── ParadeClueSet_Model                      [3D clue prop group]
│   │   ├── ParadeClueSet_Collider                   [BoxCollider]
│   │   ├── ParadeClueSet_InteractTrigger            [Trigger Collider]
│   │   ├── ParadeClueSet_InteractionMarker          [3D marker/icon, hidden by default]
│   │   └── ParadeClueSet_QuestionAnchor             [Empty GameObject]
│   │
│   ├── G5_KnowledgeLock_ParadeMeadow                [Challenge object root]
│   │   ├── KnowledgeLock_Model                      [3D pedestal model]
│   │   ├── KnowledgeLock_Collider                   [BoxCollider]
│   │   └── KnowledgeLock_InteractTrigger            [Trigger Collider]
│   │
│   ├── G5_Collectible_StoryMapFragment              [Collectible root, inactive at start]
│   │   ├── StoryMapFragment_Model                   [3D collectible model]
│   │   ├── StoryMapFragment_PickupTrigger           [Trigger Collider]
│   │   └── StoryMapFragment_Pivot                   [Empty GameObject]
│   │
│   ├── G5_Gate_To_Area02                            [Gate root]
│   │   ├── GateClosedVisual                         [3D closed gate model]
│   │   ├── GateOpenVisual                           [3D open gate model, inactive at start]
│   │   └── GateBlockerCollider                      [BoxCollider blocking path]
│   │
│   └── G5_Area01_EndMarker                          [Empty GameObject]
│
├── __UI_CANVAS__                                    [Empty GameObject]
│   ├── EventSystem                                  [Unity EventSystem]
│   │
│   ├── Canvas_GameplayHUD                           [Canvas]
│   │   └── SafeArea                                 [RectTransform]
│   │       ├── MissionHeaderPanel                   [UI Panel / Image]
│   │       │   ├── MissionTitleText                 [TextMeshProUGUI]
│   │       │   └── GradeSubjectTermText             [TextMeshProUGUI]
│   │       ├── ObjectiveTrackerPanel                [UI Panel / Image]
│   │       │   ├── CurrentObjectiveText             [TextMeshProUGUI]
│   │       │   └── ObjectiveProgressText            [TextMeshProUGUI]
│   │       ├── InventoryCounterPanel                [UI Panel / Image]
│   │       │   ├── CollectibleIcon                  [UI Image]
│   │       │   └── CollectibleCountText             [TextMeshProUGUI]
│   │       └── InteractPrompt                       [UI Panel, hidden by default]
│   │           ├── InteractIcon                     [UI Image]
│   │           └── InteractText                     [TextMeshProUGUI]
│   │
│   └── Canvas_MissionPanels                         [Canvas]
│       ├── DialoguePanel                            [UI Panel, hidden by default]
│       │   ├── NPCNameText                          [TextMeshProUGUI]
│       │   ├── DialogueBodyText                     [TextMeshProUGUI]
│       │   ├── ContinueButton                       [Button]
│       │   └── CloseButton                          [Button]
│       ├── KnowledgeLockIntroPanel                  [UI Panel, hidden by default]
│       │   ├── ChallengeTitleText                   [TextMeshProUGUI]
│       │   ├── ChallengeInstructionText             [TextMeshProUGUI]
│       │   ├── StartQuestionsButton                 [Button]
│       │   └── CloseButton                          [Button]
│       ├── QuestionPanel                            [UI Panel, hidden by default]
│       │   ├── QuestionCounterText                  [TextMeshProUGUI]
│       │   ├── QuestionText                         [TextMeshProUGUI]
│       │   ├── AnswerButton_A                       [Button]
│       │   │   └── AnswerText_A                     [TextMeshProUGUI]
│       │   ├── AnswerButton_B                       [Button]
│       │   │   └── AnswerText_B                     [TextMeshProUGUI]
│       │   ├── AnswerButton_C                       [Button]
│       │   │   └── AnswerText_C                     [TextMeshProUGUI]
│       │   ├── AnswerButton_D                       [Button]
│       │   │   └── AnswerText_D                     [TextMeshProUGUI]
│       │   └── NextQuestionButton                   [Button]
│       ├── FeedbackPanel                            [UI Panel, hidden by default]
│       │   ├── FeedbackTitleText                    [TextMeshProUGUI]
│       │   ├── FeedbackBodyText                     [TextMeshProUGUI]
│       │   ├── TryAgainButton                       [Button]
│       │   └── ContinueButton                       [Button]
│       ├── ItemUnlockedPanel                        [UI Panel, hidden by default]
│       │   ├── ItemIcon                             [UI Image]
│       │   ├── ItemNameText                         [TextMeshProUGUI]
│       │   ├── ItemDescriptionText                  [TextMeshProUGUI]
│       │   └── ContinueButton                       [Button]
│       └── AreaCompletePanel                        [UI Panel, hidden by default]
│           ├── AreaCompleteTitleText                [TextMeshProUGUI]
│           ├── AreaCompleteBodyText                 [TextMeshProUGUI]
│           └── ContinueButton                       [Button]
│
└── __DATA_REFERENCES__                              [Empty GameObject]
    ├── G5_Mission1_Area01_DataReference             [ScriptableObject/JSON holder]
    ├── G5_Area01_QuestionBankReference              [Question data holder]
    └── G5_Area01_SceneBindingReference              [Scene reference holder]
```

---

# Starting state checklist

Set the scene like this before the AI agent wires logic:

```text
Player active
Main Camera active
Farmer Lira active
Parade Clue Set active
Knowledge Lock pedestal active
Story Map Fragment inactive or pickup disabled
GateClosedVisual active
GateOpenVisual inactive
GateBlockerCollider enabled
InteractPrompt inactive
DialoguePanel inactive
KnowledgeLockIntroPanel inactive
QuestionPanel inactive
FeedbackPanel inactive
ItemUnlockedPanel inactive
AreaCompletePanel inactive
```

---

# Area 1 data needed by the AI agent

The Area 1 MVP needs this data, either as ScriptableObject or local JSON:

```text
mission_id: literaquest_t1_m01_g5_area01_mvp
full_mission_id: literaquest_t1_m01_g5
grade_level: 5
subject_slug: literaquest
term_key: term_1
mission_title: Festival Storybook Rescue
area_id: g5_a01_parade_meadow
area_title: Parade Meadow
npc_id: npc_g5_farmer_lira
interaction_id: interactable_g5_a01_parade_object_cluster
knowledge_lock_id: g5_lock_story_parts
reward_collectible_id: g5_story_map_fragment
gate_id: gate_g5_area01_to_area02_stub
```

Required objective text sequence:

```text
Talk to Farmer Lira.
Inspect the parade clues.
Answer the Story Parts Knowledge Lock.
Pick up the Story Map Fragment.
Go through the opened gate.
Parade Meadow complete.
```

---

# Area 1 questions

```text
Q1: Mika joined the town parade and carried a flag. Who is the main character?
A. The rain
B. Mika
C. The festival field
D. The decorations
Correct: Mika
Wrong hint: Look for the person who does the main action in the sentence.

Q2: Dark clouds appeared, and rain began to fall on the decorations. What is the problem?
A. Mika carried a flag.
B. The parade became successful.
C. Rain may ruin the decorations.
D. The town prepared early.
Correct: Rain may ruin the decorations.
Wrong hint: A problem is the trouble that characters must solve.

Q3: What happened first?
A. The rain stopped.
B. The parade continued.
C. Mika and her friends protected the decorations.
D. The town prepared flags and decorations.
Correct: The town prepared flags and decorations.
Wrong hint: Look for the event that happened before the rain and before the parade continued.
```

---

# Acceptance criteria for the Area 1 MVP

The MVP is complete when:

```text
1. Player can move around Parade Meadow.
2. Interact prompt appears near Farmer Lira.
3. Dialogue panel opens with Farmer Lira text.
4. Objective updates after dialogue.
5. Interact prompt appears near Parade Clue Set or Knowledge Lock.
6. Knowledge Lock intro opens.
7. Question panel shows three dynamic questions.
8. Wrong answer shows hint and allows retry.
9. Correct answer advances the question flow.
10. After all three questions are correct, Story Map Fragment unlocks.
11. Player can pick up Story Map Fragment.
12. Inventory counter updates from 0/1 to 1/1.
13. Gate opens and blocker collider disables.
14. Objective updates to Area 1 complete.
15. Area Complete panel appears.
16. No Console errors are produced.
```

---

# Future objects not needed yet

Do not place these unless the owner wants them as disabled future placeholders:

```text
G5_NPC_KuyaTomas
G5_Interactable_BrokenFestivalSign
G5_Collectible_GrammarInk
G5_NPC_LolaSinta
G5_Interactable_FadedMainIdeaFlower
G5_Collectible_MainIdeaFlower
G5_Collectible_MoodPalette
G5_Interactable_BrokenFestivalStorybook
G5_Interactable_RestoredFestivalStorybook
G6_NPC_KeeperAri
G6_NPC_Tala
G6_Collectible_StorySeal
G6_Collectible_MemorySeal
G6_GrandLanternTree
```

These remain valid later, but they should not distract the Area 1 MVP implementation.
