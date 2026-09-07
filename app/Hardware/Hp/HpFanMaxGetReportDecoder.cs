namespace GHelper.Hardware.Hp;

public static class HpFanMaxGetReportDecoder
{
    private const string CommandName = "FanMaxGet";

    public static HpFanMaxGetReportDecodeResult TryDecode(
        string commandName,
        bool invocationSucceeded,
        bool invocationInvoked,
        byte[]? returnedBytes)
    {
        if (!string.Equals(commandName, CommandName, StringComparison.OrdinalIgnoreCase))
        {
            return Failed("FanMaxGet decode skipped: invocation command was not FanMaxGet.");
        }

        if (!invocationSucceeded || !invocationInvoked)
        {
            return Failed("FanMaxGet decode skipped: invocation did not succeed.");
        }

        if (returnedBytes is null)
        {
            return Failed("FanMaxGet decode skipped: invocation returned no byte array.");
        }

        if (returnedBytes.Length != HpFanMaxGetDecoder.ExpectedLength)
        {
            return Failed($"FanMaxGet decode skipped: expected {HpFanMaxGetDecoder.ExpectedLength} bytes, received {returnedBytes.Length}.");
        }

        HpFanMaxGetSnapshot decoded = HpFanMaxGetDecoder.Decode(returnedBytes);
        if (!decoded.IsValid)
        {
            return Failed(decoded.ValidationError);
        }

        return new HpFanMaxGetReportDecodeResult(true, [], decoded);
    }

    private static HpFanMaxGetReportDecodeResult Failed(string error) =>
        new(false, [error], null);
}

public sealed record HpFanMaxGetReportDecodeResult(
    bool Succeeded,
    string[] Errors,
    HpFanMaxGetSnapshot? Decoded);
