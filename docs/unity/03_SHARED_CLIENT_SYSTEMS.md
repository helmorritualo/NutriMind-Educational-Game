# Shared Client Systems — v6 Quiz-First

## UI-agnostic architecture

The overall project is hybrid Canvas + UI Toolkit, but Phase 8B quiz UI should use UI Toolkit.

Shared logic must not depend on one UI system.

Use stores, services, presenters, DTOs, provider interfaces, and view adapters.

## Active systems

- SessionStore
- ProfileStore
- SettingsStore
- SubjectTermStore
- QuizAvailabilityStore
- QuizDetailStore
- QuizSessionStore
- QuizAnswerDraftStore
- QuizValidationService
- QuizSubmissionCoordinator
- QuizResultStore
- QuizItemPresenterRegistry
- SafeErrorService
- NavigationService

## Current presenter registry

Register active presenters for:

- `multiple_choice_single`
- `multiple_choice_multiple`
- `true_false`

Register safe fallback handling for all unsupported item types.

## Separation rules

- UI views must not call HTTP directly.
- UI views must not read JSON files directly.
- UI views must not contain official scoring logic.
- Item presenters update answer-draft state.
- Submit coordinator handles submission.
- Result store controls result display data.

## Deferred systems

Do not implement yet:

- MissionProgressStore
- RewardWalletStore for spendable rewards
- ShopStore
- InventoryStore
- WorldRestorationStore
