// import the Explain namespace

using System.Collections;
using System.Globalization;
using CsvHelper;
using Explain;
using Explain.Helpers;

// load the model definition json file as a json string
var modelDefinition = LoadModelDefinition();

// instantiate a new model instance using the json string model definition
var explain = new Model(modelDefinition);

// calculate 10 seconds if the model initializes properly.
if (explain.Initialized)
{
    // add parameter to watch list
    // explain.DataCollector.AddToWatchList("ALL", "Pres");
    // explain.DataCollector.AddToWatchList("CHEST_L", "Pres");
    // explain.DataCollector.AddToWatchList("Breathing", "TidalVolume");
    explain.DataCollector.AddToWatchList("Breathing", "TidalVolume");
    // explain.DataCollector.AddToWatchList("Breathing", "RespRate");
    // explain.DataCollector.AddToWatchList("DS", "Vol");
    // explain.DataCollector.AddToWatchList("OUT", "Pres");

    // set the data collector interval
    explain.DataCollector.SetInterval(0.0005);
    
    // calculate 30 seconds
    var modelOutput = explain.Calculate(10);
    
    // print the data
    Model.PrintData(modelOutput.ModelData);
    
    // print the performance
    Model.PrintPerformance(modelOutput.Perf);
    
    // save the data
    SaveToCsv(modelOutput.ModelData);
}

// function save model output to a csv file.
void SaveToCsv(List<DataEntry> modelData)
{
    var myCsvData = new List<CsvData>();
    
    foreach (var dataline in modelData)
    {
        var d = new CsvData { Time = dataline.Time, Name = dataline.Name, Value = (double) dataline.Value };
        myCsvData.Add(d);
    }


    using var writer = new StreamWriter("data.csv");
    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
    csv.WriteRecords((IEnumerable)myCsvData);
}

// function to load a model definition json file from disk
string LoadModelDefinition(string filename = "ModelDefinitions/NormalNeonate.json")
{
    // parse the json file into the model container class
    using var r = new StreamReader(filename);
    
    // read the file and return
    return r.ReadToEnd();

}

// define a dataclass for savinf data
public class CsvData
{
    public double Time { get; set; }
    public string Name { get; set; } = "";
    public double Value { get; set; }
}
