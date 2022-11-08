using Explain.Helpers;

namespace Explain.CoreModels;

public class SensorIntegrator: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public double TimeConstant { get; set; }
    public double Activity { get; set; }
    public double Output { get; set; }
    public double UpdateInterval { get; set; }
    
    
    public SensorEntry[] InputSensors = Array.Empty<SensorEntry>();
    
    private Model? _model;
    private bool _initialized;
    private List<SensorItem> _sensors = new List<SensorItem>();
    private double _updateTimer;
    private double _t;

    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // get the modeling stepsize for easy referencing
        _t = _model.ModelingStepsize;
        
        // find the sensors and put them into a list
        foreach (var sensorEntry in InputSensors)
        {
            // find a reference to the sensor
            var s = (Sensor)_model.Components.Find(i => i.Name == sensorEntry.Name)!;
            var newSensor = new SensorItem
            {
                Sensor = s,
                EffectMagnitude = sensorEntry.Magnitude
            };
            _sensors.Add(newSensor);
        }
        
        // signal that the model component is initialized
        _initialized = true;
    }

    public void StepModel()
    {
        if (IsEnabled && _initialized)
        {
            CalcModel();
        }
    }

    public void CalcModel()
    {
        // calculate the output signal
        if (_updateTimer > UpdateInterval)
        {
            // reset the update timer
            _updateTimer = 0;
            
            // calculate the output signal of the integrator
            var totalOutput = 0.0;
            var totalMagnitude = 0.0;
            foreach (var s in _sensors)
            {
                var outS = s.Sensor.Output * s.EffectMagnitude;
                totalOutput += outS;
                totalMagnitude += s.EffectMagnitude;
            }

            var activity = totalOutput / totalMagnitude;
            
            Output = UpdateInterval * ((1 / TimeConstant) * (-Output + activity)) + Output;
        }

        _updateTimer += _t;
    }
}

public class SensorEntry
{
    public string Name = "";
    public double Magnitude;
}
public class SensorItem
{
    public Sensor Sensor;
    public double EffectMagnitude;
}