using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TFS.util;

namespace TFS;

public abstract class FileDataParser
{
    protected  string FILENAME { get; set; }
    protected  string FILEEXTENSION{ get; set; }
    protected IFinder<SolutionFinder> Finder { get; set; }

    public  void AssignFile(string filename, string extension)
    {
        FILENAME = filename;
        FILEEXTENSION = extension;
    }
    public FileDataParser(IFinder<SolutionFinder> finder)
    {
        Finder = finder;
    }

    public abstract object Parse(); 
    protected abstract Dictionary<int, string> ReadLinesInFiles();
    public abstract void PrintResults();
    
    
    protected string GetFilePath()
    {
        if (FILENAME == null || FILEEXTENSION == null)
            throw new NullReferenceException("File name or its extension is null !");
        
        return SolutionDirectory()+"/"+FILENAME+"."+FILEEXTENSION;
    }
    private string SolutionDirectory()
    {
        return Finder.FindDirectory();
    }
    protected void LogWarning(string message, int lineNum = 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        if ( lineNum != 0)
            Console.Write($"\nWarning in line {lineNum} -> ");
        else
            Console.Write($"\nWarning  -> ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(message);
        Console.ResetColor();

    }
    protected static string HandleParenthesisAndWhiteSpaces(string line)
    {
        // Replace with white space, in every occurance of '(' or ')'.
        line = line.Replace("(", " ").Replace(")", " ");
        // Remove empty spaces from the beginning and the end.
        line = line.Trim();

        // ıf there is a more than one white spaces, then reduce it to just one.
        line = Regex.Replace(line, @"\s{2,}", " ");
        
        return line;
    }
    
    
    protected static string RemoveSpecialCharacters(string line)
    {
        string pattern = @"[^a-zA-Z0-9_\.\(\)\-\s]";
        
        // There some exceptions, we hold some characters like
        // '-' for negative signed integers
        // '.' or ',' for double or float values
        // and '(' or ')' for specify the beginnings and endings of data in file content especially for jobtypes and stations
        return Regex.Replace(line, pattern, "");
    }
}