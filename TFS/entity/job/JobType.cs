namespace TFS;

public class JobType
{
    private string JobTypeID { get; }
    public Dictionary<int,List<Task>> TaskSequence { get; set; }

    public JobType(string jobTypeId)
    {
        JobTypeID = jobTypeId;
        TaskSequence = new Dictionary<int, List<Task>>();
    }
    
    public string GetJobTypeID()
    {
        return JobTypeID;
    }
}