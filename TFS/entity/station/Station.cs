using TFS.entities;
namespace TFS;


public class Station :Entity 
{
    private int Capacity { get; }
    private char FIFOFLAG { get; }
    private char MULTIFLAG { get; }

    private  Queue<Task>AcceptedTasks { set; get; }
    private  Queue<Task> TasksOnWaiting { set; get; }
    
    private  Dictionary<Task,double> TaskEndTime  = new Dictionary<Task,double>();
    private  Dictionary<Task,double> TaskStartTime  = new Dictionary<Task,double>();

    private Dictionary<Task, KeyValuePair<string, double>> _executableTaskInfo; // {instance:{"speed":"3"}}
    public Dictionary<Task,KeyValuePair<string,double>> ExecutableTaskInfo
    {
        get { return _executableTaskInfo;} set{ _executableTaskInfo=value;} 
    }
    
    //!!!
    public List<Task> ExecutableTasks { set; get; }
    public List<double> ExecutionSpeeds { set; get; } 
    

    public Station(string stationID, int capacity, char multiflag , char fifoflag, Action<Station>assignExecTinfo):base(stationID)
    {
        Capacity = capacity;
        FIFOFLAG = fifoflag == 'Y' ? 'Y' : 'N';
        MULTIFLAG = multiflag == 'Y' ? 'Y' : 'N';
        _executableTaskInfo = new Dictionary<Task, KeyValuePair<string, double>>();
        assignExecTinfo(this);

        AcceptedTasks = new Queue<Task>();
        TasksOnWaiting = new Queue<Task>();
        
        
        ExecutableTasks = new List<Task>();
        ExecutionSpeeds = new List<double>();
        
        _executableTaskInfo.Keys.ToList().ForEach(task =>
        {
            ExecutableTasks.Add(task);
        });

        foreach (var info in _executableTaskInfo)
        {
            double speed = info.Value.Value;
            //Console.WriteLine($"{info.Key.GetTaskType().GetTaskTypeID()} with {speed} in {ID} .");
            ExecutionSpeeds.Add(speed);
        }
        
        
        
    }
    
    
    private bool IsStationFull()
    {
        return Capacity == AcceptedTasks.Count;
    }

    public int GetCapacity()
    {
        return Capacity;
    }
    
    public char GetFifoflag(){return FIFOFLAG;}
    public char GetMultiflag(){return MULTIFLAG;}


    public void AcceptTask(Task task)
    {
        // If the given conditions are met, tasks will either be accepted or placed into a waiting queue.
        if (!task.CheckFinished())
        {
            if (!IsStationFull() && ExecutableTasks.Contains(task))
            {
                if (!AcceptedTasks.Contains(task))
                {
                    AcceptedTasks.Enqueue(task);
                    Console.WriteLine($"{task.GetTaskType().GetTaskTypeID()} accepted by {ID}.");
                }
            }
            else if(!IsStationFull() && !ExecutableTasks.Contains(task) && MULTIFLAG == 'Y')
            {
                if (!AcceptedTasks.Contains(task))
                {
                    AcceptedTasks.Enqueue(task);
                    Console.WriteLine($"{task.GetTaskType().GetTaskTypeID()} accepted by {ID}.");
                }
            
            }
            else if (IsStationFull() && ExecutableTasks.Contains(task))
            {
                if (!TasksOnWaiting.Contains(task) && !AcceptedTasks.Contains(task))
                {
                    TasksOnWaiting.Enqueue(task);
                    Console.WriteLine($"{task.GetTaskType().GetTaskTypeID()} is waiting in {ID}.");
                }
            }
            else if (IsStationFull() && !ExecutableTasks.Contains(task)  && MULTIFLAG == 'Y')
            {
                if (!TasksOnWaiting.Contains(task) && !AcceptedTasks.Contains(task))
                {
                    TasksOnWaiting.Enqueue(task);
                    Console.WriteLine($"{task.GetTaskType().GetTaskTypeID()} is waiting in {ID}.");
                }
            }
        }
    }
   
    //Execute a task and calculate the finish time
    public void Execute(Task task, int time)
    {
            int index = ExecutableTasks.IndexOf(task);
        
        
            double executionDuration = (task.GetSize() / ExecutionSpeeds[index]);
        
            if(!TaskEndTime.ContainsKey(task))
                TaskEndTime.Add(task,time+Convert.ToDouble(executionDuration));
        
            if(!TaskStartTime.ContainsKey(task))
                TaskStartTime.Add(task,time);
            
            string status = Capacity == AcceptedTasks.Count ? "FULL" : "";
            Console.WriteLine($"{ID} current capacity is ({AcceptedTasks.Count}/{Capacity}) {status}");
            
            Console.WriteLine($"Duration of {task.GetTaskType().GetTaskTypeID()} in {ID} is {executionDuration} seconds.");
        
            Console.WriteLine($"{ID}'s Execution speed for {task.GetTaskType().GetTaskTypeID()}: "+ ExecutionSpeeds[index] + " second.");
        
            // Logging some information about the station and the process...
            Console.WriteLine("CurrentTime:"+Time.GetCurrentTime());
            Console.WriteLine("TaskStartTime:"+TaskStartTime[task]);
            Console.WriteLine("TaskEndTime:" + TaskEndTime[task]);
            Console.WriteLine("ExecutionDuration:"+executionDuration+" second.");
        
            // If the time has come for the task, then remove it from the queue.
            if (TaskEndTime[task] <= Convert.ToDouble(Time.GetCurrentTime()))
            {
                AcceptedTasks.Dequeue().FinishTask();
                Console.WriteLine($"The execution of  {task.GetTaskType().GetTaskTypeID()} is finished.");

                if (TasksOnWaiting.Count != 0)
                {
                    TasksOnWaiting.Dequeue();
                }
                
            }
        Console.WriteLine("------------------------------------------------------------------------------");

        
    }
    
}