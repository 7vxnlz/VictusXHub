using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpSystemDesignDataReportDecoderTests
{
    [Fact]
    public void TryDecode_WithSuccessfulSystemDesignDataInvocation_DecodesReportSafeSnapshot()
    {
        byte[] sample = new byte[HpSystemDesignDataDecoder.ExpectedLength];
        sample[0] = 0xE6;
        sample[1] = 0x00;
        sample[4] = 0x01;
        sample[12] = 0xA5;

        HpSystemDesignDataReportDecodeResult result = HpSystemDesignDataReportDecoder.TryDecode(
            "SystemDesignData",
            invocationSucceeded: true,
            invocationInvoked: true,
            sample);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Decoded);
        Assert.Equal(230, result.Decoded.ShippingAdapterPowerRatingWatts);
        Assert.True(result.Decoded.DeclaresSoftwareFanControlSupport);
        Assert.Equal(116, result.Decoded.UnknownByteCount);
        Assert.Equal("12..127", result.Decoded.UnknownByteRange);
        Assert.Equal(1, result.Decoded.UnknownNonZeroByteCount);
    }

    [Fact]
    public void TryDecode_WithNonSystemDesignDataCommand_SkipsDecode()
    {
        byte[] sample = new byte[HpSystemDesignDataDecoder.ExpectedLength];

        HpSystemDesignDataReportDecodeResult result = HpSystemDesignDataReportDecoder.TryDecode(
            "OtherCommand",
            invocationSucceeded: true,
            invocationInvoked: true,
            sample);

        Assert.False(result.Succeeded);
        Assert.Null(result.Decoded);
        Assert.Contains(result.Errors, error => error.Contains("not SystemDesignData", StringComparison.Ordinal));
    }

    [Fact]
    public void TryDecode_WithUnsuccessfulInvocation_SkipsDecode()
    {
        byte[] sample = new byte[HpSystemDesignDataDecoder.ExpectedLength];

        HpSystemDesignDataReportDecodeResult result = HpSystemDesignDataReportDecoder.TryDecode(
            "SystemDesignData",
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
        byte[] sample = new byte[11];

        HpSystemDesignDataReportDecodeResult result = HpSystemDesignDataReportDecoder.TryDecode(
            "SystemDesignData",
            invocationSucceeded: true,
            invocationInvoked: true,
            sample);

        Assert.False(result.Succeeded);
        Assert.Null(result.Decoded);
        Assert.Contains(result.Errors, error => error.Contains("expected 128 bytes", StringComparison.Ordinal));
    }
}
