# G-Helper Import Shell Assessment

## What Was Imported

- `LICENSE` from the G-Helper repository root.
- `.editorconfig` from `app/.editorconfig`.
- `ghelper-import/GHelper.sln`.
- `ghelper-import/GHelper.csproj`.
- `ghelper-import/App.config`.
- `ghelper-import/app.manifest`.

## What Is Missing For Build

- G-Helper source files, including `Program.cs` and the WinForms entry path.
- Forms, controls, helpers, services, and other code referenced implicitly by the project.
- `Properties/Resources.*`, `Properties/Strings.*`, and `Properties/Settings.*` files.
- `favicon.ico`.
- Embedded `Pawn/RyzenSMU.bin` and `Pawn/IntelMSR.bin` resources.
- A confirmed local .NET SDK compatible with the imported `net10.0-windows` target.

## Rename/Rebrand Later

- `GHelper.sln` and the `GHelper` project name.
- `GHelper.csproj` assembly name, startup object, icon, embedded resource logical names, and local build target names.
- `app.manifest` assembly identity.
- Any user-visible G-Helper names after source/resources are imported.

## Imported Files That Reference Missing Source Or Resources

- `GHelper.csproj` references `favicon.ico`, `Properties/*`, and `Pawn/*.bin` resources.
- `GHelper.csproj` sets `StartupObject` to `GHelper.Program`, but `Program.cs` is not imported yet.
- `GHelper.sln` references `.editorconfig` as a solution item; that file is present at repository root.

## Smallest Next Copy Step

Copy only `app/Program.cs` into `ghelper-import/Program.cs` for a token-safe startup dependency assessment. Do not build until its immediate dependencies are mapped.
