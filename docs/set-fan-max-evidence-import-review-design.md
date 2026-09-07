# SetFanMax Evidence Import and Review Design

## Purpose

This future, documentation-only design accepts a manually completed HP Diagnostic Markdown export as untrusted evidence for review. It does not invoke WMI, execute a write, or change the current **NO-GO** status.

## Intended Input and Fields

The input is an exported diagnostic Markdown file with its manual evidence template completed. A future parser may extract the exact device identity, SystemDesignData summary, timestamped FanGetCount/FanMaxGet/FanGetLevel baselines, AC/battery and thermal observations, payload-length evidence, enable/restore readbacks, recovery notes, reviewer metadata, and human-approval checkpoint.

Developer dry-run JSON records may be used only as supporting evidence that the logging path is fail-closed. They are not manual hardware evidence because they intentionally contain no WMI invocation, no live readbacks, no write result, and no restore result.

Developer-only baseline JSON records may supply timestamped exact-device identity and the approved read-only probe summaries when their required flags and elevation are independently verified. They are baseline evidence only: they cannot validate either payload length, prove an enable/restore chain, or change the gate from **NO-GO**.

An absent baseline record, or a record produced without verified elevation, is incomplete evidence and must remain blocked during any future import review.

## Validation and Fail-Closed Behavior

Treat malformed Markdown, missing sections, duplicate or conflicting values, stale or untraceable evidence, non-exact device identity, missing restore proof, and unknown payload length as invalid. The result must report missing or conflicting evidence and remain `NO-GO`; it must never infer values from prose, defaults, repository similarity, or absent fields.

## Limits of Parser Output

Parsing can summarize evidence completeness only. It cannot automatically set the first-write gate to `GO`, grant write permission, set `DeviceValidatedInputLength`, or select one-byte versus four-byte input length. A single imported record is not hardware validation.

Imported dry-run records must remain blocked even when they contain a one-byte or four-byte payload hypothesis. `WriteExecuted=false`, `DeviceValidatedInputLength=null`, and `FirstWriteGateSatisfied=false` are expected and must not be treated as progress toward approval.

## Required Human Review

An independent reviewer must validate the original evidence, exact model/SKU/BIOS match, payload method and length, restore/readback chain, thermal/power conditions, recovery record, and approval wording using the [manual evidence review workflow](set-fan-max-manual-evidence-review-workflow.md). Before implementation, the [first-write decision gate](set-fan-max-first-write-decision-gate.md) and [proof gap checklist](set-fan-max-proof-gap-checklist.md) must be updated together with cited evidence.

## Future Test Strategy

Use pure parser tests for valid structure, missing fields, duplicate/conflicting values, old template versions, malformed Markdown, and fail-closed results. Tests must use fixtures only: no WMI, no hardware, no write payload execution, and no runtime gate mutation.

The concrete fixture matrix and required blocked outcomes are in the [evidence import parser test plan](set-fan-max-evidence-import-parser-test-plan.md).

## Recommended Next Safe Task

Keep the existing export template and review workflow documentation-only. Implement a pure, fixture-based parser only after a separately authorized design task defines a versioned input grammar and reviewer-owned evidence storage.
