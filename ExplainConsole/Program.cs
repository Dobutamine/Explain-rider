// import the Explain namespace

using System.Collections;
using System.Data;
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

    //Console.WriteLine("Model stabilisation pre-run of 180 seconds.");
    explain.Calculate(180);
    
    explain.DataCollector.ClearData();
    
    // calculate 30 seconds
    explain.Calculate(3);
    
    // save the data
    SaveToCsv();
}

// function save model output to a csv file.
void SaveToCsv()
{
    using var writer = new StreamWriter("explainCirculationData.csv");
    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
    csv.WriteRecords(explain.DataCollector.GetCirculationData());
}

// function to load a model definition json file from disk
string LoadModelDefinition(string filename = "ModelDefinitions/NormalNeonate.json")
{
    // parse the json file into the model container class
    using var r = new StreamReader(filename);
    
    // read the file and return
    return r.ReadToEnd();

}



