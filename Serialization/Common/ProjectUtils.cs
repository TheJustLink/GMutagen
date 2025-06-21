namespace Serialization.Common;

public static class ProjectUtils
{
    public static string FindProjectRoot(string currentPath)
    {
        var directory = new DirectoryInfo(currentPath);

        while (directory != null)
        {
            if (directory.GetFiles("*.csproj").Any() ||
                directory.GetFiles("*.sln").Any() ||
                directory.GetDirectories(".git").Any())
                return directory.FullName;

            directory = directory.Parent;
        }

        return currentPath;
    }
}