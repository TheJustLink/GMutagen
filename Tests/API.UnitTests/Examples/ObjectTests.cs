namespace API.UnitTests.Examples;

// TestClassName = []
public class ObjectTests
{
    // TestMethodName = [ThingUnderTest]_Should_[ExpectedResult]_[Condition]

    // Simple test
    [Fact]
    public void ToString_Should_ThrowNullException_WhenIsNull()
    {
        // Arrange
        object foo = null!;

        // Act

        // Assert
        Assert.Throws<NullReferenceException>(() => foo.ToString());
    }

    // With inlined parameters
    [Theory]
    [InlineData(0, null)]
    [InlineData(0, "")]
    [InlineData(0, 1)]
    public void Equals_Should_ReturnFalse_WhenOtherObjectIsNullOrNotSame(object object1, object? object2)
    {
        // Act
        var result = object1.Equals(object2);

        // Assert
        Assert.False(result);
    }

    // With data source
    [Theory]
    [MemberData(nameof(EqualsUniqueData))]
    [MemberData(nameof(GetEqualsUniqueData))]
    [MemberData(nameof(GetEqualsUniqueData), 0)] // Test failed for parameter = 1 or ""
    public void Equals_Should_ReturnFalse_WhenOtherObjectIsNullOrNotSame2(object object1, object? object2)
    {
        // Act
        var result = object1.Equals(object2);

        // Assert
        Assert.False(result);
    }

    // Data source property
    public static IEnumerable<object?[]> EqualsUniqueData =>
    [
        [new object(), null],
        [new object(), string.Empty],
        [new object(), 1],
        [new object(), new object()]
    ];
    // Data source method
    public static IEnumerable<object?[]> GetEqualsUniqueData()
    {
        yield return [new object(), null];
        yield return [new object(), string.Empty];
        yield return [new object(), 1];
        yield return [new object(), new object()];
    }
    // Data source method with parameters
    public static IEnumerable<object?[]> GetEqualsUniqueData(object firstObject)
    {
        yield return [firstObject, null];
        yield return [firstObject, string.Empty];
        yield return [firstObject, 1];
        yield return [firstObject, new object()];
    }

    // With class data source
    [Theory]
    [ClassData(typeof(EqualsTestData))]
    public void Equals_Should_ReturnFalse_WhenOtherObjectIsNullOrNotSame3(object object1, object? object2)
    {
        // Act
        var result = object1.Equals(object2);

        // Assert
        Assert.False(result);
    }
}
public class EqualsTestData : TheoryData<object, object?>
{
    public EqualsTestData()
    {
        Add(new object(), null);
        Add(new object(), string.Empty);
        Add(new object(), 1);
        Add(new object(), new object());
    }
}