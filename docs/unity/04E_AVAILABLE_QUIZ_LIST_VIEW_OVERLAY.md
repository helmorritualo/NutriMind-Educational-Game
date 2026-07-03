# Available Quiz List View Modal Requirements — Canvas/uGUI

## Purpose

The `View` action in Available Quiz List must open an in-screen Canvas modal/panel, not a new scene.

This keeps the Phase 8B scope smaller and preserves the student's list state.

## Owned by

The modal is owned by the Available Quiz List screen/controller/presenter.

## When to show modal

Show the modal for:

- locked quiz
- unavailable quiz
- quiz blocked by Unity compatibility
- completed quiz summary when result visibility allows lightweight viewing
- safe explanation of why Start is unavailable

## When not to show modal

Do not use the modal for:

- active playable quiz start
- full quiz session
- full review answers flow if implemented later as its own result/review view
- gameplay missions
- rewards/shop/inventory

## Required modal content

For locked/unavailable quiz:

- quiz title
- subject
- term
- grade
- item count
- status
- reason
- available date/time if provided
- compatibility status if relevant
- Close/Back

For completed quiz summary:

- quiz title
- subject
- term
- score if visible
- percentage if visible
- feedback if visible
- review allowed flag
- Back/Close
- optional View Full Result action if implemented

## State preservation

Opening and closing the modal must preserve:

- current subject filter
- scroll position
- selected row
- current list data
- loaded/empty/error state
- navigation history

## Canvas input behavior

Modal should block interaction with the list behind it.

Use a full-screen transparent or dimmed blocking Image/Button behind the modal if needed.

Back/cancel should close the modal if safe.

Touch outside may close only if that behavior is consistent with project UI.

## Data safety

The modal must never show:

- answer keys
- teacher-only notes
- admin-only fields
- other students' results
- hidden quiz items
