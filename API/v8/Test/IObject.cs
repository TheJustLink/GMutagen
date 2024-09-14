namespace GMutagen.v8.Test;

public interface IObject
{
    TContract Get<TContract>() where TContract : class;
}