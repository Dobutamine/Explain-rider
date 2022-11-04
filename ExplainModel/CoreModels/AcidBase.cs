using Explain.Helpers;

namespace Explain.CoreModels;

public class AcidBase: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    public string[] Components { get; set; } = Array.Empty<string>();
    public double Sid { get; set; }
    public double Uma { get; set; }
    public double Albumin { get; set; }
    public double Phosphates { get; set; }
    public double Tco2 { get; set; }

    // set the brent root finding properties
    private double _brentAccuracy = 1e-8;
    private double _maxIterations = 100; 
    private double _leftHp = Math.Pow(10.0, -7.8) * 1000.0;
    private double _rightHp = Math.Pow(10.0, -6.8) * 1000.0;
    
    // set the acid base constant
    private double _kw =  Math.Pow(10.0, -13.6) * 1000.0;
    private double _kc = Math.Pow(10.0, -6.1) * 1000.0;
    private double _kd = Math.Pow(10.0, -10.22) * 1000.0;
    private double _alphaCo2P = 0.03067;

    private Model? _model;
    private bool _initialized = false;
    private double _t = 0.0005;
    private readonly List<IBloodCompliance> _components = new List<IBloodCompliance>();

    public void InitModel(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // store the stepsize for easy referencing
        _t = _model.ModelingStepsize;
        
        // find the relevant components and store references 
        foreach (var comp in Components)
        {
            var bc = (IBloodCompliance)_model.Components.Find(i => i.Name == comp)!;
            _components.Add(bc);
        }
        
        // signal that the model component is initialized
        _initialized = true;
    }
    
    public void StepModel() { }
    public void CalcModel() { }

    private void Acidbase(IBloodCompliance comp)
    {
      // set and check the independent properties (tco2, sid, uma, albumin and phosphates)
      Tco2 = 0;
      Sid = 0;
      Uma = 0;
      Albumin = 0;
      Phosphates = 0;
      
      // find the hp concentration
      var hp = Brent(NetChargePlasma, _leftHp, _rightHp, _maxIterations, _brentAccuracy);
     
      // check the result
      if (!hp.Error)
      {
        Console.WriteLine("Solution found!");
      }
    }

    private double NetChargePlasma(double hpEstimate)
    {
      // calculate the pH
      var pH = Math.Log10(hpEstimate / 1000.0);
      //calculate the plasma co2 concentration based on the total co2 in the plasma, hydrogen concentration and the constants Kc and Kd
      var cco2P = Tco2 / (1.0 + _kc / hpEstimate + (_kc * _kd) / Math.Pow(hpEstimate, 2.0));
      // calculate the plasma hco3(-) concentration (bicarbonate)
      var hco3P = (_kc * cco2P) / hpEstimate;
      // calculate the plasma co3(2-) concentration (carbonate)
      var co3P = (_kd * hco3P) / hpEstimate;
      // calculate the plasma OH(-) concentration (water dissociation)
      var ohp = _kw / hpEstimate;
      // calculate the pco2 of the plasma
      var pco2P = cco2P / _alphaCo2P;
      // calculate the weak acids (albumin and phosphates)
      var aBase = Albumin * (0.123 * pH - 0.631) + Phosphates * (0.309 * pH - 0.469);
      // calculate the net charge of the plasma. If the netcharge is zero than the current hp_estimate is the correct one.
      var netcharge = hpEstimate + Sid - hco3P - 2.0 * co3P - ohp - aBase - Uma;
      
      return netcharge;
    }

    private static BrentResult Brent(Func<double, double> func, double lowerLimit, double upperLimit, double maxIter, double accuracy)
    {
      var a = lowerLimit;
      var b = upperLimit;
      var c = a;
      var fa = func(a);
      var fb = func(b);
      var fc = fa;

      // define a result object
    var result = new BrentResult();
    var setMaxIterations = maxIter;
    
    while (maxIter-- > 0) {
      var prevStep = b - a;

      if (Math.Abs(fc) < Math.Abs(fb)) {
        // Swap data for b to be the best approximation
        a = b;
        b = c;
        c = a;
        fa = fb;
        fb = fc; 
        fc = fa;
      }

      var tolAct = 1e-15 * Math.Abs(b) + accuracy / 2;
      var newStep = (c - b) / 2;

      if (Math.Abs(newStep) <= tolAct || fb == 0) {
        result.Result = b;
        result.Error = false;
        result.Iterations = setMaxIterations - maxIter;
        return result; // Acceptable approx. is found
      }
      // Decide if the interpolation can be tried
      if (Math.Abs(prevStep) >= tolAct && Math.Abs(fa) > Math.Abs(fb)) {
        // If prev_step was large enough and was in true direction, Interpolatiom may be tried
        var cb = c - b;
        double p = 0;
        double q = 0;
        double t1 = 0;
        double t2 = 0;
        if (a.Equals(c)) {
          // If we have only two distinct points linear interpolation can only be applied
          t1 = fb / fa;
          p = cb * t1;
          q = 1.0 - t1;
        } else {
          // Quadric inverse interpolation
          q = fa / fc;
          t1 = fb / fc;
          t2 = fb / fa;
          p = t2 * (cb * q * (q - t1) - (b - a) * (t1 - 1));
          q = (q - 1) * (t1 - 1) * (t2 - 1);
        }

        if (p > 0) {
          q = -q; // p was calculated with the opposite sign; make p positive
        } else {
          p = -p; // and assign possible minus to q
        }

        if ( p < 0.75 * cb * q - Math.Abs(tolAct * q) / 2 && p < Math.Abs((prevStep * q) / 2)) 
        {
          // If (b + p / q) falls in [b,c] and isn't too large it is accepted
          newStep = p / q;
        }

        // If p/q is too large then the bissection procedure can reduce [b,c] range to more extent
      }

      if (Math.Abs(newStep) < tolAct) {
        // Adjust the step to be not less than tolerance
        newStep = newStep > 0 ? tolAct : -tolAct;
      }

      // Save the previous approx.
      a = b;
      fa = fb;
      b += newStep;
      fb = func(b); // Do step to a new approxim.

      if ((fb > 0 && fc > 0) || (fb < 0 && fc < 0))
      {
        c = a;
        fc = fa; // Adjust c for it to have a sign opposite to that of b
      }
    }

    // configure the return object if not within range
    result.Result = -1;
    result.Error = true;
    result.Iterations = setMaxIterations - maxIter;
    return result; // No acceptable approximation. is found
    }
}

public struct BrentResult
{
  public double Result;
  public double Iterations;
  public bool Error;
}