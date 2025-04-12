namespace TFS;

public class TaskType
{
    private string ID { get; set; }

    public TaskType(string id)
    {
        ID = id;
    }
    
    public string GetTaskTypeID()
    {
        return ID;
    }

    public void SetTaskTypeID(string id)
    {
        ID = id;
    }
}