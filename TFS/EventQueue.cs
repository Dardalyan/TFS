
using System.Collections;

namespace TFS;

public class EventQueue
{
    private List<Station> Stations;
    private List<int> StartTimes;
    private List<Job> Jobs;
    private List<Job> JobsInProcess = new List<Job>();
    private List<Job> FinishedJobs = new List<Job>();

    private Time Time = new Time();


    public EventQueue(List<Station> stations, List<Job> jobs)
    {
        Stations = stations;
        Jobs = jobs;
        StartTimes = new List<int>();

        Jobs.ForEach(job => { StartTimes!.Add(job.GetStartTime()); });
    }

    public void Start()
    {
        while (true)
        {
            Time.Tick();

            // Get Current job  with a start time equals to the current time 
            var candidateJob = Jobs.FindAll(j => j.GetStartTime() == Time.GetCurrentTime()).FirstOrDefault()!;
            if (!JobsInProcess.Contains(candidateJob) && candidateJob != null! && !candidateJob.IsJobFinished() && !FinishedJobs.Contains(candidateJob))
                JobsInProcess.Add(Jobs.Find(j => j.GetStartTime() >= Time.GetCurrentTime())!);
            
            // We select executable task
            Station station = null!;
            IEnumerator<Job> jEnumerator = JobsInProcess.GetEnumerator();

            List<Station> stationsInProcess = new List<Station>();

            while (jEnumerator.MoveNext())
            {
                Job j = jEnumerator.Current;
                IEnumerator<Station> sEnumerator = Stations.GetEnumerator();
                
                while (sEnumerator.MoveNext())
                {
                    var s = sEnumerator.Current;
                    
                    // check if job is finished or not then if it is , added it to finished job
                    Task nextTask = j.GetNextTask();
                    if (nextTask == null! && !FinishedJobs.Contains(j))
                    {  
                        FinishedJobs.Add(j);
                    }
                    // add station for the next task in current job
                    if (s.ExecutableTasks.Contains(nextTask))
                    {
                        stationsInProcess.Add(s);
                        break;
                    }
                }
            }
            
            // If job is finished, then put that job into finished jobs
            FinishedJobs.ForEach(j =>
            {
                JobsInProcess.Remove(j);
            });
            

            if (JobsInProcess.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("All jobs are finished !");
                break;
            }

            for (int i = 0; i < JobsInProcess.Count; i++)
            { 
                Event.Work(JobsInProcess[i], stationsInProcess[i],Time.GetCurrentTime());
            }
            
        }
    }
}
