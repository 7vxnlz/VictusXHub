# UI Scope

- Preserve the compact inherited G-Helper-style shell and existing theme/layout primitives.
- In HP mode, unsupported inherited controls stay disabled/read-only and readable; they must not call ASUS or HP write paths.
- Diagnostic remains a read-only, scrollable owned side panel opened from the footer.
- Do not add fan controls, pulse/run actions, hardware probes, background writes, or normal-control affordances.
- UI telemetry displays only validated fresh values; ambiguous or missing values show Unavailable/Unknown.
- Keep edits focused on `Settings.cs`, the directly used UI helper, and focused configuration tests.
- Use `docs/context-packs/ui.md` for the default selection.
