namespace Explain.Helpers;

public interface ICompliance
{
    public double Pres { get; set; }
    public double Pres0 { get; set; }
    public double PresExt { get; set; }
    public double PresCc { get; set; }
    public double PresMus { get; set; }
    public double PresMax { get; set; }
    public double PresMin { get; set; }
    public double Vol { get; set; }
    public double VolMax { get; set; }
    public double VolMin { get; set; }
    public double ActFactor { get; set; }
}