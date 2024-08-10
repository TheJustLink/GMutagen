using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SnakeGame;

#nullable enable

/// <summary>Represents a vector with two single-precision floating-point values.</summary>
public struct Vector2 : IEquatable<Vector2>, IFormattable
{
    /// <summary>The X component of the vector.</summary>
    public int X;
    /// <summary>The Y component of the vector.</summary>
    public int Y;

    /// <summary>Creates a new <see cref="T:System.Numerics.Vector2" /> object whose two elements have the same value.</summary>
    /// <param name="value">The value to assign to both elements.</param>
    public Vector2(int value)
        : this(value, value)
    {
    }

    /// <summary>Creates a vector whose elements have the specified values.</summary>
    /// <param name="x">The value to assign to the <see cref="F:System.Numerics.Vector2.X" /> field.</param>
    /// <param name="y">The value to assign to the <see cref="F:System.Numerics.Vector2.Y" /> field.</param>
    public Vector2(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>Constructs a vector from the given <see cref="T:System.ReadOnlySpan`1" />. The span must contain at least two elements.</summary>
    /// <param name="values">The span of elements to assign to the vector.</param>
    public Vector2(ReadOnlySpan<int> values)
    {
        if (values.Length < 2)
            throw new ArgumentOutOfRangeException();
        this = Unsafe.ReadUnaligned<Vector2>(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(values)));
    }

    /// <summary>Returns a vector whose 2 elements are equal to zero.</summary>
    /// <returns>A vector whose two elements are equal to zero (that is, it returns the vector <c>(0,0)</c>).</returns>
    public static Vector2 Zero => new();

    /// <summary>Gets a vector whose 2 elements are equal to one.</summary>
    /// <returns>A vector whose two elements are equal to one (that is, it returns the vector <c>(1,1)</c>).</returns>
    public static Vector2 One => new(1);

    /// <summary>Gets the vector (1,0).</summary>
    /// <returns>The vector <c>(1,0)</c>.</returns>
    public static Vector2 UnitX => new(1, 0);

    /// <summary>Gets the vector (0,1).</summary>
    /// <returns>The vector <c>(0,1)</c>.</returns>
    public static Vector2 UnitY => new(0, 1);

    /// <summary>Adds two vectors together.</summary>
    /// <param name="left">The first vector to add.</param>
    /// <param name="right">The second vector to add.</param>
    /// <returns>The summed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator +(Vector2 left, Vector2 right)
    {
        return new Vector2(left.X + right.X, left.Y + right.Y);
    }

    /// <summary>Divides the first vector by the second.</summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector that results from dividing <paramref name="left" /> by <paramref name="right" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator /(Vector2 left, Vector2 right)
    {
        return new Vector2(left.X / right.X, left.Y / right.Y);
    }

    /// <summary>Divides the specified vector by a specified scalar value.</summary>
    /// <param name="value1">The vector.</param>
    /// <param name="value2">The scalar value.</param>
    /// <returns>The result of the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator /(Vector2 value1, int value2) => value1 / new Vector2(value2);

    /// <summary>Returns a value that indicates whether each pair of elements in two specified vectors is equal.</summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector2 left, Vector2 right)
    {
        return left.X == (double)right.X && left.Y == (double)right.Y;
    }

    /// <summary>Returns a value that indicates whether two specified vectors are not equal.</summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector2 left, Vector2 right) => !(left == right);

    /// <summary>Returns a new vector whose values are the product of each pair of elements in two specified vectors.</summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The element-wise product vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(Vector2 left, Vector2 right)
    {
        return new Vector2(left.X * right.X, left.Y * right.Y);
    }

    /// <summary>Multiples the specified vector by the specified scalar value.</summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(Vector2 left, int right) => left * new Vector2(right);

    /// <summary>Multiples the scalar value by the specified vector.</summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(int left, Vector2 right) => right * left;

    /// <summary>Subtracts the second vector from the first.</summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector that results from subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator -(Vector2 left, Vector2 right)
    {
        return new Vector2(left.X - right.X, left.Y - right.Y);
    }

    /// <summary>Negates the specified vector.</summary>
    /// <param name="value">The vector to negate.</param>
    /// <returns>The negated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator -(Vector2 value) => Zero - value;

    /// <summary>Returns a vector whose elements are the absolute values of each of the specified vector's elements.</summary>
    /// <param name="value">A vector.</param>
    /// <returns>The absolute value vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Abs(Vector2 value) => new(Math.Abs(value.X), Math.Abs(value.Y));

    /// <summary>Adds two vectors together.</summary>
    /// <param name="left">The first vector to add.</param>
    /// <param name="right">The second vector to add.</param>
    /// <returns>The summed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Add(Vector2 left, Vector2 right) => left + right;

    /// <summary>Restricts a vector between a minimum and a maximum value.</summary>
    /// <param name="value1">The vector to restrict.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The restricted vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Clamp(Vector2 value1, Vector2 min, Vector2 max)
    {
        return Min(Max(value1, min), max);
    }

    /// <summary>Computes the Euclidean distance between the two given points.</summary>
    /// <param name="value1">The first point.</param>
    /// <param name="value2">The second point.</param>
    /// <returns>The distance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vector2 value1, Vector2 value2)
    {
        return MathF.Sqrt(DistanceSquared(value1, value2));
    }

    /// <summary>Returns the Euclidean distance squared between two specified points.</summary>
    /// <param name="value1">The first point.</param>
    /// <param name="value2">The second point.</param>
    /// <returns>The distance squared.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(Vector2 value1, Vector2 value2)
    {
        var vector2 = value1 - value2;
        return Dot(vector2, vector2);
    }

    /// <summary>Divides the first vector by the second.</summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector resulting from the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Divide(Vector2 left, Vector2 right) => left / right;

    /// <summary>Divides the specified vector by a specified scalar value.</summary>
    /// <param name="left">The vector.</param>
    /// <param name="divisor">The scalar value.</param>
    /// <returns>The vector that results from the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Divide(Vector2 left, int divisor) => left / divisor;

    /// <summary>Returns the dot product of two vectors.</summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The dot product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Vector2 value1, Vector2 value2)
    {
        return (float)(value1.X * (double)value2.X + value1.Y * (double)value2.Y);
    }

    /// <summary>Performs a linear interpolation between two vectors based on the given weighting.</summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="amount">A value between 0 and 1 that indicates the weight of <paramref name="value2" />.</param>
    /// <returns>The interpolated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Lerp(Vector2 value1, Vector2 value2, int amount)
    {
        return value1 * (1 - amount) + value2 * amount;
    }

    /// <summary>Returns a vector whose elements are the maximum of each of the pairs of elements in two specified vectors.</summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The maximized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Max(Vector2 value1, Vector2 value2)
    {
        return new Vector2(value1.X > (double)value2.X ? value1.X : value2.X, value1.Y > (double)value2.Y ? value1.Y : value2.Y);
    }

    /// <summary>Returns a vector whose elements are the minimum of each of the pairs of elements in two specified vectors.</summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The minimized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Min(Vector2 value1, Vector2 value2)
    {
        return new Vector2(value1.X < (double)value2.X ? value1.X : value2.X, value1.Y < (double)value2.Y ? value1.Y : value2.Y);
    }

    /// <summary>Returns a new vector whose values are the product of each pair of elements in two specified vectors.</summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The element-wise product vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Multiply(Vector2 left, Vector2 right) => left * right;

    /// <summary>Multiplies a vector by a specified scalar.</summary>
    /// <param name="left">The vector to multiply.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Multiply(Vector2 left, int right) => left * right;

    /// <summary>Multiplies a scalar value by a specified vector.</summary>
    /// <param name="left">The scaled value.</param>
    /// <param name="right">The vector.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Multiply(int left, Vector2 right) => left * right;

    /// <summary>Negates a specified vector.</summary>
    /// <param name="value">The vector to negate.</param>
    /// <returns>The negated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Negate(Vector2 value) => -value;

    /// <summary>Returns a vector with the same direction as the specified vector, but with a length of one.</summary>
    /// <param name="value">The vector to normalize.</param>
    /// <returns>The normalized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Normalize(Vector2 value) => value / (int)value.Length();


    /// <summary>Returns a vector whose elements are the square root of each of a specified vector's elements.</summary>
    /// <param name="value">A vector.</param>
    /// <returns>The square root vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 SquareRoot(Vector2 value)
    {
        return new Vector2((int)Math.Sqrt(value.X), (int)Math.Sqrt(value.Y));
    }

    /// <summary>Subtracts the second vector from the first.</summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The difference vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Subtract(Vector2 left, Vector2 right) => left - right;


    /// <summary>Attempts to copy the vector to the given <see cref="T:System.Span`1" />. The length of the destination span must be at least 2.</summary>
    /// <param name="destination">The destination span into which the values are copied.</param>
    /// <returns>
    /// <see langword="true" /> if the source vector was successfully copied to <paramref name="destination" />. <see langword="false" /> if <paramref name="destination" /> is not large enough to hold the source vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryCopyTo(Span<int> destination)
    {
        if (destination.Length < 2)
            return false;
        Unsafe.WriteUnaligned(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(destination)), this);
        return true;
    }

    /// <summary>Returns a value that indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    /// <see langword="true" /> if the current instance and <paramref name="obj" /> are equal; otherwise, <see langword="false" />. If <paramref name="obj" /> is <see langword="null" />, the method returns <see langword="false" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Vector2 other && Equals(other);
    }

    /// <summary>Returns a value that indicates whether this instance and another vector are equal.</summary>
    /// <param name="other">The other vector.</param>
    /// <returns>
    /// <see langword="true" /> if the two vectors are equal; otherwise, <see langword="false" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Vector2 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>The hash code.</returns>
    public readonly override int GetHashCode() => HashCode.Combine(X, Y);

    /// <summary>Returns the length of the vector.</summary>
    /// <returns>The vector's length.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Length() => MathF.Sqrt(LengthSquared());

    /// <summary>Returns the length of the vector squared.</summary>
    /// <returns>The vector's length squared.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float LengthSquared() => Dot(this, this);

#nullable enable
    /// <summary>Returns the string representation of the current instance using default formatting.</summary>
    /// <returns>The string representation of the current instance.</returns>
    public readonly override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    /// <summary>Returns the string representation of the current instance using the specified format string to format individual elements.</summary>
    /// <param name="format">A standard or custom numeric format string that defines the format of individual elements.</param>
    /// <returns>The string representation of the current instance.</returns>
    public readonly string ToString([StringSyntax("NumericFormat")] string? format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    /// <summary>Returns the string representation of the current instance using the specified format string to format individual elements and the specified format provider to define culture-specific formatting.</summary>
    /// <param name="format">A standard or custom numeric format string that defines the format of individual elements.</param>
    /// <param name="formatProvider">A format provider that supplies culture-specific formatting information.</param>
    /// <returns>The string representation of the current instance.</returns>
    public readonly string ToString([StringSyntax("NumericFormat")] string? format, IFormatProvider? formatProvider)
    {
        var numberGroupSeparator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
        var interpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 3);
        interpolatedStringHandler.AppendLiteral("<");
        interpolatedStringHandler.AppendFormatted(X.ToString(format, formatProvider));
        interpolatedStringHandler.AppendFormatted(numberGroupSeparator);
        interpolatedStringHandler.AppendLiteral(" ");
        interpolatedStringHandler.AppendFormatted(Y.ToString(format, formatProvider));
        interpolatedStringHandler.AppendLiteral(">");
        return interpolatedStringHandler.ToStringAndClear();
    }
}