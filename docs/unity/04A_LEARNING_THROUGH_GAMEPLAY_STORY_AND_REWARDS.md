# Learning-Through-Gameplay, Story, NPC, Exploration, and Rewards

## Purpose

This document adds a cross-cutting gameplay-design requirement after the shared Phase 1–4 foundation work. It does not require those completed phases to be rebuilt. Before world and station production continues, the Unity agent performs one compatibility review and adds only the missing shared extensions required by this document.

The central rule is:

> Learning must be the action the student performs to progress, not a lesson or quiz placed beside unrelated game activity.

Walking to an NPC, reading a long lesson, and answering a detached set of questions is not sufficient. The station mechanic itself must require the student to observe, classify, sequence, infer, revise, decide, or apply the target concept.

## Compatibility With Completed Phases 1–4

Do not restart or broadly rewrite the completed foundation. Review the approved implementation and add only compatible extensions when missing:

- game-flow states for mission introduction, discovery, hint/retry, reflection, reward presentation, and world restoration
- optional DTO fields for narrative, NPC guidance, hint policy, discoveries, reward previews, and restoration state
- reusable UI Toolkit components for mission briefing, NPC dialogue, hints, discoveries, feedback, reflection, and rewards
- shared stores/services for temporary discovery state and provider-confirmed reward/progress state
- tests for the new additive states and unknown optional fields

Existing public interfaces should remain stable when possible. New server fields must be optional and additive so older local fixtures or server responses do not crash the client.

## Default Demo Story Framework

The current demo may use a configurable **Knowledge Restoration** story:

- the learning realms have lost pieces of their knowledge energy or learning crystals
- each term world contains two missions represented by its two playable stations
- completing a station restores a visible part of the term world, such as a path, bridge, lantern, garden, information board, or community area
- completing both stations restores the term landmark and awards a subject-themed crystal or badge
- LiteraQuest may award **Language Crystals**
- PE/Health Quest may award **Wellness Crystals**
- Science remains an exploration preview in the current milestone and must not award station-completion crystals, official progress, or rewards

The title, story text, NPC names, dialogue, mission summaries, restoration labels, and reward display data should come from the active data provider when available. Unity owns presentation and animation, not canonical story content.

## Required Learning Cycle

Every playable station follows this cycle:

1. **Discover** — the student encounters a problem, mystery, unsafe condition, broken structure, confused NPC, or incomplete world state.
2. **Practice** — the student explores and performs smaller concept-related interactions such as finding evidence, testing classifications, arranging pieces, or comparing choices.
3. **Apply** — the student uses the concept to solve the station mission and create a visible result in the environment.
4. **Review** — the provider returns feedback; Unity shows what worked, a misconception-aware explanation or hint, the progress/reward result, and a short reflection when appropriate.

In the data contract this cycle is delivered as the canonical `learning_cycle` **object** — `{ discover, practice, apply, review }`, one short student-safe guidance string per phase — not as an array of strings. See the canonical schema in `docs/SERVER_REQUIREMENTS.md` (Canonical Unity Data Contract Schemas) and `docs/unity/04_SERVER_CONNECTION_AND_UNITY_API.md`. Phase guidance fields are optional and additive.

A station must not skip directly from a decorative walk to a detached question panel unless accessibility or content type makes world interaction impossible. Even then, the UI task must be framed as the mission action rather than a generic quiz list.

## NPC Mentor and Mission-Giver Rules

NPC teachers or guides may:

- introduce the problem in one short, child-friendly briefing
- point the student toward the first meaningful interaction
- respond to world progress
- provide provider-approved hints after failed attempts
- celebrate completion and prompt a short reflection

NPCs must not deliver long lectures before gameplay. Dialogue should be concise, skippable/replayable where appropriate, and represented by stable NPC/dialogue keys. NPC presentation must remain functional when a character model or voice asset is unavailable by using a safe UI Toolkit dialogue fallback.

## Safe Mistakes and Hint Progression

Wrong attempts should teach, not punish.

Required behavior:

- do not use a bare `Incorrect` response as the only feedback
- preserve the student's current station progress where safe
- allow another attempt according to the provider-approved attempt policy
- provide encouraging, misconception-aware feedback
- offer tiered hints after failed attempts without immediately exposing hidden answer keys
- avoid harsh loss of coins, progress, or access for ordinary learning mistakes
- distinguish local interaction feedback from provider-confirmed correctness

A suggested hint progression is:

```txt
Attempt 1 -> encouragement plus a reminder of the relevant clue or rule
Attempt 2 -> point to a specific piece of evidence or reduce the search space
Attempt 3 -> provide stronger scaffolding or a worked partial example, if approved
```

The server or local demo provider controls official attempt limits, scoring, hint usage, feedback, and reward effects. Health hints must use approved educational content and must not invent medical guidance.

The hint policy is delivered as the canonical `hint_policy` object — `{ max_hint_tier, preserve_world_progress, penalize_ordinary_mistake, tiers: [ { tier, text } ] }`. NPC guidance is delivered as `npc_guides[]` — `{ npc_key, display_name, role, avatar_key, intro_dialogue, completion_dialogue }`. Per-attempt feedback uses the canonical attempt feedback fields (`is_correct`, `message`, `explanation`, `misconception_message`, `encouraging_message`, `retry_action`, `retry_allowed`, `remaining_attempts`, `current_hint_tier`, `next_hint_tier`, `hint_text`); station success feedback uses `success_feedback { message, encouraging_phrases[] }`. See the canonical schema in `docs/SERVER_REQUIREMENTS.md`.

## Exploration and Optional Discoveries

Term worlds may contain optional discovery content such as:

- hidden fun facts
- optional bonus coins
- secret cosmetic areas
- story notes or environmental clues
- non-required NPC conversations

Optional discoveries must:

- use approved server/demo content
- never be required to complete the two term stations
- not mislead students into thinking Science has functional stations in the current milestone
- avoid grind, random farming, combat, or unrelated collection
- have a clear discovery state and accessible presentation
- fail safely when content or an asset is missing

Science preview worlds may use static environmental storytelling and an exploration-preview label, but they must not add collectible rewards, missions, station completion, or progress in the current milestone.

## Reward and Progression Design

Rewards should recognize learning progress and support motivation without replacing the learning goal. Supported catalog ideas include:

- coins
- subject crystals
- badges
- titles
- cosmetic character skins
- cosmetic pets or companions
- optional cosmetic-area unlocks

For the current demo, the minimum visible reward set is:

- coins or stars for accepted station completion
- one subject-themed crystal or badge for completing both stations in a term
- visible world restoration and portal completion state

Cosmetic skins, pets, titles, and additional areas may remain future scope unless assets and server support already exist. Official reward grants, wallet changes, and unlocks remain provider/server-authoritative. Unity may preview possible rewards but may animate them as earned only after the accepted result.

In the data contract: motivational previews use `reward_preview[]` — `{ code, reward_key, reward_type, display_name, icon_key, quantity, grant_scope }` (a subject crystal uses `grant_scope = "term_after_both_stations"`). Earned grants in attempt/completion results use `reward_grant` — `{ reward_code, reward_type, display_name, quantity }`. A term crystal/badge is awarded on `term_completion { subject_slug, term_number, completed, crystal, badge }` when both required stations are complete. Authored restoration uses `world_restoration_state { state_key, apply_after_accepted_completion, state_data }` and the provider-confirmed result is `world_restoration_result { state_key, restored }`, applied only after an accepted completion. See the canonical schema in `docs/SERVER_REQUIREMENTS.md`.

## Station Design Contract

Every playable station must document and implement:

- the story problem or mission
- the NPC/guide role, if any
- the Discover interaction
- the Practice interaction
- the Apply interaction
- the Review/feedback interaction
- safe-mistake and hint behavior
- visible environmental consequence or restoration
- optional discoveries, if any
- official completion rule
- provider-owned reward result

World interaction must directly represent the learning skill. Unrelated walking, platforming, combat, or collection cannot be counted as educational progress.

## Current Subject Application

### LiteraQuest

- Vocabulary Clue Trail: investigate environmental clues to restore a word lantern or Language Crystal.
- Sequence Path: arrange story events to rebuild a broken path.
- Inference Investigation: inspect a scene and help a guide solve a mystery using evidence.
- Fact or Opinion Market: evaluate claims so the market information board can reopen.
- Sentence Repair Workshop: repair sentences to restart workshop devices.
- Paragraph Order Bridge: organize paragraph parts to rebuild a bridge.

### PE/Health Quest

- Wellness Choice Trail: help an NPC complete a healthy daily route and restore the wellness trail.
- Sanitation Sorting Center: classify practices and objects to clean and reopen a community area.
- First-Aid Sequence Station: arrange approved response steps and guide the character toward trusted-adult help.
- Safety Decision Branch: choose safe routes through a child-appropriate scenario.
- Medicine Label Safety Check: inspect a fabricated label to unlock a safe-storage area.
- Healthy Habit and Advertisement Truth Board: evaluate claims and restore a trustworthy community information board.

These are presentation and interaction directions. The actual prompts, choices, correct answers, hints, story text, dialogue, rewards, and feedback remain data-driven.

## Acceptance

This design amendment is satisfied when:

- completed Phase 1–4 systems were reviewed without unnecessary rewrites
- required additive states, DTO support, shared UI components, and tests exist
- every future LiteraQuest and PE/Health station uses the four-step learning cycle
- each station has a meaningful world action tied to its learning skill
- mistakes provide safe feedback and hints
- world restoration and rewards occur only after accepted provider results
- optional discoveries remain optional and data-driven
- Science remains exploration-only in the current milestone
