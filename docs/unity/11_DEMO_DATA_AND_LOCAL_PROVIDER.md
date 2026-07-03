# Demo Data and Local Provider — v8

## Purpose

LocalDemoJson simulates the Laravel REST contract for the quiz-first Unity milestone.

## UI technology note

The data/provider layer is UI-agnostic. It must work with Canvas/uGUI views and must not reference Canvas objects directly.

## Simulate

- login
- logout
- refresh/bootstrap
- profile
- settings
- subjects
- terms
- quiz list
- subject filters
- available quiz action
- locked quiz action
- unavailable quiz action
- completed quiz result action where visible
- in-screen View modal data
- locked quiz state
- empty quiz state
- quiz detail/instructions
- multiple choice single
- multiple choice multiple
- true/false
- unsupported item fallback
- submit confirmation
- quiz attempt submission
- quiz result
- error/retry
- sync status
- reset

## Do not simulate

- mission progress
- rewards shop
- spendable currency
- inventory
- cosmetics
- pets
- world restoration

## Production guard

Production builds must reject LocalDemoJson and must not silently fall back from HTTP.
