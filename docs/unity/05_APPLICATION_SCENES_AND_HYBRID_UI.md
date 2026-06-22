# Application Scenes and Hybrid UI

## Existing Designs

Agents must inspect existing scene designs before creating new UI.

Expected existing or partially existing designs may include:

- Splash
- Login
- Main Interface
- Profile
- Settings
- Subject Selection
- Term Selection
- Loading/Transition

The exact repository contents must be inspected before changes.

## Missing Scenes

If an application scene is missing, create it using the same visual language and UI system as the existing app scenes unless the docs say otherwise.

## Hybrid Rules

- Canvas is allowed for existing app-scene designs.
- UI Toolkit is allowed for new structured screens.
- Avoid duplicate UI logic.
- Use shared presenters/services.
- Keep scene-specific view scripts thin.
- Do not create confusing backgrounds behind static UI.
- Preserve Android landscape and safe-area behavior.

## Quiz UI

Quiz UI may use Canvas, UI Toolkit, or hybrid based on what integrates best with existing app scenes.

The agent must choose one primary UI system per quiz screen and document the choice.
