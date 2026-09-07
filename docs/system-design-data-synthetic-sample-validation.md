# SystemDesignData Synthetic Sample Validation

## Purpose

Validate decoder behavior with inert byte arrays before using real captured HP WMI output.

## 128-Byte Synthetic Sample

- A full 128-byte sample should parse without throwing.
- Known synthetic fields should decode deterministically.
- Reserved or unmapped bytes should remain observable for diagnostics.

## Too-Short Sample

- Short samples should fail gracefully with a clear invalid-length result or diagnostic.
- The decoder should not infer missing bytes or pad silently.

## Empty or Null Sample

- Empty input should be treated as invalid.
- Null input should be rejected before parsing.
- Neither case should produce partial decoded state.

## Unknown Tail Handling

- Unknown trailing bytes should be preserved or reported as unmapped data.
- Unknown tails should not block parsing of known fields when the declared length is valid.

## Why This Is Safe

- Synthetic validation uses local byte arrays only.
- It does not invoke WMI, run `--hp-wmi-readonly-test`, access hardware, or add hardware writes.

## Next Step

Compare decoder output against the previously captured 128-byte `SystemDesignData` sample once captured bytes are made available in a safe, redacted fixture.
