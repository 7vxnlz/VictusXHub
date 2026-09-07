using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanGetLevelReportDecoderTests
{
    [Fact]
    public void TryDecode_WithSuccessfulFanGetLevelInvocation_DecodesRawReportSnapshot()
    {
        byte[] sample = new byte[HpFanGetLevelDecoder.ExpectedLength];
        sample[0] = 0x21;
        sample[1] = 0x2A;

        HpFanGetLevelReportDecodeResult result = HpFanGetLevelReportDecoder.TryDecode(
            "FanGetLevel",
            invocationSucceeded: true,
            invocationInvoked: true,
            sample);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Decoded);
        Assert.Equal<byte?>(0x21, result.Decoded.Fan1RawValue);
        Assert.Equal<byte?>(0x2A, result.Decoded.Fan2RawValue);
        Assert.Equal([0x21, 0x2A], result.Decoded.RawValueBytes);
        Assert.Equal(126, result.Decoded.UnknownByteCount);
    }

    [Fact]
    public void TryDecode_WithDifferentCommand_SkipsDecode()
    {
        byte[] sample = new byte[HpFanGetLevelDecoder.ExpectedLength];

        HpFanGetLevelReportDecodeResult result = HpFanGetLevelReportDecoder.TryDecode(
            "FanMaxGet",
            invocationSucceeded: true,
            invocationInvoked: true,
            sample);

        Assert.False(result.Succeeded);
        Assert.Null(result.Decoded);
        Assert.Contains(result.Errors, error => error.Contains("not FanGetLevel", StringComparison.Ordinal));
    }

    [Fact]
    public void TryDecode_WithUnsuccessfulInvocation_SkipsDecode()
    {
        byte[] sample = new byte[HpFanGetLevelDecoder.ExpectedLength];

        HpFanGetLevelReportDecodeResult result = HpFanGetLevelReportDecoder.TryDecode(
            "FanGetLevel",
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
        byte[] sample = [0x21, 0x2A];

        HpFanGetLevelReportDecodeResult result = HpFanGetLevelReportDecoder.TryDecode(
            "FanGetLevel",
            invocationSucceeded: true,
            invocationInvoked: true,
            sample);

        Assert.False(result.Succeeded);
        Assert.Null(result.Decoded);
        Assert.Contains(result.Errors, error => error.Contains("expected 128 bytes", StringComparison.Ordinal));
    }

    [Fact]
    public void TryDecode_WithMissingBytes_SkipsDecode()
    {
        HpFanGetLevelReportDecodeResult result = HpFanGetLevelReportDecoder.TryDecode(
            "FanGetLevel",
            invocationSucceeded: true,
            invocationInvoked: true,
            returnedBytes: null);

        Assert.False(result.Succeeded);
        Assert.Null(result.Decoded);
        Assert.Contains(result.Errors, error => error.Contains("no byte array", StringComparison.Ordinal));
    }
}
