# SystemDesignData Decode Verification Guide

## Purpose

Verify decoded report output from an elevated `SystemDesignData` read-only invocation.

## Human Command

Run from an elevated Administrator terminal:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test
```

## Report Path

```text
%APPDATA%\VictusX\hp-capability-report.json
```

## Fields To Copy Back

- `SystemDesignDataInvocationSucceeded`
- `SystemDesignDataReturnedByteCount`
- `SystemDesignDataDecodeSucceeded`
- `SystemDesignDataDecodeErrors`
- `SystemDesignDataDecoded`

## Expected Safe Result

- Invocation succeeds.
- Returned byte count is `128`.
- Decode succeeds or fails safely with report errors.
- Unknown tail remains summarized only.

## Still Forbidden

- Do not invoke WMI from Codex.
- Do not run `--hp-wmi-readonly-test` from Codex.
- Do not add fan, performance, battery, RGB, keyboard lighting, GPU, EC, BIOS write, or hardware write behavior.
