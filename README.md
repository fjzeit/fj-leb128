# FJ.Leb128

A .NET 8 library for encoding and decoding integers using the LEB128 (Little Endian Base 128) variable-length encoding scheme. Supports both signed and unsigned integers up to 128 bits with modern span-based APIs for zero-allocation scenarios.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Key Features

- **🚀 Performance**: Modern span-based APIs for zero-allocation encoding/decoding
- **📏 Complete Coverage**: Supports 8, 32, 64, and 128-bit integers (signed and unsigned)  
- **🛡️ Type Safety**: Rich type system with safe conversions and overflow detection
- **📱 Compact Encoding**: Variable-length format (1-19 bytes for 128-bit integers)
- **🔧 Easy Integration**: Clean API following .NET conventions
- **✅ Well Tested**: 838+ tests covering edge cases and performance scenarios

## Installation

```bash
dotnet add package FJ.Leb128
```

## Quick Start

### Zero-Allocation Encoding (Recommended)

```csharp
using FJ.Leb128.Encoding;
using FJ.Leb128.Decoding;

// High-performance encoding with stackalloc
UInt128 value = 12345678901234567890UL;
Span<byte> buffer = stackalloc byte[20]; // Max LEB128 size

// Encode directly to span (zero allocations)
var encoded = UnsignedLeb128Encoding.Encode(value, buffer);
Console.WriteLine($"Encoded {value} to {encoded.Length} bytes");

// Decode directly from span (zero allocations)  
var decoded = UnsignedLeb128Decoding.Decode(encoded, out int bytesConsumed);
Console.WriteLine($"Decoded: {decoded.AsInt128} (consumed {bytesConsumed} bytes)");
```

### Safe Encoding with Try-Patterns

```csharp
using FJ.Leb128.Encoding;

UInt128 largeValue = UInt128.MaxValue;
Span<byte> smallBuffer = stackalloc byte[5]; // Intentionally too small

// Safe encoding - never throws exceptions
if (UnsignedLeb128Encoding.TryEncode(largeValue, smallBuffer, out int bytesWritten))
{
    Console.WriteLine($"Successfully encoded {bytesWritten} bytes");
}
else
{
    // Calculate required buffer size
    int required = UnsignedLeb128Encoding.GetEncodedByteCount(largeValue);
    Console.WriteLine($"Buffer too small. Need {required} bytes, have {smallBuffer.Length}");
}
```

### Signed Integer Encoding

```csharp
using FJ.Leb128.Encoding;
using FJ.Leb128.Decoding;

// Negative numbers with two's complement encoding
Int128 negativeValue = -9876543210L;
Span<byte> buffer = stackalloc byte[20];

var encoded = SignedLeb128Encoding.Encode(negativeValue, buffer);
var decoded = SignedLeb128Decoding.Decode(encoded, out _);

Console.WriteLine($"Original: {negativeValue}");
Console.WriteLine($"Round-trip: {decoded.AsInt128}");
Console.WriteLine($"Bytes used: {encoded.Length}");
```

## Advanced Usage

### Type Size Detection & Safe Conversion

```csharp
using FJ.Leb128.Decoding;

// Decode with automatic size detection
byte[] data = { 0xFF, 0x01 }; // Encoded 255
var info = UnsignedLeb128Decoding.Decode(data, out _);

// Check what types can represent this value
Console.WriteLine($"Can be byte: {info.CanBeByte}");       // True
Console.WriteLine($"Can be int: {info.CanBeInt32}");       // True  
Console.WriteLine($"Can be long: {info.CanBeInt64}");      // True
Console.WriteLine($"Minimum size: {info.MinSize}");        // Size.Bits8

// Safe conversion (throws if value too large)
byte byteValue = info.AsByte;           // Safe: 255 fits in byte
uint uintValue = info.AsInt32;          // Safe: 255 fits in uint
UInt128 fullValue = info.AsInt128;      // Always safe
```

### Stream Processing

```csharp
using FJ.Leb128.Encoding;
using FJ.Leb128.Decoding;

// Encode multiple values to a stream
var values = new UInt128[] { 100, 200, 300 };
using var stream = new MemoryStream();

foreach (var value in values)
{
    UnsignedLeb128Encoding.Encode(value, stream);
}

// Decode from stream
stream.Position = 0;
while (stream.Position < stream.Length)
{
    var decoded = UnsignedLeb128Decoding.Decode(stream);
    Console.WriteLine($"Decoded: {decoded.AsInt128}");
}
```

### Error Handling

```csharp
using FJ.Leb128.Decoding;

// Safe parsing with try-pattern
byte[] possiblyCorruptData = { 0x80, 0x80, 0x80 }; // Invalid: all continuation bits

if (UnsignedLeb128Decoding.TryDecode(possiblyCorruptData, out var result, out int consumed))
{
    Console.WriteLine($"Successfully decoded: {result.AsInt128}");
}
else
{
    Console.WriteLine("Failed to decode - data may be corrupt");
}

// Traditional exception-based parsing
try
{
    var result = UnsignedLeb128Decoding.Decode(possiblyCorruptData, out _);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid LEB128 data: {ex.Message}");
}
catch (OverflowException ex)
{
    Console.WriteLine($"Value too large: {ex.Message}");
}
```

## Performance Comparison

| Method | Allocations | Speed | Use Case |
|--------|-------------|--------|----------|
| `Encode(value, buffer)` | Zero | Fastest | High-performance scenarios |
| `TryEncode(value, buffer, out written)` | Zero | Fast | Safe encoding with validation |
| `Encode(value, out byteCount)` | 1 allocation | Moderate | Simple usage, backward compatibility |

## LEB128 Format Overview

LEB128 uses 7 bits per byte for data, with the 8th bit as a continuation flag:

- **Unsigned**: Direct binary representation in 7-bit chunks
- **Signed**: Two's complement with sign extension
- **Size**: 1-19 bytes for 128-bit integers
- **Byte Order**: Little-endian (least significant bits first)

### Encoding Examples

```
Value: 624485
Binary: 10011000001110100101
LEB128: [0xE5, 0x8E, 0x26]
        └─7 bits─┘ └─7 bits─┘ └─6 bits + 0─┘
        │         │         │
        │         │         └─ Final byte (no continuation bit)
        │         └─ Middle byte (continuation bit set) 
        └─ First byte (continuation bit set)
```

## Requirements

- .NET 8.0 or later
- Supports `Int128` and `UInt128` types

## Contributing

Contributions are welcome! Please ensure:
- All tests pass (`dotnet test`)
- Code follows existing conventions
- New features include comprehensive tests
- Performance-critical paths use span-based APIs

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Note**: For maximum performance, always prefer span-based APIs (`Encode(value, buffer)`, `TryEncode`, etc.) over allocation-based methods (`Encode(value, out byteCount)`).
