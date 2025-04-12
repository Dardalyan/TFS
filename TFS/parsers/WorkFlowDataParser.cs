using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using TFS.util;

namespace TFS;

public class WorkFlowDataParser : FileDataParser
{
    
        private 
        Tuple<Dictionary<string, double>, 
        Dictionary<string, List<List<KeyValuePair<string, double>>>>, 
        Dictionary<string, Dictionary<string, string>>> 
        ParseData { get; set; }
        
        
    public WorkFlowDataParser(IFinder<SolutionFinder> finder) : base(finder)
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

    public override void PrintResults(){
        
        if(ParseData == null)
            throw new Exception("Data has not been parsed !");
        
        
        var taskTypes = ParseData.Item1;
        var jobTypes = ParseData.Item2;
        var stations = ParseData.Item3;
        
        
        //PRINTING OUT TASKTYPES
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nWorkflow file content...");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("\nTASKTYPES:");
        foreach (var taskType in taskTypes)
        {
            Console.Write($"(ID: {taskType.Key} size: ");
            Console.Write(taskType.Value == 0 ? "not declared  " : taskType.Value +"), ");
        }
        Console.WriteLine();
        
        //PRINTING OUT JOBTYPES
        Console.WriteLine("\nJOBTYPES:");
        foreach (var pair in jobTypes)
        {
            string jobTypeID = pair.Key;
            foreach (var optionList in pair.Value)
            {
                Console.Write($"JobTypeID: {pair.Key} ");
                // SHOWING THE RESULTS OF JOBTYPES
                Console.Write($"Task Sequence -> [");                
                foreach (var keyValuePair in optionList)
                    Console.Write($"Tasktype {keyValuePair.Key} with Size {keyValuePair.Value}, ");
                Console.WriteLine("]");
            }
        }
        
        //PRINTING OUT STATIONS
        Console.WriteLine("\nSTATIONS:");
        foreach (var pair in stations)
        {
            string stationID = pair.Key;
            Console.Write($"ID: {stationID} ");
            foreach (var info in pair.Value)
            {
                Console.Write($"({info.Key}:{info.Value}), ");
            }
            Console.WriteLine();
        }
        Console.ResetColor();

    }

    public override object Parse()
    {
        Dictionary<int, string> lineByLine = ReadLinesInFiles();

        Dictionary<string, double> taskTypes = new Dictionary<string, double>();
        Dictionary<string,List<List<KeyValuePair<string,double>>>> jobTypes = new Dictionary<string,List<List<KeyValuePair<string,double>>>>();
        Dictionary<string,Dictionary<string,string>> stations = new Dictionary<string,Dictionary<string,string>>();

        //Line by line, this loop parses tasktypes, jobtypes and stations and put them into unique string lists.
        //If there is a problem in the file format, prints the warning.
        int lineNumber = 0;
        
        // for logging if there is some data which are declared before but never executed in any stations, later on 
        int line_number_for_beginning_of_station_data = 0; 
        
        string currentTitle = "";
        int t = -1;
        int s = -1;
        string tempy = "";
        
        // Temporary list to control number of occurance of the data in the original list. NOTE: in order to log just once !
        List<string> tempParsedLine = null!; 
        
        // To Control current jobID Ocucrence to identify the number of option to handle a job 
        Dictionary<string,int> currentJobIDOccurance = new Dictionary<string, int>();
        
        //Later, we'll use this list to check whether the tasktype is not in any station but declared in TaskTypes !
        List<string> taskTypesInStations = null; 
        
        foreach (var pairs in lineByLine)
        {
            lineNumber = pairs.Key;
            string LINE = pairs.Value;

            // If we have only '(' or ')' in line, we just pass through that 
            // If we have parsable data in the line, we handle parenthesis and whitespaces below...
            if (LINE.Equals("(") || LINE.Equals(")")) continue;

            if (LINE.Contains("TASKTYPES"))
            {
                tempParsedLine    = new List<string>();
                currentTitle = "TASKTYPES";
            }

            if (LINE.Contains("JOBTYPES"))
            {
                tempParsedLine    = new List<string>();
                currentTitle = "JOBTYPES";
            }

            if (LINE.Contains("STATIONS"))
            {
                if (line_number_for_beginning_of_station_data == 0)
                    line_number_for_beginning_of_station_data = lineNumber;
                
                tempParsedLine    = new List<string>();
                currentTitle = "STATIONS";
                taskTypesInStations = s <= -0?  taskTypes.Keys.ToList() :null!;
            }
            
            
            if (currentTitle == "TASKTYPES")
            {
                t++;
                // Parsing each data into a list
                List<string> parsedLine = HandleParenthesisAndWhiteSpaces(LINE).Split(" ").ToList();
                
                //parsedLine.ForEach(i=> Console.Write(i+" "));

                // Enumerator to move through the list item by item
                IEnumerator<string> enumerator = parsedLine.GetEnumerator();
                // To keep the previous data 
                string prevData = t>=1 ? tempParsedLine[tempParsedLine.Count - 1] : parsedLine[0] ;
                
                while (enumerator.MoveNext())
                {
                    tempParsedLine.Add(enumerator.Current);
                    // Pass the first one (it is the title or the class name like TASKTYPES or STATIONS)
                    if (enumerator.Current.Equals("TASKTYPES"))
                    {
                        continue;
                    }
                    
                    // Check the item whether the format of tasktypeID is correct or not
                    if (enumerator.Current.Contains('T') && !enumerator.Current[0].Equals('T'))
                    {
                        LogWarning($"Invalid tasktypeID: {enumerator.Current}",lineNumber);
                        
                        // Correct the invalid tasktypeID 
                        var charList = enumerator.Current.ToCharArray();
                        
                        // Adjusting the invalid tasktype ID
                        string adjustedID = "T";
                        foreach (var c in charList)
                        {
                            if (c != 'T')
                                adjustedID += c;
                        }
                        taskTypes.Add(adjustedID,0.0);
                        prevData = enumerator.Current;
                        continue;
                    }

                    // After the tasktypeID, if the current doesn't have 'T', then check whether the size number is unsigned or not.
                    if (!prevData.Equals("TASKTYPES") && prevData.Contains('T') && !enumerator.Current.Contains('T'))
                    {
                        bool isNegative = enumerator.Current.Contains('-') && enumerator.Current[0] == '-';
                        string unsignedSize = "";
                        if (isNegative) 
                        { 
                            LogWarning($"Invalid Task Size for Tasktype {prevData} with Size {enumerator.Current}",lineNumber);
                            // Adjusting the invalid size of the current task
                            unsignedSize = enumerator.Current.Remove(0,1);
                        }
                        else
                        {
                            unsignedSize = enumerator.Current;
                        }
                        // To convert decimal data by seperated '.' correctly into a double 
                        NumberFormatInfo provider = new NumberFormatInfo();
                        provider.CurrencyDecimalSeparator = ".";
                        double size = Convert.ToDouble(unsignedSize,provider); 
                        
                        // Check if that tasktypeID is defined in the dictionary, if it is, then update the value
                        if (!taskTypes.Keys.Contains(prevData))
                            taskTypes.Add(prevData, size);
                        else taskTypes[prevData] = size;
                        
                        prevData = enumerator.Current;
                        continue;
                    }

                    // Check whether the current data is listed before or not
                    if (tempParsedLine.Count(i=>i == enumerator.Current) > 1)
                    {
                        LogWarning($"The Tasktype {enumerator.Current} is listed more than one",lineNumber);
                        prevData = enumerator.Current;
                        continue;
                    }

                    if(enumerator.Current.Contains('T') )
                        taskTypes.Add(enumerator.Current,0.0);
                    
                    prevData = enumerator.Current;
                   

                }
                /*
                 Console.WriteLine("\nLog Of Adjusted TaskType Format");
                 
                // SHOWING THE RESULTS OF TASKTYPES
                foreach (var keyValuePair in taskTypes)
                {
                    Console.WriteLine(keyValuePair.Key +":"+ keyValuePair.Value);
                }
                */ 
                
            }
            else if (currentTitle == "JOBTYPES")
            {
                List<string> parsedLine = HandleParenthesisAndWhiteSpaces(LINE).Split(" ").ToList();
                
                IEnumerator<string> enumerator = parsedLine.GetEnumerator();
                
                // To keep the previous data 
                string prevData = tempParsedLine.Count == 0 ? parsedLine[0] : tempParsedLine[tempParsedLine.Count - 1]; ;
                
                
                string currentJobID = "";// This will reset in each occurance of jobTypeID 
                
                while (enumerator.MoveNext()  )
                {
                    if (enumerator.Current == "JOBTYPES") continue;
                    
                    // Adjusting JobTypeID if it has an invalid text format
                    if (enumerator.Current.Contains('J') && !enumerator.Current[0].Equals('J'))
                    {
                        LogWarning($"Invalid jobTypeID: {enumerator.Current}",lineNumber);
                        
                        // Correct the invalid jobTypeID 
                        var charList = enumerator.Current.ToCharArray();
                        
                        // Adjusting the invalid jobTypeID
                        string adjustedID = "J";
                        foreach (var c in charList)
                        {
                            if (c != 'J')
                                adjustedID += c;
                        }
                        prevData = enumerator.Current;
                        tempParsedLine.Add(enumerator.Current);
                        currentJobID = adjustedID;
                        // We check the occurence of each jobTypeID . EX: if J1 occures only once and J2 occurs 2 times then we hold {J1:1, J2:2} as a Dict.
                        try
                        {
                            currentJobIDOccurance.Add(adjustedID,0);
                        }
                        catch
                        {
                            currentJobIDOccurance[adjustedID]++;
                        }
                        //Here we are trying to put jobtypeID into dictionary, if the key is already exist, then it throws an error. 
                        try
                        {
                            jobTypes.Add(adjustedID, new List<List<KeyValuePair<string, double>>>());
                            jobTypes[adjustedID].Add(new List<KeyValuePair<string, double>>());
                        }
                        //We do not need to log error, only thing we are doing here adding another option for that jobtypeID
                        catch
                        {
                            LogWarning($"{enumerator.Current} already declared.",lineNumber);
                            jobTypes[adjustedID].Add(new List<KeyValuePair<string, double>>());
                        }
                        continue;
                    }
                    // If there is no problem in jobTypeID format
                    if (enumerator.Current.Contains('J') && enumerator.Current[0].Equals('J'))
                    {
                        // We check the occurence of each jobTypeID . EX: if J1 occures only once and J2 occurs 2 times then we hold {J1:0, J2:1} as a Dict.
                        try
                        {
                            currentJobIDOccurance.Add(enumerator.Current,0);
                        }
                        catch
                        {
                            currentJobIDOccurance[enumerator.Current]++;
                        } 
                        //Here we are trying to put jobtypeID into dictionary, if the key is already exist, then it throws an error.                        try
                        try
                        {
                            jobTypes.Add(enumerator.Current, new List<List<KeyValuePair<string, double>>>());
                            jobTypes[enumerator.Current].Add(new List<KeyValuePair<string, double>>());
                        }
                        //We do not need to catch error, only thing we are doing here adding another option for that jobtypeID
                        catch
                        {
                            LogWarning($"{enumerator.Current} already declared.",lineNumber);
                            jobTypes[enumerator.Current].Add(new List<KeyValuePair<string, double>>());
                        }

                        currentJobID = enumerator.Current;
                        prevData = enumerator.Current;
                        tempParsedLine.Add(enumerator.Current);

                        continue;
                    }
                    
                    // If taskTypeID doesn't exist in TaskTypes, we need to log the warning.
                    if (!taskTypes.Keys.Contains(prevData) && prevData.Contains('T'))
                    {
                        LogWarning($"The taskTypeID:{prevData} is not declared in TaskTypes",lineNumber);
                        prevData = enumerator.Current;
                        tempParsedLine.Add(enumerator.Current);

                        continue;
                    }

                    // If the current data is a taskTypeID, we need to add that id into sequence
                    if (enumerator.Current.Contains("T") && enumerator.Current[0].Equals('T'))
                    {
                        //Console.WriteLine($"Occurance of {currentJobID}:"+  currentJobIDOccurance[currentJobID]);
                        
                        jobTypes[currentJobID][currentJobIDOccurance[currentJobID]]
                            .Add(KeyValuePair.Create(enumerator.Current,taskTypes[enumerator.Current]));
                        
                    }
                    // if the current doesn't have 'T' and 'J', then check whether the size number is unsigned or not.
                    if (!prevData.Contains("J") && prevData.Contains('T') && !enumerator.Current.Contains('T'))
                    {
                        bool isNegative = enumerator.Current.Contains('-') && enumerator.Current[0] == '-';
                        string unsignedSize = "";
                        if (isNegative) 
                        { 
                            LogWarning($"Invalid Task Size for TaskID:{prevData} Size:{enumerator.Current}",lineNumber);
                            // Adjusting the invalid size of the current task
                            unsignedSize = enumerator.Current.Remove(0,1);
                        }
                        else
                        {
                            unsignedSize = enumerator.Current;
                        }
                        // To convert decimal data by seperated '.' correctly into a double 
                        NumberFormatInfo provider = new NumberFormatInfo();
                        provider.CurrencyDecimalSeparator = ".";
                        double size = Convert.ToDouble(unsignedSize,provider);
                        
                        // We are tyring to find in jobTypes(currentJobTypeID : [n'th option] [found index of pair.Key matched with taskTypeID])
                        // Then re-define that key-pair with {taskTypeID : size}
                        jobTypes[currentJobID][currentJobIDOccurance[currentJobID]]
                            [jobTypes[currentJobID][currentJobIDOccurance[currentJobID]]
                                .FindIndex(pair => pair.Key == prevData)] = KeyValuePair.Create(prevData,size);
                        
                        prevData = enumerator.Current;
                        tempParsedLine.Add(enumerator.Current);

                        continue;
                    }
                    
                    prevData = enumerator.Current;
                }
                
                // Adjusting tasktypes and jobtypes
                // Look each jobtypeID
                foreach (var pair in jobTypes)
                {   // We need to check each option of current jobTypeID.  example: J1 's 1st option [T1:1, T2:2, T3:3]
                    foreach (var optionList in pair.Value)
                    {   // Now we'll log and make it correct
                        optionList.RemoveAll(keyValuePair =>
                        {   // All pairs will be removed, if size is not declared in both taskTypes and jobTypes
                            var p =  keyValuePair.Value == 0;
                            if (p)
                            {
                                const double defaultSize = 1.0;
                                // If p is true, then we need to log 
                                LogWarning($"{keyValuePair.Key} has no default size, either a default size must be declared in TASKTYPE list or the size must be declared within the job.",lineNumber);
                                // We add default size = 1 to current tasktypeID in tasktypes
                                taskTypes[keyValuePair.Key] = defaultSize;
                                // Then removed data will be added again with a default size = 1
                                keyValuePair = KeyValuePair.Create(keyValuePair.Key, defaultSize);
                                optionList.Add(keyValuePair);
                            }
                            return p;
                        });
                        optionList.ForEach(keyValuePair =>
                        {
                            // If tasktype size is declared in jobtype but not declared in taskype, then we'll update that tasktype size in tasktypes !
                            // EXAMPLE T25 is not declared in tasktypes but declared in jobtypes as 5, 
                            if (taskTypes[keyValuePair.Key] == 0)
                                taskTypes[keyValuePair.Key] = keyValuePair.Value;
                        });
                    }
                }
                


            }
            else if (currentTitle == "STATIONS")
            {
                // Now like we did in the jobtypes we can move on 
                List<string> parsedLine = HandleParenthesisAndWhiteSpaces(LINE).Split(" ").ToList();
                
                IEnumerator<string> enumerator = parsedLine.GetEnumerator();
                
                
                // To keep the previous data 
                string prevData = tempParsedLine.Count == 0 ? "" : tempParsedLine[tempParsedLine.Count - 1];
                
                string currentStationID = "";// This will reset in each occurance of stationID 
                
                
                int i = 1;
                while (enumerator.MoveNext())
                {
                    if(enumerator.Current == "STATIONS") continue;

                    //Console.WriteLine("Current: "+ enumerator.Current);
                    
                    // Adjusting StationID if it has an invalid text format
                    if (enumerator.Current.Contains('S') && !enumerator.Current[0].Equals('S'))
                    {                       


                        LogWarning($"Invalid stationID: {enumerator.Current}",lineNumber);
                        
                        // Correct the invalid StationID 
                        var charList = enumerator.Current.ToCharArray();
                        
                        // Adjusting the invalid jobTypeID
                        string adjustedID = "S";
                        foreach (var c in charList)
                        {
                            if (c != 'S')
                                adjustedID += c;
                        }
                        prevData = enumerator.Current;
                        tempParsedLine.Add(enumerator.Current);
                        currentStationID = adjustedID;
                        
                        // When we find a station id, we need to start holding information
                        stations.Add(adjustedID,new Dictionary<string, string>());
                        i = 1;
                        continue;
                    }
                    
                    // If there is no problem in stationID format
                    if (enumerator.Current.Contains('S') && enumerator.Current[0].Equals('S'))
                    {
                        currentStationID = enumerator.Current;
                        prevData = enumerator.Current;
                        tempParsedLine.Add(enumerator.Current);
                        

                        // When we find a station id, we need to start holding information
                        stations.Add(currentStationID,new Dictionary<string, string>());
                        i = 1;
                        continue;
                    }

                    if (i == 1)
                    {
                        stations[currentStationID].Add("max_capacity",enumerator.Current);
                        prevData = enumerator.Current;
                        tempParsedLine.Add(enumerator.Current);
                        i++;
                        continue;
                    }
                    if (i == 2)
                    {
                        stations[currentStationID].Add("MULTIFLAG",enumerator.Current);
                        tempParsedLine.Add(enumerator.Current);
                        prevData = enumerator.Current;
                        i++;
                        continue;
                    }
                    if (i == 3)
                    {
                        stations[currentStationID].Add("FIFOFLAG",enumerator.Current);
                        tempParsedLine.Add(enumerator.Current);
                        prevData = enumerator.Current;
                        i++;
                        continue;
                    }
                    
                    if (enumerator.Current.Contains('T'))
                    {
                        if (taskTypes.ContainsKey(enumerator.Current))
                            taskTypesInStations?.Remove(enumerator.Current);
                        tempParsedLine.Add(enumerator.Current);
                        prevData = enumerator.Current;
                        continue;
                    }

                    if (!enumerator.Current.Contains('T') && prevData.Contains('T'))
                    {
                        stations[currentStationID].Add(prevData,enumerator.Current);
                        tempParsedLine.Add(enumerator.Current);
                        prevData = enumerator.Current;
                        continue;
                    }
                    
                    if (enumerator.Current != "" && !enumerator.Current.Contains('T') && !prevData.Contains('T'))
                    {
                        string plusMinusUnicode = "\u00B1";
                        int numberOfData = stations[currentStationID].Count;
                        int x = 1;
                        foreach (var data in stations[currentStationID])
                        {
                            if (x == numberOfData)
                            {
                                stations[currentStationID][data.Key] = $"{data.Value} {plusMinusUnicode}{enumerator.Current}";
                            }
                            x++;
                        }
                        tempParsedLine.Add(enumerator.Current);
                        prevData = enumerator.Current;
                        continue;
                    }
                    
                }
            }
            else
            {
                throw new Exception("\nError occured while parsing workflow data content.\nThe file might have been broken or some information might be missing !");
            }
        }
        
        // Handle tasktypes which are ezist in tasktypes but not executed in any station
        string notExistTaskTypes = "";
        string notExistButPartOfAJobTaskTypes = "";
        taskTypesInStations.ForEach(i =>
        {
            if (!jobTypes.Any(jobType =>
                    jobType.Value.Any(options=>
                        options.Any(taskType=>taskType.Key == i))))
            {
                notExistTaskTypes += $"{i}, ";
            }
            else
            {
                notExistButPartOfAJobTaskTypes += $"{i}, ";
            }
        });
        
        
        if (!notExistButPartOfAJobTaskTypes.Equals(""))
        {
            string plusMinusUnicode = "\u00B1";

            Random generator = new Random();
            int sizeOfstations = stations.Count;
            string newStationID = $"S{sizeOfstations+1}";

            stations.Add(newStationID,new Dictionary<string, string>());
            stations[newStationID].Add("max_capacity",generator.Next(1,4).ToString());
            stations[newStationID].Add("MULTIFLAG",generator.Next(0,2) == 1 ? "Y" : "N");
            stations[newStationID].Add("FIFOFLAG",generator.Next(0,2) == 1 ? "Y" : "N");

            var temp = notExistButPartOfAJobTaskTypes.Split(" ").ToList();
            temp.RemoveAll(i => !i.Contains('T'));
            temp.ForEach(taskType =>
            {
                taskType = taskType.Trim(',');
                stations[newStationID].Add(taskType,generator.Next(1,3).ToString());

                if (generator.Next(0, 2) == 1)
                {
                    var randPM = (Math.Floor(Math.Round(generator.NextDouble(), 2)*10)/10).ToString("0.00",CultureInfo.InvariantCulture); // To make 0,9 to 0.90 format
                    stations[newStationID][taskType] =
                        $"{stations[newStationID][taskType]} {plusMinusUnicode}{randPM}";
                }

            }); // speed
            
            
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"\nIn Lines {line_number_for_beginning_of_station_data}-{lineNumber} :in STATIONS");
        LogWarning($"{notExistTaskTypes} are not executed in any STATIONs even though they are listed as possible task types. This shall raise a warning.");
        LogWarning($"There are no STATIONs which execute {notExistButPartOfAJobTaskTypes}, however, both {notExistButPartOfAJobTaskTypes} are a part of some job type.");
        
        
        
        ParseData = new Tuple<Dictionary<string, double>, Dictionary<string,List<List<KeyValuePair<string,double>>>>, Dictionary<string,Dictionary<string,string>>>(taskTypes, jobTypes, stations);
        return (taskTypes,jobTypes,stations);
    }

}
