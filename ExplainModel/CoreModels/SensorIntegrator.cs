using Explain.Helpers;

namespace Explain.CoreModels;

public class SensorIntegrator: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    
    private Model? _model;
    private bool _initialized;

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
    public void CalcModel() { }
}