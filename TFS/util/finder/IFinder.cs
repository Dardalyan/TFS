namespace TFS.util;

public interface IFinder<out T>
{
    public string FindDirectory();
    
}