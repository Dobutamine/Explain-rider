using CodexMicroORM.Core;
using Explain.Helpers;

namespace Explain.CoreModels;

public class Myocardium: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    
    public string HeartModelName { get; set; }
    private Heart? _heart;
    public string[] HeartVentricles { get; set; } = Array.Empty<string>();
    private List<BloodTimeVaryingElastance> _heartVentricles = new List<BloodTimeVaryingElastance>();

    public string LeftVentricleName { get; set; } = "";
    public string RightVentricleName { get; set; } = "";
    public string CoronariesName { get; set; } = "";
    
    public BloodTimeVaryingElastance? Lv;
    public BloodTimeVaryingElastance? Rv;
    public BloodTimeVaryingElastance? Cor;
    public double LvStrokeWork { get; set; }
    public double RvStrokeWork { get; set; }
    
    private Model? _model;
    private double _t;
    private bool _initialized;
    private double _swLv = 0;
    private double _swRv = 0;
    
    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // store the modeling step size for easy referencing
        _t = _model.ModelingStepsize;
        
        // find a reference to the necessary models
        _heart = (Heart)_model.Components.Find(i => i.Name == HeartModelName)!;
        
        Lv = (BloodTimeVaryingElastance)_model.Components.Find(i => i.Name == LeftVentricleName)!;
        Rv = (BloodTimeVaryingElastance)_model.Components.Find(i => i.Name == RightVentricleName)!;
        Cor = (BloodTimeVaryingElastance)_model.Components.Find(i => i.Name == CoronariesName)!;

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
        var ventricularDuration = _heart.CqtTime +_heart.QrsTime;
        
        // calculate the stroke work during the ventricular contraction
        if (_heart.NccVentricular >= 0 && _heart.NccVentricular < ventricularDuration / _t)
        {
            _swLv += (Lv.Vol * Lv.Pres);
            _swRv += (Rv.Vol * Rv.Pres);
        }
        
        if (Math.Abs(_heart.NccVentricular - 1) < 0.00001)
        {
            // stroke work in l * mmHg.    1 Joule = 1 Pa * m^3
            LvStrokeWork = _swLv * _heart.HeartRate;
            RvStrokeWork = _swRv * _heart.HeartRate;
            _swLv = 0;
            _swRv = 0;
        }
        
        // convert stroke work to Joule where 1 Joule = 1 Pa * m^3 => 1 J = 7.5 mmHg * l.
        // 1 kcal/hour = 3.5 ml O2/min  and 1 kcal = 4184 J
        // 1 J/min = 0.00001394 ml O2/min
        var mlO2Min = (LvStrokeWork + RvStrokeWork) * 7.5 * 0.00001394;
        
        // translate the VO2 in ml/min to VO2 in mmol for this stepsize (assumption is 37 degrees and atmospheric pressure)
        var dTo2 = ((0.039 * mlO2Min) / 60) * _t;
        
        // calculate the new oxygen concentration in the coronaries
        Cor!.Solutes[0].Conc = (Cor.Solutes[0].Conc * Cor.Vol - dTo2 / 1000.0) / Cor.Vol;
        if (Cor.Solutes[0].Conc < 0)
        {
            Cor.Solutes[0].Conc = 0;
        }
        
        // calculate the influence of ischemia on the heart period and contractility using a activation curve
        
    }
    
}