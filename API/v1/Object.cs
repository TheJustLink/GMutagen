namespace GMutagen.v1;

public class Object
{
    public int Id;
    public Container Contracts;

    public ObjectTemplate Template;

    public Object(int id, Container contracts, ObjectTemplate template)
    {
        Id = id;
        Contracts = contracts;
        Template = template;
    }

    public T Get<T>() where T : class
    {
        return Contracts.Get<T>();
    }

    public Object Set<T>(T contract) where T : class
    {
        Contracts.Set(contract);

        return this;
    }
}