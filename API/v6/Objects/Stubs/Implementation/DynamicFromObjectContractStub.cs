namespace GMutagen.v6.Objects.Stubs.Implementation;

public class DynamicFromObjectContractStub<T> where T : class
{
    private readonly Object _targetObject;

    public DynamicFromObjectContractStub(Object targetObject)
    {
        _targetObject = targetObject;
    }

    public static implicit operator T(DynamicFromObjectContractStub<T> dynamicFromObjectContractStub)
    {
        if (dynamicFromObjectContractStub._targetObject.TryGet<T>(out var contract))
            return contract;

        // ReSharper disable once NullableWarningSuppressionIsUsed
        return null!;
    }
}