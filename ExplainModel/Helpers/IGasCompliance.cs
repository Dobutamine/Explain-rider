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
    public double CTotal { get; set; }
    public double CO2 { get; set; }
    public double CCO2 { get; set; }
    public double CN2 { get; set; }
    public double CH2O { get; set; }
    public double PH2O { get; set; }
    public double PO2 { get; set; }
    public double PCO2 { get; set; }
    public double PN2 { get; set; }
    public double FTotal { get; set; }
    public double FH2O { get; set; }
    public double FO2 { get; set; }
    public double FCO2 { get; set; }
    public double FN2 { get; set; }
    public double FH2ODry { get; set; }
    public double FO2Dry { get; set; }
    public double FCO2Dry { get; set; }
    public double FN2Dry { get; set; }
    public double Temp { get; set; }
    public double TempEffect { get; set; }
    
    public void VolumeIn(double dvol, IGasCompliance compFrom);
    
    public double VolumeOut(double dvol);
    
}