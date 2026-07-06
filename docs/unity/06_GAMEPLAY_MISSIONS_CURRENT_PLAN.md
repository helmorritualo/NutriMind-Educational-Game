# Gameplay Missions Current Plan — Grade 5 Area 1 MVP

## Current status

Quiz Portal / Assessment Room implementation is temporarily on hold. Preserve existing quiz docs, quiz assets, and quiz code, but do not continue quiz implementation during this gameplay task unless the project owner asks to resume it.

The current active gameplay target is **only the first playable LiteraQuest loop**, not the whole Term 1 mission.

```text
Immediate MVP target:
Grade 5 LiteraQuest Term 1 Mission 1 — Area 1 only
Area: Parade Meadow
Mission: Festival Storybook Rescue
```

Future content remains planned but not part of the immediate MVP:

```text
Grade 5 Area 2: Sign Repair Barn
Grade 5 Area 3: Main Idea Garden
Grade 5 Final: Festival Storybook Stage
Grade 6 Mission 1: Echoes of the Lantern Village
```

## Why Area 1 only

The current goal is to prove the gameplay loop and let the team understand how the mission mechanics work before building the full mission.

The MVP should prove this loop:

```text
Walk/explore
→ approach NPC or object
→ show Interact prompt
→ open dialogue/situation panel
→ open Knowledge Lock question panel
→ answer wrong and receive hint
→ answer correctly
→ unlock Story Map Fragment
→ pick up Story Map Fragment
→ update objective and inventory counter
→ open the gate/path to the next area
→ show Area Complete panel
```

This is enough to validate the logic framework. Area 2, Area 3, the final stage, and Grade 6 can reuse the same framework later.

## Current approved MVP scene

Recommended scene name:

```text
LiteraQuest_Term1_Mission1_MVP
```

The scene should contain only:

```text
Grade 5 Area 1: Parade Meadow
one player
one camera
one NPC: Farmer Lira
one interactable clue set
one Knowledge Lock pedestal
one collectible: Story Map Fragment
one gate/path blocker to future Area 2
owner-designed Canvas UI panels
```

Do not create a complete Grade 5 mission scene yet. Do not create the Grade 6 environment yet unless the owner wants it as disabled/future reference only.

## Responsibility split

The project owner designs and places:

```text
environment ground and paths
basic boundary colliders
player visual asset or placeholder
Farmer Lira NPC model/placeholder
Parade Meadow clue props
Knowledge Lock pedestal model/placeholder
Story Map Fragment collectible model/placeholder
closed/open gate models or gate visual states
Canvas HUD and mission panels
TextMeshPro fields and buttons inside the UI panels
```

The AI coding agent implements:

```text
player movement if not already available
interaction trigger detection
Interact prompt behavior
NPC dialogue wiring
Knowledge Lock question flow
answer validation
wrong-answer hint and retry
Story Map Fragment unlock
collectible pickup
inventory counter update
gate opening/objective update
Area Complete panel
local reset/testing support
```

## Not approved for the Area 1 MVP

Do not implement:

```text
Grade 5 Area 2
Grade 5 Area 3
Grade 5 final storybook restoration
Grade 6 mission gameplay
Mission 2 or any future mission
Health/PE missions
Science missions
server mission tracking
teacher-authored missions
reward shop
spendable coins
EXP economy
inventory economy
pets/cosmetics/titles/equipment
world restoration system
multiplayer/WebSocket features
drag-and-drop minigames
audio/VFX requirement
```

Audio and VFX may be added later. They are not required for the current MVP.

## Data source decision

Use local Unity data first.

Recommended for the MVP:

```text
ScriptableObject mission data
or
local JSON demo data
```

The data must include only the Area 1 content needed for the current build, or it may reference the full Mission 1 data while the runtime loads only `g5_a01_parade_meadow`.

Do not require Laravel/server mission APIs for this MVP.
