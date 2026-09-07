using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanGetCountDecoderTests
{
    [Fact]
    public void Decode_WithFourByteInput_Succeeds()
    {
        byte[] sample = [0x02, 0x03, 0x00, 0x00];

        HpFanGetCountSnapshot decoded = HpFanGetCountDecoder.Decode(sample);

        Assert.True(decoded.IsValid);
        Assert.Equal(4, decoded.Length);
        Assert.Equal(4, decoded.ExpectedLength);
        Assert.Equal(string.Empty, decoded.ValidationError);
        Assert.Equal<byte?>(2, decoded.FanCount);
        Assert.Equal<byte?>(0x03, decoded.ProtectionStatusRaw);
        Assert.True(decoded.OverCurrentProtectionTripped);
        Assert.True(decoded.OverTemperatureProtectionTripped);
        Assert.Equal("02-03", decoded.KnownPrefixHex);
        Assert.Equal(2, decoded.UnknownByteCount);
        Assert.Equal("2..3", decoded.UnknownByteRange);
        Assert.Equal(0, decoded.UnknownNonZeroByteCount);
    }

    [Fact]
    public void Decode_WithTooShortInput_FailsSafely()
    {
        byte[] sample = [0x02];

        HpFanGetCountSnapshot decoded = HpFanGetCountDecoder.Decode(sample);

        Assert.False(decoded.IsValid);
        Assert.Equal(1, decoded.Length);
        Assert.Contains("at least 2 bytes", decoded.ValidationError, StringComparison.Ordinal);
        Assert.Null(decoded.FanCount);
        Assert.Null(decoded.ProtectionStatusRaw);
        Assert.Equal(string.Empty, decoded.KnownPrefixHex);
    }

    [Fact]
    public void Decode_WithEmptyInput_FailsSafely()
    {
        HpFanGetCountSnapshot decoded = HpFanGetCountDecoder.Decode([]);

        Assert.False(decoded.IsValid);
        Assert.Equal(0, decoded.Length);
        Assert.Contains("at least 2 bytes", decoded.ValidationError, StringComparison.Ordinal);
        Assert.Null(decoded.OverCurrentProtectionTripped);
        Assert.Equal(0, decoded.UnknownByteCount);
    }

    [Fact]
    public void Decode_WithUnknownTail_SummarizesOnly()
    {
        byte[] sample = [0x02, 0x00, 0xAA, 0x55];

        HpFanGetCountSnapshot decoded = HpFanGetCountDecoder.Decode(sample);

        Assert.True(decoded.IsValid);
        Assert.Equal<byte?>(2, decoded.FanCount);
        Assert.False(decoded.OverCurrentProtectionTripped);
        Assert.False(decoded.OverTemperatureProtectionTripped);
        Assert.Equal(2, decoded.UnknownByteCount);
        Assert.Equal("2..3", decoded.UnknownByteRange);
        Assert.Equal(2, decoded.UnknownNonZeroByteCount);
    }
}
