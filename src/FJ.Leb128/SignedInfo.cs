namespace FJ.Leb128;

/// <summary>
/// Represents information about a decoded signed integer, including its value and size characteristics.
/// </summary>
public class SignedInfo(Int128 int128) {
    /// <summary>
    /// Gets a value indicating whether the stored value can be represented as a byte.
    /// </summary>
    public bool CanBeByte { get; } = int128 <= 127 && int128 >= -128;
    /// <summary>
    /// Gets a value indicating whether the stored value can be represented as an Int32.
    /// </summary>
    public bool CanBeInt32 { get; } = int128 <= int.MaxValue && int128 >= int.MinValue;
    /// <summary>
    /// Gets a value indicating whether the stored value can be represented as an Int64.
    /// </summary>
    public bool CanBeInt64 { get; } = int128 <= long.MaxValue && int128 >= long.MinValue;

    /// <summary>
    /// Gets the minimum size required to represent the stored value.
    /// </summary>
    public Size MinSize {
        get {
            if (this.CanBeByte) {
                return Size.Bits8;
            }

            if (this.CanBeInt32) {
                return Size.Bits32;
            }

            return this.CanBeInt64 ? Size.Bits64 : Size.Bits128;
        }
    }

    /// <summary>
    /// Gets the stored value as a byte, if possible.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the value cannot be represented as a byte.</exception>
    public byte AsByte => this.CanBeByte ? (byte)int128 : throw new InvalidOperationException();
    /// <summary>
    /// Gets the stored value as an Int32, if possible.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the value cannot be represented as an Int32.</exception>
    public int AsInt32 => this.CanBeInt32 ? (int)int128 : throw new InvalidOperationException();
    /// <summary>
    /// Gets the stored value as an Int64, if possible.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the value cannot be represented as an Int64.</exception>
    public long AsInt64 => this.CanBeInt64 ? (long)int128 : throw new InvalidOperationException();

    /// <summary>
    /// Gets the stored value as an Int128.
    /// </summary>
    public Int128 AsInt128 => int128;
}
