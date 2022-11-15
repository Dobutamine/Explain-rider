using CodexMicroORM.Core.Helper;
using Explain.Helpers;

namespace Explain.CoreModels;

public class Sensor
{
    public double SetPoint { get; set; }
    public double Sensitivity { get; set; }
    public double TimeConstant { get; set; }
    public double Output { get; set; }
    public double UpdateInterval { get; set; }
    
    public Sensor(double setPoint, double sensitivity, double timeConstant, double updateInterval)
    {
        SetPoint = setPoint;
        Sensitivity = sensitivity;
        TimeConstant = timeConstant;
        UpdateInterval = updateInterval;
    }
    public double Update(double sensorValue)
    {
        // calculate the sensor activity
        var expFactor = Math.Exp((sensorValue - SetPoint) / Sensitivity);
        var activity = expFactor / ( 1 + expFactor);
            
        // calculate the sensor output
        Output = UpdateInterval * ((1.0 / TimeConstant) * (-Output + activity)) + Output;
            
        // return the sensor output
        return Output;
    }
}