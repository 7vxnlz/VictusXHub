# G-Helper First Import Plan

## Copy In First Import Step

- `LICENSE` -> `LICENSE`
- `app\.editorconfig` -> `.editorconfig`
- `app\App.config` -> `ghelper-import\App.config`
- `app\app.manifest` -> `ghelper-import\app.manifest`
- `app\GHelper.sln` -> `ghelper-import\GHelper.sln`
- `app\GHelper.csproj` -> `ghelper-import\GHelper.csproj`

## Exclude In First Import Step

- `.git/`
- `.github/`
- `docs/`
- `app\*.cs`
- `app\*.Designer.cs`
- `app\*.resx`
- `app\Pawn/`
- `app\Resources/`
- `app\favicon.ico`
- `app\Ally/`
- `app\AnimeMatrix/`
- `app\Peripherals/`
- `app\USB/`
- any `bin/`, `obj/`, `logs/`, or generated output

## Must Not Be Overwritten

- `AGENTS.md`
- `AI_CONTEXT.md`
- `SESSION_STATE.md`
- `TOKEN_STRATEGY.md`
- `CONTEXT_RECIPES.md`
- `REFERENCE_POLICY.md`
- `REFERENCE_SOURCES.md`
- `.repomixignore`
- `repomix.config.json`
- `.github/copilot-instructions.md`
- `.github/prompts/`
- `docs/`

## Expected Git Status After Future Import

- deleted: `VictusControl.sln`
- deleted: tracked files under `src/`
- deleted: tracked files under `tests/`
- added: `LICENSE`
- added or modified: `.editorconfig`
- added: `ghelper-import\App.config`
- added: `ghelper-import\app.manifest`
- added: `ghelper-import\GHelper.sln`
- added: `ghelper-import\GHelper.csproj`
- no G-Helper source files added yet
- no preserved workflow/docs files overwritten

## Future Import Commit Message

```text
chore: add initial G-Helper import metadata
```
