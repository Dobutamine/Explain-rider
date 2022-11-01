namespace Explain.Helpers;

public interface IGasCompliance
{
    public double Pres { get; set; }
    public double PresAtm { get; set; }
    public double PresMax { get; set; }
    public double PresMin { get; set; }
    public double Vol { get; set; }
    public double VolMax { get; set; }
    public double VolMin { get; set; }
    public double ActFactor { get; set; }
    public void VolumeIn(double dvol, IGasCompliance compFrom);
    
    public double VolumeOut(double dvol);
    
}