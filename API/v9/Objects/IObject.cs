namespace GMutagen.v9.Objects;

public interface IObject<out TId>
{
    TId Id { get; }
    TContract Get<TContract>() where TContract : class;
}