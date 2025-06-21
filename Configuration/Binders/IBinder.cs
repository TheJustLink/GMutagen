using Configuration.Sections.Interfaces;

namespace Configuration.Binders;

public interface IBinder<TKey, TValue>
{
    void Bind(object instance, Type type, IConfiguration<TKey, TValue> section);
}