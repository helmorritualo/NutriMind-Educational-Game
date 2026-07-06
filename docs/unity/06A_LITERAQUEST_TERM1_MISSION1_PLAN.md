# LiteraQuest Term 1 Mission 1 Plan — Current MVP and Future Expansion

## Purpose

This document defines the first playable LiteraQuest gameplay slice. The project owner designs the environment, places objects, and designs the dynamic Canvas UI. The AI coding agent implements game logic and wires that logic to the owner-placed objects and UI references.

The current implementation target is intentionally small:

```text
Grade 5 Mission 1, Area 1 only: Parade Meadow
```

The goal is to prove the gameplay loop first, not to finish the whole mission.

---

# Immediate MVP — Grade 5 Area 1 only

## MVP scene

Recommended scene name:

```text
LiteraQuest_Term1_Mission1_MVP
```

## Mission identity

```text
Grade: 5
Subject: LiteraQuest
Term: Term 1
Mission: Festival Storybook Rescue
Current playable area: Area 1 — Parade Meadow
```

## MVP story situation

The farm is preparing for a Philippine national-holiday celebration. The Festival Storybook was damaged by strong wind, and the first missing story clue is hidden in the Parade Meadow. Farmer Lira asks the student to inspect the parade clues and answer story questions before the Story Map Fragment can be collected.

## MVP gameplay loop

```text
1. Player starts in Parade Meadow.
2. Player approaches Farmer Lira.
3. Interact prompt appears.
4. Player interacts and reads dialogue.
5. Objective updates to inspect the parade clues.
6. Player approaches Parade Clue Set or Knowledge Lock pedestal.
7. Knowledge Lock question panel opens.
8. Player answers three questions.
9. Wrong answer shows a hint and allows retry.
10. Correct answers advance the question flow.
11. After all questions are correct, Story Map Fragment becomes available.
12. Player picks up Story Map Fragment.
13. Inventory counter updates to 1/1.
14. Gate/path to future Area 2 opens.
15. Objective changes to Area 1 complete.
16. Area Complete panel appears.
```

## MVP learning focus

```text
story grammar
main character
story problem
sequential plot / first event
```

## MVP NPC

```text
Farmer Lira
```

## MVP interactables

```text
G5_NPC_FarmerLira
G5_Interactable_ParadeClueSet
G5_KnowledgeLock_ParadeMeadow
G5_Collectible_StoryMapFragment
G5_Gate_To_Area02
```

## MVP questions

```text
Q1: Mika joined the town parade and carried a flag. Who is the main character?
A. The rain
B. Mika
C. The festival field
D. The decorations
Correct: Mika
Hint: Look for the person who does the main action in the sentence.

Q2: Dark clouds appeared, and rain began to fall on the decorations. What is the problem?
A. Mika carried a flag.
B. The parade became successful.
C. Rain may ruin the decorations.
D. The town prepared early.
Correct: Rain may ruin the decorations.
Hint: A problem is the trouble that characters must solve.

Q3: What happened first?
A. The rain stopped.
B. The parade continued.
C. Mika and her friends protected the decorations.
D. The town prepared flags and decorations.
Correct: The town prepared flags and decorations.
Hint: Look for the event that happened before the rain and before the parade continued.
```

## MVP collectible

```text
Story Map Fragment
```

The collectible is inactive or not pickupable at scene start. It becomes active only after the Knowledge Lock is completed.

## MVP gate behavior

```text
Gate starts closed.
Gate blocker collider starts enabled.
After Story Map Fragment pickup, closed gate visual hides or changes.
Open gate visual shows or blocker is removed.
Gate blocker collider becomes disabled.
```

The gate does not need to lead to a completed Area 2 yet. It only needs to prove that the first area unlocks the next path.

## MVP completion state

Show an Area Complete panel, not full mission complete.

```text
Area Complete: Parade Meadow Restored
Story Map Fragment Collected
Next Area: Sign Repair Barn locked for later
```

---

# Future Grade 5 expansion — not part of current MVP

After Area 1 works, the same framework can expand to:

```text
Area 2: Sign Repair Barn
Area 3: Main Idea Garden
Final Area: Festival Storybook Stage
```

## Area 2 — Sign Repair Barn

Future learning focus:

```text
subject-verb agreement
abstract nouns
demonstrative pronouns
relative pronouns
```

Future collectible:

```text
Grammar Ink
```

## Area 3 — Main Idea Garden

Future learning focus:

```text
main idea
mood through color/layout
```

Future collectibles:

```text
Main Idea Flower
Mood Palette
```

## Final Area — Festival Storybook Stage

Future final object:

```text
Broken Festival Storybook -> Restored Festival Storybook
```

Future completion message:

```text
Festival Storybook Restored
Storykeeper Badge Earned
Mission 2 Locked for Later
```

---

# Future Grade 6 expansion — not part of current MVP

Grade 6 Mission 1 remains planned but should not be implemented until the Grade 5 Area 1 loop is working.

```text
Grade 6 Mission 1:
Echoes of the Lantern Village
```

Future Grade 6 areas:

```text
Area 1: Story Hut
Area 2: Memory Pond
Area 3: Message Market
Final Area: Grand Lantern Tree
```

The Grade 6 implementation should reuse the same interaction, dialogue, Knowledge Lock, collectible, gate, objective, and UI framework created for the Grade 5 Area 1 MVP.

---

# Shared acceptance criteria for the current MVP

The Area 1 MVP is ready when it can do all of this:

```text
1. Load the Grade 5 Area 1 MVP scene.
2. Spawn the player at PlayerSpawnPoint.
3. Show the initial objective.
4. Show Interact prompt near Farmer Lira.
5. Open dialogue from Farmer Lira.
6. Update objective to inspect parade clues.
7. Open Knowledge Lock questions.
8. Accept wrong answers and show hints.
9. Accept correct answers and progress through all three questions.
10. Unlock Story Map Fragment only after all questions are correct.
11. Allow pickup only after unlock.
12. Update inventory counter to 1/1 after pickup.
13. Open the gate/path after pickup.
14. Show Area Complete panel.
15. Produce no Console errors.
```

## Design note

Keep the scene readable. The Parade Meadow can have decorations, flags, and farm details, but the NPC, interactable clue set, collectible, gate, and UI panels must be easy to identify during testing.
