using Explain.Helpers;

namespace Explain.CoreModels;

public class AcidBase : ICoreModel
{
  public string Name { get; set; } = "";
  public string Description { get; set; } = "";
  public string ModelType { get; set; } = "";
  public bool IsEnabled { get; set; }

  // set the brent root finding properties
  private double _brentAccuracy = 1e-8;
  private double _maxIterations = 100;
  private double _leftHp = Math.Pow(10.0, -7.8) * 1000.0;
  private double _rightHp = Math.Pow(10.0, -6.8) * 1000.0;

  // set the acid base constant
  private double _kw = Math.Pow(10.0, -13.6) * 1000.0;
  private double _kc = Math.Pow(10.0, -6.1) * 1000.0;
  private double _kd = Math.Pow(10.0, -10.22) * 1000.0;
  private double _alphaCo2P = 0.03067;

  // bloodgas
  private double _tco2 = 0;
  private double _sid = 0;
  private double _albumin = 0;
  private double _phosphates = 0;
  private double _uma = 0;
  private double _pH = 0;
  private double _pco2 = 0;
  private double _hco3 = 0;
  private double _cco2 = 0;
  private double _cco3 = 0;
  private double _oh = 0;

  private double _be = 0;

  private Model? _model;
  private BrentRootFinding Brent;
  private bool _initialized = false;
  private double _t = 0.0005;

  public void InitModel(Model model)
  {
    // store a reference to the whole model
    _model = model;

    // store the stepsize for easy referencing
    _t = _model.ModelingStepsize;
    
    // instantiate a brent root finding object
    Brent = new BrentRootFinding();

    // signal that the model component is initialized
    _initialized = true;
  }

  public void StepModel()
  {
    // don't call us, we call you
  }

  public void CalcModel()
  {
  }

  private BloodGas CalcAcidBase(double tco2, double sid = 35.9, double alb = 25.0, double pi = 1.64, double u = 0.0)
  {
    // set new blood gas
    var bg = new BloodGas
    {
      // assume an error
      Error = true,
      Iterations = 0
    };

    // set the parameters
    _tco2 = tco2;
    _albumin = alb;
    _phosphates = pi;
    _sid = sid;
    _uma = u;

    // find the hp concentration
    var hp = Brent.Brent(NetChargePlasma, _leftHp, _rightHp, _maxIterations, _brentAccuracy);

    bg.Iterations = hp.Iterations;
    bg.Error = hp.Error;
    
    // get the result
    if (!bg.Error)
    {
      bg.Ph = _pH;
      bg.Pco2 = _pco2;
      bg.Hco3 = _hco3;
      bg.Be = _be;
      
    }
    
    return bg;
  }

  private double NetChargePlasma(double h)
  {
    //calculate the plasma co2 concentration based on the total co2 in the plasma, hydrogen concentration and the constants Kc and Kd
    _cco2 = _tco2 / (1.0 + (_kc / h) + (_kc * _kd) / (Math.Pow(h, 2.0))); // Eq. 6
    // calculate the plasma hco3(-) concentration (bicarbonate)
    _hco3 = (_kc * _cco2) / h; // Eq. 3
    // calculate the plasma co3(2-) concentration (carbonate)
    _cco3 = (_kd * _hco3) / h; // Eq. 4
    // calculate the plasma OH(-) concentration (water dissociation)
    _oh = _kw / h; // Eq. 7
    // calculate the pco2 of the plasma
    _pco2 = _cco2 / _alphaCo2P; // Eq. 13
    // calculate the pH
    _pH = Math.Log10(h / 1000.0); // Eq. 9
    // calculate the weak acids (albumin and phosphates)
    var a = _albumin * (0.123 * _pH - 0.631) + _phosphates * (0.309 * _pH - 0.469); // Eq. 8
    var ac = h - _hco3 - a - _oh - (2 * _cco3); // Eq. 10
    // calculate the net charge of the plasma. If the netcharge is zero than the current hp_estimate is the correct one.
    var nc = ac + _sid - _uma; // Eq. 12
    // return the net charge
    return nc;
  }
}

public struct BloodGas
{
  public double Ph;
  public double Pco2;
  public double Hco3;
  public double Be;
  public double Iterations;
  public bool Error;
}