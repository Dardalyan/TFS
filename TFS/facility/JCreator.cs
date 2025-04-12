namespace TFS.facility;

public class JCreator : ICreator<Job>
{
    private  Dictionary<string, List<List<KeyValuePair<string, double>>>> JobTypeInfo { set; get;} 

    public JCreator( Dictionary<string, List<List<KeyValuePair<string, double>>>>jobTypeInfo)
    {
        JobTypeInfo = jobTypeInfo;
    }

    public Job CreateEntity(object information)
    {

        KeyValuePair<string, Dictionary<string, string>> job;
        
        try
        {
             job = (KeyValuePair<string, Dictionary<string, string>>)information;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null!;
        }
        
        JobType jt = new JobType(JobTypeInfo.Keys.ToList().FirstOrDefault(x => x == job.Value["JobType"])!);
        
        
        
        int optCount = 0;
        foreach (var options in JobTypeInfo[jt.GetJobTypeID()])
        {
            jt.TaskSequence.Add(optCount, new List<Task>());
            options.ForEach(task => { jt.TaskSequence[optCount].Add(new Task(task.Value, new TaskType(task.Key))); });
            optCount++;
        }

        Job j = new Job(job.Key, Convert.ToInt32(job.Value["StartTime"]),
            Convert
                .ToInt32(job.Value["Duration"]), jt);
        
        return j;    
    }
    
    
}