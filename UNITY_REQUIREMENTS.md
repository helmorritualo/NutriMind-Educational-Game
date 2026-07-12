# NutriMind Unity Requirements — Static Grade-Based Gameplay and Server Integration

## Document status

This is the Unity-side source of truth for the server milestone and the next Unity continuation.

```text
Current server milestone: complete Laravel–Inertia–React server.
Current Unity baseline: Grade 5 LiteraQuest Mission 1 Area 1 is implemented.
Gameplay content ownership: static/fixed in Unity.
Server gameplay role: authenticate, select grade context, unlock, track, report, and synchronize progress.
Future content: detailed stories and area content will be added later.
```

---

## Unity content ownership

Unity must contain the complete runtime gameplay content for:

- Grade 5 and Grade 6
- LiteraQuest
- PE & Health
- Science
- terms
- missions
- areas
- final challenges

Static Unity content includes:

- scenes and prefabs
- NPCs
- dialogue
- stories and mission introductions
- learning clues
- static gameplay questions and choices
- hints, reminders, explanations, and review panels
- investigation actions
- health actions
- world/environment results
- collectible visuals
- journals/guides/meters
- local content IDs and manifest files

The server must not be required to deliver this content before a mission can run.

---

## Bootstrap and grade selection

Unity must determine the active grade from the authenticated server profile.

### Online bootstrap

```text
Launch game
→ ping server
→ authenticate using LRN and PIN
→ receive bootstrap response
→ read stable grade ID
→ select matching local grade catalog
→ merge server availability and progress
→ show subjects and missions allowed for that grade
```

Required bootstrap fields include:

```text
student ID
display name
grade ID: grade_5 or grade_6
section
active school year
available subjects
available terms
mission availability/unlock states
quiz availability
mission progress summaries
content manifest version
settings
announcements
sync revision
```

The server does not return full mission story/gameplay content.

### Offline bootstrap

After a successful prior login:

```text
server unavailable
→ load cached authenticated profile and grade
→ open the matching local grade catalog
→ load local SQLite progress
→ allow offline gameplay
→ enqueue progress events
→ sync later
```

A first-time production student should not select a grade manually to bypass the server. Local demo profiles may be used in editor/development mode.

### Grade changes

If an administrator changes a student’s grade:

- the next authenticated bootstrap returns the new grade
- Unity changes to the matching local catalog
- incompatible mission-selection cache is invalidated
- old progress remains stored under its original grade/mission IDs
- Unity must not merge Grade 5 and Grade 6 progress accidentally

---

## Local content registry

Unity should maintain a grade-aware local registry.

Recommended hierarchy:

```text
GradeCatalog
  grade_5
    LiteraQuest
      Term 1
        Mission IDs and Area IDs
    PE & Health
    Science
  grade_6
    LiteraQuest
    PE & Health
    Science
```

Each mission definition should reference:

- stable ID
- display title
- subject
- grade
- term
- order
- scene/addressable key
- area definitions
- encounter IDs
- static question IDs
- collectible/token IDs
- required counts
- final challenge ID
- local content version

The server and Unity must share the IDs and manifest version, not the full content.

---

## Mission availability

For every subject:

- Mission 1 starts available.
- Later missions remain locked.
- Completing the previous mission unlocks the next mission.
- An area unlocks after the current area’s required item/token is collected.
- The final challenge unlocks after all required areas/items are complete.
- Server progress is canonical after sync.
- Unity may calculate an immediate local unlock offline and later reconcile with the server.

---

## Shared mission interface

Each subject should reuse common systems for:

- subject selection
- term selection
- mission selection and locked states
- mission introduction
- objective tracking
- guidance marker
- interact prompt
- learning panel
- question panel
- two-attempt wrong-answer handling where applicable
- review panel
- item/token collection
- area unlock
- final challenge
- learning summary
- mission completed panel
- local save and server sync

Subject-specific controllers can add health or science actions without duplicating the shared framework.

---

## LiteraQuest loop

```text
Select LiteraQuest
→ select term
→ select mission
→ mission introduction
→ enter mission map
→ NPC gives task
→ explore and follow marker
→ interact with NPC/object
→ read/watch/observe clue
→ answer up to five questions
→ hint/retry/review
→ area completion
→ unlock and collect one Story Fragment
→ update progress
→ unlock next area
→ repeat
→ final story/restoration challenge
→ learning summary
→ mission complete
```

Question types:

- multiple choice
- true or false
- multiple answers

Multiple-choice behavior:

- two attempts maximum
- first wrong: hint
- second wrong: explanation/correct concept and mark for review
- review panel if any question needed correction
- immediate completion panel if all correct

Core loop:

```text
Explore → Interact → Learn → Answer → Review → Unlock → Collect → Progress
```

---

## PE & Health loop

```text
Select PE & Health
→ select term
→ select mission
→ mission introduction
→ Health Guide gives task
→ explore open-world park
→ discover health situation
→ observe health/environment clues
→ interact
→ read health learning clue
→ answer up to five questions
→ reminder after wrong answer
→ apply healthy action in world
→ observe wellness result
→ collect one Wellness Symbol
→ update Wellness Guide and meter
→ unlock next area
→ repeat
→ final wellness challenge
→ learning summary
→ mission complete
```

The defining PE & Health mechanic is not only answering:

```text
Choose healthy/safe answer → perform action in world → observe visible result
```

Track locally:

- health situation observed
- questions/attempts
- reminder reviewed
- healthy action applied
- wellness result observed
- Wellness Symbol collected
- area restored
- Wellness Meter
- final challenge

Core loop:

```text
Explore → Observe → Learn → Decide → Review → Apply Healthy Action → See Result → Collect → Unlock
```

---

## Science loop

```text
Select Science
→ select term
→ select mission
→ mission introduction
→ Science Guide gives task and journal
→ explore
→ discover problem
→ observe clues
→ record observations
→ interact with station
→ read learning clue
→ make prediction
→ collect materials
→ conduct investigation
→ record result
→ answer evidence-based questions
→ review
→ form conclusion
→ apply scientific solution
→ observe environment change
→ collect Science Evidence Token
→ update journal
→ unlock next area
→ repeat
→ final science challenge
→ learning summary
→ mission complete
```

Grade 5:

- four areas
- three to four questions per area
- guided single-step investigation
- visual clues
- simple measurements and conclusions

Grade 6:

- four to five areas
- four to five questions per area
- fair testing and variables
- repeated trials when appropriate
- more precise measurements
- evidence-supported conclusions

Prediction rules:

- save prediction
- do not mark a wrong prediction as a failed scored question
- compare prediction with result in conclusion flow when appropriate

Scored-question rules:

- two attempts maximum
- first wrong: Science Hint
- second wrong: correct explanation/evidence and mark review
- continue without life loss or restart

Core loop:

```text
Explore → Observe → Predict → Investigate → Record → Answer → Explain → Apply → Restore → Collect → Unlock
```

---

## Local persistence

### Recommended production approach

Use SQLite for durable local progress and the offline outbox.

Location:

```text
Application.persistentDataPath
```

SQLite is recommended over one large JSON save because NutriMind has:

- two grade levels
- three subjects
- many missions and areas
- question-level summaries
- collectibles
- unlock dependencies
- local/server revisions
- queued offline events
- retry and acknowledgement states

The exact SQLite integration package must be selected later for the actual Unity version and Android/desktop targets.

### Required local records

```text
LocalProfile
LocalSettings
LocalMissionProgress
LocalAreaProgress
LocalQuestionProgress
LocalCollectibleProgress
LocalOutboxEvent
LocalSyncState
```

### Atomic local save rule

Whenever meaningful progress occurs:

```text
begin local SQLite transaction
→ update local progress
→ insert outbox event with UUID and sequence
→ commit
→ update UI
```

Never update the UI-only state without persisting the corresponding local state/outbox event.

### Outbox event shape

```text
event_uuid
student_id
grade_id
subject_id
term_id
mission_id
area_id
encounter_id nullable
question_id nullable
collectible_id nullable
event_type
local_sequence
payload JSON
client_created_at
sync_status
server_revision nullable
```

---

## Synchronization

Unity should synchronize:

- at successful login/bootstrap
- after important progress when online
- when returning to the main menu
- on application pause/quit when safe
- when connectivity is restored
- on explicit retry

Do not send frame-by-frame telemetry.

Send semantic events only.

Accepted events are marked synchronized. Rejected events remain available for diagnostics or corrected retry. Duplicate UUIDs must be safe.

The server returns:

- accepted/duplicate/rejected/deferred result
- stable error code
- canonical mission/area state
- canonical revision
- newly unlocked mission/area when applicable

---

## Progress tracked by subject

### Common

- mission started/completed
- area started/completed
- review required
- collectible unlocked/collected
- next area unlocked
- final challenge started/completed
- mission unlock

### LiteraQuest

- clue viewed
- question attempts
- hint shown
- story review
- Story Fragment collected

### PE & Health

- situation observed
- reminder shown/reviewed
- healthy action selected
- healthy action applied
- wellness result observed
- Wellness Symbol collected
- Wellness Meter progress

### Science

- observation recorded
- prediction recorded
- material collected
- investigation performed
- result recorded
- question attempts/review
- conclusion recorded
- solution applied
- Evidence Token collected

---

## Current LiteraQuest Area 1 update

Current implemented baseline:

```text
Grade 5 • LiteraQuest • Term 1
Festival Storybook Rescue
Area 1: Parade Meadow
Farmer Lira
three Knowledge Lock questions
Story Map Fragment
gate opens
Area Complete
```

Future alignment work must preserve the scene and add/confirm:

- grade-selected local catalog entry
- mission selection/locked-state support
- mission introduction panel
- guidance marker for Farmer Lira/clue/fragment/path
- maximum two attempts for multiple choice
- first-wrong hint
- second-wrong explanation and review marker
- review panel if any wrong
- all-correct completion panel
- SQLite local save
- outbox events for lock completion, fragment collection, and area completion
- server canonical revision after sync

The current three questions remain valid even though the LiteraQuest maximum is five.

---

## Unity acceptance criteria after server integration

- student grade is chosen from server bootstrap
- correct local grade content loads
- no full gameplay content is required from the server
- offline gameplay works after prior authentication
- local SQLite progress survives restart
- every meaningful progress update produces an outbox event
- duplicate sync is safe
- Grade 5 and Grade 6 progress remain isolated
- static gameplay questions remain local
- Quiz Portal uses server-delivered questions and server scoring
- current Area 1 still works after adding save/sync integration
