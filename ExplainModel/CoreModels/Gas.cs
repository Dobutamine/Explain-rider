using Explain.Helpers;

namespace Explain.CoreModels;

public class Gas: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public double Pres0 { get; set; }
    public double PresSTP { get; set; }
    public double TempSTP { get; set; }
    public InspiredAir InspiredAirAtPres0 { get; set; }
    public TempSetting[] TempSettings { get; set; }
    
    private Model _model;
    private bool _initialized;
    private List<GasCompliance> _inspAirCompliances = new List<GasCompliance>();

    private const double GasConstant = 62.36367; // L·mmHg/mol·K

    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // get a reference to all the inspired air components and initialize them
        foreach (var comp in InspiredAirAtPres0.Components)
        {
            var inspAirComp = (GasCompliance)_model.Components.Find(i => i.Name == comp)!;
            // initialize the inspired air compliance
            inspAirComp.PresSTP = PresSTP;
            inspAirComp.Pres0 = Pres0;
            inspAirComp.TempSTP = TempSTP;
            
            // set the temperatures of the inspired air compliance
            inspAirComp.Temp = InspiredAirAtPres0.Temp;
            inspAirComp.TargetTemp = InspiredAirAtPres0.Temp;
            
            // calculate the pressure in the inspired air compliance, should be pAtm
            inspAirComp.StepModel();
            
            // calculate the molar concentration at this pressure and temperature
            inspAirComp.CTotal = inspAirComp.Pres / (GasConstant * (273.15 + inspAirComp.Temp));
            
            // set the inspired air fractions
            inspAirComp.Fh2O = InspiredAirAtPres0.Fh2O;
            inspAirComp.Fo2 = InspiredAirAtPres0.Fo2;
            inspAirComp.Fco2 = InspiredAirAtPres0.Fco2;
            inspAirComp.Fn2 = InspiredAirAtPres0.Fn2;
            
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
        
        // set the gas compliances temperatures and STP
        foreach (var tempSetting in TempSettings)
        {
            var comp = (GasCompliance)_model.Components.Find(i => i.Name == tempSetting.Name)!;
            comp.TempSTP = TempSTP;
            comp.PresSTP = PresSTP;
            comp.Temp = tempSetting.TargetTemp;
            comp.TargetTemp = tempSetting.TargetTemp;
        }
        
        // signal that the model component is initialized
        _initialized = true;
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
    public double Temp;
}

public struct TempSetting
{
    public string Name;
    public double TargetTemp;
}