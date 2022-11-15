using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Explain;

using Explain.Helpers;

public class Model
{
    // model properties
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool Initialized { get; set; }
    public double Weight { get; private set; }
    public double ModelTimeTotal = 0;
    public readonly double ModelingStepsize = 0;

    // declare a list with all model components
    public readonly List<ICoreModel> Components = new List<ICoreModel>();

    // declare the model engine which actually runs the model
    private readonly ModelEngine _modelEngine;

    // declare some helpers
    public readonly DataCollector DataCollector;
    public readonly HardwareInterface HardwareInterface;
    public readonly DataInterface DataInterface;

    public Model(string modelDefinition)
    {
        // declare an error counter
        var errors = 0;

        // convert the string to a json object
        JObject json = JObject.Parse(modelDefinition);

        // set the global model properties from the json file
        Name = json["Name"]?.ToString() ?? string.Empty;
        Description = json["Description"]?.ToString() ?? string.Empty;
        Weight = (double)(json["Weight"] ?? 3.3);
        ModelTimeTotal = (double)(json["ModelTimeTotal"] ?? 0);
        ModelingStepsize = (double)(json["ModelingStepsize"] ?? 0.0005);

        // now start parsing this json objects component list
        foreach (var child in json["Components"]!)
        {
            // try to find out the model type
            if (child.First != null)
            {
                var classType = "Explain.CoreModels." + child.First["ModelType"]!;

                // find the model type class
                Type? componentType = Type.GetType(classType);

                // if the component type is found then instantiate it with the correct parameters
                if (componentType == null)
                {
                    errors += 1;
                    Console.WriteLine("Could not find model type {0}", classType);
                    continue;
                }

                // instantiate a new component with the correct parameters
                var newComponent = (ICoreModel)child.First.ToObject(componentType)!;

                // add a reference to the new component to the components list
                Components.Add(newComponent);
            }
        }

        // initialize all model components and give them a reference to the rest of the model
        foreach (ICoreModel model in Components)
        {
            model.InitModel(this);
        }

        // add a data collector
        DataCollector = new DataCollector(this);

        // add a hardware interface (e.g. Paul)
        HardwareInterface = new HardwareInterface(this);
        
        // add a mode interface for easy data extraction
        DataInterface = new DataInterface(this);

        // instantiate the model engine
        _modelEngine = new ModelEngine(this);

        if (errors == 0)
        {
            Initialized = true;
            Console.WriteLine("Explain model: '{0}' loaded.", Name);
        }
        else
        {
            Initialized = false;
            Console.WriteLine("Explain model: '{0}' failed to load! {1} errors found.", Name, errors);
        }
    }

    public void Start()
    {
        // start the model in realtime
        _modelEngine.Start();
    }

    public void Stop()
    {
        // stop the realtime model
        _modelEngine.Stop();
    }

    public List<DataEntry> GetModelData()
    {
        return DataCollector.GetModelData();
    }

    public ModelOutput Calculate(double timeToCalculate = 10)
    {
        // calculate a given period of time and get the performance data
        var perf = _modelEngine.Calculate(timeToCalculate);

        // get the model data
        var modelData = DataCollector.GetModelData();

        // combine the two data structures
        var modelOutput = new ModelOutput
        {
            Perf = perf,
            ModelData = modelData
        };

        // return the performance data
        return modelOutput;
    }

    public static void PrintData(List<DataEntry> data)
    {
        foreach (var dataItem in data)
        {
            Console.WriteLine("At {0, -8 } sec: {1, -25} = {2} ", Math.Round(dataItem.Time, 4), dataItem.Name, dataItem.Value);
        }
    }
    public static void PrintPerformance(Performance perf)
    {
        Console.WriteLine("Modeling of {0} seconds in {1} steps took {2} seconds with {3} ms per model step.", perf.TimeInterval, perf.NoSteps, (perf.CalcTimeTotal / 1000f),Math.Round(perf.StepTime, 5));
    }
}