namespace GMutagen.v9.Objects.Interfaces;

public interface IObject<out TId> : IObject
{
    TId Id { get; }
}

public interface IObject
{
    TContract Get<TContract>() where TContract : class;
    bool TryGet<TContract>(out TContract contract) where TContract : class;
}