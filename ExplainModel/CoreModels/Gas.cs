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

        var pAtm = 760.0;
        var Temp = 20;
        
        // set the temperatures of the gas compliances
        var MOUTH = (GasCompliance)_model.Components.Find(i => i.Name == "MOUTH")!;
        MOUTH.Temp = 20;
        var DS = (GasCompliance)_model.Components.Find(i => i.Name == "DS")!;
        DS.Temp = 32;
        var ALL = (GasCompliance)_model.Components.Find(i => i.Name == "ALL")!;
        ALL.Temp = 37;
        var ALR = (GasCompliance)_model.Components.Find(i => i.Name == "ALR")!;
        ALR.Temp = 37;
        
        // set the total gas concentration of the inspired air depending on the pressure and temperature
        // the inspired air is coming from the MOUTH compliance
        MOUTH.CTotal = pAtm / (GasConstant * (273.15 + MOUTH.Temp));
        
        // calculate the water vapour pressure, fraction and concentration depending on the temperature
        MOUTH.PH2O = CalcWaterVapourPressure(MOUTH.Temp);
        MOUTH.FH2O = MOUTH.PH2O / pAtm;
        MOUTH.CH2O = MOUTH.FH2O * MOUTH.CTotal;
        
        // set the gas fractions of the dry part of the inspired air
        MOUTH.FO2Dry = 0.2095;
        MOUTH.FCO2Dry = 0.0004;
        MOUTH.FN2Dry = 0.7901;

        // set the fractions of the inspired air which has water vapour.
        MOUTH.FO2 = MOUTH.FO2Dry * (1 - MOUTH.FH2O);
        MOUTH.FCO2 = MOUTH.FCO2Dry * (1 - MOUTH.FH2O);
        MOUTH.FN2 = MOUTH.FN2Dry * (1 - MOUTH.FH2O);
        
        // calculate the concentrations of the gasses
        MOUTH.CO2 = MOUTH.FO2 * MOUTH.CTotal;
        MOUTH.CCO2 = MOUTH.FCO2 * MOUTH.CTotal;
        MOUTH.CN2 = MOUTH.FN2 * MOUTH.CTotal;
        
        // calculate the partial pressures of the gasses
        MOUTH.PO2 = MOUTH.FO2 * pAtm;
        MOUTH.PCO2 = MOUTH.FCO2 * pAtm;
        MOUTH.PN2 = MOUTH.FN2 * pAtm;

        Console.WriteLine("PTotal {0, 10}, FTotal = 1, CTotal = {1} mol/l", pAtm, MOUTH.CTotal);
        Console.WriteLine("PH2O {0, 10}, FH2O = {1}, CH2O = {2} mol/l", MOUTH.PH2O, MOUTH.FH2O, MOUTH.CH2O);
        Console.WriteLine("PO2 {0, 10}, FO2 = {1}, CO2 = {2} mol/l", MOUTH.PO2, MOUTH.FO2, MOUTH.CO2);
        Console.WriteLine("PCO2 {0, 10}, FCO2 = {1}, CCO2 = {2} mol/l", MOUTH.PCO2, MOUTH.FCO2, MOUTH.CCO2);

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