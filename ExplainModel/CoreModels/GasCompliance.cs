using Explain.Helpers;

namespace Explain.CoreModels;

public class GasCompliance: ICoreModel, ICompliance, IGasCompliance
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }  = false;
    public double Pres { get; set; }
    public double PresSTP { get; set; }
    public double Pres0 { get; set; }
    public double PresTemp { get; set; }
    public double PresMus { get; set; }
    public double PresExt { get; set; }
    public double PresCc { get; set; }
    public double PresMax { get; set; }
    public double PresMin { get; set; }
    public double Vol { get; set; }
    public double Uvol { get; set; }
    public double VolMax { get; set; }
    public double VolMin { get; set; }
    public double ElBase { get; set; }
    public double ElK { get; set; }
    public double ActFactor { get; set; }
    public double CTotal { get; set; } = 0;
    public double Co2 { get; set; }
    public double Cco2 { get; set; }
    public double Cn2 { get; set; }
    public double Ch2O { get; set; }
    public double Ph2O { get; set; }
    public double Po2 { get; set; }
    public double Pco2 { get; set; }
    public double Pn2 { get; set; }
    public double Temp { get; set; }
    public double TargetTemp { get; set; }
    public double TempSTP { get; set; }

    public double FTotal { get; set; }
    public double Fh2O { get; set; }
    public double Fo2 { get; set; }
    public double Fco2 { get; set; }
    public double Fn2 { get; set; }
    public bool FixedComposition { get; set; } = false;

    private const double GasConstant = 62.36367; // L·mmHg/mol·K
    
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
        // Add heat the get to the target temperature if this not a fixed composition compliance
        if (!FixedComposition)
        {
            // add heat to the gas compliance
            AddHeat();
            
            // add water vapour
            AddWaterVapour();
        }
        
        // calculate the pressure depending on the elastance at 0 degrees celsius
        Pres = ElBase * (1 + ElK * (Vol - Uvol)) * (Vol - Uvol) + Pres0 + PresExt + PresCc  + PresMus;

        // calculate the new gas composition of this compliance based on the new pressure and volume.
        CalcGasComposition();
        
        // reset the external pressure
        PresTemp = 0;
        PresMus = 0;
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
        _evalTimer += _model.ModelingStepsize;

    }

    public void AddHeat()
    {
        // calculate a temperature change depending on the target temperature and the current temperature
        var dT = (TargetTemp - Temp) * 0.00005;
        Temp += dT;
            
        // change to volume as the temperature changes
        if (Pres != 0)
        {
            var dV = (CTotal * Vol * GasConstant * dT) / Pres;
            Vol += dV;
                
            // guard against negative volumes
            if (Vol < 0)
            {
                Vol = 0;
            }
        }
    }
    
    public void AddWaterVapour()
    {
        if (FixedComposition) return;
        
        // Calculate water vapour pressure at compliance temperature
        var pH2Ot = CalcWaterVapourPressure(Temp);
        
        // do the diffusion from water vapour depending on the tissue water vapour and gas water vapour pressure
        var dH2O = 0.00001 * (pH2Ot - Ph2O) * _model.ModelingStepsize;
        if (Vol != 0)
        {
            Ch2O = ((Ch2O * Vol) + dH2O) / Vol;
        }
        
        // as the water vapour also takes volume this is added to the compliance
        if (Pres != 0)
        {
            Vol += ((GasConstant * (273.15 + Temp)) / Pres) * dH2O;
        }
        
    }

    public void CalcGasComposition()
    {
        if (FixedComposition) return;

        // calculate CTotal sum of all concentrations
        CTotal = Ch2O + Co2 + Cco2 + Cn2;
        
        // calculate the partial pressures
        if (CTotal != 0)
        {
            // calculate the partial pressures
            Ph2O = (Ch2O / CTotal) * Pres;
            Po2 = (Co2 / CTotal) * Pres;
            Pco2 = (Cco2 / CTotal) * Pres;
            Pn2 = (Cn2 / CTotal) * Pres;
        }
        

    }
    
    public void VolumeIn(double dVol, IGasCompliance compFrom)
    {
        // change the volume
        Vol += dVol;
        
        if (FixedComposition) return;

        if (Vol != 0)
        {
            // change the gas concentrations
            var dCo2 = (compFrom.Co2 - Co2) * dVol;
            Co2 = ((Co2 * Vol) + dCo2) / Vol;
        
            var dCco2 = (compFrom.Cco2 - Cco2) * dVol;
            Cco2 = ((Cco2 * Vol) + dCco2) / Vol;
        
            var dCn2 = (compFrom.Cn2 - Cn2) * dVol;
            Cn2 = ((Cn2 * Vol) + dCn2) / Vol;
        
            var dCh2O = (compFrom.Ch2O - Ch2O) * dVol;
            Ch2O = ((Ch2O * Vol) + dCh2O) / Vol;
        
            // change temperature due to influx of gas
            var dTemp = (compFrom.Temp - Temp) * dVol;
            Temp += dTemp;

        }
        
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
    
    public static double CalcWaterVapourPressure(double temp) {
        // calculate the water vapour pressure in air depending on the temperature
        return Math.Pow(Math.E, 20.386 - 5132 / (temp + 273));
    }
}