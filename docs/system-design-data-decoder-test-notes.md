# SystemDesignData Decoder Test Notes

No test project exists in the current VictusX solution, so decoder coverage is recorded here instead of adding new test infrastructure.

Future tests for `HpSystemDesignDataDecoder` should use pure byte arrays only. They must not call WMI, run `--hp-wmi-readonly-test`, invoke `hpqBIOSInt*`, or touch hardware.

## Required coverage

1. 128-byte input succeeds
   - Create a 128-byte array.
   - Set known prefix bytes.
   - Verify `IsValid` is `true`.
   - Verify `Length` is `128`.
   - Verify `ValidationError` is empty.

2. Too-short input fails safely
   - Pass an array shorter than 12 bytes.
   - Verify `IsValid` is `false`.
   - Verify no exception is thrown.
   - Verify decoded fields remain `null`.

3. Null and empty input fail safely
   - Pass `null`.
   - Pass `Array.Empty<byte>()`.
   - Verify both return invalid snapshots without throwing.

4. Unknown tail is summarized, not interpreted
   - Pass a 128-byte array with non-zero bytes after index 11.
   - Verify `UnknownByteCount` is `116`.
   - Verify `UnknownByteRange` is `12..127`.
   - Verify `UnknownNonZeroByteCount` reflects the tail.
   - Do not assert meanings for bytes `[12..127]`.

## Safety expectation

The decoder should remain a side-effect-free parser. It must never connect to WMI, invoke HP BIOS methods, control fans, change power state, write EC/BIOS values, or infer that any control feature is safe.
