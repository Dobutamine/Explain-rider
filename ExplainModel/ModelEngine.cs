using System.Diagnostics;
using Explain.CoreModels;
using Explain.Helpers;

namespace Explain;
using System.Timers;
public class ModelEngine
{
    public bool EngineRunning { get; set; } = false;
    
    public double RtUpdateInterval { get; set; } = 0.015;
    
    private double _rtSteps = 30;
    private readonly Timer _rtTimer;
    private readonly Model _model;
    public ModelEngine(Model model)
    {
        _model = model;
        // initialize the timers
        _rtTimer = new System.Timers.Timer(RtUpdateInterval);
        _rtTimer.Elapsed += RtStep!;
        _rtTimer.AutoReset = true;
 
    }
    public void Start()
    {
        EngineRunning = true;
        _rtSteps = RtUpdateInterval / _model.ModelingStepsize;
        _rtTimer.Enabled = true;
    }

    private void RtStep(object source, ElapsedEventArgs e)
    {
        for (var i = 0; i < _rtSteps; i++)
        {
            StepModel();
        }

    }

    public void Stop()
    {
        EngineRunning = false;
        _rtTimer.Enabled = false;

    }
    
    public Performance Calculate(double timeToCalculate = 10.0)
    {
        // declare a stopwatch for performance testing
        var stopwatch = new Stopwatch();
        
        // start the stopwatch
        stopwatch.Start();
        
        // calculate the number of steps needed
        var noSteps = (int) (timeToCalculate / _model.ModelingStepsize);

        // do the calculation
        for (var i = 0; i < noSteps; i++)
        {
            StepModel();
        }
        
        // stop the stopwatch
        stopwatch.Stop();
        
        // calculate the performance data
        var noMs = stopwatch.ElapsedMilliseconds;
        var stepTime = noMs / (timeToCalculate / _model.ModelingStepsize);
        
        var performance = new Performance
        {
            TimeInterval = timeToCalculate,
            NoSteps = noSteps,
            CalcTimeTotal = noMs,
            StepTime = stepTime
            
        };

        return performance;
    }
    
    private void StepModel()
    {
        // iterate over all models in the component list and execute the a model step
        foreach (var component in _model.Components)
        {
            component.StepModel();
        }

        // update the data collector
        _model.DataCollector.Update();

        // update the hardware interface
        _model.HardwareInterface.StepModel();

        // increase model time
        _model.ModelTimeTotal += _model.ModelingStepsize;
    }
}
