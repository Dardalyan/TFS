namespace TFS;

public class Task
{
    private TaskType TaskType { get; }
    private double Size { get; }
    
    private bool isFinished = false;

    public Task(double size,TaskType taskType)
    {
        TaskType = taskType;
        Size = size;
    }

    public TaskType GetTaskType()
    {
        return TaskType;
    }

    public double GetSize()
    {
        return Size;
    }
    
    public bool CheckFinished()
    {
        return isFinished;
    }
    
    public void FinishTask()
    {
        isFinished = true;
    }
}