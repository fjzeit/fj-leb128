namespace FJ.Leb128.Decoding;

/// <summary>
/// Provides methods for decoding LEB128 (Little Endian Base 128) encoded signed integers.
/// </summary>
public static class SignedLeb128Decoding {
    /// <summary>
    /// Decodes a LEB128 encoded signed integer from a byte array.
    /// </summary>
    /// <param name="bytes">The byte array containing the LEB128 encoded value.</param>
    /// <returns>A SignedInfo object containing the decoded value.</returns>
    public static SignedInfo Decode(byte[] bytes) {
        using var memoryStream = new MemoryStream(bytes);
        return Decode(memoryStream);
    }

    /// <summary>
    /// Decodes a LEB128 encoded signed integer from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the LEB128 encoded value.</param>
    /// <returns>A SignedInfo object containing the decoded value.</returns>
    /// <exception cref="EndOfStreamException">Thrown when the end of the stream is reached unexpectedly.</exception>
    /// <exception cref="OverflowException">Thrown when the encoded value exceeds the maximum allowed size.</exception>
    public static SignedInfo Decode(Stream stream) {
        Int128 result = 0;

        var shift = 0;
        var size = 128;
        int aByte;
        var byteCount = 0;
        var maxByteCount = (int)Math.Ceiling(128d / 7d);

        do {
            aByte = stream.ReadByte();
            if (aByte == -1) {
                throw new EndOfStreamException();
            }

            result |= ((Int128)aByte & 0x7F) << shift;
            shift += 7;

            ++byteCount;
            if (byteCount > maxByteCount) {
                throw new OverflowException("Out of bits");
            }
        } while ((aByte & 0x80) != 0);

        if (shift < size && (aByte & 0x40) != 0) {
            result |= Int128.MaxValue << shift;
        }

        return new SignedInfo(result);
    }

    /// <summary>
    /// Tries to decode a LEB128 encoded signed integer from a read-only span.
    /// </summary>
    /// <param name="source">The read-only span containing the LEB128 encoded value.</param>
    /// <param name="result">The decoded signed integer information.</param>
    /// <param name="bytesConsumed">The number of bytes consumed from the source span.</param>
    /// <returns>true if decoding was successful; false if the span contained invalid data.</returns>
    public static bool TryDecode(ReadOnlySpan<byte> source, out SignedInfo result, out int bytesConsumed) {
        Int128 value = 0;
        var shift = 0;
        var size = 128;
        bytesConsumed = 0;
        var maxByteCount = (int)Math.Ceiling(128d / 7d);
        int aByte = 0;

        for (var i = 0; i < source.Length && bytesConsumed < maxByteCount; i++) {
            aByte = source[i];
            value |= ((Int128)aByte & 0x7F) << shift;
            shift += 7;
            ++bytesConsumed;

            if ((aByte & 0x80) == 0) {
                break;
            }
        }

        if (bytesConsumed >= maxByteCount && (aByte & 0x80) != 0) {
            result = default!;
            bytesConsumed = 0;
            return false;
        }

        if (bytesConsumed == 0 || (bytesConsumed == source.Length && (aByte & 0x80) != 0)) {
            result = default!;
            bytesConsumed = 0;
            return false;
        }

        if (shift < size && (aByte & 0x40) != 0) {
            value |= Int128.MaxValue << shift;
        }

        result = new SignedInfo(value);
        return true;
    }

    /// <summary>
    /// Decodes a LEB128 encoded signed integer from a read-only span.
    /// </summary>
    /// <param name="source">The read-only span containing the LEB128 encoded value.</param>
    /// <param name="bytesConsumed">The number of bytes consumed from the source span.</param>
    /// <returns>A SignedInfo object containing the decoded value.</returns>
    /// <exception cref="ArgumentException">Thrown when the source span contains invalid LEB128 data.</exception>
    /// <exception cref="OverflowException">Thrown when the encoded value exceeds the maximum allowed size.</exception>
    public static SignedInfo Decode(ReadOnlySpan<byte> source, out int bytesConsumed) {
        if (!TryDecode(source, out var result, out bytesConsumed)) {
            if (bytesConsumed == 0) {
                throw new ArgumentException("Invalid LEB128 data in source span", nameof(source));
            }
            throw new OverflowException("Encoded value exceeds maximum allowed size");
        }
        
        return result;
    }
}
