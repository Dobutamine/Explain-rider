using Explain.Helpers;

namespace Explain.CoreModels;

public class GasExchanger: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public string CompGas { get; set; } = "";
    public string CompBlood { get; set; } = "";
    public double DifO2 { get; set; }
    public double DifCo2 { get; set; }

    private IBloodCompliance? _compBlood { get; set; }
    private GasCompliance? _compGas { get; set; }

    private AcidBase _ab = new AcidBase();
    private Oxygenation _oxy = new Oxygenation();
    private Model? _model;
    private bool _initialized;

    private double _fluxO2;
    private double _fluxCo2;
    private const double GasConstant = 62.36367; // L·mmHg/mol·K
    
    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // find the blood compartment and store a reference
        _compBlood = (IBloodCompliance)_model.Components.Find(i => i.Name == CompBlood)!;
        _compGas = (GasCompliance)_model.Components.Find(i => i.Name == CompGas)!;
        
        // store a reference to the acidbase and oxygenation models
        _ab = (AcidBase)_model.Components.Find(i => i.Name == "AcidBase")!;
        _oxy = (Oxygenation)_model.Components.Find(i => i.Name == "Oxygenation")!;
        
        // signal that the model component is initialized
        if (_compBlood != null && _compGas != null)
        {
            _initialized = true;
        }
        
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
        // we need to get the To2 and the Po2 of the blood and gas compartment
        var to2Blood = _compBlood.Solutes[0].Conc;
        var tco2Blood = _compBlood.Solutes[1].Conc;
        var co2Gas = _compGas.Co2;
        var cco2Gas = _compGas.Cco2;
        
        // calculate the po2 and pco2 of the blood compartment
        var po2Blood = _oxy.CalcOxygenation(to2Blood).Po2;
        var pco2Blood = _ab.CalcAcidBase(tco2Blood).Pco2;
        var po2Gas = _compGas.Po2;
        var pco2Gas = _compGas.Pco2;
        
        // calculate the O2 flux from the blood to the gas compartment
        _fluxO2 = (po2Blood - po2Gas) * DifO2 * _model.ModelingStepsize;
        
        // calculate the CO2 flux from the blood to the gas compartment
        _fluxCo2 = (pco2Blood - pco2Gas) * DifCo2 * _model.ModelingStepsize;
        
        // change the O2 concentrations of the gas and blood compartments
        var newTo2Blood = (to2Blood * _compBlood.Vol - _fluxO2) / _compBlood.Vol;
        if (newTo2Blood < 0)
        {
            newTo2Blood = 0;
        }

        var newCo2Gas = (co2Gas * _compGas.Vol + _fluxO2) / _compGas.Vol;
        if (newCo2Gas < 0)
        {
            newCo2Gas = 0;
        }
        
        // change the CO2 concentrations of the gas and blood compartments
        var newTCo2Blood = (tco2Blood * _compBlood.Vol - _fluxCo2) / _compBlood.Vol;
        if (newTCo2Blood < 0)
        {
            newTCo2Blood = 0;
        }

        var newCCo2Gas = (cco2Gas * _compGas.Vol + _fluxCo2) / _compGas.Vol;
        if (newCCo2Gas < 0)
        {
            newCCo2Gas = 0;
        }
        
        
        // transfer the new concentrations
        _compBlood.Solutes[0].Conc = newTo2Blood;
        _compBlood.Solutes[1].Conc = newTCo2Blood;
        _compGas.Co2 = newCo2Gas;
        _compGas.Cco2 = newCCo2Gas;

        _compBlood.Pco2 = pco2Blood;
        _compBlood.Po2 = po2Blood;


    }
}