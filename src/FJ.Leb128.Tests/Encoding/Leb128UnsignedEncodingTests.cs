using System.Globalization;
using FJ.Leb128.Encoding;
using FluentAssertions;

namespace FJ.Leb128.Tests.Encoding;

public class Leb128UnsignedEncodingTests {
    [Fact]
    public void LongAndIntConvertTheSame() {
        UInt128[] testValues = [0, 1, 64, 127, 128, 175, 255, 256, 515, 723, 1024, 4096, uint.MaxValue, uint.MinValue];

        foreach (var value in testValues) {
            var intResult = UnsignedLeb128Encoding.Encode(value, out _);
            var longResult = UnsignedLeb128Encoding.Encode(value, out _);

            intResult.Should().Equal(longResult);
        }
    }

    [Theory]
    [MemberData(nameof(ULongTestData))]
    public void EncodeSignedShouldProduceCorrectBytes(UInt128 input, byte[] expected) {
        // Act
        var result = UnsignedLeb128Encoding.Encode(input, out _);

        // Assert
        result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }

    public static IEnumerable<object[]> ULongTestData() {
        object[][] testData = {
            new object[] { 0, new byte[] { 0x00 } },
            new object[] { 1, new byte[] { 0x01 } },
            new object[] { 5, new byte[] { 0x05 } },
            new object[] { 63, new byte[] { 0x3F } },
            new object[] { 64, new byte[] { 0x40 } },
            new object[] { 99, new byte[] { 0x63 } },
            new object[] { 327, new byte[] { 0xC7, 0x02 } },
            new object[] { 8192, new byte[] { 0x80, 0x40 } },
            new object[] { 18193, new byte[] { 0x91, 0x8E, 0x01 } },
            new object[] { 624485, new byte[] { 0xE5, 0x8E, 0x26 } },
            new object[] { uint.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x0F } },
            new object[] { uint.MinValue, new byte[] { 0x00 } },
            new object[] { ulong.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 } },
            new object[] { ulong.MinValue, new byte[] { 0x00 } },
            new object[] { UInt128.MinValue, new byte[] { 0x00 } },
            new object[] {
                UInt128.MaxValue,
                new byte[] {
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0x03
                }
            },
        };

        foreach (var item in testData) {
            yield return item;
        }
    }

    [Theory]
    [InlineData(0)]
    public void EncodeSignedShouldHandleZero(ulong v) {
        // Act
        var result = UnsignedLeb128Encoding.Encode(v, out _);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().Be(0);
    }

    [Fact]
    public void EncodeShouldHandleConsecutiveNumbers()
    {
        for (uint i = 0; i <= 1000; i++)
        {
            var result = UnsignedLeb128Encoding.Encode(i, out var byteCount);
            result.Should().NotBeEmpty();
            byteCount.Should().Be(result.Length);
            result.Last().Should().BeLessThan(128);
        }
    }

    [Fact]
    public void EncodeShouldHandlePowersOfTwo()
    {
        UInt128 value = 1;
        for (var i = 0; i < 128; i++) // UInt128 can represent up to 2^128 - 1
        {
            var result = UnsignedLeb128Encoding.Encode(value, out var byteCount);
            result.Should().NotBeEmpty();
            byteCount.Should().Be(result.Length);
            result.Last().Should().BeLessThan(128);

            if (i < 127) // Avoid overflow on the last iteration
            {
                value *= 2;
            }
        }
    }

    [Theory]
    [InlineData(624485UL, new byte[] { 0xE5, 0x8E, 0x26 })]
    [InlineData(18446744073709551615UL, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 })]
    public void EncodeToStreamShouldProduceCorrectBytes(ulong testValue, byte[] expectedBytes)
    {
        using var memoryStream = new MemoryStream();

        // Act
        var byteCount = UnsignedLeb128Encoding.Encode(testValue, memoryStream);

        // Assert
        byteCount.Should().Be(expectedBytes.Length);
        memoryStream.ToArray().Should().BeEquivalentTo(expectedBytes);
    }

    [Fact]
    public void EncodeShouldHandleLargeNumbers()
    {
        var testCases = new[]
        {
            (UInt128.MaxValue, 19, (byte)0x03),
            (UInt128.Parse("170141183460469231731687303715884105727", CultureInfo.InvariantCulture), 19, (byte)0x01),
            (UInt128.Parse("340282366920938463463374607431768211455", CultureInfo.InvariantCulture), 19, (byte)0x03)
        };

        foreach (var (value, expectedLength, expectedLastByte) in testCases)
        {
            var result = UnsignedLeb128Encoding.Encode(value, out var byteCount);
            result.Should().HaveCount(expectedLength);
            byteCount.Should().Be(expectedLength);
            result.Last().Should().Be(expectedLastByte);
        }
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(1U)]
    [InlineData(uint.MaxValue)]
    [InlineData(ulong.MaxValue)]
    public void EncodeUnsignedShouldHandleEdgeCases(ulong value)
    {
        // Act
        var result = UnsignedLeb128Encoding.Encode(value, out var byteCount);

        // Assert
        result.Should().NotBeEmpty();
        byteCount.Should().Be(result.Length);
        result.Last().Should().BeLessThan(128); // The last byte should have its most significant bit unset
    }
}
