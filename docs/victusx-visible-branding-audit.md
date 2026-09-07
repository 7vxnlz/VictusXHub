# VictusX Visible Branding Audit

## Safe Display Changes

The inherited-shell window title and tray caption identify the running application as `VictusX`. All `Strings*.resx` display values now use `VictusX` where the inherited product name appeared, including already-running, open-window, ACPI-error, GPU-restart, and Optimus warning text. Only the product token changed; surrounding translations were preserved.

Resource keys such as `OpenGHelper` were retained, so existing callers and serialized/configured action mappings are unchanged. A focused test scans resource `<value>` elements for inherited `GHelper`, `G-Helper`, or `G Helper` branding while confirming the compatibility key remains.

## Intentionally Preserved Compatibility References

- `GHelper` namespaces, `RootNamespace`, `StartupObject`, generated resource namespaces, and resource logical names.
- The `ghelper` custom-action value and related input mapping.
- `Global\\GHelperApp-Exit`, app-data paths, embedded resource logical names, and project/internal type names.
- Updater URLs, updater user-agent text, process/update command assumptions, and inherited documentation/source attribution.
- Existing icon resource names and the final icon wiring plan.

These references are implementation, migration, configuration, upstream-attribution, or updater compatibility concerns rather than safe standalone branding text changes.

## Remaining Scope

No inherited G-Helper product token remains in localized display values. The wording and availability of inherited ASUS/GPU actions are separate behavior and capability concerns; this branding cleanup does not enable them or change HP-mode safety restrictions.

No namespace, identifier, hardware behavior, dependency, or fan-related code changed.
