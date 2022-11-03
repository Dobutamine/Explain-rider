using Explain.Helpers;

namespace Explain.CoreModels;

public class Gas: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string[] GasSourceComp { get; set; } = Array.Empty<String>();
    public List<GasCompliance> GasSources { get; set; } = new List<GasCompliance>();
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    
    private Model _model;
    private bool _initialized;

    public bool TempRoom { get; set; }
    public bool Patm { get; set; }

    private const double GasConstant = 62.36367; // L·mmHg/mol·K
    public GasCompound[] GasCompounds { get; set; } = Array.Empty<GasCompound>();

    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;

        // set the temperatures of the gas compliances
        var MOUTH = (GasCompliance)_model.Components.Find(i => i.Name == "MOUTH")!;
        MOUTH.PresAtm = 760.0;
        MOUTH.Temp = 20;
        var DS = (GasCompliance)_model.Components.Find(i => i.Name == "DS")!;
        DS.Temp = 32;
        var ALL = (GasCompliance)_model.Components.Find(i => i.Name == "ALL")!;
        ALL.Temp = 37;
        var ALR = (GasCompliance)_model.Components.Find(i => i.Name == "ALR")!;
        ALR.Temp = 37;

        // calculate the pressure in MOUTH, should be pAtm
        MOUTH.CalcModel();
        // the pressure in MOUTH should pAtm
        
        // STP : Temp 0 (273.15 K), P = 760 mmHg -> 1 mol = 22.41 l
        // set the total gas concentration of the inspired air depending on the pressure and temperature
        
        // Keep concentration constant
        // P = 760 mmHg at 0 degrees and concentration of 0.04462 mol/l
        // P = 815 mmHg at 20 degrees and concentration od 0.04462 mol/l 
        
        // Keep pressure constant
        // CTotal at 0 degrees  = 0.04462 mol/l at pAtm mmHg
        // CTotal at 20 degrees = 0.04157 mol/l at pAtm mmHg -> this is our baseline inspired air composition
        MOUTH.CTotal = MOUTH.Pres / (GasConstant * (273.15 + MOUTH.Temp));
        
        // Dry air at 20 degrees has FH2O = 0, FO2 = 0.2092, FCO2 = 0.0004, FN2 = 0.7901;
        MOUTH.FH2ODry = 0;
        MOUTH.FO2Dry = 0.2092;
        MOUTH.FCO2Dry = 0.0004;
        MOUTH.FN2Dry = 0.7901;

        // calculate the water vapour pressure depending on the temperature
        // At 20 dgs celsius and 760 mmHg the partial pressure of water vapour is about 17 mmHg (Antoine formula)
        MOUTH.PH2O = MOUTH.CalcWaterVapourPressure(MOUTH.Temp);
        MOUTH.FH2O = MOUTH.PH2O / MOUTH.Pres;
        MOUTH.CH2O = MOUTH.FH2O * MOUTH.CTotal;
        
        // adjust the fractions of the other inspired gasses (O2, CO2, N2)
        MOUTH.FO2 = MOUTH.FO2Dry  * (1 - MOUTH.FH2O);
        MOUTH.FCO2 = MOUTH.FCO2Dry  * (1 - MOUTH.FH2O);
        MOUTH.FN2 = MOUTH.FN2Dry  * (1 - MOUTH.FH2O);
        // the sum of fh2o + fo2 + fco2 + fn2 should be 1

        // calculate the concentrations of the other inspired gasses
        MOUTH.CO2 = MOUTH.FO2 * MOUTH.CTotal;
        MOUTH.CCO2 = MOUTH.FCO2 * MOUTH.CTotal;
        MOUTH.CN2 = MOUTH.FN2 * MOUTH.CTotal;
        // the sum of ch2o + co2 + cco2 + cn2 should be the same as CTotal
        
        // set the partial pressures of the other inspired gasses
        MOUTH.PO2 = MOUTH.FO2 * MOUTH.Pres;
        MOUTH.PCO2 = MOUTH.FCO2 * MOUTH.Pres;
        MOUTH.PN2 = MOUTH.FN2 * MOUTH.Pres;
        // the sum of pH2o + pO2 + pCo2 + pN2 should be pAtm
        
        // signal that the model component is initialized
        _initialized = true;
    }
    public void StepModel() { }
    public void CalcModel() { }
    
    public double CalcWaterVapourPressure(double temp) {
        // calculate the water vapour pressure in air depending on the temperature
        return Math.Pow(Math.E, 20.386 - 5132 / (temp + 273));
    }
}