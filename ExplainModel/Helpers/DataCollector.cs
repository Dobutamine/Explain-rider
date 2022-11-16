using Explain.CoreModels;

namespace Explain.Helpers;

public class DataCollector
{

    public readonly List<ModelOutputCirculation> CirculationData = new List<ModelOutputCirculation>();
    public double DataInterval { get; set; } = 0.015;

    public bool CirculationReport { get; set; } = true;
    public bool RespirationReport { get; set; } = true;

    public bool VitalsReport { get; set; } = true;

    // blood gas and o2 sats
    public double SpO2Pre { get; set; }
    public double SpO2Post { get; set; }
    public double SpO2Ven { get; set; }
    public double SpO2Svc { get; set; }
    public double SpO2Ivc { get; set; }
    
    public double Ph { get; set; }
    public double Po2 { get; set; }
    public double Pco2 { get; set; }
    public double Hco3 { get; set; }
    public double Sid { get; set; }
    public double Alb { get; set; }
    public double Pi { get; set; }
    public double U { get; set; }
    public double Na { get; set; }
    public double K { get; set; }
    public double Ca { get; set; }
    public double Mg { get; set; }
    public double Cl { get; set; }
    public double Lact { get; set; }
    public double To2 { get; set; }
    public double Tco2 { get; set; }
    public double Hb { get; set; }
    public double Dpg { get; set; }
    
    // pressures
    public double LvPres { get; set; }
    public double LvPresMax { get; set; }
    public double LvPresMin { get; set; }
    
    public double LaPres { get; set; }
    public double LaPresMax { get; set; }
    public double LaPresMin { get; set; }
    
    public double RaPres { get; set; }
    public double RaPresMax { get; set; }
    public double RaPresMin { get; set; }
    
    public double RvPres { get; set; }
    public double RvPresMax { get; set; }
    public double RvPresMin { get; set; }
    
    public double AaPres { get; set; }
    public double AaPresMax { get; set; }
    public double AaPresMin { get; set; }
    
    public double AdPres { get; set; }
    public double AdPresMax { get; set; }
    public double AdPresMin { get; set; }
    
    public double PaPres { get; set; }
    public double PaPresMax { get; set; }
    public double PaPresMin { get; set; }
    
    public double CvpPres { get; set; }
    public double CvpPresMax { get; set; }
    public double CvpPresMin { get; set; }
    
    public double IvcPres { get; set; }
    public double IvcPresMax { get; set; }
    public double IvcPresMin { get; set; }
    
    public double SvcPres { get; set; }
    public double SvcPresMax { get; set; }
    public double SvcPresMin { get; set; }
    
    public double PcPres { get; set; }
    public double PcPresMax { get; set; }
    public double PcPresMin { get; set; }
    
    public double DsPres { get; set; }
    public double DsPresMax { get; set; }
    public double DsPresMin { get; set; }
    
    public double AllPres { get; set; }
    public double AllPresMax { get; set; }
    public double AllPresMin { get; set; }
    
    public double AlrPres { get; set; }
    public double AlrPresMax { get; set; }
    public double AlrPresMin { get; set; }
    
    public double ChestLPres { get; set; }
    public double ChestLPresMax { get; set; }
    public double ChestLPresMin { get; set; }
    
    public double ChestRPres { get; set; }
    public double ChestRPresMax { get; set; }
    public double ChestRPresMin { get; set; }
    
    // volumes
    public double LaVol { get; set; }
    public double LaVolMax { get; set; }
    public double LaVolMin { get; set; }
    
    public double LvVol { get; set; }
    public double LvVolMax { get; set; }
    public double LvVolMin { get; set; }
    
    public double RaVol { get; set; }
    public double RaVolMax { get; set; }
    public double RaVolMin { get; set; }
    
    public double RvVol { get; set; }
    public double RvVolMax { get; set; }
    public double RvVolMin { get; set; }
    
    public double IvcVol { get; set; }
    public double IvcVolMax { get; set; }
    public double IvcVolMin { get; set; }
    
    public double SvcVol { get; set; }
    public double SvcVolMax { get; set; }
    public double SvcVolMin { get; set; }
    
    public double DsVol { get; set; }
    public double DsVolMax { get; set; }
    public double DsVolMin { get; set; }
    
    public double AllVol { get; set; }
    public double AllVolMax { get; set; }
    public double AllVolMin { get; set; }
    
    public double AlrVol { get; set; }
    public double AlrVolMax { get; set; }
    public double AlrVolMin { get; set; }
    
    public double ChestLVol { get; set; }
    public double ChestLVolMax { get; set; }
    public double ChestLVolMin { get; set; }
    
    public double ChestRVol { get; set; }
    public double ChestRVolMax { get; set; }
    public double ChestRVolMin { get; set; }
    
    // blood flows
    public double FLvAa { get; set; }
    public double FRvPa { get; set; }
    public double FLaLv { get; set; }
    public double FRaRv { get; set; }
    public double FCorRa { get; set; }
    public double FIvcRa { get; set; }
    public double FSvcRa { get; set; }
    public double FDa { get; set; }
    public double FFo { get; set; }
    public double FVsd { get; set; }
    public double FLungShunt { get; set; }
    public double FKidneys { get; set; }
    public double FIntestines { get; set; }
    public double FLiverSpleen { get; set; }
    public double FBrain { get; set; }
    public double FUpperBody { get; set; }
    public double FLowerBody { get; set; }
    

    private Heart _heart;
    private Breathing _breathing;
    private BloodCompliance _aa;
    private BloodCompliance _pa;
    private BloodCompliance _svc;
    private BloodCompliance _ivci;
    private BloodCompliance _ivce;
    private BloodCompliance _brain;
    private BloodTimeVaryingElastance _la;
    private BloodTimeVaryingElastance _lv;
    private BloodTimeVaryingElastance _ra;
    private BloodTimeVaryingElastance _rv;
    private BloodTimeVaryingElastance _cor;
    private GasCompliance _ds;
    private GasCompliance _all;
    private GasCompliance _alr;
    private GasCompliance _mouth;
    private Container _pc;
    private Container _chestL;
    private Container _chestR;

    private BloodResistor _fLvAa;
    private BloodResistor _fRvPa;
    private BloodResistor _fLaLv;
    private BloodResistor _fRaRv;
    private BloodResistor _fIvcRa;
    private BloodResistor _fSvcRa;
    private BloodResistor _fCorRa;
    private BloodResistor _fDa;
    private BloodResistor _fFo;
    private BloodResistor _fVsd;
    private BloodResistor _fLungShunt;
    private BloodResistor _fKidneys;
    private BloodResistor _fIntestines;
    private BloodResistor _fLiverSpleen;
    private BloodResistor _fBrain;
    private BloodResistor _fUb;
    private BloodResistor _fLb;
    
    private Model? _model;
    private double _t;

    private double _fLvAaCounter;
    private double _fRvPaCounter;
    private double _fLaLvCounter;
    private double _fRaRvCounter;
    private double _fIvcRaCounter;
    private double _fSvcRaCounter;
    private double _fCorRaCounter;
    private double _fDaCounter;
    private double _fFoCounter;
    private double _fVsdCounter;
    private double _fLungShuntCounter;
    private double _fKidneysCounter;
    private double _fIntestinesCounter;
    private double _fLiverSpleenCounter;
    private double _fBrainCounter;
    private double _fUbCounter;
    private double _fLbCounter;

    // pulmonary flows
    public double FDsIn { get; set; }
    public double FAllIn { get; set; }
    public double FAlrIn { get; set; }
    public double FMouthOut { get; set; }
    public double FDsOut { get; set; }
    public double FAllOut { get; set; }
    public double FAlrOut { get; set; }
    
    private GasResistor _fMouthDs;
    private GasResistor _fDsAll;
    private GasResistor _fDsAlr;
    
    private double _fDsInCounter;
    private double _fAllInCounter;
    private double _fAlrInCounter;
    private double _fDsOutCounter;
    private double _fAllOutCounter;
    private double _fAlrOutCounter;
    
    private double _updateCounter = 0;
    public DataCollector(Model model)
    {
        // store a reference to the whole model
        _model = model;
        
        // store a reference to the different models
        _heart = (Heart)_model.Components.Find(i => i.Name == "Heart")!;
        _breathing = (Breathing)_model.Components.Find(i => i.Name == "Breathing")!;
        _la = (BloodTimeVaryingElastance)_model.Components.Find(i => i.Name == "LA")!;
        _lv = (BloodTimeVaryingElastance)_model.Components.Find(i => i.Name == "LV")!;
        _ra = (BloodTimeVaryingElastance)_model.Components.Find(i => i.Name == "RA")!;
        _rv = (BloodTimeVaryingElastance)_model.Components.Find(i => i.Name == "RV")!;
        _aa = (BloodCompliance)_model.Components.Find(i => i.Name == "AA")!;
        _pa = (BloodCompliance)_model.Components.Find(i => i.Name == "PA")!;
        
        // store a reference to the different blood resistors
        _fLvAa = (BloodResistor)_model.Components.Find(i => i.Name == "LV_AA")!;
        _fLaLv = (BloodResistor)_model.Components.Find(i => i.Name == "LA_LV")!;
        _fRvPa = (BloodResistor)_model.Components.Find(i => i.Name == "RV_PA")!;
        _fRaRv = (BloodResistor)_model.Components.Find(i => i.Name == "RA_RV")!;
        _fSvcRa = (BloodResistor)_model.Components.Find(i => i.Name == "SVC_RA")!;
        _fIvcRa = (BloodResistor)_model.Components.Find(i => i.Name == "IVCI_RA")!;
        _fCorRa = (BloodResistor)_model.Components.Find(i => i.Name == "COR_RA")!;
        _fDa = (BloodResistor)_model.Components.Find(i => i.Name == "DA")!;
        _fFo = (BloodResistor)_model.Components.Find(i => i.Name == "FO")!;
        _fVsd = (BloodResistor)_model.Components.Find(i => i.Name == "VSD")!;
        _fLungShunt = (BloodResistor)_model.Components.Find(i => i.Name == "IPS")!;
        _fKidneys = (BloodResistor)_model.Components.Find(i => i.Name == "AD_KID")!;
        _fIntestines = (BloodResistor)_model.Components.Find(i => i.Name == "AD_INT")!;
        _fLiverSpleen = (BloodResistor)_model.Components.Find(i => i.Name == "AD_LS")!;
        _fUb = (BloodResistor)_model.Components.Find(i => i.Name == "AAR_RUB")!;
        _fLb = (BloodResistor)_model.Components.Find(i => i.Name == "AD_RLB")!;
        _fBrain = (BloodResistor)_model.Components.Find(i => i.Name == "AAR_BR")!;
        
        // store a reference to the gas flow resistors
        _fMouthDs = (GasResistor)_model.Components.Find(i => i.Name == "MOUTH_DS")!;
        _fDsAll = (GasResistor)_model.Components.Find(i => i.Name == "DS_ALL")!;
        _fDsAlr = (GasResistor)_model.Components.Find(i => i.Name == "DS_ALR")!;
        
        // store a reference to the modeling step size for easy referencing
        _t = _model.ModelingStepsize;
    }
    public void Update()
    {
        // reset the counters as a new ventricular contraction starts
        if (_heart.DcStartHeartBeat)
        {
            // signal that the data collector has started analysis
            _heart.DcStartHeartBeat = false;
            
            var hp = 60.0 / _heart.HeartRate;
            FLvAa = ((_fLvAaCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FRvPa = ((_fRvPaCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FLaLv = ((_fLaLvCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FRaRv = ((_fRaRvCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FCorRa = ((_fCorRaCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FIvcRa = ((_fIvcRaCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FSvcRa = ((_fSvcRaCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FDa = ((_fDaCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FFo = ((_fFoCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FVsd = ((_fVsdCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FLungShunt = ((_fLungShuntCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FKidneys = ((_fKidneysCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FIntestines = ((_fIntestinesCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FLiverSpleen = ((_fLiverSpleenCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FBrain = ((_fBrainCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FUpperBody = ((_fUbCounter * 1000.0 / hp) * 60.0) / _model.Weight;
            FLowerBody = ((_fLbCounter * 1000.0 / hp) * 60.0) / _model.Weight;

            // reset the runners
             _fLvAaCounter = 0;
             _fRvPaCounter = 0;
             _fLaLvCounter = 0;
             _fRaRvCounter = 0;
             _fIvcRaCounter = 0;
             _fSvcRaCounter = 0;
             _fCorRaCounter = 0;
             _fDaCounter = 0;
             _fFoCounter = 0;
             _fVsdCounter = 0;
             _fLungShuntCounter = 0;
             _fKidneysCounter = 0;
             _fIntestinesCounter = 0;
             _fLiverSpleenCounter = 0;
             _fBrainCounter = 0;
             _fUbCounter = 0;
             _fLbCounter = 0;
        }
        
        // increase the blood flow counters
        _fLvAaCounter += _fLaLv.Flow * _t;
        _fRvPaCounter += _fRvPa.Flow * _t;
        _fLaLvCounter += _fLaLv.Flow * _t;
        _fRaRvCounter += _fRaRv.Flow * _t;
        _fIvcRaCounter += _fIvcRa.Flow * _t;
        _fSvcRaCounter += _fSvcRa.Flow * _t;
        _fCorRaCounter += _fCorRa.Flow * _t;
        _fDaCounter += _fDa.Flow * _t;
        _fFoCounter += _fFo.Flow * _t;
        _fVsdCounter += _fVsd.Flow * _t;
        _fLungShuntCounter += _fLungShunt.Flow * _t;
        _fKidneysCounter += _fKidneys.Flow * _t;
        _fIntestinesCounter += _fIntestines.Flow * _t;
        _fLiverSpleenCounter += _fLiverSpleen.Flow * _t;
        _fBrainCounter += _fBrain.Flow * _t;
        _fUbCounter += _fUb.Flow * _t;
        _fLbCounter += _fLb.Flow * _t;


        if (_breathing.DcStartBreath)
        {
            // signal that the data collector has started analysis
            _breathing.DcStartBreath = false;
            
            var rr = 60.0 / _breathing.RespRate;
            FDsIn = ((_fDsInCounter * 1000.0 / rr) * 60.0) / _model.Weight;
            FDsOut = ((_fDsOutCounter * 1000.0 / rr) * 60.0) / _model.Weight;
            
            FAllIn = ((_fAllInCounter * 1000.0 / rr) * 60.0) / _model.Weight;
            FAllOut = ((_fAllOutCounter * 1000.0 / rr) * 60.0) / _model.Weight;
            
            FAlrIn = ((_fAlrInCounter * 1000.0 / rr) * 60.0) / _model.Weight;
            FAlrOut = ((_fAlrOutCounter * 1000.0 / rr) * 60.0) / _model.Weight;

            // reset the runners
            _fDsInCounter = 0;
            _fDsOutCounter = 0;
            
            _fAllInCounter = 0;
            _fAllOutCounter = 0;
            
            _fAlrInCounter = 0;
            _fAlrOutCounter = 0;
        }
        
        // increase the gas flow counters
        if (_fMouthDs.Flow > 0)
        {
            _fDsInCounter += _fMouthDs.Flow * _t;
        }
        else
        {
            _fDsOutCounter += -_fMouthDs.Flow * _t;
        }

        if (_fDsAll.Flow > 0)
        {
            _fDsOutCounter += _fDsAll.Flow * _t;
            _fAllInCounter += _fDsAll.Flow * _t;
        }
        else
        {
            _fDsInCounter += -_fDsAll.Flow * _t;
            _fAllOutCounter += -_fDsAll.Flow * _t;
        }
        
        if (_fDsAlr.Flow > 0)
        {
            _fDsOutCounter += -_fDsAlr.Flow * _t;
            _fAlrInCounter += -_fDsAlr.Flow * _t;
        }
        else
        {
            _fDsInCounter += _fDsAlr.Flow * _t;
            _fAlrOutCounter += _fDsAlr.Flow * _t;
        }
        
        // Build the data report at a different interval 
        if (_updateCounter >= DataInterval)
        {
            _updateCounter = 0;
            if (CirculationReport)  UpdateCirculationReport();
            if (RespirationReport) UpdateRespirationReport();
            if (VitalsReport) UpdateVitalsReport();
        }
        _updateCounter += _t;
        
    }

    public void ClearData()
    {
        CirculationData.Clear();
    }
    public List<ModelOutputCirculation> GetCirculationData()
    {
        // first copy the list
        List<ModelOutputCirculation> newList = new List<ModelOutputCirculation>(CirculationData);
        
        // clear the current list
        CirculationData.Clear();
        
        // return the copied list
        return newList;
    }

    private void UpdateVitalsReport()
    {
        
    }
    private void UpdateRespirationReport()
    {
        
    }
    private void UpdateCirculationReport()
    {
        var newOutputLine = new ModelOutputCirculation
        {
            Time = _model.ModelTimeTotal,
            LaPres = _la.Pres,
            LaPresMax = _la.PresMax,
            LaPresMin = _la.PresMin,
            LvPres = _lv.Pres,
            LvPresMax = _lv.PresMax,
            LvPresMin = _lv.PresMin,
            RaPres = _ra.Pres,
            RaPresMax = _ra.PresMax,
            RaPresMin = _ra.PresMin,
            RvPres = _rv.Pres,
            RvPresMax = _rv.PresMax,
            RvPresMin = _rv.PresMin,
            AaPres = _aa.Pres,
            AaPresMax = _aa.PresMax,
            AaPresMin = _aa.PresMin,
            PaPres = _pa.Pres,
            PaPresMax = _pa.PresMax,
            PaPresMin = _pa.PresMin,
            LaVol = _la.Vol,
            LaVolMax = _la.VolMax,
            LaVolMin = _la.VolMin,
            LvVol = _lv.Vol,
            LvVolMax = _lv.VolMax,
            LvVolMin = _lv.VolMin,
            RaVol = _ra.Vol,
            RaVolMax = _ra.VolMax,
            RaVolMin = _ra.VolMin,
            RvVol = _rv.Vol,
            RvVolMax = _rv.VolMax,
            RvVolMin = _rv.VolMin,
            LaLvFlow = _fLaLv.Flow, 
            LvAaFlow = _fLvAa.Flow, 
            RaRvFlow = _fRaRv.Flow, 
            RvPaFlow = _fRvPa.Flow, 
            CorFlow = _fCorRa.Flow, 
            SvcFlow = _fSvcRa.Flow, 
            IvcFlow = _fIvcRa.Flow,
            DaFlow = _fDa.Flow,
            FoFlow = _fFo.Flow,
            VsdFlow = _fVsd.Flow,
            LungShuntFlow = _fLungShunt.Flow, 
            KidneysFlow = _fKidneys.Flow, 
            IntestinesFlow = _fIntestines.Flow, 
            LiverSpleenFlow  = _fLiverSpleen.Flow, 
            BrainFlow = _fBrain.Flow, 
            UpperBodyFlow  = _fUb.Flow, 
            LowerBodyFlow = _fLb.Flow,
            LaLvFlowMin = FLaLv, 
            LvAaFlowMin = FLvAa, 
            RaRvFlowMin = FRaRv, 
            RvPaFlowMin = FRvPa, 
            CorFlowMin = FCorRa, 
            SvcFlowMin = FSvcRa, 
            IvcFlowMin = FIvcRa,
            DaFlowMin = FDa,
            FoFlowMin = FFo,
            VsdFlowMin = FVsd,
            LungShuntFlowMin = FLungShunt,
            KidneysFlowMin = FKidneys,
            IntestinesFlowMin = FIntestines,
            LiverSpleenFlowMin  = FLiverSpleen,
            BrainFlowMin = FBrain,
            UpperBodyFlowMin  = FUpperBody,
            LowerBodyFlowMin = FLowerBody
        };
        CirculationData.Add(newOutputLine);
    }
    
}