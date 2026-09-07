# HP WMI Read-Only Test Path Audit

## 1. Required flags

Real HP BIOS WMI invocation requires both command-line flags:

```bash
--hp-victus
--hp-wmi-readonly-test
```

`--hp-victus` alone runs only safe HP identity, WMI availability, CIM readiness, diagnostics, and dry-run validation.

## 2. Required elevation condition

The process must be elevated/admin before real invocation is allowed. If the process is not elevated, the invocation client returns a structured rejected result before reaching `InvokeMethod`.

## 3. Only allowed command

Only `SystemDesignData` is currently eligible for real invocation.

Required command metadata:

- `Access`: `ReadOnly`
- `Safety`: `SafeReadOnlyInvocation`
- `Command`: `0x20008`
- `CommandType`: `0x28`
- `Method`: `hpqBIOSInt128`
- `ExpectedInputSize`: `0`
- `ExpectedOutputSize`: `128`

## 4. All blocked command categories

Blocked categories include:

- Fan reads and writes
- Performance / thermal mode reads and writes
- Battery charge limit reads and writes
- Keyboard / RGB / lighting reads and writes
- GPU mode / GPU power reads and writes
- Display hardware reads and writes
- EC access
- BIOS setting writes
- Power writes
- Any unknown or ambiguous command
- Any command not marked `SafeReadOnlyInvocation`
- Any command other than `SystemDesignData`

## 5. What happens without `--hp-wmi-readonly-test`

The app does not invoke HP BIOS WMI. The report should show:

- `SystemDesignDataInvocationAllowed`: `false`
- `SystemDesignDataInvocationAttempted`: `false`
- `HpWmiInvocationBlockedReason`: missing explicit `--hp-wmi-readonly-test` flag

## 6. What happens without admin/elevation

Even if `--hp-victus --hp-wmi-readonly-test` are present, the app does not invoke HP BIOS WMI unless the process is elevated. The invocation client rejects the request before `InvokeMethod`.

The report should show:

- `ProcessElevated`: `false`
- `SystemDesignDataInvocationAllowed`: `false`
- `SystemDesignDataInvocationAttempted`: `false`
- `HpWmiInvocationBlockedReason`: process is not elevated

## 7. Report fields that prove the behavior

Check:

- `ProcessElevated`
- `HpWmiInvocationRequiresElevation`
- `HpWmiInvocationBlockedReason`
- `HpWmiRecommendedNextStep`
- `SystemDesignDataDryRunReady`
- `SystemDesignDataInvocationAllowed`
- `SystemDesignDataInvocationAttempted`
- `SystemDesignDataInvocationSucceeded`
- `SystemDesignDataInvocationError`
- `SafeReadOnlyCommandCount`
- `RejectedCommandCount`

## 8. Final manual test recommendation

The elevated manual test path is constrained enough for a controlled developer test of `SystemDesignData` only. Run it only from an elevated Administrator terminal and only with:

```bash
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test
```

Do not proceed if any fan, EC, BIOS-write, power, thermal, battery, RGB, keyboard lighting, GPU, display, or additional HP WMI command path becomes involved.
