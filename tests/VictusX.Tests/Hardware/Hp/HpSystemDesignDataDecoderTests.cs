using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpSystemDesignDataDecoderTests
{
    [Fact]
    public void Decode_With128ByteInput_Succeeds()
    {
        byte[] sample = new byte[HpSystemDesignDataDecoder.ExpectedLength];
        sample[0] = 0x18;
        sample[1] = 0x01;
        sample[3] = 0x02;
        sample[4] = 0x1F;
        sample[5] = 0x41;
        sample[6] = 0x01;
        sample[7] = 0x03;
        sample[8] = 0x37;
        sample[9] = 0xA5;
        sample[10] = 0x0E;
        sample[11] = 0x03;

        HpSystemDesignDataSnapshot decoded = HpSystemDesignDataDecoder.Decode(sample);

        Assert.True(decoded.IsValid);
        Assert.Equal(128, decoded.Length);
        Assert.Equal(128, decoded.ExpectedLength);
        Assert.Equal(string.Empty, decoded.ValidationError);
        Assert.Equal(280, decoded.ShippingAdapterPowerRatingWatts);
        Assert.True(decoded.DeclaresSoftwareFanControlSupport);
        Assert.True(decoded.DeclaresExtremeModeSupport);
        Assert.True(decoded.DeclaresExtremeModeUnlock);
        Assert.True(decoded.DeclaresDtBiosControl);
        Assert.True(decoded.DeclaresTwoBytePl4Support);
        Assert.Equal(5, decoded.LoadLineSupportLevels);
        Assert.Equal(10, decoded.DefaultLoadLine);
        Assert.Equal(116, decoded.UnknownByteCount);
        Assert.Equal("12..127", decoded.UnknownByteRange);
        Assert.Equal(0, decoded.UnknownNonZeroByteCount);
    }

    [Fact]
    public void Decode_WithTooShortInput_FailsSafely()
    {
        byte[] sample = new byte[11];

        HpSystemDesignDataSnapshot decoded = HpSystemDesignDataDecoder.Decode(sample);

        Assert.False(decoded.IsValid);
        Assert.Equal(11, decoded.Length);
        Assert.Contains("at least 12 bytes", decoded.ValidationError, StringComparison.Ordinal);
        Assert.Null(decoded.ShippingAdapterPowerRatingWatts);
        Assert.Equal(string.Empty, decoded.KnownPrefixHex);
        Assert.Equal(0, decoded.UnknownByteCount);
    }

    [Fact]
    public void Decode_WithEmptyInput_FailsSafely()
    {
        HpSystemDesignDataSnapshot decoded = HpSystemDesignDataDecoder.Decode([]);

        Assert.False(decoded.IsValid);
        Assert.Equal(0, decoded.Length);
        Assert.Contains("at least 12 bytes", decoded.ValidationError, StringComparison.Ordinal);
        Assert.Null(decoded.PlatformFeatureFlags);
        Assert.Equal(string.Empty, decoded.KnownPrefixHex);
        Assert.Equal(0, decoded.UnknownNonZeroByteCount);
    }

    [Fact]
    public void Decode_WithUnknownTail_SummarizesWithoutInterpretingTail()
    {
        byte[] sample = new byte[HpSystemDesignDataDecoder.ExpectedLength];
        sample[0] = 0xE6;
        sample[1] = 0x00;
        sample[4] = 0x01;
        sample[12] = 0xAA;
        sample[64] = 0x55;
        sample[127] = 0x01;

        HpSystemDesignDataSnapshot decoded = HpSystemDesignDataDecoder.Decode(sample);

        Assert.True(decoded.IsValid);
        Assert.Equal(230, decoded.ShippingAdapterPowerRatingWatts);
        Assert.True(decoded.DeclaresSoftwareFanControlSupport);
        Assert.Equal("E6-00-00-00-01-00-00-00-00-00-00-00", decoded.KnownPrefixHex);
        Assert.Equal(116, decoded.UnknownByteCount);
        Assert.Equal("12..127", decoded.UnknownByteRange);
        Assert.Equal(3, decoded.UnknownNonZeroByteCount);
    }
}
