# VictusX HP Mode Branding and Packaging Audit

## Current Visible Identity

`--hp-victus` sets the settings-window title to **VictusX Read-only Diagnostic**, the tray tooltip to **VictusX**, the duplicate-instance message to **VictusX is already running**, and repurposes the Donate slot as **Diagnostic**. The Diagnostic dashboard itself is correctly branded as read-only.

## Remaining HP-Mode Risks

- The imported settings shell still creates the ASUS-oriented performance, fan/power, GPU, battery, display, keyboard, visual, update, and startup surfaces. `--hp-victus` does not yet have one explicit shell-isolation branch that hides or disables every inherited control surface.
- The tray context menu still builds performance-mode and GPU-mode entries before its local-only items. The underlying control methods fail closed in unsupported-hardware mode, but their presence is misleading and unsuitable for a read-only HP diagnostic release.
- The Updates button and version-label action still lead into ASUS driver updates and an inherited G-Helper self-update channel. They should not be offered in HP mode.
- ASUS/G-Helper strings remain in inherited resources and in disabled/conditional surfaces, including ASUS ACPI, Armoury Crate, Aura, Ally, ROG, and G-Helper update wording. These are expected in the shared default path, but any that remain exposed in the HP shell are release blockers.

## Tray, Process, and Package Status

- Executable assembly name: `VictusX`; manifest identity: `VictusX.app`; normal HP-mode window title verified as `VictusX Read-only Diagnostic`.
- The single-instance exit event, namespaces, startup object, and embedded-resource logical names still use `GHelper`. These are internal compatibility details, not visible HP-mode branding defects.
- The project uses `favicon.ico` for the application icon and an inherited `standard` resource for the tray icon. Their VictusX visual identity has not been independently validated.
- The publish profile creates a framework-dependent, single-file `win-x64` output and ZIP. There is no HP-specific installer, signed package, package metadata, release channel, or update feed.
- The inherited updater still queries `seerge/g-helper` releases, uses a `G-Helper App` user agent, and contains G-Helper process/update wording. It is not safe for a VictusX release channel.

## Change Now

Implement an HP-mode shell-isolation pass only: show the read-only Diagnostic dashboard and local-only actions; hide inherited performance, fan/power, GPU, battery, keyboard, display, visual, startup, and ASUS-update controls; and replace the HP tray menu with Diagnostic and Quit only. Also suppress all automatic/manual updater entry points in HP mode. This is a UI/packaging-boundary change, not hardware work.

## Change Later

After HP mode is intentionally productized, provide a dedicated VictusX icon, product/file metadata, signed installer or package, owned update feed, release notes, and an HP-specific update policy. Revisit namespace and event naming only as a separately scoped compatibility migration.

## Preserve

Do not rename shared `GHelper` namespaces, resources, startup tasks, event names, or ASUS controls as part of HP-mode cleanup. The default ASUS/G-Helper path must keep its existing branding, controls, update behavior, and compatibility until an explicit broader migration is approved.

## Recommended Next Safe Task

Implement and test the conditional HP-mode shell isolation described above, with no WMI calls, no live refresh, no fan/performance controls, and no hardware writes.
