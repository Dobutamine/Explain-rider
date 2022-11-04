using Explain.Helpers;

namespace Explain.CoreModels;

public class Environment: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public double PresAtm { get; set; }
    public double Temp { get; set; }
    private Model? _model;
    private bool _initialized;

    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // apply the atmospheric pressure to all blood and gas compliances
        foreach (var comp in _model.Components)
        {
            switch (comp.ModelType)
            {
                case "BloodCompliance":
                    ((BloodCompliance)comp).Pres0 = 0;
                    break;
                case "BloodTimeVaryingElastance":
                    ((BloodTimeVaryingElastance)comp).Pres0 = 0;
                    break;
                case "GasCompliance":
                    ((GasCompliance)comp).Pres0 = PresAtm;
                    break;
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
    public void CalcModel() { }
}