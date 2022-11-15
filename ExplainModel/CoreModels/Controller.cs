namespace Explain.CoreModels;

// This is class is an abstraction of a controller using a activation function as described by van Meurs (2011)
public class Controller
{
    public double SetPoint { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Gain { get; set; }
    public double TimeConstant { get; set; }
    public double Output { get; set; }
    public double UpdateInterval { get; set; }
    private double _activation;
    
    public Controller(double setPoint, double min, double max, double gain, double timeConstant, double updateInterval)
    {
        SetPoint = setPoint;
        Min = min;
        Max = max;
        Gain = gain;
        TimeConstant = timeConstant;
        UpdateInterval = updateInterval;
    }
    public double Update(double sensorValue)
    {
        // calculate the sensor activity
        var a = ActivationFunction(sensorValue);
            
        // calculate the sensor output
        _activation = UpdateInterval * ((1.0 / TimeConstant) * (-_activation + a)) + _activation;
            
        // Apply the gain
        Output = _activation * Gain;
        
        // return the sensor output times the gain of this controller
        return Output;
    }
    private double ActivationFunction(double value)
    {
        double a = 0;
        
        if (value >= Max)
        {
            a = Max - SetPoint;
        }
        else
        {
            if (value <= Min)
            {
                a = Min - SetPoint;
            }
            else
            {
                a = value - SetPoint;
            }
            
        }
        
        return a;
    }
}