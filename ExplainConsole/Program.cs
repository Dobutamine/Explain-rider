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
    // add parameter to watch list
    explain.DataCollector.AddToWatchList("MOUTH", "Po2");
    explain.DataCollector.AddToWatchList("DS", "Po2");
    explain.DataCollector.AddToWatchList("ALL", "Po2");
    //
    // explain.DataCollector.AddToWatchList("MOUTH", "Pres");
    // explain.DataCollector.AddToWatchList("DS", "Pres");
    // explain.DataCollector.AddToWatchList("ALL", "Pres");
    //
    // explain.DataCollector.AddToWatchList("ALL", "Pres");
    // explain.DataCollector.AddToWatchList("ALR", "Pres");
    //
    // explain.DataCollector.AddToWatchList("MOUTH", "Ph2O");
    // explain.DataCollector.AddToWatchList("DS", "Ph2O");
    // explain.DataCollector.AddToWatchList("ALL", "Ph2O");
    //
    // explain.DataCollector.AddToWatchList("MOUTH", "Temp");
    // explain.DataCollector.AddToWatchList("DS", "Temp");
    // explain.DataCollector.AddToWatchList("ALL", "Temp");
    


    // set the data collector interval
    explain.DataCollector.SetInterval(0.005);
    
    var prerun = explain.Calculate(5);
    
    // calculate 30 seconds
    var modelOutput = explain.Calculate(60);
    
    // print the data
    // Model.PrintData(modelOutput.ModelData);
    
    // print the performance
    Model.PrintPerformance(modelOutput.Perf);
    
    // save the data
    SaveToCsv(modelOutput.ModelData);
}

// function save model output to a csv file.
void SaveToCsv(List<DataEntry> modelData)
{
    // process the model data for conversion to csv
    var groupedPropList = modelData
        .GroupBy(u => u.Name)
        .Select(grp => grp.ToList())
        .ToList();

    var noOfGroups = groupedPropList.Count;

    DataTable dt = new DataTable("test");
    dt.Columns.Add("Time", typeof(double));
    for (int i = 0; i < noOfGroups; i++)
    {
        dt.Columns.Add(groupedPropList[i][0].Name, typeof(double));
    }
    
    var counter = 0;
    foreach (var dataline in groupedPropList[0])
    {
        switch (noOfGroups)
        {
            case 1:
                dt.Rows.Add(dataline.Time, dataline.Value);
                break;
            case 2:
                dt.Rows.Add(dataline.Time, dataline.Value, groupedPropList[1][counter].Value);
                break;
            case 3:
                dt.Rows.Add(dataline.Time, dataline.Value, groupedPropList[1][counter].Value, groupedPropList[2][counter].Value);
                break;
            case 4:
                dt.Rows.Add(dataline.Time, dataline.Value, groupedPropList[1][counter].Value, groupedPropList[2][counter].Value, groupedPropList[3][counter].Value);
                break;
            case >4:
                dt.Rows.Add(dataline.Time, dataline.Value, groupedPropList[1][counter].Value, groupedPropList[2][counter].Value, groupedPropList[3][counter].Value, groupedPropList[4][counter].Value);
                break;
            
        }
        
        counter += 1;
    }

    
    using (var textWriter = File.CreateText("data.csv"))
    using (var csvTest = new CsvWriter(textWriter, CultureInfo.InvariantCulture))
    {
        // Write columns
        foreach (DataColumn column in dt.Columns)
        {
            csvTest.WriteField(column.ColumnName);
        }
        csvTest.NextRecord();
 
        // Write row values
        foreach (DataRow row in dt.Rows)
        {
            for (var i = 0; i < dt.Columns.Count; i++)
            {
                csvTest.WriteField(row[i]);
            }
            csvTest.NextRecord();
        }
    }
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
public class CsvData2
{
    public double Time { get; set; }
    public string Name1 { get; set; } = "";
    public double Value1 { get; set; }
    public string Name2 { get; set; } = "";
    public double Value2 { get; set; }
}
public class CsvData3
{
    public double Time { get; set; }
    public string Name1 { get; set; } = "";
    public double Value1 { get; set; }
    public string Name2 { get; set; } = "";
    public double Value2 { get; set; }
    public string Name3 { get; set; } = "";
    public double Value3 { get; set; }
}
public class CsvData4
{
    public double Time { get; set; }
    public string Name1 { get; set; } = "";
    public double Value1 { get; set; }
    public string Name2 { get; set; } = "";
    public double Value2 { get; set; }
    public string Name3 { get; set; } = "";
    public double Value3 { get; set; }
    public string Name4 { get; set; } = "";
    public double Value4 { get; set; }
}
public class CsvData5
{
    public double Time { get; set; }
    public string Name1 { get; set; } = "";
    public double Value1 { get; set; }
    public string Name2 { get; set; } = "";
    public double Value2 { get; set; }
    public string Name3 { get; set; } = "";
    public double Value3 { get; set; }
    public string Name4 { get; set; } = "";
    public double Value4 { get; set; }
    public string Name5 { get; set; } = "";
    public double Value5 { get; set; }
}



