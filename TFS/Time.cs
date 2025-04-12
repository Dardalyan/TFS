namespace TFS;

public class Time
{
    private static int CurrentTime = 0;
    
    public static int GetCurrentTime()
    {
        return CurrentTime;
    }

    public void Tick()
    {
        // Increase Current time  
        ++CurrentTime;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Current time: {CurrentTime}");
        Console.ResetColor();

    }
}