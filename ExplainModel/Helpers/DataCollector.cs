using Explain.CoreModels;

namespace Explain.Helpers;
using CodexMicroORM.Core.Helper;

public class DataCollector
{
    private readonly Model _model;
    private readonly List<WatchItem> _watchItems = new List<WatchItem>();
    private readonly List<DataEntry> _modelData = new List<DataEntry>();
    private double _dcInterval = 0.015;
    private double _dcTimer = 0;
    
    public DataCollector(Model model)
    {
        // store a reference to the entire model
        _model = model;
    }

    public void Update()
    {
        if (_dcTimer > _dcInterval)
        {
            _dcTimer = 0;
            StoreData();
        }

        _dcTimer += _model.ModelingStepsize;
    }

    public void SetInterval(double newInterval)
    {
        _dcInterval = newInterval;
    }
    private void StoreData()
    {
        // iterate over de watchlist and store the data
        foreach(var item in _watchItems)
        {
            var dataEntry = new DataEntry
            {
                Time = _model.ModelTimeTotal,
                Name = item.Name,
                Value = item.InstanceObject.FastGetValue(item.Prop)!
            };
            // Console.WriteLine("{0}: {1} = {2}",dataEntry.Time, dataEntry.Name, dataEntry.Value);
            _modelData.Add(dataEntry);
        }
    }
    
    public List<DataEntry> GetModelData()
    {
        // clone the list
        var clonedList = new List<DataEntry>(_modelData);

        // clear the model data list
        _modelData.Clear();

        // return the list
        return clonedList;
    }
    
    public void AddToWatchList(string componentName, string componentProp)
    {
        // find the correct model component
        var comp = _model.Components.Find(i => i.Name == componentName)!;

        // find the type of this component
        var componentType = comp.GetType();

        // convert the type of the component from the ICoreModel to the appropiate type
        var result = Convert.ChangeType(comp, componentType);

        // construct a new watch item
        var watchItem = new WatchItem
        {
            Name = componentName + "." + componentProp,
            Prop = componentProp,
            InstanceObject = comp
        };
        
        // add the item to the watchlist
        _watchItems.Add(watchItem);
        
        // check whether a duplicate item is present in the list
        var duplicate = _watchItems.GroupBy(item => item).Any(@group => @group.Count() > 1);
    
        // if a duplicate is present remove the last one causing the duplicate
        if (duplicate)
        {
            RemoveFromWatchList(componentName, componentProp);
        }
    }
    
    public void RemoveFromWatchList(string componentName, string componentProp)
    {
        var item = _watchItems.Find(i => i.Name == (componentName + "." + componentProp))!;
        _watchItems.Remove(item);
    }
    
    
}

public struct WatchItem
{
    public string Name;
    public object InstanceObject;
    public string Prop;
}

public struct DataEntry
{
    public double Time { get; set; }
    public string Name { get; set; }
    public object Value { get; set; }
}