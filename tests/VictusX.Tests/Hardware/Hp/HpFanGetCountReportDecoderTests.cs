using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanGetCountReportDecoderTests
{
    [Fact]
    public void TryDecode_WithSuccessfulFanGetCountInvocation_DecodesReportSafeSnapshot()
    {
        byte[] sample = [0x02, 0x03, 0x00, 0x00];

        HpFanGetCountReportDecodeResult result = HpFanGetCountReportDecoder.TryDecode(
            "FanGetCount",
            invocationSucceeded: true,
            invocationInvoked: true,
            sample);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Decoded);
        Assert.Equal<byte?>(2, result.Decoded.FanCount);
        Assert.Equal<byte?>(0x03, result.Decoded.ProtectionStatusRaw);
        Assert.True(result.Decoded.OverCurrentProtectionTripped);
        Assert.True(result.Decoded.OverTemperatureProtectionTripped);
        Assert.Equal(2, result.Decoded.UnknownByteCount);
        Assert.Equal("2..3", result.Decoded.UnknownByteRange);
        Assert.Equal(0, result.Decoded.UnknownNonZeroByteCount);
    }

    [Fact]
    public void TryDecode_WithNonFanGetCountCommand_SkipsDecode()
    {
        byte[] sample = [0x02, 0x00, 0x00, 0x00];

        HpFanGetCountReportDecodeResult result = HpFanGetCountReportDecoder.TryDecode(
            "SystemDesignData",
            invocationSucceeded: true,
            invocationInvoked: true,
            sample);

        Assert.False(result.Succeeded);
        Assert.Null(result.Decoded);
        Assert.Contains(result.Errors, error => error.Contains("not FanGetCount", StringComparison.Ordinal));
    }

    [Fact]
    public void TryDecode_WithUnsuccessfulInvocation_SkipsDecode()
    {
        byte[] sample = [0x02, 0x00, 0x00, 0x00];

        HpFanGetCountReportDecodeResult result = HpFanGetCountReportDecoder.TryDecode(
            "FanGetCount",
            invocationSucceeded: false,
            invocationInvoked: true,
            sample);

        Assert.False(result.Succeeded);
        Assert.Null(result.Decoded);
        Assert.Contains(result.Errors, error => error.Contains("did not succeed", StringComparison.Ordinal));
    }

    [Fact]
    public void TryDecode_WithInvalidLength_SkipsDecode()
    {
        byte[] sample = [0x02, 0x00];

        HpFanGetCountReportDecodeResult result = HpFanGetCountReportDecoder.TryDecode(
            "FanGetCount",
            invocationSucceeded: true,
            invocationInvoked: true,
            sample);

        Assert.False(result.Succeeded);
        Assert.Null(result.Decoded);
        Assert.Contains(result.Errors, error => error.Contains("expected 4 bytes", StringComparison.Ordinal));
    }
}
