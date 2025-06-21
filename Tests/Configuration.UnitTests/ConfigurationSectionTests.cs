using GMutagen.v9.Resolving.Nodes.Internal.Common;
using Xunit;

namespace ConfigurationIntegrationTests;

public class ConfigurationSectionTests
{
    [Fact]
    public void Add_And_Get_Value_Works()
    {
        var config = new ConfigurationSection();
        config.Add("TestKey", "TestValue");

        Assert.True(config.TryGetValue("TestKey", out var value));
        Assert.Equal("TestValue", value);
        Assert.Equal("TestValue", config["TestKey"]);
    }

    [Fact]
    public void Remove_Key_Works()
    {
        var config = new ConfigurationSection();
        config.Add("Key", "Value");
        config.Remove("Key");

        Assert.False(config.TryGetValue("Key", out _));
    }

    [Fact]
    public void Nested_Subsection_Works()
    {
        var config = new ConfigurationSection();
        config.AddSection("Sub").Add("Key", "123");

        Assert.True(config.TryGetValue("Sub.Key", out var value));
        Assert.Equal("123", value);
    }

    [Fact]
    public void Subsection_Enumeration_Works()
    {
        var config = new ConfigurationSection();
        config.AddSection("One");
        config.AddSection("Two");

        var names = config.GetAllSubSectionNames().ToList();
        Assert.Contains("One", names);
        Assert.Contains("Two", names);
    }
    
    [Fact]
    public void Getting_Unrepresented_Key_Works()
    {
        var config = new ConfigurationSection();
        config.AddSection("Sub").Add("Key", "123");

        Assert.False(config.TryGetValue("Sub.Key.Asd", out var value));
    }
}