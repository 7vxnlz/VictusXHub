namespace GHelper.Hardware.Hp;

public static class HpFanGetCountReportDecoder
{
    private const string CommandName = "FanGetCount";

    public static HpFanGetCountReportDecodeResult TryDecode(
        string commandName,
        bool invocationSucceeded,
        bool invocationInvoked,
        byte[]? returnedBytes)
    {
        if (!string.Equals(commandName, CommandName, StringComparison.OrdinalIgnoreCase))
        {
            return Failed("FanGetCount decode skipped: invocation command was not FanGetCount.");
        }

        if (!invocationSucceeded || !invocationInvoked)
        {
            return Failed("FanGetCount decode skipped: invocation did not succeed.");
        }

        if (returnedBytes is null)
        {
            return Failed("FanGetCount decode skipped: invocation returned no byte array.");
        }

        if (returnedBytes.Length != HpFanGetCountDecoder.ExpectedLength)
        {
            return Failed($"FanGetCount decode skipped: expected {HpFanGetCountDecoder.ExpectedLength} bytes, received {returnedBytes.Length}.");
        }

        HpFanGetCountSnapshot decoded = HpFanGetCountDecoder.Decode(returnedBytes);
        if (!decoded.IsValid)
        {
            return Failed(decoded.ValidationError);
        }

        return new HpFanGetCountReportDecodeResult(true, [], decoded);
    }

    private static HpFanGetCountReportDecodeResult Failed(string error) =>
        new(false, [error], null);
}

public sealed record HpFanGetCountReportDecodeResult(
    bool Succeeded,
    string[] Errors,
    HpFanGetCountSnapshot? Decoded);
