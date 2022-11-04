using Explain.Helpers;

namespace Explain.CoreModels;

public class Container: ICoreModel, ICompliance
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public double Pres { get; set; }
    public double PresMus { get; set; }
    public double Pres0 { get; set; }
    public double PresExt { get; set; }
    public double PresCc { get; set; }
    public double PresMax { get; set; }
    public double PresMin { get; set; }
    public double Vol { get; set; }
    public double VolExt { get; set; }
    public double Uvol { get; set; }
    public double VolMax { get; set; }
    public double VolMin { get; set; }
    public double ActFactor { get; set; }
    public double ElBase { get; set; }
    public double ElK { get; set; }
    public string[]? Comps { get; set; }
    
    private List<ICompliance> Compliances { get; set; } = new List<ICompliance>();
    
    private Model? _model;
    private bool _initialized;
    private double _tempPresMax = -1000;
    private double _tempPresMin = 1000;
    private double _tempVolMax = -1000;
    private double _tempVolMin = 1000;
    private double _evalTimer = 0;
    private const double EvalTime = 1D;

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
        // set the volume to the externally added volume
        Vol = VolExt;
        
        // calculate the current volume of the container
        foreach (var comp in Compliances)
        {
            Vol += comp.Vol;
        }
        
        // calculate the pressure depending on the elastance
        Pres = ElBase * (1 + ElK * (Vol - Uvol)) * (Vol - Uvol) + Pres0 + PresExt + PresCc + PresMus;
        
        // transfer the pressures to the compliances the container contains
        foreach (var comp in Compliances)
        {
            // increase the external pressure of the enclosed compliances
            comp.PresExt += Pres;
        }
        
        // reset the external pressures and volumes
        PresMus = 0;
        PresExt = 0;
        PresCc = 0;
        VolExt = 0;
        
        // calculate the minimal and maximal volumes and pressures
        CalcMinMax();
    }
    private void CalcMinMax()
    {
        if (Pres > _tempPresMax)
        {
            _tempPresMax = Pres;
        }

        if (Pres < _tempPresMin)
        {
            _tempPresMin = Pres;
        }
        
        if (Vol > _tempVolMax)
        {
            _tempVolMax = Vol;
        }

        if (Vol < _tempVolMin)
        {
            _tempVolMin = Vol;
        }
        
        if (_evalTimer > EvalTime)
        {
            _evalTimer = 0;
            PresMax = _tempPresMax;
            PresMin = _tempPresMin;
            _tempPresMax = -1000;
            _tempPresMin = 1000;
            VolMax = _tempVolMax;
            VolMin = _tempVolMin;
            _tempVolMax = -1000;
            _tempVolMin = 1000;
        }
        
        // every model step the eval timer is increased with the modeling step size
        _evalTimer += _model.ModelingStepsize;

    }

}