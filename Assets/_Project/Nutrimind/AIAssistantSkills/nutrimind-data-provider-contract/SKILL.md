---
name: nutrimind-data-provider-contract
description: Preserve NutriMind LocalDemoJson and Http provider parity, DTO compatibility, stable keys, idempotent attempts, and server-authoritative outcomes.
required_editor_version: ">=6000.4.10f1"
---
# Provider Contract
Consumers depend on shared provider interfaces and stores. Screens and scenes do not read JSON directly, create HTTP clients, branch on provider mode, or calculate official server outcomes. LocalDemoJson and Http share DTOs, stores, UI, scenes, station mechanics, errors, progress, and reward presentation. Preserve stable keys, reuse `client_attempt_uuid` for retries, handle optional fields safely, and never silently fall back to local data in production.
