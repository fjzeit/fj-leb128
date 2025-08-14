namespace FJ.Leb128;

/// <summary>
/// Represents information about an unsigned 128-bit integer, including its size in smaller integer types.
/// </summary>
public class UnsignedInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsignedInfo"/> class with the specified UInt128 value.
    /// </summary>
    /// <param name="uint128">The UInt128 value to analyze.</param>
    public UnsignedInfo(UInt128 uint128)
    {
        this.CanBeByte = uint128 <= 255 && uint128 >= 0;
        this.CanBeInt32 = uint128 <= uint.MaxValue && uint128 >= uint.MinValue;
        this.CanBeInt64 = uint128 <= ulong.MaxValue && uint128 >= ulong.MinValue;
        this.AsInt128 = uint128;
    }

    /// <summary>
    /// Gets a value indicating whether the stored value can be represented as a byte.
    /// </summary>
    public bool CanBeByte { get; }

    /// <summary>
    /// Gets a value indicating whether the stored value can be represented as a UInt32.
    /// </summary>
    public bool CanBeInt32 { get; }

    /// <summary>
    /// Gets a value indicating whether the stored value can be represented as a UInt64.
    /// </summary>
    public bool CanBeInt64 { get; }

    /// <summary>
    /// Gets the minimum size required to represent the stored value.
    /// </summary>
    public Size MinSize
    {
        get
        {
            if (this.CanBeByte)
            {
                return Size.Bits8;
            }

            if (this.CanBeInt32)
            {
                return Size.Bits32;
            }

            return this.CanBeInt64 ? Size.Bits64 : Size.Bits128;
        }
    }

    /// <summary>
    /// Gets the stored value as a byte.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the value cannot be represented as a byte.</exception>
    public byte AsByte => this.CanBeByte ? (byte)this.AsInt128 : throw new InvalidOperationException();

    /// <summary>
    /// Gets the stored value as a UInt32.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the value cannot be represented as a UInt32.</exception>
    public uint AsInt32 => this.CanBeInt32 ? (uint)this.AsInt128 : throw new InvalidOperationException();

    /// <summary>
    /// Gets the stored value as a UInt64.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the value cannot be represented as a UInt64.</exception>
    public ulong AsInt64 => this.CanBeInt64 ? (ulong)this.AsInt128 : throw new InvalidOperationException();

    /// <summary>
    /// Gets the stored value as a UInt128.
    /// </summary>
    public UInt128 AsInt128 { get; }
}
