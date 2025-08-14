using System.Globalization;
using FJ.Leb128.Encoding;
using FJ.Leb128.Decoding;
using FluentAssertions;

namespace FJ.Leb128.Tests;

public class UnsignedLeb128RoundTripTests {
    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(127UL)]
    [InlineData(128UL)]
    [InlineData(255UL)]
    [InlineData(16383UL)]
    [InlineData(16384UL)]
    [InlineData(ulong.MaxValue)]
    public void RoundTripShouldReturnOriginalValueForUInt64Values(ulong value) {
        // Arrange
        var input = (UInt128)value;

        // Act
        var encoded = UnsignedLeb128Encoding.Encode(input, out var byteCount);
        var decoded = UnsignedLeb128Decoding.Decode(encoded);

        // Assert
        decoded.AsInt128.Should().Be(input);
        encoded.Length.Should().Be(byteCount);
    }

    [Fact]
    public void RoundTripShouldReturnOriginalValueForUInt128MaxValue() {
        // Arrange
        var input = UInt128.MaxValue;

        // Act
        var encoded = UnsignedLeb128Encoding.Encode(input, out var byteCount);
        var decoded = UnsignedLeb128Decoding.Decode(encoded);

        // Assert
        decoded.AsInt128.Should().Be(input);
        encoded.Length.Should().Be(byteCount);
    }

    [Fact]
    public void RoundTripShouldReturnOriginalValueForRandomUInt128Values() {
        // Arrange
        var random = new Random();
        for (var i = 0; i < 1000; i++) {
            var input = GenerateRandomUInt128(random);

            // Act
            var encoded = UnsignedLeb128Encoding.Encode(input, out var byteCount);
            var decoded = UnsignedLeb128Decoding.Decode(encoded);

            // Assert
            decoded.AsInt128.Should().Be(input);
            encoded.Length.Should().Be(byteCount);
        }
    }

    [Fact]
    public void RoundTripShouldReturnOriginalValueForUInt128MinValue() {
        // Arrange
        var input = UInt128.MinValue;

        // Act
        var encoded = UnsignedLeb128Encoding.Encode(input, out var byteCount);
        var decoded = UnsignedLeb128Decoding.Decode(encoded);

        // Assert
        decoded.AsInt128.Should().Be(input);
        encoded.Length.Should().Be(byteCount);
    }

    [Fact]
    public void RoundTripShouldReturnOriginalValueForUInt128One() {
        // Arrange
        var input = UInt128.One;

        // Act
        var encoded = UnsignedLeb128Encoding.Encode(input, out var byteCount);
        var decoded = UnsignedLeb128Decoding.Decode(encoded);

        // Assert
        decoded.AsInt128.Should().Be(input);
        encoded.Length.Should().Be(byteCount);
    }

    [Fact]
    public void RoundTripShouldReturnOriginalValueForLargeUInt128Value() {
        // Arrange
        var input = UInt128.Parse("340282366920938463463374607431768211455", CultureInfo.InvariantCulture);

        // Act
        var encoded = UnsignedLeb128Encoding.Encode(input, out var byteCount);
        var decoded = UnsignedLeb128Decoding.Decode(encoded);

        // Assert
        decoded.AsInt128.Should().Be(input);
        encoded.Length.Should().Be(byteCount);
    }

    [Theory]
    [MemberData(nameof(GetPowersOfTwo))]
    public void RoundTripShouldReturnOriginalValueForPowersOfTwo(UInt128 input) {
        // Act
        var encoded = UnsignedLeb128Encoding.Encode(input, out var byteCount);
        var decoded = UnsignedLeb128Decoding.Decode(encoded);

        // Assert
        decoded.AsInt128.Should().Be(input);
        encoded.Length.Should().Be(byteCount);
    }

    public static IEnumerable<object[]> GetPowersOfTwo() {
        for (var i = 0; i < 128; i++) {
            yield return new object[] { UInt128.One << i };
        }
    }

    private static UInt128 GenerateRandomUInt128(Random random) {
        var bytes = new byte[16];
        random.NextBytes(bytes);
        return new UInt128(BitConverter.ToUInt64(bytes, 8), BitConverter.ToUInt64(bytes, 0));
    }

    [Theory]
    [InlineData(255UL, Size.Bits8)]
    [InlineData(256UL, Size.Bits32)]
    [InlineData(4294967295UL, Size.Bits32)]
    [InlineData(4294967296UL, Size.Bits64)]
    [InlineData(18446744073709551615UL, Size.Bits64)]
    public void RoundTripShouldReturnOriginalValueForSizeBoundaries(ulong value, Size expectedSize) {
        // Arrange
        var input = (UInt128)value;

        // Act
        var encoded = UnsignedLeb128Encoding.Encode(input, out var byteCount);
        var decoded = UnsignedLeb128Decoding.Decode(encoded);

        // Assert
        decoded.AsInt128.Should().Be(input);
        encoded.Length.Should().Be(byteCount);
        decoded.MinSize.Should().Be(expectedSize);
    }

    [Theory]
    [InlineData(127UL, 1)]
    [InlineData(128UL, 2)]
    [InlineData(16383UL, 2)]
    [InlineData(16384UL, 3)]
    [InlineData(2097151UL, 3)]
    [InlineData(2097152UL, 4)]
    public void RoundTripShouldUseCorrectNumberOfBytes(ulong value, int expectedByteCount) {
        // Arrange
        var input = (UInt128)value;

        // Act
        var encoded = UnsignedLeb128Encoding.Encode(input, out var byteCount);
        var decoded = UnsignedLeb128Decoding.Decode(encoded);

        // Assert
        decoded.AsInt128.Should().Be(input);
        encoded.Length.Should().Be(byteCount);
        byteCount.Should().Be(expectedByteCount);
    }

    [Theory]
    [MemberData(nameof(GetValuesAroundPowerOfTwo))]
    public void RoundTripShouldHandleValuesAroundPowerOfTwo(UInt128 input) {
        // Act
        var encoded = UnsignedLeb128Encoding.Encode(input, out var byteCount);
        var decoded = UnsignedLeb128Decoding.Decode(encoded);

        // Assert
        decoded.AsInt128.Should().Be(input);
        encoded.Length.Should().Be(byteCount);
    }

    public static IEnumerable<object[]> GetValuesAroundPowerOfTwo() {
        for (var i = 0; i < 128; i++) {
            var powerOfTwo = UInt128.One << i;
            yield return new object[] { powerOfTwo - 1 };
            yield return new object[] { powerOfTwo };
            yield return new object[] { powerOfTwo + 1 };
        }
    }

    [Fact]
    public void RoundTripShouldHandleSequenceOfValues()
    {
        // Arrange
        var inputs = new UInt128[]
        {
            0,
            1,
            127,
            128,
            255,
            16383,
            16384,
            UInt128.MaxValue,
            UInt128.Parse("340282366920938463463374607431768211455", CultureInfo.InvariantCulture)
        };

        // Act
        using var memoryStream = new MemoryStream();
        var totalByteCount = 0;

        // Encode all values
        foreach (var input in inputs)
        {
            totalByteCount += UnsignedLeb128Encoding.Encode(input, memoryStream);
        }

        // Reset stream position for reading
        memoryStream.Position = 0;

        var decodedValues = new List<UInt128>();

        // Decode all values
        while (memoryStream.Position < memoryStream.Length)
        {
            var decoded = UnsignedLeb128Decoding.Decode(memoryStream);
            decodedValues.Add(decoded.AsInt128);
        }

        // Assert
        decodedValues.Should().BeEquivalentTo(inputs, options => options.WithStrictOrdering());
        memoryStream.Length.Should().Be(totalByteCount);
    }
}
