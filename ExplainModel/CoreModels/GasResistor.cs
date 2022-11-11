using Explain.Helpers;

namespace Explain.CoreModels;

public class GasResistor: ICoreModel
{
    public string Name { get; set; }  = "";
    public string Description { get; set; }  = "";
    public string ModelType { get; set; }  = "";
    public bool IsEnabled { get; set; }  = false;
    
    public double Flow { get; set; }
    public bool NoFlow { get; set; }
    public bool NoBackFlow { get; set; }
    public double Res { get; private set; }
    public double Rfor { get; set; }
    public double Rback { get; set; }
    public double Rk { get; set; }
    public string CompFrom { get; set; }
    public string CompTo { get; set; }
    
    private bool _initialized = false;
    private Model _model;
    private GasCompliance? _compFrom;
    private GasCompliance? _compTo;

    public void InitModel(Model model)
    {
        // store a reference to the model container
        _model = model;

        // find the correct components in the component list
        _compTo = (GasCompliance)_model.Components.Find(i => i.Name == CompTo)!;
        _compFrom = (GasCompliance)_model.Components.Find(i => i.Name == CompFrom)!;

        // flag that the initialization is complete
        if (_compFrom != null && _compTo != null)
        {
            _initialized = true;
        }
        else
        {
            Console.WriteLine("Failed to initialize connector {0}.", Name);
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
        // reset the flow
        Flow = 0;
        
        // get the pressures from the connected compliances
        var pU = _compFrom!.Pres;
        var pD = _compTo!.Pres;

        // calculate the flow in l/sec
        if (pU >= pD)
        {
            Res = (Rfor * (1 + Rk * Flow));
            Flow = (pU - pD) / Res;
        }
        else
        {
            if (!NoBackFlow)
            {
                Res = (Rback * (1 + Rk * Flow));
                Flow = (pU - pD) / Res;
            }
            else
            {
                Flow = 0;
            }
        }

        if (NoFlow)
        {
            Flow = 0;
        }

        // calculate the flow in this stepsize
        var dVol = Flow * _model.ModelingStepsize;

        // change the volumes of the connected compliance
        if (dVol > 0)
        {
            var mbPos = _compFrom.VolumeOut(dVol);
            _compTo.VolumeIn(dVol - mbPos, _compFrom);
        }
        else
        {
            var mbNeg = _compTo.VolumeOut(-dVol);
            _compFrom.VolumeIn(-(dVol - mbNeg), _compTo);

        }
    }
}