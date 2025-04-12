namespace TFS.util;

public class SolutionFinder : IFinder<SolutionFinder>
{
    public string FindDirectory()
    {
        const char SLASH = '/';
        
        List<string>dirs = Environment.CurrentDirectory.Split('/').ToList();
        string directory = "/";
        
        dirs.RemoveAt(0);
        for (int i = 0; i < 3; i++)
        {
            dirs.RemoveAt(dirs.Count-1);
        }
        
        dirs.ForEach(i=> directory+=$"{i}"+SLASH);
        directory.Remove(directory.Length - 1);
        
        
        return directory;
    }
}