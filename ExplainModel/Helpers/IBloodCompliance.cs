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
    // oxygenation
    public double To2 { get; set; }
    public double Po2 { get; set; }
    public double So2 { get; set; }
    public double Hb { get; set; }
    public double Dpg { get; set; }
    // acid base
    public double Tco2 { get; set; }
    public double Ph { get; set; }
    public double Pco2 { get; set; }
    public double Hco3 { get; set; }
    public double Be { get; set; }

    // Solutes
    public BloodCompound[] Solutes { get; set; }
    
    public void VolumeIn(double dvol, IBloodCompliance compFrom);
    
    public double VolumeOut(double dvol);
    
}