namespace Explain.Helpers;

public interface IBloodCompliance
{
    public double Pres { get; set; }
    public double Pres0 { get; set; }
    public double PresMax { get; set; }
    public double PresMin { get; set; }
    public double Vol { get; set; }
    public double VolMax { get; set; }
    public double VolMin { get; set; }
    public double ActFactor { get; set; }


    public BloodCompound[] Solutes { get; set; }
    
    public void VolumeIn(double dvol, IBloodCompliance compFrom);
    
    public double VolumeOut(double dvol);
    
}