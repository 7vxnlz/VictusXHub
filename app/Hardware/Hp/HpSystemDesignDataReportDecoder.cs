namespace GHelper.Hardware.Hp;

public static class HpSystemDesignDataReportDecoder
{
    private const string CommandName = "SystemDesignData";

    public static HpSystemDesignDataReportDecodeResult TryDecode(
        string commandName,
        bool invocationSucceeded,
        bool invocationInvoked,
        byte[]? returnedBytes)
    {
        if (!string.Equals(commandName, CommandName, StringComparison.OrdinalIgnoreCase))
        {
            return Failed("SystemDesignData decode skipped: invocation command was not SystemDesignData.");
        }

        if (!invocationSucceeded || !invocationInvoked)
        {
            return Failed("SystemDesignData decode skipped: invocation did not succeed.");
        }

        if (returnedBytes is null)
        {
            return Failed("SystemDesignData decode skipped: invocation returned no byte array.");
        }

        if (returnedBytes.Length != HpSystemDesignDataDecoder.ExpectedLength)
        {
            return Failed($"SystemDesignData decode skipped: expected {HpSystemDesignDataDecoder.ExpectedLength} bytes, received {returnedBytes.Length}.");
        }

        HpSystemDesignDataSnapshot decoded = HpSystemDesignDataDecoder.Decode(returnedBytes);
        if (!decoded.IsValid)
        {
            return Failed(decoded.ValidationError);
        }

        return new HpSystemDesignDataReportDecodeResult(true, [], decoded);
    }

    private static HpSystemDesignDataReportDecodeResult Failed(string error) =>
        new(false, [error], null);
}

public sealed record HpSystemDesignDataReportDecodeResult(
    bool Succeeded,
    string[] Errors,
    HpSystemDesignDataSnapshot? Decoded);
