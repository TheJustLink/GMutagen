using GMutagen.v9.Resolving.Nodes.Internal.Common;
using Xunit;

namespace ConfigurationIntegrationTests;

public class ConfigurationProxyTests
{
    [Fact]
    public void Proxy_Reads_Values_And_Casts()
    {
        var source = new ConfigurationSection();
        source.Add("Number", "42");

        var proxy = new ConfigurationProxy<string, int>(source);
        Assert.True(proxy.TryGetValue("Number", out var result));
        Assert.Equal(42, result);
    }

    [Fact]
    public void Proxy_Is_ReadOnly()
    {
        var source = new ConfigurationSection();
        var proxy = new ConfigurationProxy<string, int>(source);

        Assert.Throws<NotSupportedException>(() => proxy.Add("Key", 5));
        Assert.Throws<NotSupportedException>(() => proxy.Remove("Key"));
    }

    [Fact]
    public void Proxy_Subsection_Works()
    {
        var source = new ConfigurationSection();
        source.GetSubSection("Sub").Add("Age", "25");

        var proxy = new ConfigurationProxy<string, int>(source);
        var sub = proxy.GetSubSection("Sub");

        Assert.True(sub.TryGetValue("Age", out var age));
        Assert.Equal(25, age);
    }

    [Fact]
    public void Proxy_GetAllSettings_Works()
    {
        var source = new ConfigurationSection();
        source.Add("Key", "99");

        var proxy = new ConfigurationProxy<string, int>(source);
        var all = proxy.GetAllSettings();

        Assert.Contains(all, kv => kv.Key == "Key" && kv.Value == 99);
    }
}