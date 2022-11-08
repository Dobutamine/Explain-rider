using Explain.Helpers;

namespace Explain.CoreModels;

public class Blood: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public BloodCompound[] Solutes { get; set; } = Array.Empty<BloodCompound>();
    private Model? _model;

    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // initialize the solutes in all blood containing models
        foreach (var c in _model.Components)
        {
            switch (c.ModelType)
            {
                case "BloodCompliance":
                    ((BloodCompliance)c).Solutes = (BloodCompound[]) Solutes.Clone();
                    break;
                case "BloodTimeVaryingElastance":
                    ((BloodTimeVaryingElastance)c).Solutes = (BloodCompound[]) Solutes.Clone();
                    break;
            }
        }
    }

    public void StepModel() { }
    public void CalcModel() { }
}