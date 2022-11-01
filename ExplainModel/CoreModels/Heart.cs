using Explain.Helpers;

namespace Explain.CoreModels;

public class Heart: ICoreModel
{
    public string Name { get; set; }  = "";
    public string Description { get; set; }  = "";
    public string ModelType { get; set; } = "";
    public bool IsEnabled { get; set; } = false;
    
    public double NccVentricular { get; set; }
    public double NccAtrial { get; set; }
    public double EcgSignal { get; set; }
    public double MeasuredHeartRate { get; set; }
    public double Aaf { get;  set; }
    public double Vaf { get;  set; }
    public string[] AafRightTargets { get; set; }
    public string[] AafLeftTargets { get; set; }
    public string[] VafRightTargets { get; set; }
    public string[] VafLeftTargets { get; set; }
    public string[] Coronaries { get; set; }
    public double HeartRate { get; set; }
    public double HeartRateRef { get; set; }
    public double VenticularEscapeRate { get; set; }
    public double RhythmType { get; set; }
    public double PqTime { get; set; }
    public double AvDelay { get; set; }
    public double QrsTime { get; set; }
    public double QtTime { get; set; }
    public double CqtTime { get; set; }
    
    // private fields
    private Model _model;
    private List<IBloodCompliance> _aafRightTargets = new List<IBloodCompliance>();
    private List<IBloodCompliance> _aafLeftTargets = new List<IBloodCompliance>();
    private List<IBloodCompliance> _vafRightTargets = new List<IBloodCompliance>();
    private List<IBloodCompliance> _vafLeftTargets = new List<IBloodCompliance>();
    private List<IBloodCompliance> _coronaries = new List<IBloodCompliance>();
    private bool _initialized = false;
    private double _saNodePeriod = 0;
    private double _saNodeTimer = 0;
    private bool _pqRunning = false;
    private double _pqTimer = 0;
    private bool _ventricleIsRefractory = false;
    private bool _qrsRunning = false;
    private double _qrsTimer = 0;
    private bool _qtRunning = false;
    private double _qtTimer = 0;
    private int _pWaveSignalCounter = 0;
    private int _qrsWaveSignalCounter = 0;
    private int _qtWaveSignalCounter = 0;
    private double _measuredQrsCounter = 0;
    private double _measuredQrsTimer = 0;
    private const double NoOfBeats = 5;
    private const double Kn = 0.579;

    public void InitModel(Model model)
    {
        // store a reference to the model
        _model = model;

        // find the targets in the model
        foreach(var target in AafRightTargets)
        {
            var t = (IBloodCompliance)_model.Components.Find(i => i.Name == target)!;
            _aafRightTargets.Add(t);
        }
        foreach (var target in AafLeftTargets)
        {
            var t = (IBloodCompliance)_model.Components.Find(i => i.Name == target)!;
            _aafLeftTargets.Add(t);
        }
        foreach (var target in VafRightTargets)
        {
            var t = (IBloodCompliance)this._model.Components.Find(i => i.Name == target)!;
            _vafRightTargets.Add(t);
        }
        foreach (var target in VafLeftTargets)
        {
            var t = (IBloodCompliance)this._model.Components.Find(i => i.Name == target)!;
            _vafLeftTargets.Add(t);
        }
        foreach (var target in Coronaries)
        {
            var t = (IBloodCompliance)this._model.Components.Find(i => i.Name == target)!;
            _coronaries.Add(t);
        }

        // flag that the initialization is complete
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
        // calculate the qtc time depending on the heart rate
            CqtTime = Qtc() - QrsTime;

            // calculate the SA node interval in seconds depending on the heart rate
            _saNodePeriod = 60.0;
            if (HeartRate > 0)
            {
                _saNodePeriod = 60.0 / HeartRate;
            }

            // has the sa node time elapsed?
            if (_saNodeTimer > _saNodePeriod)
            {
                // reset the sa node timer
                _saNodeTimer = 0;
                // signal that the pq time starts running
                _pqRunning = true;
                // reset the atrial activation curve counter
                NccAtrial = -1;
            }

            // has the pq time elapsed?
            if (_pqTimer > PqTime)
            {
                // reset the pq timer
                _pqTimer = 0;
                // signal that the pq timer has stopped
                _pqRunning = false;
                // check whether the ventricles are in a refractory state
                if (!_ventricleIsRefractory)
                {
                    // signal that the qrs time time starts running
                    _qrsRunning = true;
                    // reset the ventricular activation curve
                    NccVentricular = -1;
                    // increase the measured qrs counter with 1 beat
                    _measuredQrsCounter += 1;
                }
            }

            // has the qrs time elapsed?
            if (_qrsTimer > QrsTime)
            {
                // reset the qrs timer
                _qrsTimer = 0;
                // signal that the qrs timer has stopped
                _qrsRunning = false;
                // signal that qt time starts running
                _qtRunning = true;
                // signal that the ventricles are going into a refractory state
                _ventricleIsRefractory = true;
            }

            // has the qt time elapsed?
            if (_qtTimer > CqtTime)
            {
                // reset the qt time counter
                _qtTimer = 0;
                // signal that the qt timer has stopped
                _qtRunning = false;
                // signal that the ventricles are coming out of their refractory state
                _ventricleIsRefractory = false;
            }

            // increase the ecg timers
            _saNodeTimer += _model.ModelingStepsize;

            // increase the pq timer if the pq timer is running
            if (_pqRunning)
            {
                _pqTimer += _model.ModelingStepsize;
                // increase the p-wave signal counter
                _pWaveSignalCounter += 1;
                // build the p wave
                BuildPWave();
            } else
            {
                // reset the p wave
                _pWaveSignalCounter = 0;
            }

            // increase the qrs timer if the qrs timer is running
            if (_qrsRunning)
            {
                _qrsTimer += _model.ModelingStepsize;
                // increase the qrs-wave signal counter
                _qrsWaveSignalCounter += 1;
                // build the qrs wave
                BuildQrsWave();
            }
            else
            {
                // reset the qrs wave
                _qrsWaveSignalCounter = 0;
            }

            // increase the qt timer if the qt timer is running
            if (_qtRunning)
            {
                _qtTimer += _model.ModelingStepsize;
                // increase the qt-wave signal counter
                _qtWaveSignalCounter += 1;
                // build the qrs wave
                BuildTWave();
            }
            else
            {
                // reset the qrs wave
                _qtWaveSignalCounter = 0;
            }

            // if nothing is running then there's no electrical signal
            if (!_pqRunning && !_qrsRunning && !_qtRunning)
            {
                EcgSignal = 0;
            }

            // calculate the heartrate every 5 qrs complexes
            if (_measuredQrsCounter > NoOfBeats)
            {
                // calculate the measured heart rate depending how long it took to counter no_beats.
                MeasuredHeartRate = 60.0 / (_measuredQrsTimer / _measuredQrsCounter);
                // reset the counter
                _measuredQrsCounter = 0;
                // reset the timer
                _measuredQrsTimer = 0;
                
            }
            // increase the time counter for the measured heart rate routine
            _measuredQrsTimer += _model.ModelingStepsize;

            // increase the heart activation function counters
            NccAtrial += 1;
            NccVentricular += 1;

            // calculate the varying elastance function
            CalcVaryingElastanceFactor();

            // transfer the calculated activatino factors to the target components
            TransferActivationFactors();

    }
    
    private double Qtc()
    {
        if (HeartRate > 10)
        {
            return QtTime * Math.Sqrt(60.0 / HeartRate);
        } else
        {
            return QtTime * Math.Sqrt(6.0);
        }
    }
    
    private void CalcVaryingElastanceFactor()
    {
        var t = _model.ModelingStepsize;
        var atrialDuration = PqTime;
        var ventricularDuration = CqtTime + QrsTime;
        
        if (NccAtrial >= 0 && NccAtrial < atrialDuration / t)
        {
            Aaf = Math.Sin(Math.PI * (NccAtrial / (atrialDuration / t)));
        } else
        {
            Aaf = 0;
        }
        if (NccVentricular >= 0 && NccVentricular < ventricularDuration / t)
        {
            var f = NccVentricular / (Kn * (ventricularDuration / t ));
            Vaf = f * Math.Sin(Math.PI * (NccVentricular / (ventricularDuration / t)));
        } else
        {
            Vaf = 0;
        }
    }
    
    private void TransferActivationFactors()
    {
        // transfer the activation factors to the targets
        foreach (var target in _aafLeftTargets)
        {
            target.ActFactor = Aaf;
        }
        foreach (var target in _aafRightTargets)
        {
            target.ActFactor = Aaf;
        }
        foreach (var target in _vafLeftTargets)
        {
            target.ActFactor = Vaf;
        }
        foreach (var target in _vafRightTargets)
        {
            target.ActFactor = Vaf;
        }
        foreach (var target in _coronaries)
        {
            target.ActFactor = Vaf;
        }
    }
    
    private void BuildPWave()
    {

    }

    private void BuildQrsWave()
    {

    }

    private void BuildTWave()
    {

    }
}