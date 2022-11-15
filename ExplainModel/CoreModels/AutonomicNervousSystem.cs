using System.Security;
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
    
    public double SetBaro { get; set; }
    public double MinBaro { get; set; }
    public double MaxBaro { get; set; }
    public double TcHpBaro { get; set; }
    public double GHpBaro { get; set; }
    public double GContBaro { get; set; }
    public double TcContBaro { get; set; }
    public double GUvolBaro { get; set; }
    public double TcUvolBaro { get; set; }
    public double GResBaro { get; set; }
    public double TcResBaro { get; set; }
    
    public double SetPo2 { get; set; }
    public double MinPo2 { get; set; }
    public double MaxPo2 { get; set; }
    public double GVePo2 { get; set; }
    public double TcVePo2 { get; set; }
    public double GHpPo2 { get; set; }
    public double TcHpPo2 { get; set; }
    
    public double SetPco2 { get; set; }
    public double MinPco2 { get; set; }
    public double MaxPco2 { get; set; }
    public double GVePco2 { get; set; }
    public double TcVePco2 { get; set; }
    public double GHpPco2 { get; set; }
    public double TcHpPco2 { get; set; }
    
    public double SetPh { get; set; }
    public double MinPh { get; set; }
    public double MaxPh { get; set; }
    public double GVePh { get; set; }
    public double TcVePh { get; set; }
    
    public double SetLungStretch { get; set; }
    public double MinLungStretch { get; set; }
    public double MaxLungStretch { get; set; }
    public double GVeLungStretch { get; set; }
    public double TcVeLungStretch { get; set; }

    private Controller _hpBaroController;
    private Controller _hpPco2Controller;
    private Controller _hpPo2Controller;
    private Controller _vePco2Controller;
    private Controller _vePo2Controller;
    private Controller _vePhController;
    private Controller _veLungStretchController;
    
    // Targets
    public string HeartModelName { get; set; } = "";
    private Heart? _heart;
    public string BreathingModelName { get; set; } = "";
    private Breathing? _breathing;
    
    public string[] LungCompartments { get; set; } = Array.Empty<string>();
    private List<GasCompliance> _lungs = new List<GasCompliance>();
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
        
        // initialize the controller which controls the heart period using the baro receptor
        _hpBaroController = new Controller(SetBaro, MinBaro, MaxBaro, GHpBaro, TcHpBaro, UpdateInterval);
        
        // initialize the controller which controls the heart period using the po2
        _hpPo2Controller = new Controller(SetPo2, MinPo2, MaxPo2, GHpPo2, TcHpPo2, UpdateInterval);
        
        // initialize the controller which controls the heart period using the po2
        _hpPco2Controller = new Controller(SetPco2, MinPco2, MaxPco2, GHpPco2, TcHpPco2, UpdateInterval);
        
        // initialize the controller which controls the exhaled minute volume by the pco2
        _vePco2Controller = new Controller(SetPco2, MinPco2, MaxPco2, GVePco2, TcVePco2, UpdateInterval);
        
        // initialize the controller which controls the exhaled minute volume by the po2
        _vePo2Controller = new Controller(SetPo2, MinPo2, MaxPo2, GVePo2, TcVePo2, UpdateInterval);
        
        // initialize the controller which controls the exhaled minute volume by the pH
        _vePhController = new Controller(SetPh, MinPh, MaxPh, GVePh, TcVePh, UpdateInterval);

        // initialize the controller which controls the exhaled minute volume by the lung stretch receptor
        _veLungStretchController = new Controller(SetLungStretch, MinLungStretch, MaxLungStretch, GVeLungStretch, TcVeLungStretch, UpdateInterval);
        
        // find the effector sites
        _heart = (Heart)_model.Components.Find(i => i.Name == HeartModelName)!;
        _breathing = (Breathing)_model.Components.Find(i => i.Name == BreathingModelName)!;
        
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
        
        foreach (var lungCompartment in LungCompartments)
        {
            var lc = (GasCompliance)_model.Components.Find(i => i.Name == lungCompartment)!;
            _lungs.Add(lc);
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
        var ab = _ab.CalcAcidBase(_chemoreceptor.Solutes[1].Conc);
        var or = _oxy.CalcOxygenation(_chemoreceptor.Solutes[0].Conc);
        
        // store the results
        _chemoreceptor.Po2 = or.Po2;
        _chemoreceptor.So2 = or.So2;
        _chemoreceptor.Pco2 = ab.Pco2;
        _chemoreceptor.Ph = ab.Ph;
        _chemoreceptor.Hco3 = ab.Hco3;
        
        // In neonates the most accurate mean is given by MAP = DBP + (0.466 * (SBP-DBP))
        var map = _baroreceptor.PresMin  + 0.466 * (_baroreceptor.PresMax - _baroreceptor.PresMin);
        
        // get the lung volume for the lung stretch receptor
        double lungVolume = 0.0;
        foreach (var lung in _lungs)
        {
            lungVolume += lung.Vol;
        }

        // calculate the effect of the mean arterial pressure, po2, pco2 and pH on the heart period.
        _heart!.HeartPeriodChangeAns = _hpBaroController.Update(map) + _hpPo2Controller.Update(map) + _hpPco2Controller.Update(map);

        // calculate the effect of the po2, pco2, pH and lung stretch on the exhaled minute volume
        _breathing!.TargetMinuteVolume = _breathing.RefMinuteVolume +
                                         _vePhController.Update(_chemoreceptor.Ph) +
                                         _vePco2Controller.Update(_chemoreceptor.Pco2) +
                                         _vePo2Controller.Update(_chemoreceptor.Po2) +
                                         _veLungStretchController.Update(lungVolume);




    }
}