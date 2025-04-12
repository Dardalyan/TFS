namespace TFS.facility;

public class SCreator : ICreator<Station>
{
    public Station CreateEntity(object information)
    {

        KeyValuePair<string, Dictionary<string, string>> station;
        try
        {
            station = (KeyValuePair<string, Dictionary<string, string>>)information;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null!;
        }
        
        
        Station s = new Station(station.Key,Convert.ToInt32(station.Value["max_capacity"]),
            Convert.ToChar(station.Value["MULTIFLAG"]),Convert.ToChar(station.Value["FIFOFLAG"]),
            (stationInstance) =>
            {
                foreach (var stationData in station.Value)
                {
                    if (!stationData.Key.Equals("max_capacity") &&
                        !stationData.Key.Equals("MULTIFLAG") &&
                        !stationData.Key.Equals("FIFOFLAG"))
                    {
                        var speed = stationData.Value; // example->  "3 ±0.20"
                        double speedInt;
                        if (speed.Contains("\u00B1"))
                        {
                            speed = speed.Replace("\u00B1", ""); //-> 3 0.20
                            speedInt = new Random().Next(0, 2) == 0
                                ? Convert.ToDouble(speed.Split(" ").ToList()[0]) * (100+Convert.ToDouble(speed.Split(" ").ToList()[1]))/100
                                : Convert.ToDouble(speed.Split(" ").ToList()[0]) * (100-Convert.ToDouble(speed.Split(" ").ToList()[1]))/100;
                        }
                        else
                        {
                            speedInt = Convert.ToInt32(speed);
                        }
                        // Save speed as double ...
                        Program.Tasks.FindAll(task => task.GetTaskType().GetTaskTypeID() == stationData.Key).ForEach(
                            task =>
                            {
                                stationInstance.ExecutableTaskInfo.Add(
                                    task, 
                                    new KeyValuePair<string, double>("speed", speedInt)
                                );
                            }); 
                    }
                }
            });
       

        return s;
    }
}

