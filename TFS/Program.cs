using TFS;
using TFS.facility;
using TFS.util;
using Task = TFS.Task;

public class Program
{
    public static List<Task>Tasks = new List<Task>();
    
    private static List<Job>Jobs = new List<Job>();
    private static List<Station>Stations = new List<Station>();
    
    public static void Main(String[] args)
    {
    // Parsed workflow data
    (Dictionary<string, double>, Dictionary<string, List<List<KeyValuePair<string, double>>>>,
        Dictionary<string, Dictionary<string, string>>) parsedFlow = (null!, null!, null!);


    // Parsed job file data
    Dictionary<string, Dictionary<string, string>> parsedJob = null!; 
    
    
    IFinder<SolutionFinder> finder = new SolutionFinder();
    FileDataParser parser = null!;
    
    // Workflow file 
    while (true)
    {   
        try
        {
            parser = new WorkFlowDataParser(finder);
            // Take user input to find correct file name 
            Console.WriteLine("Please enter your workflow file name with its extension...");
            var input = Console.ReadLine();
            parser.AssignFile(input.Split(".").ToList()[0],input.Split(".").ToList()[1]);
            //parser.AssignFile("workflow","txt"); // For test ...
            parsedFlow = (
                            (Dictionary<string, double>,
                                Dictionary<string, List<List<KeyValuePair<string, double>>>>,
                                        Dictionary<string, Dictionary<string, string>>)
                          )parser.Parse();
            break;
        }
        catch(Exception e)
        {
            //Console.WriteLine(e.Message);
            Console.WriteLine("There is no such file or directory ! Please try again ...");
            continue;
        }
    }
    // Printing Adjusted Data
    parser.PrintResults();
    Console.WriteLine();
    
    
    // Job file
    while (true)
    {
        try
        {
            parser = new JobFileDataParser(finder);
            // Take user input to find correct file name 
            Console.WriteLine("Please enter your job file name with its extension...");
            var input = Console.ReadLine();
            parser.AssignFile(input.Split(".").ToList()[0],input.Split(".").ToList()[1]);
            //parser.AssignFile("jobfile","txt"); // For test...
            parsedJob = (Dictionary<string, Dictionary<string, string>>)parser.Parse();
            break;
        }
        catch(Exception e)
        {
            //Console.WriteLine(e);
            Console.WriteLine("There is no such file or directory ! Please try again ...");
            continue;
        }
    }
    // Printing Adjusted Data
    parser.PrintResults();

    
    // All station info we need to create Job instances
    var stations = parsedFlow.Item3;
    
    // All job info we need to create Job instances
    var jobs = parsedJob;
    var jobTypes = parsedFlow.Item2;

    
    // Creators
    ICreator<Station> stationCreator = new SCreator();
    ICreator<Job> jobCreator = new JCreator(jobTypes);

    // Creating job instances
    foreach (var job in jobs)
    {
        Job j = jobCreator.CreateEntity(job);
        // For each created job, we collect all possible tasks, then put them into a Global Task list to use it later.
        foreach (var option in j.GetJobType().TaskSequence)
        {
            option.Value.ForEach(task =>
            {
                if(!Tasks.Contains(task))
                    Tasks.Add(task);
            });
        }
        if(!j.Equals(null)) Jobs.Add(j);
    }
    
    // Creating station instances 
    foreach (var station in stations)
    {
        Station s = stationCreator.CreateEntity(station);
        if(!s.Equals(null))Stations.Add(s);
    }
    
    Console.WriteLine();
    
    EventQueue eventQueue = new EventQueue(Stations,Jobs);
    eventQueue.Start();
    
    
}
    
}
