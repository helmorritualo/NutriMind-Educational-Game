---
name: nutrimind-validation
description: Validate NutriMind Unity changes through compilation, Console review, tests, UI Toolkit preview, references, Android landscape, and cleanup checks.
required_editor_version: ">=6000.4.10f1"
---
# Validation Workflow
After every unit, save scenes/assets, wait for compilation, review new Console issues, validate references and stable keys, run relevant Edit Mode and Play Mode tests, preview UI Toolkit output, check touch/safe-area behavior, and verify cancellation, event unregistration, and scene unload cleanup. Record checks that could not run and never claim an unexecuted check passed.
