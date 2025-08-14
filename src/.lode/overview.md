# FJ.Leb128 Project Context

## Quick Context Loading
- **Project Type**: .NET 8 library implementing LEB128 encoding/decoding
- **Core Purpose**: Variable-length integer encoding for signed/unsigned up to 128-bit
- **Key Innovation**: Modern span-based APIs for zero-allocation performance
- **Test Coverage**: 838 tests with comprehensive edge case validation

## Critical Implementation Facts
- **Namespace Structure**: `FJ.Leb128.Encoding` and `FJ.Leb128.Decoding`
- **Key Types**: `SignedInfo`, `UnsignedInfo`, `Size` enum (Bits8/32/64/128)
- **Encoding Range**: 1-19 bytes for 128-bit integers
- **Platform**: .NET 8, uses `Int128`/`UInt128` primitives

## API Patterns Used
```csharp
// Allocation-based (legacy)
byte[] Encode(UInt128 value, out int byteCount)
UnsignedInfo Decode(byte[] data)

// Span-based (modern, zero-allocation) 
bool TryEncode(UInt128 value, Span<byte> destination, out int bytesWritten)
bool TryDecode(ReadOnlySpan<byte> source, out UnsignedInfo result, out int bytesConsumed)
ReadOnlySpan<byte> Encode(UInt128 value, Span<byte> buffer)
int GetEncodedByteCount(UInt128 value)
```

## Code Conventions
- Static classes only (no instance state)
- Try-patterns for safe operations  
- Comprehensive XML documentation
- Modern C#: nullable references, primary constructors, implicit usings
- FluentAssertions for tests, xUnit framework

## Critical Implementation Details
- **Unsigned encoding**: 7-bit chunks, continuation bit in MSB
- **Signed encoding**: Two's complement with sign extension
- **Overflow protection**: Max 19 bytes (ceil(128/7)) with bounds checking
- **Error handling**: `EndOfStreamException`, `OverflowException`, `ArgumentException`

## Test Data Patterns
- Edge values: 0, 1, -1, max/min values for each size
- Boundary values: 127/128, 16383/16384 (7-bit boundaries)  
- Powers of two: 2^0 to 2^127
- Consecutive ranges: -1000 to +1000, 0 to 1000
- Random validation: 1000+ iterations with deterministic seed

## Performance Characteristics
- Span-based APIs enable stackalloc patterns
- Zero-allocation encoding/decoding paths
- Optimized bit manipulation (shift operations)
- Stream-based processing for large data

## Files to Examine for Context
- Core: `*Encoding.cs`, `*Decoding.cs`, `*Info.cs`, `Size.cs`
- Tests: `SpanBasedApiTests.cs` (modern patterns), `*RoundTripTests.cs` (validation)
- Build: `*.csproj` files show dependencies and configuration