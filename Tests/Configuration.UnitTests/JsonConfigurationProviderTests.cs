using GMutagen.v9.Resolving.Nodes.Internal.Common;
using Xunit;

namespace ConfigurationIntegrationTests;

public class JsonConfigurationProviderTests
{
    private const string TestFile = "testsettings.json";

    [Fact]
    public void Load_Config_And_Access_Value()
    {
        var provider = new JsonConfigurationProvider(TestFile);
        var config = provider.Configuration;

        Assert.True(config.TryGetValue("RootSetting", out var value));
        Assert.Equal("RootValue", value);

        Assert.True(config.TryGetValue("SectionA.Setting1", out var s1));
        Assert.Equal("Value1", s1);
    }

    [Fact]
    public void Load_Nested_Subsection()
    {
        var provider = new JsonConfigurationProvider(TestFile);
        var config = provider.Configuration;

        Assert.True(config.TryGetValue("SectionB.Nested.Bool", out var boolStr));
        Assert.Equal("true", boolStr);
    }

    [Fact]
    public async Task AutoReload_Updates_Cache()
    {
        var tempFile = "temp_config.json";
        File.Copy(TestFile, tempFile, overwrite: true);

        var path = Path.Join(Directory.GetCurrentDirectory(), tempFile);
        var provider = new JsonConfigurationProvider(path, reloadOnChange: true);
        var config = provider.Configuration;

        Assert.True(config.TryGetValue("RootSetting", out var original));
        Assert.Equal("RootValue", original);

        await Task.Delay(100);
        await File.WriteAllTextAsync(tempFile, @"{ ""RootSetting"": ""Changed"" }");

        await Task.Delay(3000); // wait for file system watcher

        config = provider.Configuration;
        Assert.True(config.TryGetValue("RootSetting", out var changed));
        Assert.Equal("Changed", changed);

        File.Delete(tempFile);
    }
}