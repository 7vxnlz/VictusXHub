# SetFanMax Evidence Import Parser Test Plan

## Scope and Inputs

Test a future pure parser against sanitized, manually completed diagnostic Markdown fixtures only. Accepted inputs are the versioned VictusX export template and explicitly supported older template versions. The parser may extract identity, SystemDesignData, baseline telemetry, power/thermal observations, payload evidence, enable/restore readbacks, recovery notes, reviewer details, and approval text.

## Forbidden Side Effects

Tests must not invoke WMI, access hardware, construct or execute write payloads, modify report/gate state, select a payload length, or approve a write.

## Required Fixture Cases

- **Completeness:** a structurally complete exact-device fixture reports fields present but remains `NO-GO` pending human review and gate-document updates.
- **Missing fields:** omit each required identity, baseline, payload, restore, thermal/power, recovery, reviewer, and approval field; each result lists the gap and fails closed.
- **Conflicts:** duplicate values, conflicting model/SKU/BIOS, incompatible baseline/readback values, or conflicting payload claims are rejected.
- **Optimistic or unsafe claims:** `GO`, approved, enabled, selected length, or success claims without all traceable proof are informational only and remain blocked.
- **Malformed Markdown:** missing headings, broken lists, truncated files, unknown template version, unsupported formatting, and unparseable values yield a safe invalid result.
- **Exact-device mismatch:** any model, SKU, BIOS, or thermal-policy mismatch from Victus `16-s0035nt` / `7Z5Z2EA#AB8` / `F.31` fails closed.
- **Payload ambiguity:** absent length, both lengths, a length without matching evidence, or any one-byte/four-byte contradiction remains unresolved and blocked.

## Expected Results

Every invalid, incomplete, ambiguous, or optimistic fixture returns explicit missing/conflict reasons with gate status `NO-GO` and gate satisfaction `false`. Even the most complete fixture can only report `ready for human review`; it cannot set `DeviceValidatedInputLength`, select a payload shape, or grant write permission.

## Recommended Next Safe Task

Keep this as a test design until a separately authorized task defines the versioned Markdown grammar and pure parser contract. Any later tests must remain fixture-only and preserve fail-closed results.
