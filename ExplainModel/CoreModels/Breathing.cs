using Explain.Helpers;

namespace Explain.CoreModels;

public class Breathing: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public bool BreathingEnabled { get; set; }
    public double RespRate { get; set; }
    public double RefMinuteVolume { get; set; }
    public double RefTidalVolume { get; set; }
    public double TargetMinuteVolume { get; set; }
    public double TargetTidalVolume { get; set; }
    public double VtRrRatio { get; set; }
    public double IeRatio { get; set; }
    public double RespMusclePressure { get; set; }
    public double RmpGain { get; set; } = 1;
    public double RmpTimeConstant { get; set; } = 1;
    public string[]? Comps { get; set; }

    private List<ICompliance> Compliances { get; set; } = new List<ICompliance>();
    
    private Model? _model;
    private bool _initialized;
    private double _ti = 0;
    private double _te = 0;

    private double _breathInterval = 0;
    private double _breathTimer = 0;
    private bool _inspRunning = true;
    private double _inspTimer = 0;
    private bool _expRunning = false;
    private double _expTimer = 0;
    private double _nccInsp  = -1;
    private double _nccExp  = -1;
    public double TidalVolume { get; set; } = 0;

    private static readonly double EMin4 = Math.Pow(Math.E, -4);

    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // find the compliances and store references to them in the list
        if (Comps != null)
        {
            foreach (var comp in Comps)
            {
                var c = (ICompliance)_model.Components.Find(i => i.Name == comp)!;
                if (c != null)
                {
                    Compliances.Add(c);
                }
            }
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
        // calculate the respiratory rate and target tidal volume from the target minute volume
        VtRrController();
        
        // calculate the inspiratory and expiratory time
        if (RespRate > 0)
        {
            _breathInterval = 60.0 / RespRate;
            _ti = IeRatio * _breathInterval;   // in seconds
            _te = _breathInterval - _ti;       // in seconds
        }
    
        // check whether it is time to start a breath
        if (_breathTimer > _breathInterval)
        {
            
            // reset the breath timer
            _breathTimer = 0;
            
            // start an inspiration
            _inspRunning = true;
            _inspTimer = 0;
            
            // reset the activation curve counter for the inspiration
            _nccInsp = 0;
        }

        // has the inspiration time elapsed?
        if (_inspTimer >= _ti)
        {
            // reset the inspiration timer
            _inspTimer = 0; 
            
            // signal that the inspiration has stopped
            _inspRunning = false;
            
            // signal that the expiration has started
            _expRunning = true;
            
            // reset the activation curve counter for the expiration
            _nccExp = 0;
        }

        // has the expiration time elapsed?
        if (_expTimer >= _te)
        {
            // reset the expiration timer
            _expTimer = 0;
            
            // signal that the expiration has stopped
            _expRunning = false;
        }
        
        // increase the breath
        _breathTimer += _model.ModelingStepsize;
        
        // increase the inspiration timer if it is running
        if (_inspRunning)
        {
            _inspTimer += _model.ModelingStepsize;
            _nccInsp += 1;
        }

        // increase the expiration timer if it is running
        if (_expRunning)
        {
            _expTimer += _model.ModelingStepsize;
            _nccExp += 1;
        }
        
        // calculate the respiratory muscle pressure
        CalcRespMusclePressure();
        
        // transfer the pressures to the targets
        var tv = 0D;
        foreach (var comp in Compliances)
        {
            // apply the respiratory muscle pressures on the compliances
            comp.PresExt += (-RespMusclePressure);
            
            // calculate the tidal volume (sum of all volume changes)
            tv += (comp.VolMax - comp.VolMin);
        }
        // store the tidal volume
        TidalVolume = tv;
    }

    private void CalcRespMusclePressure()
    {
        // reset respiratory muscle pressure
        RespMusclePressure = 0;
        
        // store the stepsize for easy reference
        var t = _model.ModelingStepsize;
        
        // inspiration
        if (_inspRunning)
        {
            RespMusclePressure = (_nccInsp / (_ti / t)) * RmpGain;
        }
        
        // expiration
        if (_expRunning)
        {
            RespMusclePressure = ((Math.Pow(Math.E, -4 * (_nccExp / (_te / t))) - EMin4) / (1 - EMin4)) * RmpGain;
        }
        
    }
    private void VtRrController() {
        // calculate the spontaneous resp rate depending on the target minute volume (from ANS) and the set vt-rr ratio
        RespRate = Math.Sqrt(TargetMinuteVolume / VtRrRatio);

        // calculate the target tidal volume depending on the target resp rate and target minute volume (from ANS)
        if (RespRate > 0)
        {
            TargetTidalVolume = TargetMinuteVolume / RespRate;
        }
        
    }
    
}