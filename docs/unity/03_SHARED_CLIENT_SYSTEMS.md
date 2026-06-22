# Shared Client Systems

## UI-Agnostic Architecture

Because the project uses hybrid Canvas + UI Toolkit, shared logic must not depend on one UI system.

Use:

- stores;
- services;
- presenters;
- UI adapters;
- DTOs;
- provider interfaces.

Canvas views and UI Toolkit views should call the same presenter/service layer.

## Active Systems

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

## Deferred Systems

- MissionProgressStore
- RewardWalletStore for spendable rewards
- ShopStore
- InventoryStore
- WorldRestorationStore
