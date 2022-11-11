using Explain.Helpers;

namespace Explain.CoreModels;

public class AutonomicNervousSystem: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public double UpdateInterval { get; set; } = 0.015;
    public string BaroreceptorLocation { get; set; } = "";
    public string ChemoreceptorLocation { get; set; } = "";
    public string AcidBaseModelName { get; set; } = "";
    public string OxygenationModelName { get; set; } = "";
    public string HeartModelName { get; set; } = "";
    public double SetPointPres { get; set; } 
    public double SensitivityPres { get; set; } 
    public double TimeConstantPres { get; set; } 
    public double PresSensorOutput { get; set; }
    public double SympatheticOutput { get; set; }
    public double VagalOutput { get; set; }
    public double EffectSizePres { get; set; }
    public double AnsInput { get; set; }
    public double MaxHeartRate { get; set; }
    public double RefHeartRate { get; set; }
    
    public double VagalTone { get; set; }
    public double SympatheticTone { get; set; }
    
    private Model? _model;
    private bool _initialized;
    private double _t;
    private IBloodCompliance _baroreceptor;
    private IBloodCompliance _chemoreceptor;
    private Heart _heart;
    private AcidBase _ab;
    private Oxygenation _oxy;
    private Sensor _presSensor;
    private Sensor _phSensor;
    private Sensor _po2Sensor;
    private Sensor _pco2Sensor;
    private Sensor _lungStretchSensor;

    private double _updateTimer;

    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;

        // get the modeling stepsize for easy referencing
        _t = _model.ModelingStepsize;
        
        
        // find the chemo en baro receptor source
        foreach (var comp in _model.Components)
        {
            _chemoreceptor = (IBloodCompliance)_model.Components.Find(i => i.Name == ChemoreceptorLocation)!;
            _baroreceptor = (IBloodCompliance)_model.Components.Find(i => i.Name == BaroreceptorLocation)!;
        }
        
        // initialize the sensors
        _presSensor = new Sensor(setPoint: SetPointPres, sensitivity: SensitivityPres, timeConstant: TimeConstantPres,
            updateInterval: UpdateInterval);

        // store a reference to the heart, acid base and oxygenation models
        _ab = (AcidBase)_model.Components.Find(i => i.Name == AcidBaseModelName)!;
        _oxy = (Oxygenation)_model.Components.Find(i => i.Name == OxygenationModelName)!;
        _heart = (Heart)_model.Components.Find(i => i.Name == HeartModelName)!;
        
        // signal that the model component is initialized
        _initialized = true;
        
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
            _updateTimer = 0;
            CalcAutonomicControl();
        }

        _updateTimer += _t;
    }

    private void CalcAutonomicControl()
    {
        // Adapted from Ursino et al.
        
        // Interaction between carotid baro regulation and the pulsating heart: a mathematical model Mauro Ursino
        // Am J Physiol Heart Circ Physiol 275:H1733-H1747, 1998. ;
        
        // Get the arterial baro receptor firing rate (between 0 - 1) with time constant and sensitivity
        PresSensorOutput =  _presSensor.Update(_baroreceptor.Pres);
        
        // combine the different sensor outputs to 1 output
        if ((EffectSizePres) > 0)
        {
            AnsInput = PresSensorOutput * EffectSizePres / EffectSizePres;
        }
        else
        {
            AnsInput = 0.5;
        }
        
        // Feed the pressure sensor output into the sympathetic and vagal (parasympathetic) nerves and calculate the activity
        SympatheticOutput = Math.Exp(-SympatheticTone * PresSensorOutput);
        VagalOutput = Math.Exp((PresSensorOutput - 0.5) / VagalTone) / (1 + Math.Exp((PresSensorOutput - 0.5) / VagalTone));
        
        
        
        // // calculate the target minute volume
        // var newHeartRate = RefHeartRate + (AnsOutput - 0.5) * MaxHeartRate;
        //
        // // guard against zero
        // if (newHeartRate < 10)
        // {
        //     newHeartRate = 10;
        // }
        //
        // _heart.HeartRate = newHeartRate;

    }
}