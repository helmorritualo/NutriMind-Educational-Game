# UI Controls, Accessibility, and Presentation — Hybrid Canvas + UI Toolkit

## Purpose

This document defines UI requirements for the current quiz-first Unity milestone.

The project uses hybrid Canvas + UI Toolkit because some application-scene UI designs already exist, while the Quiz Portal UI still needs to be built.

## Required UI Scope

Implement and validate UI for:

```text
Bootstrap/Application Root
Splash
Login
Main Interface
Profile
Settings
Subject Selection
Term Selection or Quiz Filter
Loading/Transition
Quiz Portal Home
Available Quiz List
Empty Quiz State
Locked Quiz State
Quiz Instructions
Quiz Session Shell
Quiz Item Presenters
Submit Confirmation
Quiz Result Screen
Quiz Error/Retry State
Unsupported Item State
```

## Hybrid UI Ownership Rules

A screen may use Canvas, UI Toolkit, or both.

Prefer Canvas when:

```text
an existing valid app-scene Canvas design already exists
visual layout is already approved
rewriting would slow the milestone
```

Prefer UI Toolkit when:

```text
the screen is mostly structured UI
lists/forms/panels dominate the screen
new quiz UI can be implemented faster and cleaner with UXML/USS/controllers
```

If a screen mixes Canvas and UI Toolkit, document:

```text
which layer owns input
which layer owns focus
which layer owns navigation
which layer owns modal blocking
which layer owns safe-area layout
```

## Reuse Rule

Before creating UI, inspect:

```text
existing scenes
existing Canvas prefabs
existing UI Toolkit assets
owner-provided image references
third-party UI assets
existing variants
current scripts/controllers
```

Reuse valid existing application-scene UI first.

Do not rebuild a finished screen from scratch only to change UI technology.

## Shared Logic Rule

Canvas and UI Toolkit views must not duplicate business logic.

Both must use shared:

```text
stores
services
presenters
DTOs
navigation service
safe error service
quiz session state
answer draft state
quiz submission coordinator
```

## Android Landscape Requirements

All screens must support Android landscape.

Check:

```text
safe area
large tap targets
readable text
no clipped buttons
no hidden submit controls
no overlap with device cutouts
correct scaling across common resolutions
```

## Accessibility Requirements

Use:

```text
clear labels
large readable text
visible focus/selection states
non-color-only feedback
consistent button behavior
safe contrast
simple navigation
child-friendly error messages
```

Avoid:

```text
small tap targets
confusing decorative backgrounds behind static UI
color-only correctness feedback
unlabeled icons
modal traps
unexpected input stealing between Canvas and UI Toolkit
```

## Quiz UI Requirements

Quiz screens must include:

```text
question prompt
item number/progress
answer controls
navigation controls
clear selected answer state
validation error state
submit confirmation
safe loading/submitting state
result summary
feedback when allowed
unsupported item message when needed
```

Unsupported item types must not crash the game.

## Deferred UI

Do not implement UI for:

```text
gameplay missions
reward shop
inventory
pets
cosmetics
titles
equipment
world restoration
mission progress dashboard
```

If a placeholder Adventure button exists, it must clearly show that gameplay missions are not available in this milestone.
