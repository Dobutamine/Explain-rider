using Explain.Helpers;

namespace Explain.CoreModels;

public class AutonomicNervousSystem: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public double UpdateInterval { get; set; } = 0.015;

    public double Test1 { get; set; } = 0;
    public double Test2 { get; set; } = 0;
    public double Test3 { get; set; } = 0;
    public double Test4 { get; set; } = 0;
    
    // sensors
    public string ChemoreceptorLocation { get; set; } = "";
    private IBloodCompliance _chemoreceptor;

    public string BaroreceptorLocation { get; set; } = "";
    private IBloodCompliance _baroreceptor;

    private double _baroreceptorSignal = 0.5;
    public double SetBaro { get; set; }
    public double SensBaro { get; set; }
    public double TcHpBaro { get; set; }
    public double GHpBaro { get; set; }

    public double GContBaro { get; set; }
    public double TcContBaro { get; set; }
    public double GUvolBaro { get; set; }
    public double TcUvolBaro { get; set; }
    public double GResBaro { get; set; }
    public double TcResBaro { get; set; }
    
    // Targets
    public string HeartModelName { get; set; } = "";
    private Heart? _heart;
    public string[] SystemicResistors { get; set; } = Array.Empty<string>();
    private List<BloodResistor> _systemicResistors = new List<BloodResistor>();
    private string [] UnstressedVolumes { get; set; } = Array.Empty<string>();
    private List<IBloodCompliance> _unstressedVolumes = new List<IBloodCompliance>();

    
    // helpers
    public string AcidBaseModelName { get; set; } = "";
    private AcidBase _ab;
    public string OxygenationModelName { get; set; } = "";
    private Oxygenation _oxy;

    private Model? _model;
    private bool _initialized;
    private double _t;
    private double _updateTimer;

    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;

        // get the modeling stepsize for easy referencing
        _t = _model.ModelingStepsize;
        
        
        // find the chemo rececptor and baroreceptor source
        foreach (var comp in _model.Components)
        {
            _chemoreceptor = (IBloodCompliance)_model.Components.Find(i => i.Name == ChemoreceptorLocation)!;
            _baroreceptor = (IBloodCompliance)_model.Components.Find(i => i.Name == BaroreceptorLocation)!;
        }
        
        
        // find the effector sites
        _heart = (Heart)_model.Components.Find(i => i.Name == HeartModelName)!;
        foreach (var systemicResistor in SystemicResistors)
        {
            var res = (BloodResistor)_model.Components.Find(i => i.Name == systemicResistor)!;
            _systemicResistors.Add(res);
        }
        foreach (var unstressedVolume in UnstressedVolumes)
        {
            var uvol = (IBloodCompliance)_model.Components.Find(i => i.Name == unstressedVolume)!;
            _unstressedVolumes.Add(uvol);
        }

        // store a reference to the heart, acid base and oxygenation models
        _ab = (AcidBase)_model.Components.Find(i => i.Name == AcidBaseModelName)!;
        _oxy = (Oxygenation)_model.Components.Find(i => i.Name == OxygenationModelName)!;
        
        
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
        // calculate the acid base and oxygenation properties of chemoreceptor site
        _ab.CalcAcidBase(_chemoreceptor.Solutes[1].Conc);
        _oxy.CalcOxygenation(_chemoreceptor.Solutes[0].Conc);

        // https://www.ncbi.nlm.nih.gov/books/NBK538172/
        // A decrease in mean arterial pressure causes a drop in de receptor activity (firing rate)
        // In neonates the most accurate mean is given by MAP = DBP + (0.466 * (SBP-DBP))
        var pres = _baroreceptor.PresMin  + 0.466 * (_baroreceptor.PresMax - _baroreceptor.PresMin);
        // calculate the sensor activity normalized to a range of 0 to 1 with an operating point at OpBaro (where activity = 0.5) and a slope determined by SensBaro
        var activity = (Math.Exp((pres - SetBaro) / SensBaro)) / (1 + Math.Exp((pres - SetBaro) / SensBaro));
        
        // apply the time constant of the hp effect
        _baroreceptorSignal = UpdateInterval * ((1.0 / TcHpBaro) * (-_baroreceptorSignal + activity)) + _baroreceptorSignal;

        // calculate the effector
        _heart.HeartPeriodChangeAns = (_baroreceptorSignal - 0.5) * GHpBaro;

        Test1 = _baroreceptorSignal;
    }
}