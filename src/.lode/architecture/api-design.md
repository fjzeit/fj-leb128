# API Design Patterns & Conventions

## Core Design Principles
- **Static classes**: No instance state, pure functional approach
- **Namespace separation**: `Encoding` and `Decoding` clearly separated
- **Try-patterns**: Safe operations with boolean success indicators  
- **Span-first**: Modern APIs prioritize span-based operations
- **Backward compatibility**: Traditional byte[] APIs maintained alongside span APIs

## API Evolution Pattern
```csharp
// Generation 1: Traditional allocation-based
byte[] Encode(UInt128 value, out int byteCount)
UnsignedInfo Decode(byte[] bytes)

// Generation 2: Stream support
int Encode(UInt128 value, Stream stream)  
UnsignedInfo Decode(Stream stream)

// Generation 3: Span-based (zero-allocation)
bool TryEncode(UInt128 value, Span<byte> destination, out int bytesWritten)
ReadOnlySpan<byte> Encode(UInt128 value, Span<byte> buffer)
bool TryDecode(ReadOnlySpan<byte> source, out UnsignedInfo result, out int bytesConsumed)
```

## Consistent Method Signatures
```csharp
// Encoding methods always:
// - Take value as first parameter  
// - Return byte count or success boolean
// - Use 'out' for additional results

// Decoding methods always:
// - Take source data as first parameter
// - Return result object or success boolean  
// - Use 'out' for bytes consumed

// Try-methods always:
// - Return bool success
// - Use 'out' parameters for results
// - Never throw on expected failures
```

## Error Handling Strategy
```csharp
// Exception types by scenario:
// - ArgumentException: Invalid parameters (buffer too small, null args)
// - OverflowException: Value exceeds encoding limits  
// - EndOfStreamException: Unexpected end of stream during decoding
// - InvalidOperationException: Type conversion failures in Info classes

// Try-methods convert exceptions to false returns
// Regular methods let exceptions bubble up
```

## Wrapper Type Pattern
```csharp
// Info classes provide:
// 1. Type safety wrapper around Int128/UInt128
// 2. Size detection (CanBeByte, CanBeInt32, etc.)  
// 3. Safe conversion (AsByte, AsInt32 with overflow detection)
// 4. Minimum size calculation (MinSize property)

public class UnsignedInfo {
    public bool CanBeByte { get; }     // <= 255
    public bool CanBeInt32 { get; }    // <= uint.MaxValue  
    public bool CanBeInt64 { get; }    // <= ulong.MaxValue
    public Size MinSize { get; }       // Bits8/32/64/128
    public byte AsByte => /* with validation */
    public UInt128 AsInt128 { get; }   // Always safe
}
```

## Size Enum Usage
```csharp
public enum Size {
    Bits8,    // byte/sbyte range
    Bits32,   // uint/int range  
    Bits64,   // ulong/long range
    Bits128   // UInt128/Int128 required
}
// Used for minimum storage size determination
// Useful for protocol designers choosing field sizes
```

## Stream Integration Pattern
```csharp
// Stream methods delegate to Action<byte> for flexibility:
public static int Encode(Int128 i, Stream stream) => Encode(i, stream.WriteByte);
public static int Encode(Int128 i, BinaryWriter writer) => Encode(i, writer.Write);

private static int Encode(Int128 i, Action<byte> write) {
    // Core implementation here
}
```

## Span API Safety Rules
```csharp
// Always check buffer size before writing:
var requiredBytes = GetEncodedByteCount(value);
if (destination.Length < requiredBytes) {
    bytesWritten = 0;
    return false;
}

// Always validate span length before reading:
if (bytesConsumed >= maxByteCount) {
    result = default!;
    bytesConsumed = 0; 
    return false;
}

// Use stackalloc-friendly patterns:
ReadOnlySpan<byte> Encode(UInt128 value, Span<byte> buffer) // Returns slice of input
```

## Documentation Standards
- **All public APIs**: Have XML documentation
- **Parameters**: Documented with behavior on edge cases  
- **Returns**: Documented with success/failure conditions
- **Exceptions**: All possible exceptions documented with conditions
- **Examples**: Complex methods include usage examples

## Naming Conventions
- **Classes**: `{Type}Leb128{Operation}` (e.g., `UnsignedLeb128Encoding`)
- **Methods**: Verb-first (`Encode`, `Decode`, `TryEncode`, `GetEncodedByteCount`)
- **Parameters**: Clear intent (`destination`, `source`, `bytesWritten`, `bytesConsumed`)
- **Properties**: State/capability descriptions (`CanBeByte`, `MinSize`, `AsInt128`)

## Performance Considerations in API Design  
- **Span-first**: All new APIs use spans to avoid allocations
- **Try-patterns**: Avoid exception overhead for expected failures
- **Struct returns**: Info classes could be structs but readability prioritized
- **Method inlining**: Simple methods marked for aggressive inlining potential
- **Buffer reuse**: APIs designed to work with caller-provided buffers