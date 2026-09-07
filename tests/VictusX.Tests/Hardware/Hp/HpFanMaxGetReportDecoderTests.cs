using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanMaxGetReportDecoderTests
{
    [Fact]
    public void TryDecode_WithSuccessfulFanMaxGetInvocation_DecodesReportSafeSnapshot()
    {
        byte[] sample = [0x01, 0x00, 0x00, 0x00];

        HpFanMaxGetReportDecodeResult result = HpFanMaxGetReportDecoder.TryDecode(
            "FanMaxGet",
            invocationSucceeded: true,
            invocationInvoked: true,
            sample);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Decoded);
        Assert.Equal<byte?>(1, result.Decoded.MaxFanStateRaw);
        Assert.True(result.Decoded.IsMaxFanEnabled);
        Assert.Equal(3, result.Decoded.UnknownByteCount);
        Assert.Equal("1..3", result.Decoded.UnknownByteRange);
        Assert.Equal(0, result.Decoded.UnknownNonZeroByteCount);
    }

    [Fact]
    public void TryDecode_WithNonFanMaxGetCommand_SkipsDecode()
    {
        byte[] sample = [0x00, 0x00, 0x00, 0x00];

        HpFanMaxGetReportDecodeResult result = HpFanMaxGetReportDecoder.TryDecode(
            "FanGetCount",
            invocationSucceeded: true,
            invocationInvoked: true,
            sample);

        Assert.False(result.Succeeded);
        Assert.Null(result.Decoded);
        Assert.Contains(result.Errors, error => error.Contains("not FanMaxGet", StringComparison.Ordinal));
    }

    [Fact]
    public void TryDecode_WithUnsuccessfulInvocation_SkipsDecode()
    {
        byte[] sample = [0x00, 0x00, 0x00, 0x00];

        HpFanMaxGetReportDecodeResult result = HpFanMaxGetReportDecoder.TryDecode(
            "FanMaxGet",
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
        byte[] sample = [0x00];

        HpFanMaxGetReportDecodeResult result = HpFanMaxGetReportDecoder.TryDecode(
            "FanMaxGet",
            invocationSucceeded: true,
            invocationInvoked: true,
            sample);

        Assert.False(result.Succeeded);
        Assert.Null(result.Decoded);
        Assert.Contains(result.Errors, error => error.Contains("expected 4 bytes", StringComparison.Ordinal));
    }
}
