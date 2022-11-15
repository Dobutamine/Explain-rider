using Explain.CoreModels;

namespace Explain.Helpers;

public class DataInterface
{
    public const string AbpCompartment = "AA";
    private readonly IBloodCompliance? _abp;
    public const string AdCompartment = "AD";
    private readonly IBloodCompliance? _ad;
    public const string PapCompartment = "PA";
    private readonly IBloodCompliance? _pap;
    public const string CvpCompartment = "RA";
    private readonly IBloodCompliance? _cvp;
    public const string HeartModel = "Ecg";
    private readonly Heart? _heart;
    public const string BreathingModel = "Breathing";
    private readonly Breathing? _breathing;
    public const string EtCo2Compartment = "DS";
    private readonly GasCompliance? _etco2;
    public readonly string[] ChestCompartments = {"CHEST_L", "CHEST_R"};
    private readonly List<Container> _lungs = new List<Container>();
    
    private readonly Model? _model;

    public DataInterface(Model model)
    {
        // store a reference to the model
        _model = model;
        
        // find the model components and get a reference to them to extract the needed signals
        _heart = (Heart)_model?.Components.Find(i => i.Name == HeartModel)!;
        _breathing = (Breathing)_model?.Components.Find(i => i.Name == BreathingModel)!;
        _abp = (IBloodCompliance)_model?.Components.Find(i => i.Name == AbpCompartment)!;
        _ad = (IBloodCompliance)_model?.Components.Find(i => i.Name == AdCompartment)!;
        _pap = (IBloodCompliance)_model?.Components.Find(i => i.Name == PapCompartment)!;
        _cvp = (IBloodCompliance)_model?.Components.Find(i => i.Name == CvpCompartment)!;
        _etco2 = (GasCompliance)_model?.Components.Find(i => i.Name == EtCo2Compartment)!;
        foreach (var lung in ChestCompartments)
        {
            var lu = (Container)_model?.Components.Find(i => i.Name == lung)!; 
            _lungs.Add(lu);
        }
        
    }

    public double HeartRate() => _heart!.HeartRate;
    public double AbpSystole() => _abp!.PresMax;
    public double AbpDiastole() => _abp!.PresMin;
    public double PapSystole() => _pap!.PresMax;
    public double PapDiastole() => _pap!.PresMin;
    public double Cvp() => (_cvp!.PresMin * 2 + _cvp!.PresMax) / 3;
    public double Spo2() => _abp!.So2;
    public double Spo2Pre() => _abp!.So2;
    public double Spo2Post() => _ad!.So2;
    public double EtCo2() => _etco2!.Pco2;
    public double RespRate() => _breathing!.RespRate;
    public double Ph() => _abp!.Ph;
    public double Po2() => _abp!.Po2;
    public double Pco2() => _abp!.Pco2;
    public double Hco3() => _abp!.Hco3;
    
    public double AbpSignal() => _abp!.Pres;
    public double Spo2PreSignal() => _abp!.Pres;
    public double Spo2PostSignal() => _ad!.Pres;
    public double PapSignal() => _pap!.Pres;
    public double CvpSignal() => _cvp!.Pres;
    public double EcgSignal() => _heart!.EcgSignal;
    public double EtCo2Signal() => _etco2!.Pco2;
    public double RespSignal()
    {
        double signal = 0;
        foreach (var lung in _lungs)
        {
            signal += lung.Vol;
        }

        return signal;
    }
}