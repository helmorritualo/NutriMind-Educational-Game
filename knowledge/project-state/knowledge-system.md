# Repository Knowledge System

- Status: approved
- Owner: Foreman
- Last updated: 2026-06-18
- Tags: knowledge, workflow, governance
- Applicable versions: repository state as of 2026-06-18
- Supersedes: n/a
- Superseded by: n/a

## Summary

This repository already has an approved knowledge structure at `knowledge/`. Do not create a competing knowledge tree. Foreman remains the sole authority for approved long-term project knowledge; specialist contributions must stay advisory until Foreman verifies evidence and promotes them.

No `knowledge-template/` directory exists at the repository root as of this verification. Because `knowledge/index.md` already exists, knowledge bootstrap must preserve this structure and create only missing required items.

## Current structure notes

- Root index: `knowledge/index.md`.
- Approved categories include `architecture/`, `standards/`, `project-state/`, `lessons-learned/`, `agent-observations/`, `decisions/`, `proposals/`, `archive/`, and `templates/`.
- Pending specialist proposals belong under `knowledge/proposals/pending/<agent-name>/index.md`.
- Approved Foreman observations about specialist delegation/evidence belong under `knowledge/agent-observations/<agent-name>.md`.

## Evidence

- `knowledge/index.md` states Foreman authority, advisory specialist proposals, and task-relevant loading rules.
- `knowledge/project-state/index.md` lists approved project-state documents.
- `knowledge/templates/proposal-template.md` requires pending proposal metadata and evidence.
- `knowledge/templates/agent-observation-template.md` requires approved observation metadata and evidence.
- Repository search on 2026-06-18 found `knowledge/index.md` and did not find a root `knowledge-template/` directory.
