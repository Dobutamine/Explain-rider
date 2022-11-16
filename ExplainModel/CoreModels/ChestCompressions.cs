using Explain.Helpers;

namespace Explain.CoreModels;

public class ChestCompressions: ICoreModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ModelType { get; set; }
    public bool IsEnabled { get; set; }
    
    private Model? _model;
    private double _t;
    private bool _initialized;
    
    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // store a reference to the modeling step size for easy referencing
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
        
    }

}