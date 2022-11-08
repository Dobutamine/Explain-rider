using CodexMicroORM.Core.Helper;
using Explain.Helpers;

namespace Explain.CoreModels;

public class Sensor: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public string InputModel { get; set; } = "";
    public string InputProperty { get; set; } = "";
    public double SetPoint { get; set; }
    public double Sensitivity { get; set; }
    public double TimeConstant { get; set; }
    public double Activity { get; set; }
    public double SensorOutput { get; set; }
    public double UpdateInterval { get; set; }
    
    private Model? _model;
    private bool _initialized;
    private double _t;

    private ICoreModel? _inputModel;

    private double _updateTimer;
    
    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;

        // get the modeling stepsize for easy referencing
        _t = _model.ModelingStepsize;
        
        // store a reference to the sensor input model
        foreach (var comp in _model.Components )
        {
            // find the correct component 
            _inputModel = _model.Components.Find(i => i.Name == InputModel)!;
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
        if (_updateTimer > UpdateInterval)
        {
            // reset the update timer
            _updateTimer = 0;
            
            // get the input signal
            var sensorValue = (double)_inputModel.FastGetValue(InputProperty);
            
            // calculate the sensor activity
            Activity = (100.0 / (1 + Math.Pow(Math.E, (sensorValue - SetPoint) * -Sensitivity)));
            
            // calculate the sensor output
            SensorOutput = UpdateInterval * ((1 / TimeConstant) * (-SensorOutput + Activity)) + SensorOutput;
        }

        _updateTimer += _t;

    }
}