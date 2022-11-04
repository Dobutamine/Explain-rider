namespace Explain.Helpers;

public interface IGasCompliance
{
    public double Pres { get; set; }
    public double Pres0 { get; set; }
    public double PresSTP { get; set; }
    public double PresMax { get; set; }
    public double PresMin { get; set; }
    public double Vol { get; set; }
    public double VolMax { get; set; }
    public double VolMin { get; set; }
    public double ActFactor { get; set; }
    public double CTotal { get; set; }
    public double Co2 { get; set; }
    public double Cco2 { get; set; }
    public double Cn2 { get; set; }
    public double Ch2O { get; set; }
    public double Ph2O { get; set; }
    public double Po2 { get; set; }
    public double Pco2 { get; set; }
    public double Pn2 { get; set; }
    public double FTotal { get; set; }
    public double Fh2O { get; set; }
    public double Fo2 { get; set; }
    public double Fco2 { get; set; }
    public double Fn2 { get; set; }

    public double Temp { get; set; }
    public double TempSTP { get; set; }
    public bool FixedComposition { get; set; }
    
    public void VolumeIn(double dvol, IGasCompliance compFrom);
    
    public double VolumeOut(double dvol);
    
}