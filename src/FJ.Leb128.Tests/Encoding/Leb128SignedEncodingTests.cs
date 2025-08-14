using FJ.Leb128.Encoding;
using FluentAssertions;
using System.Globalization;

namespace FJ.Leb128.Tests.Encoding;

public class Leb128SignedEncodingTests {
    [Theory]
    [MemberData(nameof(TestData))]
    public void EncodeSignedShouldProduceCorrectBytes(Int128 input, byte[] expected) {
        // Act
        var result = SignedLeb128Encoding.Encode(input, out var byteCount);

        // Assert
        result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        byteCount.Should().Be(expected.Length);
    }

    public static IEnumerable<object[]> TestData() {
        object[][] testData = [
            [0, new byte[] { 0x00 }],
            [1, new byte[] { 0x01 }],
            [3, new byte[] { 0x03 }],
            [-1, new byte[] { 0x7F }],
            [63, new byte[] { 0x3F }],
            [-64, new byte[] { 0x40 }],
            [64, new byte[] { 0xC0, 0x00 }],
            [-65, new byte[] { 0xBF, 0x7F }],
            [8192, new byte[] { 0x80, 0xC0, 0x00 }],
            [-8193, new byte[] { 0xFF, 0xBF, 0x7F }],
            [624485, new byte[] { 0xE5, 0x8E, 0x26 }],
            [int.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x07 }],
            [int.MinValue, new byte[] { 0x80, 0x80, 0x80, 0x80, 0x78 }],
            [long.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 }],
            [long.MinValue, new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x7F }],
            [
                Int128.Parse("85070591730234615865843651857942052864", CultureInfo.InvariantCulture),
                new byte[] {
                    0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
                    0x80, 0x80, 0x01
                }
            ],
            [
                Int128.Parse("-85070591730234615865843651857942052864", CultureInfo.InvariantCulture),
                new byte[] {
                    0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
                    0x80, 0x80, 0x7F
                }
            ]
        ];
        foreach (var item in testData) {
            yield return item;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(127)]
    [InlineData(128)]
    [InlineData(-128)]
    [InlineData(255)]
    [InlineData(256)]
    [InlineData(515)]
    [InlineData(-515)]
    [InlineData(1024)]
    [InlineData(-1024)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void LongAndIntConvertTheSame(int value) {
        var intResult = SignedLeb128Encoding.Encode(value, out var intByteCount);
        var longResult = SignedLeb128Encoding.Encode((long)value, out var longByteCount);
        var int128Result = SignedLeb128Encoding.Encode(value, out var int128ByteCount);

        intResult.Should().Equal(longResult).And.Equal(int128Result);
        intByteCount.Should().Be(longByteCount).And.Be(int128ByteCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void EncodeSignedShouldHandleEdgeCases(long value) {
        // Act
        var result = SignedLeb128Encoding.Encode(value, out var byteCount);

        // Assert
        result.Should().NotBeEmpty();
        byteCount.Should().Be(result.Length);
        result.Last().Should().BeLessThan(128); // The last byte should have its most significant bit unset
    }

    [Fact]
    public void EncodeShouldHandleConsecutiveNumbers() {
        for (var i = -1000; i <= 1000; i++) {
            var result = SignedLeb128Encoding.Encode(i, out var byteCount);
            result.Should().NotBeEmpty();
            byteCount.Should().Be(result.Length);
            result.Last().Should().BeLessThan(128);
        }
    }

    [Fact]
    public void EncodeShouldHandlePowersOfTwo() {
        Int128 value = 1;
        for (var i = 0; i < 127; i++) // Int128 can represent 2^127 - 1 as its max value
        {
            var result = SignedLeb128Encoding.Encode(value, out var byteCount);
            result.Should().NotBeEmpty();
            byteCount.Should().Be(result.Length);
            result.Last().Should().BeLessThan(128);

            if (i < 126) // Avoid overflow on the last iteration
            {
                value *= 2;
            }
        }
    }

    [Theory]
    [InlineData(624485, new byte[] { 0xE5, 0x8E, 0x26 })]
    [InlineData(-624485, new byte[] { 0x9B, 0xF1, 0x59 })]
    public void EncodeToStreamShouldProduceCorrectBytes(long testValue, byte[] expectedBytes) {
        using var memoryStream = new MemoryStream();

        // Act
        var byteCount = SignedLeb128Encoding.Encode(testValue, memoryStream);

        // Assert
        byteCount.Should().Be(expectedBytes.Length);
        memoryStream.ToArray().Should().BeEquivalentTo(expectedBytes);
    }

    [Fact]
    public void EncodeShouldHandleLargeNumbers() {
        var testCases = new[] {
            (Int128.MaxValue, 19, (byte)0x01),
            (Int128.MinValue, 19, (byte)0x7E),
            (Int128.Parse("170141183460469231731687303715884105727", CultureInfo.InvariantCulture), 19, (byte)0x01),
            (Int128.Parse("-170141183460469231731687303715884105728", CultureInfo.InvariantCulture), 19, (byte)0x7E)
        };

        foreach (var (value, expectedLength, expectedLastByte) in testCases) {
            var result = SignedLeb128Encoding.Encode(value, out var byteCount);
            result.Should().HaveCount(expectedLength);
            byteCount.Should().Be(expectedLength);
            result.Last().Should().Be(expectedLastByte);
        }
    }
}
