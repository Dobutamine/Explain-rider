using Explain.Helpers;

namespace Explain.CoreModels;

public class BloodCompliance: ICoreModel, ICompliance, IBloodCompliance
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }  = false;
    public double Pres { get; set; } = 0;
    public double PresMus { get; set; } = 0;
    public double Pres0 { get; set; } = 0;
    public double PresExt { get; set; } = 0;
    public double PresCc { get; set; } = 0;
    public double PresMax { get; set; } = 0;
    public double PresMin { get; set; } = 0;
    public double Vol { get; set; } = 0;
    public double Uvol { get; set; } = 0;
    public double VolMax { get; set; } = 0;
    public double VolMin { get; set; } = 0;
    public double ElBase { get; set; } = 0;
    public double ElK { get; set; } = 0;
    public double ActFactor { get; set; }
    
    // oxygenation
    public double Po2 { get; set; }
    public double So2 { get; set; }
    public double Hb { get; set; }
    public double Dpg { get; set; }
    public double To2 { get; set; }
    public double Tco2 { get; set; }
    
    // acid base
    public double Ph { get; set; }
    public double Pco2 { get; set; }
    public double Hco3 { get; set; }
    public double Be { get; set; }
    
    // Solutes
    public BloodCompound[] Solutes { get; set;  }= Array.Empty<BloodCompound>();

    private Model? _model;
    private bool _initialized;
    private double _tempPresMax = -1000;
    private double _tempPresMin = 1000;
    private double _tempVolMax = -1000;
    private double _tempVolMin = 1000;
    private double _evalTimer = 0;
    private const double EvalTime = 1D;

    private double _t = 0;

    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // store the stepsize for easy referencing
        _t = _model.ModelingStepsize;

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
        // calculate the pressure depending on the elastance
        Pres = ElBase * (1 + ElK * (Vol - Uvol)) * (Vol - Uvol) + Pres0 + PresExt + PresCc + PresMus;
        
        // reset the external pressures
        PresMus = 0;
        Pres0 = 0;
        PresExt = 0;
        PresCc = 0;
        
        // do the statistics
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
        _evalTimer += _t;
    }

    public void VolumeIn(double dVol, IBloodCompliance compFrom)
    {
        // change the volume
        Vol += dVol;
        
        // calculate the solutes
        for (var i = 0; i < Solutes.Length; i++)
        {
            var dSol = (compFrom.Solutes[i].Conc - Solutes[i].Conc) * dVol;
            Solutes[i].Conc = ((Solutes[i].Conc * Vol) + dSol) / Vol;
        }
        
        To2 = Solutes[0].Conc;
        Tco2 = Solutes[1].Conc;
    }

    public double VolumeOut(double dVol)
    {
        // declare a volume which couldn't be removed
        double volDeficit = 0;
        
        // change the volume
        Vol -= dVol;
        
        // guard against negative volume
        if (Vol < 0)
        {
            volDeficit = -Vol;
            Vol = 0;
        }

        // protect the mass balance by returning the volume which couldn't be removed
        return volDeficit;

    }
}