using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanMaxGetDecoderTests
{
    [Fact]
    public void Decode_WithFourByteInput_Succeeds()
    {
        byte[] sample = [0x01, 0x00, 0x00, 0x00];

        HpFanMaxGetSnapshot decoded = HpFanMaxGetDecoder.Decode(sample);

        Assert.True(decoded.IsValid);
        Assert.Equal(4, decoded.Length);
        Assert.Equal(4, decoded.ExpectedLength);
        Assert.Equal(string.Empty, decoded.ValidationError);
        Assert.Equal<byte?>(1, decoded.MaxFanStateRaw);
        Assert.True(decoded.IsMaxFanEnabled);
        Assert.Equal("01", decoded.KnownPrefixHex);
        Assert.Equal(3, decoded.UnknownByteCount);
        Assert.Equal("1..3", decoded.UnknownByteRange);
        Assert.Equal(0, decoded.UnknownNonZeroByteCount);
    }

    [Fact]
    public void Decode_WithZeroState_ReportsDisabled()
    {
        byte[] sample = [0x00, 0x00, 0x00, 0x00];

        HpFanMaxGetSnapshot decoded = HpFanMaxGetDecoder.Decode(sample);

        Assert.True(decoded.IsValid);
        Assert.Equal<byte?>(0, decoded.MaxFanStateRaw);
        Assert.False(decoded.IsMaxFanEnabled);
    }

    [Fact]
    public void Decode_WithEmptyInput_FailsSafely()
    {
        HpFanMaxGetSnapshot decoded = HpFanMaxGetDecoder.Decode([]);

        Assert.False(decoded.IsValid);
        Assert.Equal(0, decoded.Length);
        Assert.Contains("at least 1 byte", decoded.ValidationError, StringComparison.Ordinal);
        Assert.Null(decoded.MaxFanStateRaw);
        Assert.Null(decoded.IsMaxFanEnabled);
        Assert.Equal(0, decoded.UnknownByteCount);
    }

    [Fact]
    public void Decode_WithNullInput_FailsSafely()
    {
        HpFanMaxGetSnapshot decoded = HpFanMaxGetDecoder.Decode(null);

        Assert.False(decoded.IsValid);
        Assert.Equal(0, decoded.Length);
        Assert.Contains("null", decoded.ValidationError, StringComparison.Ordinal);
        Assert.Null(decoded.MaxFanStateRaw);
        Assert.Null(decoded.IsMaxFanEnabled);
    }

    [Fact]
    public void Decode_WithUnknownTail_SummarizesOnly()
    {
        byte[] sample = [0x00, 0xAA, 0x00, 0x55];

        HpFanMaxGetSnapshot decoded = HpFanMaxGetDecoder.Decode(sample);

        Assert.True(decoded.IsValid);
        Assert.False(decoded.IsMaxFanEnabled);
        Assert.Equal(3, decoded.UnknownByteCount);
        Assert.Equal("1..3", decoded.UnknownByteRange);
        Assert.Equal(2, decoded.UnknownNonZeroByteCount);
    }
}
