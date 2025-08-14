# LEB128 Algorithm Implementation Details

## Algorithm Quick Reference
**LEB128** = Little Endian Base 128 variable-length encoding
- **Unsigned**: Direct 7-bit chunking with continuation bits
- **Signed**: Two's complement with sign extension logic

## Unsigned LEB128 Encoding Logic
```csharp
// Core encoding loop (UnsignedLeb128Encoding.cs:27-39)
do {
    var chunk = (byte)(value & 0b_0111_1111);  // Extract 7 bits
    value >>= 7;                               // Shift right 7 bits
    if (value != 0) {
        chunk |= 0b_1000_0000;                 // Set continuation bit
    }
    stream.WriteByte(chunk);
    ++count;
} while (value != 0);
```

## Signed LEB128 Encoding Logic  
```csharp
// Core signed encoding (SignedLeb128Encoding.cs:37-49)
while (more) {
    var chunk = (byte)(value & 0b_0111_1111);
    value >>= 7;  // Arithmetic right shift preserves sign
    
    // Termination condition: check sign bit consistency
    if ((value == 0 && (chunk & 0b_0100_0000) == 0) ||     // Positive: value=0, bit6=0
        (value == -1 && (chunk & 0b_0100_0000) != 0)) {    // Negative: value=-1, bit6=1
        more = false;
    } else {
        chunk |= 0b_1000_0000;  // Set continuation bit
    }
    write(chunk);
}
```

## Unsigned LEB128 Decoding Logic
```csharp
// Core decoding loop (UnsignedLeb128Decoding.cs:32-44)
do {
    aByte = stream.ReadByte();
    if (aByte == -1) throw new EndOfStreamException();
    
    result |= (UInt128)(aByte & 0x7F) << shift;  // Extract 7 bits, shift into position
    shift += 7;
    
    if (++byteCount > maxByteCount) throw new OverflowException("Out of bits");
} while ((aByte & 0x80) != 0);  // Continue while continuation bit set
```

## Signed LEB128 Decoding Logic
```csharp  
// Core signed decoding with sign extension (SignedLeb128Decoding.cs:33-50)
do {
    aByte = stream.ReadByte();
    result |= ((Int128)aByte & 0x7F) << shift;
    shift += 7;
} while ((aByte & 0x80) != 0);

// Sign extension for negative numbers
if (shift < size && (aByte & 0x40) != 0) {  // Check sign bit (bit 6) of last byte
    result |= Int128.MaxValue << shift;      // Fill remaining bits with 1s
}
```

## Critical Bit Patterns
- **Continuation bit**: MSB (bit 7) = 1 means more bytes follow, 0 means last byte
- **Sign bit**: Bit 6 of final byte determines sign extension for signed values
- **Data bits**: Bits 0-6 contain actual integer data (7 bits per byte)

## Encoding Size Calculations
```csharp
// Unsigned: ceil(significant_bits / 7)
// Examples:
// 0-127 (7 bits) → 1 byte
// 128-16383 (14 bits) → 2 bytes  
// 16384-2097151 (21 bits) → 3 bytes
// UInt128.MaxValue → 19 bytes (ceil(128/7) = 19)

// Signed: More complex due to sign extension requirements
// Positive numbers similar to unsigned
// Negative numbers may need extra byte for sign clarity
```

## Overflow Protection Strategy
- **Max bytes**: `(int)Math.Ceiling(128d / 7d)` = 19 bytes
- **Bounds check**: Count bytes read/written, throw on overflow
- **Early termination**: Stop on invalid continuation patterns

## Common Pitfalls & Solutions
1. **Arithmetic vs Logical Shift**: Signed uses `>>` (arithmetic), preserves sign bit
2. **Sign Extension**: Critical for negative values, check bit 6 of final byte
3. **Continuation Logic**: Different termination rules for signed vs unsigned
4. **Overflow Detection**: Must check bounds before processing more bytes

## Test Validation Patterns
- **Round-trip consistency**: `encode(decode(x)) == x`
- **Byte-level verification**: Known values produce expected byte sequences
- **Boundary testing**: Values around 7-bit boundaries (127/128, 16383/16384)
- **Overflow testing**: Values requiring maximum bytes (19 bytes for 128-bit)
- **Malformed input**: Invalid continuation patterns, oversized encodings