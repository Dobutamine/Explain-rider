namespace Explain.Helpers;

public interface ICoreModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ModelType { get; set; }
    public bool IsEnabled { get; set; }
    
    public void InitModel(Model model) { }
    public void StepModel() { }
    public void CalcModel() { }
}