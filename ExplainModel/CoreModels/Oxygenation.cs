using Explain.Helpers;

namespace Explain.CoreModels;

public class Oxygenation: ICoreModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; }
    
    // set the brent root finding properties
    private readonly double _brentAccuracy = 1e-8;
    private readonly double _maxIterations = 100.0;
    private readonly double _leftO2 = 0.01;
    private readonly double _rightO2 = 100.0;
    
    // oxygenation constants
    private double _mmolToMl = 22.2674;
    
    private BrentRootFinding _brent = new BrentRootFinding();
    private bool _initialized;
    private const double GasConstant = 62.36367; // L·mmHg/mol·K
    
    private double _to2;
    private double _ph;
    private double _temp;
    private double _be;
    private double _dpg;
    private double _hemoglobin;
    private double _so2;
    private readonly double _po2 = 0;
    private double _pres = 760;

    public void InitModel(Model model)
    {
        // instantiate a brent root finding object
        _brent = new BrentRootFinding();

        // signal that the model component is initialized
        _initialized = true;
    }
    public void StepModel() { }
    public void CalcModel() { }

    public OxyResult CalcOxygenation(double to2, double hb = 8.0, double temp = 37.0, double ph = 7.40,  double dpg = 5, double be = 0.0, double pres = 760)
    {
        // set new oxygenation result
        var oxy = new OxyResult
        {
            // assume an error
            Iterations = 0,
            Error = true
        };

        if (!_initialized) return oxy;
        
        // set the parameters
        _to2 = to2;
        _hemoglobin = hb;
        _temp = temp;
        _be = be;
        _dpg = dpg;
        _ph = ph;
        _pres = pres;
        
        // calculate the po2 from the to2 using a brent root finding function and oxygen dissociation curve
        var hp = _brent.Brent(OxygenContent, _leftO2, _rightO2, _maxIterations, _brentAccuracy);
        oxy.Iterations = hp.Iterations;
        oxy.Error = hp.Error;
        
        // get the result
        if (!hp.Error)
        {
            oxy.Po2 = _po2;
            oxy.So2 = _so2;
        }
        return oxy;
    }
    private double OxygenContent(double po2) {
        // calculate the saturation from the current po2 from the current po2 estimate
        _so2 = this.OxygenDissociationCurve(po2);

        // calculate the to2 from the current po2 estimate
        // convert the hemoglobin unit from mmol/l to g/dL
        // convert the po2 from kPa to mmHg
        // convert to output from ml O2/dL blood to ml O2/l blood
        var to2NewEstimate = (0.0031 * (po2 / 0.1333) + 1.36 * (_hemoglobin / 0.6206) * _so2) * 10.0;

        // convert the ml O2/l to mmol/l with the gas law with (GasConstant * (273.15 + _temp)) / Pres) / to2 (mol/l)
        _mmolToMl = GasConstant * (273.15 + _temp) / _pres;
        
        // calculate ml O2 / ml blood.
        to2NewEstimate /= _mmolToMl;

        // calculate the difference between the real to2 and the to2 based on the new po2 estimate and return it to the brent root finding function
        var dto2 = _to2 - to2NewEstimate;
        
        // return the difference
        return dto2;
    }
    
    private double OxygenDissociationCurve(double po2) {
        // calculate the saturation from the po2 depending on the ph, be, temperature and dpg level.
        var a = 1.04 * (7.4 - _ph) + 0.005 * _be + 0.07 * (_dpg - 5.0);
        var b = 0.055 * (_temp + 273.15 - 310.15);
        const double y0 = 1.875;
        var x0 = 1.875 + a + b;
        var h0 = 3.5 + a;
        const double k = 0.5343;
        var x = Math.Log(po2, Math.E);
        var y = x - x0 + h0 * Math.Tanh(k * (x - x0)) + y0;

        // return the o2 saturation
        return 1.0 / (Math.Pow(Math.E, -y) + 1.0);
    }
    
    public struct OxyResult
    {
        public double Po2;
        public double So2;
        public double Iterations;
        public bool Error;
    }
}