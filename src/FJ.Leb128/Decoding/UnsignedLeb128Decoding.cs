namespace FJ.Leb128.Decoding;

/// <summary>
/// Provides methods for decoding LEB128 (Little Endian Base 128) encoded unsigned integers.
/// </summary>
public static class UnsignedLeb128Decoding {
    /// <summary>
    /// Decodes a LEB128 encoded unsigned integer from a byte array.
    /// </summary>
    /// <param name="bytes">The byte array containing the LEB128 encoded value.</param>
    /// <returns>An UnsignedInfo object containing the decoded value.</returns>
    public static UnsignedInfo Decode(byte[] bytes) {
        using var memoryStream = new MemoryStream(bytes);
        return Decode(memoryStream);
    }

    /// <summary>
    /// Decodes a LEB128 encoded unsigned integer from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the LEB128 encoded value.</param>
    /// <returns>An UnsignedInfo object containing the decoded value.</returns>
    /// <exception cref="EndOfStreamException">Thrown when the end of the stream is reached unexpectedly.</exception>
    /// <exception cref="OverflowException">Thrown when the encoded value exceeds the maximum allowed size.</exception>
    public static UnsignedInfo Decode(Stream stream) {
        UInt128 result = 0;
        var shift = 0;
        int aByte;
        var byteCount = 0;
        var maxByteCount = (int)Math.Ceiling(128d / 7d);

        do {
            aByte = stream.ReadByte();
            if (aByte == -1) {
                throw new EndOfStreamException();
            }

            result |= (UInt128)(aByte & 0x7F) << shift;
            shift += 7;

            ++byteCount;
            if (byteCount > maxByteCount) {
                throw new OverflowException("Out of bits");
            }
        } while ((aByte & 0x80) != 0);

        return new UnsignedInfo(result);
    }

    /// <summary>
    /// Tries to decode a LEB128 encoded unsigned integer from a read-only span.
    /// </summary>
    /// <param name="source">The read-only span containing the LEB128 encoded value.</param>
    /// <param name="result">The decoded unsigned integer information.</param>
    /// <param name="bytesConsumed">The number of bytes consumed from the source span.</param>
    /// <returns>true if decoding was successful; false if the span contained invalid data.</returns>
    public static bool TryDecode(ReadOnlySpan<byte> source, out UnsignedInfo result, out int bytesConsumed) {
        UInt128 value = 0;
        var shift = 0;
        bytesConsumed = 0;
        var maxByteCount = (int)Math.Ceiling(128d / 7d);

        for (var i = 0; i < source.Length && bytesConsumed < maxByteCount; i++) {
            var aByte = source[i];
            value |= (UInt128)(aByte & 0x7F) << shift;
            shift += 7;
            ++bytesConsumed;

            if ((aByte & 0x80) == 0) {
                result = new UnsignedInfo(value);
                return true;
            }
        }

        if (bytesConsumed >= maxByteCount) {
            result = default!;
            bytesConsumed = 0;
            return false;
        }

        result = default!;
        bytesConsumed = 0;
        return false;
    }

    /// <summary>
    /// Decodes a LEB128 encoded unsigned integer from a read-only span.
    /// </summary>
    /// <param name="source">The read-only span containing the LEB128 encoded value.</param>
    /// <param name="bytesConsumed">The number of bytes consumed from the source span.</param>
    /// <returns>An UnsignedInfo object containing the decoded value.</returns>
    /// <exception cref="ArgumentException">Thrown when the source span contains invalid LEB128 data.</exception>
    /// <exception cref="OverflowException">Thrown when the encoded value exceeds the maximum allowed size.</exception>
    public static UnsignedInfo Decode(ReadOnlySpan<byte> source, out int bytesConsumed) {
        if (!TryDecode(source, out var result, out bytesConsumed)) {
            if (bytesConsumed == 0) {
                throw new ArgumentException("Invalid LEB128 data in source span", nameof(source));
            }
            throw new OverflowException("Encoded value exceeds maximum allowed size");
        }
        
        return result;
    }
}
