namespace GHelper.Hardware.Hp;

public static class HpFanGetLevelReportDecoder
{
    private const string CommandName = "FanGetLevel";

    public static HpFanGetLevelReportDecodeResult TryDecode(
        string commandName,
        bool invocationSucceeded,
        bool invocationInvoked,
        byte[]? returnedBytes)
    {
        if (!string.Equals(commandName, CommandName, StringComparison.OrdinalIgnoreCase))
        {
            return Failed("FanGetLevel decode skipped: invocation command was not FanGetLevel.");
        }

        if (!invocationSucceeded || !invocationInvoked)
        {
            return Failed("FanGetLevel decode skipped: invocation did not succeed.");
        }

        if (returnedBytes is null)
        {
            return Failed("FanGetLevel decode skipped: invocation returned no byte array.");
        }

        if (returnedBytes.Length != HpFanGetLevelDecoder.ExpectedLength)
        {
            return Failed($"FanGetLevel decode skipped: expected {HpFanGetLevelDecoder.ExpectedLength} bytes, received {returnedBytes.Length}.");
        }

        HpFanGetLevelSnapshot decoded = HpFanGetLevelDecoder.Decode(returnedBytes);
        if (!decoded.IsValid)
        {
            return Failed(decoded.ValidationError);
        }

        return new HpFanGetLevelReportDecodeResult(true, [], decoded);
    }

    private static HpFanGetLevelReportDecodeResult Failed(string error) =>
        new(false, [error], null);
}

public sealed record HpFanGetLevelReportDecodeResult(
    bool Succeeded,
    string[] Errors,
    HpFanGetLevelSnapshot? Decoded);
