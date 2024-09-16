namespace GMutagen.v8.Objects;

public interface IObject<out TId>
{
    TId Id { get; }
    TContract Get<TContract>() where TContract : class;
}