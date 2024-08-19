namespace GMutagen.v1;

public class ObjectTemplate
{
    private static int s_currentId;
    private Container _container;

    public ObjectTemplate()
    {
        _container = new Container();
    }

    public ObjectTemplate(ObjectTemplate template)
    {
        _container = template._container.Clone();
    }

    public Object Create()
    {
        return new Object(s_currentId++, new Container(_container), this);
    }

    public ObjectTemplate AddEmpty<T>() where T : class
    {
        _container.AddEmpty<T>();

        return this;
    }

    public ObjectTemplate Add<T>() where T : class, new()
    {
        _container.Add<T>();

        return this;
    }

    public ObjectTemplate Add<T>(T contract) where T : class
    {
        _container.Add(contract);

        return this;
    }

    public ObjectTemplate Set<T>(T contract) where T : class
    {
        _container.Set(contract);
        return this;
    }
}