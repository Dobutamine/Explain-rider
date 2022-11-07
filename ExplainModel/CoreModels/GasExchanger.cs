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
    private IGasCompliance? _compGas { get; set; }

    private AcidBase _ab;
    private Oxygenation _oxy;
    private Model _model;
    private bool _initialized;
    private int _to2Index = -1;
    private int _tco2Index = -1;
    private double _fluxO2 = 0;
    private double _fluxCo2 = 0;
    private const double GasConstant = 62.36367; // L·mmHg/mol·K
    
    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // find the blood compartment and store a reference
        _compBlood = (IBloodCompliance)_model.Components.Find(i => i.Name == CompBlood)!;
        _compGas = (IGasCompliance)_model.Components.Find(i => i.Name == CompGas)!;
        
        // store a reference to the acidbase and oxygenation models
        _ab = (AcidBase)_model.Components.Find(i => i.Name == "AcidBase")!;
        _oxy = (Oxygenation)_model.Components.Find(i => i.Name == "Oxygenation")!;
        
        // signal that the model component is initialized
        if (_compBlood != null && _compGas != null)
        {
            _initialized = true;
        }
        
    }

    private void FindIndices()
    {
        // find the tco2 and to2 indices
        int counter = 0;
        foreach (var solute in _compBlood.Solutes)
        {
            if (solute.Name == "tco2")
            {
                _tco2Index = counter;
            }
            if (solute.Name == "to2")
            {
                _to2Index = counter;
            }

            counter += 1;
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
        if (_to2Index == -1 || _tco2Index == -1)
        {
            FindIndices();
        }
        else
        {
            // we need calculate the pco2 and po2 of the blood compartment
            var to2Blood = _compBlood.Solutes[_to2Index].Conc;
            var tco2Blood = _compBlood.Solutes[_tco2Index].Conc;
            
            var po2Blood = _oxy.CalcOxygenation(to2Blood).Po2;
            var pco2Blood = _ab.CalcAcidBase(tco2Blood).Pco2;

            _fluxO2 = (po2Blood - _compGas.Po2) * DifO2 * _model.ModelingStepsize;
            // change the oxygen content of the blood_compartment
            var newTo2Blood = (to2Blood * _compBlood.Vol - _fluxO2) / _compBlood.Vol;
            if (newTo2Blood < 0) {
                newTo2Blood = 0;
            }
            _compBlood.Solutes[_to2Index].Conc = newTo2Blood;
            
            var newTo2Gas = (_compGas.Co2 * _compGas.Vol + _fluxO2) / _compGas.Vol;
            if (newTo2Gas < 0) {
                newTo2Gas = 0;
            }
            _compGas.Co2 = newTo2Gas;

            _fluxCo2 = (pco2Blood - _compGas.Pco2) * DifCo2 * _model.ModelingStepsize;
            // change the oxygen content of the blood_compartment
            var newTco2Blood = (tco2Blood * _compBlood.Vol - _fluxCo2) / _compBlood.Vol;
            if (newTco2Blood < 0) {
                newTco2Blood = 0;
            }
            _compBlood.Solutes[_tco2Index].Conc = newTco2Blood;
            
            var newTco2Gas = (_compGas.Cco2 * _compGas.Vol + _fluxCo2) / _compGas.Vol;
            if (newTco2Gas < 0) {
                newTco2Gas = 0;
            }
            _compGas.Cco2 = newTco2Gas;


        }
        
    }
}