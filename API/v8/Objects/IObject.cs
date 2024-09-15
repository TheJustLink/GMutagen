namespace GMutagen.v8.Objects;

public interface IObject
{
    TContract Get<TContract>() where TContract : class;
}