namespace Configuration.Providers.Interfaces;

public interface IConfigurationProvider : IDisposable
{
    string Name { get; }
    int Priority { get; }
    event Action ConfigurationChanged;
    void Load();
    void Unload();
}