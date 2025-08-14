# Performance Optimization Strategies

## Zero-Allocation Patterns
```csharp
// HIGH PERF: Span-based with stackalloc
Span<byte> buffer = stackalloc byte[20];  // Max LEB128 size
var encoded = UnsignedLeb128Encoding.Encode(value, buffer);
var decoded = UnsignedLeb128Decoding.Decode(encoded, out _);

// MEDIUM PERF: Caller-provided buffer reuse
var buffer = new byte[20];
if (UnsignedLeb128Encoding.TryEncode(value, buffer, out int written)) {
    var slice = buffer.AsSpan()[..written];
}

// AVOID: Allocation per call
byte[] encoded = UnsignedLeb128Encoding.Encode(value, out _);  // Allocates
```

## Hot Path Optimizations
1. **Bit manipulation over arithmetic**: `value >>= 7` not `value /= 128`
2. **Branch prediction friendly**: Predictable loop termination conditions
3. **Memory access patterns**: Sequential byte writing/reading
4. **Bounds checking**: Pre-calculate sizes to eliminate runtime checks

## Critical Performance Paths
```csharp
// GetEncodedByteCount() - Called before every span operation
// Must be fast as it's used for buffer sizing
public static int GetEncodedByteCount(UInt128 value) {
    if (value == 0) return 1;  // Fast path for common case
    
    var count = 0;
    do {
        value >>= 7;  // Efficient shift instead of division
        ++count;
    } while (value != 0);
    
    return count;
}
```

## Buffer Size Strategies
```csharp
// Conservative sizing (always sufficient):
const int MAX_LEB128_BYTES = 19;  // ceil(128/7) = 19
Span<byte> buffer = stackalloc byte[MAX_LEB128_BYTES];

// Optimal sizing (calculate exact):
int requiredBytes = GetEncodedByteCount(value);
Span<byte> buffer = stackalloc byte[requiredBytes];

// Batch processing (reuse large buffer):
byte[] sharedBuffer = new byte[MAX_LEB128_BYTES];
for (int i = 0; i < values.Length; i++) {
    if (TryEncode(values[i], sharedBuffer, out int written)) {
        ProcessBytes(sharedBuffer.AsSpan()[..written]);
    }
}
```

## Stream Performance Considerations
```csharp
// AVOID: Multiple ReadByte() calls with bounds checking
int byteRead = stream.ReadByte();
if (byteRead == -1) throw new EndOfStreamException();

// PREFER: Bulk read when possible (future enhancement idea)
// Read chunk of bytes, then process in memory
```

## Memory Layout Optimizations  
- **Struct vs Class**: Info classes could be structs but readability won over perf
- **Field ordering**: No specific optimizations (small objects)  
- **Readonly fields**: Properties are calculated, not stored
- **Primary constructors**: Minimal allocation overhead

## Compiler Optimization Hints
```csharp
// Methods likely to be inlined by JIT:
// - GetEncodedByteCount (small, frequently called)
// - Bit manipulation helpers
// - Range checks

// Methods unlikely to be inlined:
// - Stream processing (I/O calls)
// - Exception throwing paths
// - Complex validation logic
```

## Benchmarking Patterns for Future Validation
```csharp
// Key scenarios to benchmark:
// 1. Stackalloc encoding (zero allocation)
// 2. Array-based encoding (single allocation)  
// 3. Stream-based encoding (unknown allocation)
// 4. Round-trip operations (encode + decode)
// 5. Bulk processing (arrays of values)

// Value ranges to test:
// - Small values (1 byte encoding): 0-127
// - Medium values (2-3 bytes): 128-16383  
// - Large values (19 bytes): UInt128.MaxValue
// - Mixed distributions (realistic workloads)
```

## Performance Anti-Patterns to Avoid
```csharp
// DON'T: Allocate in loops
for (int i = 0; i < values.Length; i++) {
    byte[] encoded = Encode(values[i], out _);  // Allocates each iteration
}

// DON'T: Use exceptions for control flow
try {
    var result = Decode(possiblyInvalidData);
} catch (OverflowException) {
    // Handle invalid data
}

// DON'T: Repeated size calculations  
for (int i = 0; i < 1000; i++) {
    int size = GetEncodedByteCount(sameValue);  // Same result each time
}
```

## Future Performance Enhancement Opportunities
1. **Vectorization**: SIMD operations for bulk encoding/decoding
2. **Unsafe code**: Direct memory access for maximum speed
3. **Pooled buffers**: ArrayPool<byte> integration
4. **Async streams**: IAsyncEnumerable for large datasets
5. **Specialized overloads**: Optimized paths for common integer sizes
6. **JIT compilation**: ReadyToRun/AOT considerations

## Memory Pressure Considerations
- **Stack allocation limits**: stackalloc limited to ~1KB, LEB128 max 19 bytes is safe
- **GC pressure**: Span-based APIs eliminate allocation pressure
- **Working set**: Small memory footprint (static classes, no instance data)
- **Cache efficiency**: Sequential access patterns, small data structures