# Cursor Prompt — Grade 5 LiteraQuest Mission 1 Area 1 MVP Logic

You are working in Cursor on the NutriMind Unity project. This is a new AI coding session.

The quiz system is temporarily on hold. Do not continue Quiz Portal / Assessment Room implementation in this session.

Your current task is to implement only the gameplay logic for the **Grade 5 LiteraQuest Term 1 Mission 1 Area 1 MVP**.

```text
Scene: LiteraQuest_Term1_Mission1_MVP
Mission: Festival Storybook Rescue
Grade: 5
Area: Area 1 — Parade Meadow
```

Before changing files, read `AGENTS.md`, all files in `.cursor/rules/`, `UNITY_AI_ASSISTANT_CUSTOM_INSTRUCTIONS.txt` if present, `README.md`, `docs/UNITY_REQUIREMENTS.md`, `docs/unity/06_GAMEPLAY_MISSIONS_CURRENT_PLAN.md`, `docs/unity/06A_LITERAQUEST_TERM1_MISSION1_PLAN.md`, `docs/unity/06B_LITERAQUEST_MISSION1_SCENE_OBJECT_AND_ASSET_CHECKLIST.md`, `docs/unity/06C_LITERAQUEST_MISSION1_AI_AGENT_IMPLEMENTATION_PLAN.md`, and `docs/unity/examples/literaquest-term1-mission1-demo-data.json`.

The project owner is responsible for designing and placing the Parade Meadow environment objects and for designing the dynamic Canvas UI. Do not redesign the environment or replace the owner’s UI art direction. Use the owner-placed Player, Farmer Lira, Parade Clue Set, Knowledge Lock pedestal, Story Map Fragment, gate, Canvas panels, TextMeshPro fields, and buttons. If an object or UI reference is missing, create only a clearly named temporary placeholder and report it as an owner task.

Implement only this gameplay loop:

```text
walk/explore → approach Farmer Lira → show Interact prompt → open dialogue → update objective → inspect Parade Clue Set or Knowledge Lock → answer 3 questions → wrong answer shows hint and retry → all correct answers unlock Story Map Fragment → pick up Story Map Fragment → inventory updates to 1/1 → gate opens → Area Complete panel appears
```

Do not implement the full Grade 5 mission. Do not implement Area 2, Area 3, the final Festival Storybook Stage, Grade 6 gameplay, Mission 2, Health/PE gameplay, Science gameplay, reward shop, spendable coins, EXP, inventory economy, pets, cosmetics, titles, equipment, teacher-authored missions, server mission tracking, multiplayer, WebSocket, audio requirements, VFX requirements, or drag-and-drop minigames.

Use Canvas/uGUI for mission UI. Use the owner-designed dynamic UI prefabs and panels. Dynamic text must use TextMeshPro. Do not bake dialogue, question text, answer choices, objective text, feedback, collectible counts, or area complete messages into images. Fixed labels such as Continue, Retry, and Close may be static only if the owner intentionally designed them as fixed button sprites.

Use local data first: ScriptableObject or JSON. Do not require Laravel/server mission APIs for this MVP. Do not hardcode all question content inside MonoBehaviours.

Required data/content:

```text
mission_id: literaquest_t1_m01_g5_area01_mvp
area_id: g5_a01_parade_meadow
npc_id: npc_g5_farmer_lira
interaction_id: interactable_g5_a01_parade_object_cluster
knowledge_lock_id: g5_lock_story_parts
collectible_id: g5_story_map_fragment
gate_id: gate_g5_area01_to_area02_stub
```

Required questions:

```text
Q1: Mika joined the town parade and carried a flag. Who is the main character?
Correct: Mika
Hint: Look for the person who does the main action in the sentence.

Q2: Dark clouds appeared, and rain began to fall on the decorations. What is the problem?
Correct: Rain may ruin the decorations.
Hint: A problem is the trouble that characters must solve.

Q3: What happened first?
Correct: The town prepared flags and decorations.
Hint: Look for the event that happened before the rain and before the parade continued.
```

Required systems:

```text
interaction detection
Interact prompt UI
Farmer Lira dialogue binding
Knowledge Lock intro panel binding
Question panel binding
answer validation
wrong-answer hint feedback
Story Map Fragment unlock
collectible pickup
inventory counter update
gate open / blocker disable
objective tracker update
Area Complete panel
local reset for testing
```

Validate the implementation manually in Play Mode. At minimum, test starting the scene, moving to Farmer Lira, showing Interact prompt, opening dialogue, interacting with the Knowledge Lock, answering wrong once, receiving a hint, answering correctly, unlocking the Story Map Fragment, picking it up, updating the inventory counter to 1/1, opening the gate, showing the Area Complete panel, and confirming there are no Console errors.

Report files changed, prefabs changed, scenes changed, data files changed, UI references wired, scene objects wired, placeholders created, owner tasks still needed, tests/checks run, exact results, Console warnings/errors, and remaining gaps. Stop after reporting.
