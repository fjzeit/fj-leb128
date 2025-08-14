using FJ.Leb128.Decoding;

namespace FJ.Leb128.Tests.Decoding;

public class Leb128UnsignedDecodingTests {
    [Fact]
    public void DecodeSingleByteValueReturnsCorrectResult() {
        var input = new byte[] { 0x35 };
        var result = UnsignedLeb128Decoding.Decode(input);

        Assert.Equal((UInt128)53, result.AsInt128);
        Assert.True(result.CanBeByte);
        Assert.True(result.CanBeInt32);
        Assert.True(result.CanBeInt64);
        Assert.Equal(Size.Bits8, result.MinSize);
        Assert.Equal(53, result.AsByte);
        Assert.Equal(53u, result.AsInt32);
        Assert.Equal(53ul, result.AsInt64);
    }

    [Fact]
    public void DecodeMultiByteValueReturnsCorrectResult() {
        var input = new byte[] { 0xE5, 0x8E, 0x26 };
        var result = UnsignedLeb128Decoding.Decode(input);

        Assert.Equal((UInt128)624485, result.AsInt128);
        Assert.False(result.CanBeByte);
        Assert.True(result.CanBeInt32);
        Assert.True(result.CanBeInt64);
        Assert.Equal(Size.Bits32, result.MinSize);
        Assert.Equal(624485u, result.AsInt32);
        Assert.Equal(624485ul, result.AsInt64);
        Assert.Throws<InvalidOperationException>(() => result.AsByte);
    }

    [Fact]
    public void DecodeMaxUInt128ValueReturnsCorrectResult() {
        var input = new byte[] {
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F
        };
        var result = UnsignedLeb128Decoding.Decode(input);

        Assert.Equal(UInt128.MaxValue, result.AsInt128);
        Assert.False(result.CanBeByte);
        Assert.False(result.CanBeInt32);
        Assert.False(result.CanBeInt64);
        Assert.Equal(Size.Bits128, result.MinSize);
        Assert.Throws<InvalidOperationException>(() => result.AsByte);
        Assert.Throws<InvalidOperationException>(() => result.AsInt32);
        Assert.Throws<InvalidOperationException>(() => result.AsInt64);
    }

    [Fact]
    public void DecodeFromByteArrayReturnsCorrectResult() {
        var input = new byte[] { 0xE5, 0x8E, 0x26 };
        var result = UnsignedLeb128Decoding.Decode(input);

        Assert.Equal((UInt128)624485, result.AsInt128);
    }

    [Fact]
    public void DecodeFromStreamReturnsCorrectResult() {
        var input = new byte[] { 0xE5, 0x8E, 0x26 };
        using var stream = new MemoryStream(input);
        var result = UnsignedLeb128Decoding.Decode(stream);

        Assert.Equal((UInt128)624485, result.AsInt128);
    }

    [Fact]
    public void DecodeEmptyByteArrayThrowsException() {
        var input = Array.Empty<byte>();
        Assert.Throws<EndOfStreamException>(() => UnsignedLeb128Decoding.Decode(input));
    }

    [Fact]
    public void DecodeValueWithLeadingZerosReturnsCorrectResult() {
        var input = new byte[] { 0x80, 0x80, 0x80, 0x00 };
        var result = UnsignedLeb128Decoding.Decode(input);

        Assert.Equal((UInt128)0, result.AsInt128);
        Assert.True(result.CanBeByte);
        Assert.True(result.CanBeInt32);
        Assert.True(result.CanBeInt64);
        Assert.Equal(Size.Bits8, result.MinSize);
        Assert.Equal(0, result.AsByte);
        Assert.Equal(0u, result.AsInt32);
        Assert.Equal(0ul, result.AsInt64);
    }

    [Fact]
    public void DecodeInt32MaxValueReturnsCorrectResult() {
        var input = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x0F };
        var result = UnsignedLeb128Decoding.Decode(input);

        Assert.Equal(uint.MaxValue, result.AsInt128);
        Assert.False(result.CanBeByte);
        Assert.True(result.CanBeInt32);
        Assert.True(result.CanBeInt64);
        Assert.Equal(Size.Bits32, result.MinSize);
        Assert.Equal(uint.MaxValue, result.AsInt32);
        Assert.Equal(uint.MaxValue, result.AsInt64);
        Assert.Throws<InvalidOperationException>(() => result.AsByte);
    }

    [Fact]
    public void DecodeInt64MaxValueReturnsCorrectResult() {
        var input = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 };
        var result = UnsignedLeb128Decoding.Decode(input);

        Assert.Equal(ulong.MaxValue, result.AsInt128);
        Assert.False(result.CanBeByte);
        Assert.False(result.CanBeInt32);
        Assert.True(result.CanBeInt64);
        Assert.Equal(Size.Bits64, result.MinSize);
        Assert.Equal(ulong.MaxValue, result.AsInt64);
        Assert.Throws<InvalidOperationException>(() => result.AsByte);
        Assert.Throws<InvalidOperationException>(() => result.AsInt32);
    }
}
