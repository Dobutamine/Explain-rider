using Explain.Helpers;

namespace Explain.CoreModels;

public class Drugs: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public void InitModel(Model model) { }
    public void StepModel() { }
    public void CalcModel() { }
}