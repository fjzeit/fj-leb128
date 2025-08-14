# Testing Strategy & Patterns

## Test Architecture Overview
- **Total Tests**: 838 tests across multiple categories
- **Framework**: xUnit with FluentAssertions
- **Coverage**: Round-trip, encoding validation, span APIs, edge cases
- **Approach**: Property-based + example-based testing

## Test Data Sources & Reuse Strategy
```csharp
// Centralized test data in static methods:
public static IEnumerable<object[]> SignedTestCases() { ... }
public static IEnumerable<object[]> UnsignedEncodingTestData() { ... }
public static IEnumerable<object[]> GetPowersOfTwo() { ... }

// Reused across multiple test classes:
// - UnsignedLeb128RoundTripTests
// - SignedLeb128RoundTripTests  
// - SpanBasedApiTests (171 additional tests from reused data)
```

## Critical Test Categories

### 1. Round-Trip Validation
```csharp
// Pattern: encode(decode(x)) == x
[Theory]
[MemberData(nameof(TestCases))]
public void RoundTripShouldReturnOriginalValue(Int128 input) {
    var encoded = SignedLeb128Encoding.Encode(input, out _);
    var decoded = SignedLeb128Decoding.Decode(encoded);
    decoded.AsInt128.Should().Be(input);
}
```

### 2. Byte-Level Encoding Verification
```csharp
// Ensures exact byte sequences match expected values
[Theory]
[InlineData(327UL, new byte[] { 0xC7, 0x02 })]
[InlineData(18193UL, new byte[] { 0x91, 0x8E, 0x01 })]
public void EncodeSignedShouldProduceCorrectBytes(UInt128 input, byte[] expected) {
    var result = UnsignedLeb128Encoding.Encode(input, out _);
    result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
}
```

### 3. Boundary Value Testing
```csharp
// Test values at 7-bit boundaries (critical for LEB128)
[Theory]
[InlineData(127UL, 1)]    // Max 1-byte value
[InlineData(128UL, 2)]    // Min 2-byte value
[InlineData(16383UL, 2)]  // Max 2-byte value
[InlineData(16384UL, 3)]  // Min 3-byte value
```

### 4. Property-Based Testing
```csharp
// Random value validation with reproducible seeds
var random = new Random(42);
for (var i = 0; i < 1000; i++) {
    var input = GenerateRandomUInt128(random);
    // Round-trip test
    var encoded = UnsignedLeb128Encoding.Encode(input, out _);
    var decoded = UnsignedLeb128Decoding.Decode(encoded);
    decoded.AsInt128.Should().Be(input);
}
```

### 5. Error Condition Testing
```csharp
// Invalid data patterns
ReadOnlySpan<byte> invalidData = stackalloc byte[] { 
    0x80, 0x80, 0x80, /* ... */ 0x80  // All continuation bits
};
var success = UnsignedLeb128Decoding.TryDecode(invalidData, out _, out _);
success.Should().BeFalse();

// Buffer overflow scenarios  
var value = UInt128.MaxValue;
Span<byte> tooSmallBuffer = stackalloc byte[1];
var success = UnsignedLeb128Encoding.TryEncode(value, tooSmallBuffer, out _);
success.Should().BeFalse();
```

## Span-Based API Testing Patterns
```csharp
// Zero-allocation verification
Span<byte> buffer = stackalloc byte[20];
var encoded = UnsignedLeb128Encoding.Encode(value, buffer);
// Test that returned span is slice of input buffer
// Test that no allocations occurred (manual verification)

// Stackalloc compatibility validation  
public void SpanApis_ShouldWorkWithStackallocPatterns() {
    Span<byte> buffer = stackalloc byte[20];  // Must compile and work
    // Comprehensive round-trip testing with stackalloc
}
```

## Test Data Design Patterns

### Edge Values
- **Zero**: Most common value, often special-cased
- **±1**: Simplest non-zero values
- **Type boundaries**: byte.MaxValue, int.MaxValue, long.MaxValue, etc.
- **Sign boundaries**: -64/-65 (signed LEB128 boundary)
- **Max values**: UInt128.MaxValue, Int128.MaxValue/MinValue

### Systematic Coverage
- **Powers of 2**: 2^0 to 2^127 (128 test cases)
- **Around powers of 2**: value-1, value, value+1 patterns
- **Consecutive sequences**: -1000 to +1000, 0 to 1000
- **Size boundaries**: Values that transition between Size enum values

### Performance Stress Testing
```csharp
// Large-scale validation (consecutive numbers)
for (uint i = 0; i <= 1000; i++) {
    var encoded = UnsignedLeb128Encoding.Encode(i, buffer);
    encoded.Length.Should().BeGreaterThan(0);
    encoded[^1].Should().BeLessThan(128);  // No continuation bit on last byte
}
```

## Assertion Patterns
```csharp
// FluentAssertions patterns used throughout:
result.AsInt128.Should().Be(expectedValue);
encoded.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
decoded.MinSize.Should().Be(Size.Bits32);
success.Should().BeTrue();
bytesConsumed.Should().Be(encoded.Length);

// Span-specific patterns (avoid FluentAssertions issues):
encoded.Length.Should().BeGreaterThan(0);  // Instead of encoded.Should().NotBeEmpty()
```

## Test Organization Strategy
```csharp
// File naming: {Component}{TestType}Tests.cs
// - SpanBasedApiTests.cs: Modern span-based APIs
// - UnsignedLeb128RoundTripTests.cs: Unsigned round-trip validation
// - SignedLeb128RoundTripTests.cs: Signed round-trip validation  
// - Leb128UnsignedEncodingTests.cs: Detailed encoding verification

// Method naming: {Operation}_{Condition}_{ExpectedOutcome}
// public void UnsignedTryEncode_WithSufficientBuffer_ShouldReturnTrueAndEncodeCorrectly()
```

## Code Analysis Suppressions
```csharp
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
// Applied to test classes due to descriptive test naming convention
```

## Future Testing Enhancements
1. **Performance benchmarks**: BenchmarkDotNet integration
2. **Mutation testing**: Verify test quality with mutants
3. **Property-based framework**: FsCheck or similar for more systematic testing
4. **Memory usage validation**: Allocation tracking for span APIs
5. **Concurrent access testing**: Thread safety validation
6. **Fuzzing**: Random malformed input validation