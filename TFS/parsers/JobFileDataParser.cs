using System.Globalization;
using System.Text.RegularExpressions;
using TFS.util;

namespace TFS;

public class JobFileDataParser : FileDataParser
{
    private Dictionary<string, Dictionary<string, string>> ParseData;
    public JobFileDataParser(IFinder<SolutionFinder> finder) : base(finder)
    {
        
    }

    protected override Dictionary<int, string> ReadLinesInFiles()
    {
        string line;
        Dictionary<int, string> lineByLine = new Dictionary<int, string>();
        try
        {
            StreamReader sr;
            try
            {
                sr = new StreamReader(GetFilePath());
            }
            catch (Exception e)
            {
                throw new Exception($"{FILENAME} file cannot open");
            }

            line = sr.ReadLine()!;
            
            int lineNumber = 1;
            while (line != null!)
            {
                // removing white spaces...
                line = line.Trim();
                // removing Specşal Chars...
                line = RemoveSpecialCharacters(line);
                
                
                if (string.IsNullOrWhiteSpace(line))
                {
                    line = sr.ReadLine()!;
                    lineNumber++;
                    continue;
                }

                line = line.Trim();
                lineByLine.Add(lineNumber, line);
                
                line = sr.ReadLine()!;
                lineNumber++;
                
            }

        }
        catch (Exception e)
        {
            lineByLine = null!;
            Console.WriteLine(e.Message);
        }
        return lineByLine;    
    }

    public override void PrintResults()
    {
        if(ParseData == null)
            throw new Exception("Data has not been parsed !");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nJob file content...\n");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        foreach (var job in ParseData)
        {
            Console.WriteLine($"JobID: {job.Key} " +
                              $"JobType:{ParseData[job.Key]["JobType"]} " +
                              $"StartTime: {ParseData[job.Key]["StartTime"]} " +
                              $"Duration: {ParseData[job.Key]["Duration"]}");
        }    
        Console.ResetColor();
    }

    public override object Parse()
    {
    
        // jobs hold => {'Job1' : { {"jobType":"J1"}, {"startTime":1}, {"duration":"30"}  } }
        Dictionary<string,Dictionary<string,string>> jobs = new Dictionary<string,Dictionary<string,string>>();

        
        Dictionary<int, string> lineByLine = ReadLinesInFiles();

        var keys = lineByLine.Keys.ToList();
        foreach (var key in keys)
        {
            var value = lineByLine[key];
            value = value.Trim();
            value = Regex.Replace(value, @"\s{2,}", " ");

            lineByLine[key] = value;
        }

        int lineNum;
        
        foreach (var line in lineByLine)
        {
            lineNum = line.Key;
            
            string jobInfo = line.Value;
            List<string>parsedInfo = jobInfo.Split(" ").ToList();

            string jobID = null!; // current jobID
            string prevData = null;
            
            IEnumerator<string> enumerator = parsedInfo.GetEnumerator();

            while (enumerator.MoveNext())
            {

                if (jobID == null && enumerator.Current.ToLower()[0] != 'j')
                {
                    LogWarning($"Invalid JobID {enumerator.Current}",lineNum: lineNum);
                    
                    // Correct the invalid jobID 
                    var charList = enumerator.Current.ToLower().ToCharArray();
                        
                    // Adjusting the invalid jobID
                    string adjustedID = "Job";
                    foreach (var c in charList)
                    {
                        if (c != 'j' && c != 'o' && c != 'b')
                            adjustedID += c;
                    }
                    jobID = adjustedID;
                    jobs.Add(adjustedID,new Dictionary<string, string>());
                    prevData = adjustedID;
                    continue;
                }
                
                if (enumerator.Current.ToLower().Contains("job"))
                {
                    jobID = enumerator.Current;
                    try
                    {
                        // if JobID is listed more than one, then log and continue with other jobs
                        jobs.Add(enumerator.Current, new Dictionary<string, string>());
                    }
                    catch
                    {
                        LogWarning($"JobID:{jobID} is listed more than one !",lineNum: lineNum);
                        break;
                    }
                    prevData = enumerator.Current;
                    continue;
                }
                
                if (!enumerator.Current.ToLower().Contains("job") && enumerator.Current.ToLower().Contains("j"))
                {
                    if (enumerator.Current.ToLower()[0] != 'j')
                    {
                        LogWarning($"Invalid JobTypeID for {enumerator.Current}",lineNum: lineNum);
                        var charList = enumerator.Current.ToLower().ToCharArray();
                        
                        string adjustedID = "J";
                        foreach (var c in charList)
                        {
                            if (c != 'j')
                                adjustedID += c;
                        }
                        jobs[jobID].Add("JobType",adjustedID);
                        prevData = adjustedID;
                        continue;
                    }
                    
                    jobs[jobID].Add("JobType",enumerator.Current);
                    prevData = enumerator.Current;
                    continue;
                }
                
                
                if (prevData!.ToLower().Contains('j') && !enumerator.Current.ToLower().Contains("j"))
                {
                    if (Convert.ToInt32(enumerator.Current) < 0)
                        LogWarning($"Invalid start time {enumerator.Current} for {jobID}",lineNum: lineNum);
                    
                    jobs[jobID].Add("StartTime", Math.Abs(Convert.ToInt32(enumerator.Current)).ToString());
                    prevData = enumerator.Current;
                    continue;
                }
                
                if (!prevData.ToLower().Contains('j') && !enumerator.Current.ToLower().Contains("j"))
                {
                    if (Convert.ToInt32(enumerator.Current) < 0)
                        LogWarning($"Invalid duration {enumerator.Current} for {jobID}",lineNum: lineNum);
                    
                    jobs[jobID].Add("Duration", Math.Abs(Convert.ToInt32(enumerator.Current)).ToString());
                    prevData = enumerator.Current;
                    continue;
                }
                
            }
        }

        ParseData = jobs;
        return jobs; 
    }

}