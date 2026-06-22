# NutriMind Unity Requirements — Hybrid UI Quiz-First Milestone

## Current Goal

Finish Unity up to the Quiz Portal / Assessment Room.

Do not start open-world mission gameplay until the mission catalog is finalized.

## UI System Decision

Unity uses a **hybrid Canvas + UI Toolkit** approach for this milestone.

Reason:

- some application-scene UI designs already exist;
- some application scenes are still missing;
- Quiz Portal UI still needs to be designed/implemented;
- replacing all existing work with UI Toolkit would slow the milestone.

## Hybrid UI Rules

Use existing application-scene UI designs first.

Allowed:

- Canvas/uGUI for existing application-scene UI designs;
- UI Toolkit for new quiz-system screens where practical;
- Canvas for visual designs already made or faster child-friendly UI implementation;
- UI Toolkit for structured menus/forms/lists where practical.

Required:

- shared navigation and state logic independent of UI technology;
- no duplicate business logic in Canvas and UI Toolkit;
- consistent safe-area and Android landscape rules;
- consistent theme, fonts, colors, and button behavior;
- no confusing backgrounds behind static UI;
- UI adapters/presenters for both Canvas and UI Toolkit where needed.

## Active Unity Scope

- finish existing application scenes;
- create missing application scenes;
- complete hybrid UI foundation;
- Quiz Portal home;
- Quiz list;
- Quiz instructions;
- Quiz session shell;
- quiz item presenters;
- submit confirmation;
- result screen;
- LocalDemoJson quiz data;
- HTTP-compatible DTOs.

## Deferred Unity Scope

- LiteraQuest mission gameplay;
- PE/Health mission gameplay;
- Science mission gameplay;
- mission tracking;
- mission rewards;
- item shop;
- inventory;
- world restoration.

## Rewards and Shop

Deferred until mission gameplay content is finalized.

Do not implement:

- spendable coins;
- EXP economy;
- shop UI;
- inventory;
- pets/cosmetics/titles;
- purchases/equipping.

Quiz results may show score, pass/fail, feedback, and optional non-economic badges/stars as display-only UI if needed.

## Application Scenes

Existing scene designs must be found, inspected, reused, and completed.

Missing application scenes must be created using the project style.

Expected app scenes:

- Bootstrap/Application Root
- Splash
- Login
- Main Interface
- Profile
- Settings
- Subject Selection
- Term Selection or quiz filter support
- Loading/Transition
- Quiz Portal
- Quiz Instructions
- Quiz Session
- Quiz Result

## Quiz Item Types

Support contract for:

- multiple choice single
- multiple choice multiple
- true/false
- matching
- ordering
- fill blank
- short answer
- categorization
- drag/drop
- image hotspot
- labeling
- scenario choice
- reading passage
- cloze
- numeric
- reflection

Unity first implementation should prioritize:

- multiple choice single
- multiple choice multiple
- true/false
- matching
- ordering
- fill blank
- short answer
- safe unsupported item state.
