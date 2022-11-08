using Explain.Helpers;

namespace Explain.CoreModels;

public class Gas: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public double Pres0 { get; set; }
    public InspiredAir InspiredAir { get; set; }
    public TempSetting[]? TempSettings { get; set; }
    
    private Model? _model;
    private readonly List<GasCompliance> _inspAirCompliances = new List<GasCompliance>();

    private const double GasConstant = 62.36367; // L·mmHg/mol·K

    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // set the gas compliances temperatures and STP
        if (TempSettings != null)
        {
            foreach (var tempSetting in TempSettings)
            {
                var comp = (GasCompliance)_model.Components.Find(i => i.Name == tempSetting.Name)!;
                comp.Temp = tempSetting.TargetTemp;
                comp.TargetTemp = tempSetting.TargetTemp;
            }
        }

        // get a reference to all the inspired air components and initialize them
        foreach (var comp in InspiredAir.Components)
        {
            var inspAirComp = (GasCompliance)_model.Components.Find(i => i.Name == comp)!;
            // initialize the inspired air compliance
            inspAirComp.Pres0 = Pres0;

            // calculate the pressure in the inspired air compliance, should be pAtm
            inspAirComp.StepModel();
            
            // calculate the concentration at this pressure and temperature in mmol/l !
            inspAirComp.CTotal = (inspAirComp.Pres / (GasConstant * (273.15 + inspAirComp.Temp))) * 1000.0;
            
            // set the inspired air fractions
            inspAirComp.Fh2O = InspiredAir.Fh2O;
            inspAirComp.Fo2 = InspiredAir.Fo2;
            inspAirComp.Fco2 = InspiredAir.Fco2;
            inspAirComp.Fn2 = InspiredAir.Fn2;
            
            // calculate the inspired air concentrations
            inspAirComp.Ch2O = inspAirComp.Fh2O * inspAirComp.CTotal;
            inspAirComp.Co2 = inspAirComp.Fo2 * inspAirComp.CTotal;
            inspAirComp.Cco2 = inspAirComp.Fco2 * inspAirComp.CTotal;
            inspAirComp.Cn2 = inspAirComp.Fn2 * inspAirComp.CTotal;
            
            // calculate the inspired air partial pressures
            inspAirComp.Ph2O = inspAirComp.Fh2O * inspAirComp.Pres;
            inspAirComp.Po2 = inspAirComp.Fo2 * inspAirComp.Pres;
            inspAirComp.Pco2 = inspAirComp.Fco2 * inspAirComp.Pres;
            inspAirComp.Pn2 = inspAirComp.Fn2 * inspAirComp.Pres;
            
            // set the fixed composition flag to true to keep the inspired air stable
            inspAirComp.FixedComposition = true;
            
            // add the inspired air compliance to the list
            _inspAirCompliances.Add(inspAirComp);
        }
    }

    public void SetInspiredAir(InspiredAir inspiredAir)
    {
        foreach (var inspAirComp in _inspAirCompliances)
        {
            // set the inspired air fractions
            inspAirComp.Fh2O = InspiredAir.Fh2O;
            inspAirComp.Fo2 = InspiredAir.Fo2;
            inspAirComp.Fco2 = InspiredAir.Fco2;
            inspAirComp.Fn2 = InspiredAir.Fn2;
            
            // calculate the inspired air concentrations
            inspAirComp.Ch2O = inspAirComp.Fh2O * inspAirComp.CTotal;
            inspAirComp.Co2 = inspAirComp.Fo2 * inspAirComp.CTotal;
            inspAirComp.Cco2 = inspAirComp.Fco2 * inspAirComp.CTotal;
            inspAirComp.Cn2 = inspAirComp.Fn2 * inspAirComp.CTotal;
            
            // calculate the inspired air partial pressures
            inspAirComp.Ph2O = inspAirComp.Fh2O * inspAirComp.Pres;
            inspAirComp.Po2 = inspAirComp.Fo2 * inspAirComp.Pres;
            inspAirComp.Pco2 = inspAirComp.Fco2 * inspAirComp.Pres;
            inspAirComp.Pn2 = inspAirComp.Fn2 * inspAirComp.Pres;
        }
    }
    public void StepModel() { }
    public void CalcModel() { }
    
}

public struct InspiredAir
{
    public string[] Components;
    public double Fh2O;
    public double Fo2;
    public double Fco2;
    public double Fn2;
}

public struct TempSetting
{
    public string Name;
    public double TargetTemp;
}