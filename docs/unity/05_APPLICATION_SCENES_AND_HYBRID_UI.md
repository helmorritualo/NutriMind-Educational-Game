# Application Scenes and UI Technology — v8

## Overall project UI

The project may remain hybrid Canvas + UI Toolkit because some application scenes were created before the quiz UI decision changed.

Do not rebuild valid existing application scenes only to change UI technology.

## Quiz system UI

The Quiz Portal / Assessment Room must use Canvas/uGUI for Phase 8B.

This includes:

- Quiz Portal Home
- Available Quiz List
- Empty Quiz State
- Locked/Unavailable View modal
- Quiz Instructions
- Quiz Session Shell
- Multiple Choice Single
- Multiple Choice Multiple
- True/False
- Unsupported Item State
- Submit Confirmation
- Quiz Result Screen
- Error/Retry State

## Shared logic rule

Even when using Canvas, business logic must remain UI-agnostic.

Canvas views should call shared presenters/services/stores. Do not put provider calls, scoring, or state-machine decisions directly inside UI button handlers except simple forwarding.
