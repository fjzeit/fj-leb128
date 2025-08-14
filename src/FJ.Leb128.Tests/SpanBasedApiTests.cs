using System.Diagnostics.CodeAnalysis;
using FJ.Leb128.Encoding;
using FJ.Leb128.Decoding;
using FluentAssertions;

namespace FJ.Leb128.Tests;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class SpanBasedApiTests {
    [Fact]
    public void UnsignedTryEncode_WithSufficientBuffer_ShouldReturnTrueAndEncodeCorrectly() {
        // Arrange
        var value = (UInt128)12345;
        Span<byte> buffer = stackalloc byte[20];

        // Act
        var success = UnsignedLeb128Encoding.TryEncode(value, buffer, out var bytesWritten);

        // Assert
        success.Should().BeTrue();
        bytesWritten.Should().BeGreaterThan(0);

        // Verify by decoding
        var decoded = UnsignedLeb128Decoding.Decode(buffer[..bytesWritten], out var consumed);
        decoded.AsInt128.Should().Be(value);
        consumed.Should().Be(bytesWritten);
    }

    [Fact]
    public void UnsignedTryEncode_WithInsufficientBuffer_ShouldReturnFalse() {
        // Arrange
        var value = UInt128.MaxValue; // Requires many bytes
        Span<byte> buffer = stackalloc byte[1]; // Too small

        // Act
        var success = UnsignedLeb128Encoding.TryEncode(value, buffer, out var bytesWritten);

        // Assert
        success.Should().BeFalse();
        bytesWritten.Should().Be(0);
    }

    [Fact]
    public void UnsignedEncode_WithSpan_ShouldReturnCorrectSlice() {
        // Arrange
        var value = (UInt128)255;
        Span<byte> buffer = stackalloc byte[20];

        // Act
        var encoded = UnsignedLeb128Encoding.Encode(value, buffer);

        // Assert
        encoded.Length.Should().BeGreaterThan(0);

        // Verify by decoding
        var decoded = UnsignedLeb128Decoding.Decode(encoded, out _);
        decoded.AsInt128.Should().Be(value);
    }

    [Fact]
    public void UnsignedEncode_WithInsufficientBuffer_ShouldThrowArgumentException() {
        // Arrange
        var value = UInt128.MaxValue;
        Span<byte> buffer = stackalloc byte[1];

        // Act & Assert
        try {
            UnsignedLeb128Encoding.Encode(value, buffer);
            Assert.Fail("Expected ArgumentException was not thrown");
        } catch (ArgumentException ex) {
            ex.Message.Should().Contain("Buffer too small for encoded value");
        }
    }

    [Fact]
    public void UnsignedGetEncodedByteCount_ShouldReturnCorrectCount() {
        // Arrange & Act & Assert
        UnsignedLeb128Encoding.GetEncodedByteCount(0).Should().Be(1);
        UnsignedLeb128Encoding.GetEncodedByteCount(127).Should().Be(1);
        UnsignedLeb128Encoding.GetEncodedByteCount(128).Should().Be(2);
        UnsignedLeb128Encoding.GetEncodedByteCount(16383).Should().Be(2);
        UnsignedLeb128Encoding.GetEncodedByteCount(16384).Should().Be(3);
    }

    [Fact]
    public void UnsignedTryDecode_WithValidData_ShouldReturnTrueAndDecodeCorrectly() {
        // Arrange
        var originalValue = (UInt128)54321;
        var encoded = UnsignedLeb128Encoding.Encode(originalValue, out _);
        ReadOnlySpan<byte> source = encoded;

        // Act
        var success = UnsignedLeb128Decoding.TryDecode(source, out var result, out var bytesConsumed);

        // Assert
        success.Should().BeTrue();
        result.AsInt128.Should().Be(originalValue);
        bytesConsumed.Should().Be(encoded.Length);
    }

    [Fact]
    public void UnsignedTryDecode_WithInvalidData_ShouldReturnFalse() {
        // Arrange - create invalid LEB128 data (all continuation bits set)
        // ReSharper disable once UseCollectionExpression
        ReadOnlySpan<byte> invalidData = stackalloc byte[] {
            0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
            0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80
        };

        // Act
        var success = UnsignedLeb128Decoding.TryDecode(invalidData, out _, out var bytesConsumed);

        // Assert
        success.Should().BeFalse();
        bytesConsumed.Should().Be(0);
    }

    [Fact]
    public void SignedTryEncode_WithSufficientBuffer_ShouldReturnTrueAndEncodeCorrectly() {
        // Arrange
        var value = (Int128)(-12345);
        Span<byte> buffer = stackalloc byte[20];

        // Act
        var success = SignedLeb128Encoding.TryEncode(value, buffer, out var bytesWritten);

        // Assert
        success.Should().BeTrue();
        bytesWritten.Should().BeGreaterThan(0);

        // Verify by decoding
        var decoded = SignedLeb128Decoding.Decode(buffer[..bytesWritten], out var consumed);
        decoded.AsInt128.Should().Be(value);
        consumed.Should().Be(bytesWritten);
    }

    [Fact]
    public void SignedTryEncode_WithInsufficientBuffer_ShouldReturnFalse() {
        // Arrange
        var value = Int128.MaxValue; // Requires many bytes
        Span<byte> buffer = stackalloc byte[1]; // Too small

        // Act
        var success = SignedLeb128Encoding.TryEncode(value, buffer, out var bytesWritten);

        // Assert
        success.Should().BeFalse();
        bytesWritten.Should().Be(0);
    }

    [Fact]
    public void SignedEncode_WithSpan_ShouldReturnCorrectSlice() {
        // Arrange
        var value = (Int128)(-255);
        Span<byte> buffer = stackalloc byte[20];

        // Act
        var encoded = SignedLeb128Encoding.Encode(value, buffer);

        // Assert
        encoded.Length.Should().BeGreaterThan(0);

        // Verify by decoding
        var decoded = SignedLeb128Decoding.Decode(encoded, out _);
        decoded.AsInt128.Should().Be(value);
    }

    [Fact]
    public void SignedGetEncodedByteCount_ShouldReturnCorrectCount() {
        // Arrange & Act & Assert
        SignedLeb128Encoding.GetEncodedByteCount(0).Should().Be(1);
        SignedLeb128Encoding.GetEncodedByteCount(63).Should().Be(1);
        SignedLeb128Encoding.GetEncodedByteCount(64).Should().Be(2);
        SignedLeb128Encoding.GetEncodedByteCount(-1).Should().Be(1);
        SignedLeb128Encoding.GetEncodedByteCount(-64).Should().Be(1);
        SignedLeb128Encoding.GetEncodedByteCount(-65).Should().Be(2);
    }

    [Fact]
    public void SignedTryDecode_WithValidData_ShouldReturnTrueAndDecodeCorrectly() {
        // Arrange
        var originalValue = (Int128)(-54321);
        var encoded = SignedLeb128Encoding.Encode(originalValue, out _);
        ReadOnlySpan<byte> source = encoded;

        // Act
        var success = SignedLeb128Decoding.TryDecode(source, out var result, out var bytesConsumed);

        // Assert
        success.Should().BeTrue();
        result.AsInt128.Should().Be(originalValue);
        bytesConsumed.Should().Be(encoded.Length);
    }

    // Reuse comprehensive test data from UnsignedLeb128RoundTripTests
    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(127UL)]
    [InlineData(128UL)]
    [InlineData(255UL)]
    [InlineData(16383UL)]
    [InlineData(16384UL)]
    [InlineData(ulong.MaxValue)]
    public void UnsignedSpanBasedRoundTrip_ShouldMatchOriginalValue(ulong testValue) {
        // Arrange
        var value = (UInt128)testValue;
        Span<byte> buffer = stackalloc byte[20];

        // Act
        var encoded = UnsignedLeb128Encoding.Encode(value, buffer);
        var decoded = UnsignedLeb128Decoding.Decode(encoded, out var bytesConsumed);

        // Assert
        decoded.AsInt128.Should().Be(value);
        bytesConsumed.Should().Be(encoded.Length);
    }

    // Reuse test data from SignedLeb128RoundTripTests
    [Theory]
    [MemberData(nameof(SignedTestCases))]
    public void SignedSpanBasedRoundTrip_ShouldMatchOriginalValue(Int128 testValue) {
        // Arrange
        Span<byte> buffer = stackalloc byte[20];

        // Act
        var encoded = SignedLeb128Encoding.Encode(testValue, buffer);
        var decoded = SignedLeb128Decoding.Decode(encoded, out var bytesConsumed);

        // Assert
        decoded.AsInt128.Should().Be(testValue);
        bytesConsumed.Should().Be(encoded.Length);
    }

    public static IEnumerable<object[]> SignedTestCases() {
        yield return [(Int128)0];
        yield return [(Int128)1];
        yield return [(Int128)(-1)];
        yield return [(Int128)long.MaxValue];
        yield return [(Int128)long.MinValue];
        yield return [(Int128)long.MaxValue + 1];
        yield return [(Int128)long.MinValue - 1];
        yield return [Int128.MaxValue];
        yield return [Int128.MinValue];
    }

    // Reuse comprehensive test data from encoding tests
    [Theory]
    [MemberData(nameof(UnsignedEncodingTestData))]
    public void UnsignedSpanBased_ShouldProduceCorrectBytes(UInt128 input, byte[] expected) {
        // Arrange
        Span<byte> buffer = stackalloc byte[20];

        // Act
        var encoded = UnsignedLeb128Encoding.Encode(input, buffer);

        // Assert
        encoded.ToArray().Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());

        // Verify by decoding
        var decoded = UnsignedLeb128Decoding.Decode(encoded, out _);
        decoded.AsInt128.Should().Be(input);
    }

    public static IEnumerable<object[]> UnsignedEncodingTestData() {
        object[][] testData = {
            new object[] { (UInt128)0, new byte[] { 0x00 } },
            new object[] { (UInt128)1, new byte[] { 0x01 } },
            new object[] { (UInt128)5, new byte[] { 0x05 } },
            new object[] { (UInt128)63, new byte[] { 0x3F } },
            new object[] { (UInt128)64, new byte[] { 0x40 } },
            new object[] { (UInt128)99, new byte[] { 0x63 } },
            new object[] { (UInt128)327, new byte[] { 0xC7, 0x02 } },
            new object[] { (UInt128)8192, new byte[] { 0x80, 0x40 } },
            new object[] { (UInt128)18193, new byte[] { 0x91, 0x8E, 0x01 } },
            new object[] { (UInt128)624485, new byte[] { 0xE5, 0x8E, 0x26 } },
            new object[] { (UInt128)uint.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x0F } },
            new object[] { (UInt128)uint.MinValue, new byte[] { 0x00 } },
            new object[]
                { (UInt128)ulong.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 } },
            new object[] { (UInt128)ulong.MinValue, new byte[] { 0x00 } },
            new object[] { UInt128.MinValue, new byte[] { 0x00 } },
            new object[] {
                UInt128.MaxValue,
                new byte[] {
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0x03
                }
            }
        };

        foreach (var item in testData) {
            yield return item;
        }
    }

    [Theory]
    [MemberData(nameof(SignedEncodingTestData))]
    public void SignedSpanBased_ShouldProduceCorrectBytes(Int128 input, byte[] expected) {
        // Arrange
        Span<byte> buffer = stackalloc byte[20];

        // Act
        var encoded = SignedLeb128Encoding.Encode(input, buffer);

        // Assert
        encoded.ToArray().Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());

        // Verify by decoding
        var decoded = SignedLeb128Decoding.Decode(encoded, out _);
        decoded.AsInt128.Should().Be(input);
    }

    public static IEnumerable<object[]> SignedEncodingTestData() {
        object[][] testData = [
            [(Int128)0, new byte[] { 0x00 }],
            [(Int128)1, new byte[] { 0x01 }],
            [(Int128)3, new byte[] { 0x03 }],
            [(Int128)(-1), new byte[] { 0x7F }],
            [(Int128)63, new byte[] { 0x3F }],
            [(Int128)(-64), new byte[] { 0x40 }],
            [(Int128)64, new byte[] { 0xC0, 0x00 }],
            [(Int128)(-65), new byte[] { 0xBF, 0x7F }],
            [(Int128)8192, new byte[] { 0x80, 0xC0, 0x00 }],
            [(Int128)(-8193), new byte[] { 0xFF, 0xBF, 0x7F }],
            [(Int128)624485, new byte[] { 0xE5, 0x8E, 0x26 }],
            [(Int128)int.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x07 }],
            [(Int128)int.MinValue, new byte[] { 0x80, 0x80, 0x80, 0x80, 0x78 }],
            [(Int128)long.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 }],
            [(Int128)long.MinValue, new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x7F }]
        ];

        foreach (var item in testData) {
            yield return item;
        }
    }

    // Test powers of two with span-based APIs (reusing pattern from encoding tests)
    [Theory]
    [MemberData(nameof(GetPowersOfTwo))]
    public void UnsignedSpanBased_ShouldHandlePowersOfTwo(UInt128 input) {
        // Arrange
        Span<byte> buffer = stackalloc byte[20];

        // Act
        var encoded = UnsignedLeb128Encoding.Encode(input, buffer);
        var decoded = UnsignedLeb128Decoding.Decode(encoded, out var bytesConsumed);

        // Assert
        decoded.AsInt128.Should().Be(input);
        bytesConsumed.Should().Be(encoded.Length);
        encoded[^1].Should().BeLessThan(128); // Last byte should not have continuation bit
    }

    public static IEnumerable<object[]> GetPowersOfTwo() {
        for (var i = 0; i < 128; i++) {
            yield return new object[] { UInt128.One << i };
        }
    }

    // Test consecutive numbers with span-based APIs
    [Fact]
    public void SpanBasedApis_ShouldHandleConsecutiveNumbers() {
        Span<byte> buffer = stackalloc byte[20];

        // Test unsigned consecutive numbers
        for (uint i = 0; i <= 1000; i++) {
            var encoded = UnsignedLeb128Encoding.Encode(i, buffer);
            encoded.Length.Should().BeGreaterThan(0);
            encoded[^1].Should().BeLessThan(128);

            var decoded = UnsignedLeb128Decoding.Decode(encoded, out _);
            decoded.AsInt128.Should().Be(i);
        }

        // Test signed consecutive numbers
        for (var i = -1000; i <= 1000; i++) {
            var encoded = SignedLeb128Encoding.Encode(i, buffer);
            encoded.Length.Should().BeGreaterThan(0);
            encoded[^1].Should().BeLessThan(128);

            var decoded = SignedLeb128Decoding.Decode(encoded, out _);
            decoded.AsInt128.Should().Be(i);
        }
    }

    // Test boundary values with span-based APIs
    [Theory]
    [InlineData(255UL, Size.Bits8)]
    [InlineData(256UL, Size.Bits32)]
    [InlineData(4294967295UL, Size.Bits32)]
    [InlineData(4294967296UL, Size.Bits64)]
    [InlineData(18446744073709551615UL, Size.Bits64)]
    public void UnsignedSpanBased_ShouldHandleSizeBoundaries(ulong value, Size expectedSize) {
        // Arrange
        var input = (UInt128)value;
        Span<byte> buffer = stackalloc byte[20];

        // Act
        var encoded = UnsignedLeb128Encoding.Encode(input, buffer);
        var decoded = UnsignedLeb128Decoding.Decode(encoded, out var bytesConsumed);

        // Assert
        decoded.AsInt128.Should().Be(input);
        bytesConsumed.Should().Be(encoded.Length);
        decoded.MinSize.Should().Be(expectedSize);
    }

    [Fact]
    public void SpanApis_ShouldWorkWithStackallocPatterns() {
        // Arrange
        var unsignedValue = (UInt128)999999;
        var signedValue = (Int128)(-999999);

        // Act & Assert - This should compile and work without allocations
        Span<byte> buffer = stackalloc byte[20];

        // Unsigned
        var unsignedEncoded = UnsignedLeb128Encoding.Encode(unsignedValue, buffer);
        var unsignedDecoded = UnsignedLeb128Decoding.Decode(unsignedEncoded, out _);
        unsignedDecoded.AsInt128.Should().Be(unsignedValue);

        // Signed (reuse buffer)
        var signedEncoded = SignedLeb128Encoding.Encode(signedValue, buffer);
        var signedDecoded = SignedLeb128Decoding.Decode(signedEncoded, out _);
        signedDecoded.AsInt128.Should().Be(signedValue);
    }
}
