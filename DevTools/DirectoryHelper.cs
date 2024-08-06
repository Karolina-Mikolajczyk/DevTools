namespace DevTools;

public static class DirectoryHelper
{
    public static List<string> ToListRecursively(string path)
    {
        return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();
    }
}