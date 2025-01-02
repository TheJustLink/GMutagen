namespace GMutagen.v9.Objects;

public interface IObject<out TId> : IObject
{
    TId Id { get; }
    TContract Get<TContract>() where TContract : class;
}

public interface IObject
{
    
}