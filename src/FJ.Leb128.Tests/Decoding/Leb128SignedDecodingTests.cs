using System.Globalization;
using FJ.Leb128.Decoding;
using Int128 = System.Int128;

namespace FJ.Leb128.Tests.Decoding;

public class Leb128SignedDecodingTests {
    [Theory]
    [InlineData(new byte[] { 0x00 }, 0, Size.Bits8)]
    [InlineData(new byte[] { 0x01 }, 1, Size.Bits8)]
    [InlineData(new byte[] { 0x7F }, -1, Size.Bits8)]
    [InlineData(new byte[] { 0x03 }, 3, Size.Bits8)]
    [InlineData(new byte[] { 0x83, 0x00 }, 3, Size.Bits8)]
    [InlineData(new byte[] { 0x7e }, -2, Size.Bits8)]
    [InlineData(new byte[] {0xFE, 0x7F }, -2, Size.Bits8)]
    [InlineData(new byte[] {0xFE, 0xFF, 0x7F }, -2, Size.Bits8)]
    [InlineData(new byte[] { 0x80, 0x01 }, 128, Size.Bits32)]
    [InlineData(new byte[] { 0xFF, 0x7E }, -129, Size.Bits32)]
    [InlineData(new byte[] { 0x80, 0x80, 0x80, 0x80, 0x08 }, (long)int.MaxValue + 1, Size.Bits64)]
    [InlineData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x07 }, int.MaxValue, Size.Bits32)]
    [InlineData(new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x7F }, long.MinValue, Size.Bits64)]
    [InlineData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 }, long.MaxValue, Size.Bits64)]
    public void DecodeShouldReturnCorrectValue(byte[] input, long expectedValue, Size minSize) {
        // Act
        var result = SignedLeb128Decoding.Decode(input);

        // Assert
        Assert.Equal(expectedValue, result.AsInt128);
        Assert.Equal(minSize, result.MinSize);
    }

    [Fact]
    public void DecodeShouldHandleLargePositiveInt128() {
        // Arrange
        var input = new byte[] {
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x3F
        };
        var expectedValue = Int128.Parse("42535295865117307932921825928971026431", CultureInfo.InvariantCulture);

        // Act
        var result = SignedLeb128Decoding.Decode(input);

        // Assert
        Assert.Equal(expectedValue, result.AsInt128);
        Assert.Equal(Size.Bits128, result.MinSize);
    }

    [Fact]
    public void DecodeShouldHandleLargeNegativeInt128() {
        // Arrange
        var input = new byte[] {
            0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
            0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x7F
        };
        var expectedValue = Int128.Parse("-85070591730234615865843651857942052864", CultureInfo.InvariantCulture);

        // Act
        var result = SignedLeb128Decoding.Decode(input);

        // Assert
        Assert.Equal(expectedValue, result.AsInt128);
        Assert.Equal(Size.Bits128, result.MinSize);
    }

    [Fact]
    public void DecodeShouldThrowExceptionForIncompleteInput() {
        // Arrange
        var input = new byte[] { 0x80, 0x80 }; // Incomplete LEB128 encoding

        // Act & Assert
        Assert.Throws<EndOfStreamException>(() => SignedLeb128Decoding.Decode(input));
    }
}
