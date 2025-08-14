using FJ.Leb128.Encoding;
using FJ.Leb128.Decoding;
using FluentAssertions;

namespace FJ.Leb128.Tests;

public class SignedLeb128RoundTripTests
{
    public static IEnumerable<object[]> TestCases()
    {
        yield return new object[] { (Int128)0 };
        yield return new object[] { (Int128)1 };
        yield return new object[] { (Int128)(-1) };
        yield return new object[] { (Int128)long.MaxValue };
        yield return new object[] { (Int128)long.MinValue };
        yield return new object[] { (Int128)long.MaxValue + 1 };
        yield return new object[] { (Int128)long.MinValue - 1 };
        yield return new object[] { Int128.MaxValue };
        yield return new object[] { Int128.MinValue };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void RoundTripShouldReturnOriginalValue(Int128 input)
    {
        // Act
        var encoded = SignedLeb128Encoding.Encode(input, out _);
        var decoded = SignedLeb128Decoding.Decode(encoded);

        // Assert
        decoded.AsInt128.Should().Be(input, because: "the decoded value should match the original input");
    }

    [Fact]
    public void RoundTripRandomValuesShouldReturnOriginalValue()
    {
        // Arrange
        var random = new Random(42); // Use a seed for reproducibility

        // Act & Assert
        for (var i = 0; i < 1000; i++)
        {
            // Generate a random Int128 value
            var int128Bytes = new byte[16];
            random.NextBytes(int128Bytes);
            var input = new Int128(BitConverter.ToUInt64(int128Bytes, 8), BitConverter.ToUInt64(int128Bytes, 0));

            // Perform round-trip test
            var encoded = SignedLeb128Encoding.Encode(input, out _);
            var decoded = SignedLeb128Decoding.Decode(encoded);

            decoded.AsInt128.Should().Be(input, because: $"the decoded value should match the original input (iteration {i})");
        }
    }
}
