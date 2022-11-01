using Explain.Helpers;

namespace Explain.CoreModels;

public class GasCompliance: ICoreModel, ICompliance, IGasCompliance
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }  = false;
    public double Pres { get; set; }
    public double Pres0 { get; set; }
    public double PresAtm { get; set; } = 760;
    public double PresMus { get; set; }
    public double PresExt { get; set; }
    public double PresCC { get; set; }
    public double PresMax { get; set; }
    public double PresMin { get; set; }
    public double Vol { get; set; }
    public double Uvol { get; set; }
    public double VolMax { get; set; }
    public double VolMin { get; set; }
    public double ElBase { get; set; }
    public double ElK { get; set; }
    public double ActFactor { get; set; }

    
    public GasCompound[] GasCompounds { get; set; }

    private Model _model;
    private bool _initialized;
    private double _tempPresMax = -1000;
    private double _tempPresMin = 1000;
    private double _tempVolMax = -1000;
    private double _tempVolMin = 1000;
    private double _evalTimer = 0;
    private const double EvalTime = 3D;

    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // initialize an empty array of blood compounds
        GasCompounds = Array.Empty<GasCompound>();
        
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
        Pres = ElBase * (1 + ElK * (Vol - Uvol)) * (Vol - Uvol) + Pres0 + PresExt + PresCC + PresAtm + PresMus;
        
        // reset the external pressure
        PresMus = 0;
        PresExt = 0;
        PresCC = 0;
        Pres0 = 0;

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
        _evalTimer += _model.ModelingStepsize;

    }

    public void VolumeIn(double dVol, IGasCompliance compFrom)
    {
        // change the volume
        Vol += dVol;
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