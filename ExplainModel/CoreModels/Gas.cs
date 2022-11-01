using Explain.Helpers;

namespace Explain.CoreModels;

public class Gas: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    
    public bool GasConstant { get; set; }
    public bool TempRoom { get; set; }
    public bool Patm { get; set; }
    
    public void InitModel(Model model) { }
    public void StepModel() { }
    public void CalcModel() { }
}