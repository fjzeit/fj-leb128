namespace FJ.Leb128.Encoding;

/// <summary>
/// Provides methods for encoding unsigned integers using LEB128 (Little Endian Base 128) encoding.
/// </summary>
public static class UnsignedLeb128Encoding {
    /// <summary>
    /// Encodes a UInt128 value to a byte array using LEB128 encoding.
    /// </summary>
    /// <param name="i">The UInt128 value to encode.</param>
    /// <param name="byteCount">The number of bytes used in the encoding.</param>
    /// <returns>A byte array containing the LEB128 encoded value.</returns>
    public static byte[] Encode(UInt128 i, out int byteCount) {
        using var memoryStream = new MemoryStream();
        byteCount = Encode(i, memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Encodes a UInt128 value to a stream using LEB128 encoding.
    /// </summary>
    /// <param name="i">The UInt128 value to encode.</param>
    /// <param name="stream">The stream to write the encoded value to.</param>
    /// <returns>The number of bytes written to the stream.</returns>
    public static int Encode(UInt128 i, Stream stream) {
        var count = 0;
        do {
            var chunk = (byte)(i & 0b_0111_1111);
            i >>= 7;
            if (i != 0) {
                chunk |= 0b_1000_0000;
            }

            stream.WriteByte(chunk);
            ++count;
        } while (i != 0);

        return count;
    }

    /// <summary>
    /// Gets the number of bytes required to encode the specified UInt128 value using LEB128 encoding.
    /// </summary>
    /// <param name="value">The UInt128 value to analyze.</param>
    /// <returns>The number of bytes required for encoding.</returns>
    public static int GetEncodedByteCount(UInt128 value) {
        if (value == 0) return 1;
        
        var count = 0;
        do {
            value >>= 7;
            ++count;
        } while (value != 0);
        
        return count;
    }

    /// <summary>
    /// Tries to encode a UInt128 value to the specified span using LEB128 encoding.
    /// </summary>
    /// <param name="value">The UInt128 value to encode.</param>
    /// <param name="destination">The span to write the encoded bytes to.</param>
    /// <param name="bytesWritten">The number of bytes written to the destination.</param>
    /// <returns>true if the encoding was successful; false if the destination span was too small.</returns>
    public static bool TryEncode(UInt128 value, Span<byte> destination, out int bytesWritten) {
        var requiredBytes = GetEncodedByteCount(value);
        if (destination.Length < requiredBytes) {
            bytesWritten = 0;
            return false;
        }

        bytesWritten = 0;
        do {
            var chunk = (byte)(value & 0b_0111_1111);
            value >>= 7;
            if (value != 0) {
                chunk |= 0b_1000_0000;
            }

            destination[bytesWritten] = chunk;
            ++bytesWritten;
        } while (value != 0);

        return true;
    }

    /// <summary>
    /// Encodes a UInt128 value to the specified span using LEB128 encoding.
    /// </summary>
    /// <param name="value">The UInt128 value to encode.</param>
    /// <param name="buffer">The span to write the encoded bytes to. Must be large enough to hold the encoded value.</param>
    /// <returns>A read-only span containing the encoded bytes.</returns>
    /// <exception cref="ArgumentException">Thrown when the buffer is too small to hold the encoded value.</exception>
    public static ReadOnlySpan<byte> Encode(UInt128 value, Span<byte> buffer) {
        if (!TryEncode(value, buffer, out var bytesWritten)) {
            throw new ArgumentException("Buffer too small for encoded value", nameof(buffer));
        }
        
        return buffer[..bytesWritten];
    }
}
