# Unity Game Flow and State Model

## Active Flow

```text
Splash
→ Login
→ Main Interface
→ Quiz Portal
→ Quiz List
→ Quiz Instructions
→ Quiz Session
→ Submit Quiz
→ Quiz Result
→ Return
```

## Optional Placeholder

A disabled Adventure/Open World button may exist but must clearly show that missions are not available in this milestone.

## Required States

- Bootstrapping
- Splash
- Unauthenticated
- Authenticating
- MainInterface
- Profile
- Settings
- SubjectTermFilter
- Loading
- QuizPortal
- QuizList
- QuizInstructions
- QuizSession
- QuizSubmitting
- QuizResult
- ErrorRecovery

## Deferred States

- PlayingWorld
- PlayingMission
- CompletingMission
- RewardShop
- Inventory
- WorldRestoration
