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
    public double CTotal { get; set; } = 0;
    public double CO2 { get; set; }
    public double CCO2 { get; set; }
    public double CN2 { get; set; }
    public double CH2O { get; set; }
    public double PH2O { get; set; }
    public double PO2 { get; set; }
    public double PCO2 { get; set; }
    public double PN2 { get; set; }
    public double FH2O { get; set; }
    public double FO2 { get; set; }
    public double FCO2 { get; set; }
    public double FN2 { get; set; }
    public double FO2Dry { get; set; }
    public double FCO2Dry { get; set; }
    public double FN2Dry { get; set; }
    

    public double Temp { get; set; }
    
    private const double GasConstant = 62.36367; // L·mmHg/mol·K
    
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
        // add water vapour
        AddWaterVapour();
        
        // calculate the pressure depending on the elastance
        Pres = ElBase * (1 + ElK * (Vol - Uvol)) * (Vol - Uvol) + Pres0 + PresExt + PresCC + PresAtm + PresMus;
        
        // calculate the new gas composition of this compliance based on the new pressure and volume.
        CalcGasComposition();
        
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

    public void AddWaterVapour()
    {
        // calculate the current water vapour pressure of the gas compliance
        PH2O = GasConstant * (273.15 + Temp) * CH2O;

        // Calculate water vapour pressure at compliance temperature
        var pH2Ot = CalcWaterVapourPressure(Temp);
        
        // do the diffusion from water vapour depending on the tissue water vapour and gas water vapour pressure
        var dH2O = 0.00001 * (pH2Ot - PH2O) * _model.ModelingStepsize;
        CH2O = ((CH2O * Vol) + dH2O) / Vol;

        // as the water vapour also takes volume this is added to the compliance
        Vol += 25.8 * dH2O;

    }

    public void CalcGasComposition()
    {
        // we now have the PH2O and the total pressure so we can calculate the FH2O. 
        CTotal = Pres / (GasConstant * (273.15 + Temp));
        FH2O = CH2O / CTotal;
        
        // calculate the fractions from the fh2o and the dry fractions.
        FO2 = FO2Dry * (1 - FH2O);
        FCO2 = FCO2Dry * (1 - FH2O);
        FN2 = FN2Dry * (1 - FH2O);
        
        // calculate the partial pressures from the fractions and current compliance pressure.
        PO2 = FO2  * Pres;
        PCO2 = FCO2  * Pres;
        PN2 = FN2  * Pres;
        
        // calculate the concentrations from the fractions and the current compliance total gas concentration.
        CO2 = FO2  * CTotal;
        CCO2 = FCO2 * CTotal;
        CN2 = FN2  * CTotal;

    }
   

    public void VolumeIn(double dVol, IGasCompliance compFrom)
    {
        // change the volume
        Vol += dVol;   
        
        // calculate the new the dry gas fractions
        var dFO2 = (compFrom.FO2Dry - FO2Dry) * dVol;
        FO2Dry = ((FO2Dry * Vol) + dFO2) / Vol;
        
        var dFCO2 = (compFrom.FCO2Dry - FCO2Dry) * dVol;
        FCO2Dry = ((FCO2Dry * Vol) + dFCO2) / Vol;
        
        var dFN2 = (compFrom.FN2Dry - FN2Dry) * dVol;
        FN2Dry = ((FN2Dry * Vol) + dFN2) / Vol;

        // calculate the new h2o concentration
        var dCH2O = (compFrom.CH2O - CH2O) * dVol;
        CH2O = ((CH2O * Vol) + dCH2O) / Vol;
        
    }
    public void VolumeInINACTIVE(double dVol, IGasCompliance compFrom)
    {
        // change the volume
        Vol += dVol;
        
        // change the gasconcentrations
        var dC = (compFrom.CTotal - CTotal) * dVol;
        CTotal = ((CTotal * Vol) + dC) / Vol;
        
        var dCO2 = (compFrom.CO2 - CO2) * dVol;
        CO2 = ((CO2 * Vol) + dCO2) / Vol;
        
        var dCCO2 = (compFrom.CCO2 - CCO2) * dVol;
        CCO2 = ((CCO2 * Vol) + dCCO2) / Vol;
        
        var dCN2 = (compFrom.CN2 - CN2) * dVol;
        CN2 = ((CN2 * Vol) + dCN2) / Vol;
        
        var dCH2O = (compFrom.CH2O - CH2O) * dVol;
        CH2O = ((CH2O * Vol) + dCH2O) / Vol;
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
    public double CalcWaterVapourPressure(double temp) {
        // calculate the water vapour pressure in air depending on the temperature
        return Math.Pow(Math.E, 20.386 - 5132 / (temp + 273));
    }
}