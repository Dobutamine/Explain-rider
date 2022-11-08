using System.Diagnostics.Contracts;
using Explain.Helpers;

namespace Explain.CoreModels;

public class Metabolism: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public double Vo2 { get; set; }
    public double RespQ { get; set; }
    public Comp[] Comps { get; set; } = Array.Empty<Comp>();
    
    private Model? _model;
    
    private bool _initialized;
    public List<ActiveComp> ActiveComps { get; set; } = new List<ActiveComp>();
    
    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;

        // find all the active blood compliances
        foreach (var comp in Comps)
        {
            var b = new ActiveComp
            {
                Comp = (IBloodCompliance)_model.Components.Find(i => i.Name == comp.Name)!,
                Name = comp.Name,
                FVo2 = comp.FVo2
            };
            ActiveComps.Add(b);
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

    public void CalcModel()
    {
        // translate the VO2 in ml/kg/min to VO2 in mmol for this stepsize (assumption is 37 degrees and atmospheric pressure)
        var vo2Step = ((0.039 * Vo2 * _model.Weight) / 60) * _model.ModelingStepsize;
        
        // do the metabolism for each active blood compliance
        foreach (var activeComp in ActiveComps)
        {
            // get the to2 from the blood compartment
            var to2 = activeComp.Comp.Solutes[0].Conc;
            // calculate the change in oxygen concentration in this step
            var dTo2 = (vo2Step * activeComp.FVo2);
            // calculate the new oxygen concentration in blood
            activeComp.Comp.Solutes[0].Conc = (to2 * activeComp.Comp.Vol - dTo2) / activeComp.Comp.Vol;
            // Guard against negative oxygen concentrations.
            if (activeComp.Comp.Solutes[0].Conc < 0)
            {
                activeComp.Comp.Solutes[0].Conc = 0;
            }
            // get the tco2 from the blood compartment
            var tco2 = activeComp.Comp.Solutes[1].Conc;
            // calculate the change in co2 concentration in this step
            var dTco2 = (vo2Step * activeComp.FVo2 * RespQ);
            // calculate the new co2 concentration in blood
            activeComp.Comp.Solutes[1].Conc = (tco2 * activeComp.Comp.Vol + dTco2) / activeComp.Comp.Vol;
            // Guard against negative oxygen concentrations.
            if (activeComp.Comp.Solutes[1].Conc < 0)
            {
                activeComp.Comp.Solutes[1].Conc = 0;
            }
        }
    }
    
    public void Co2Storage() {}
}

public struct Comp
{
    public string Name;
    public double FVo2;
}

public class ActiveComp
{
    public string Name;
    public IBloodCompliance Comp;
    public double FVo2;
}