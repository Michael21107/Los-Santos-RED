﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

[Serializable()]
public class SettingsManager
{
    private List<ISettingsDefaultable> AllDefaultableSettings;
    public SettingsManager()
    {

    }
    [Category("Player")]
    [Description("Violation Settings")]
    public ViolationSettings ViolationSettings { get; set; } = new ViolationSettings();
    [Category("Player")]
    [Description("Criminal History Settings")]
    public CriminalHistorySettings CriminalHistorySettings { get; set; } = new CriminalHistorySettings();
    [Category("Player")]
    [Description("Investigation Settings")]
    public InvestigationSettings InvestigationSettings { get; set; } = new InvestigationSettings();
    [Category("Player")]
    [Description("Needs Settings")]
    public NeedsSettings NeedsSettings { get; set; } = new NeedsSettings();
    [Category("Player")]
    [Description("Vehicle Settings")]
    public VehicleSettings VehicleSettings { get; set; } = new VehicleSettings();
    [Category("Player")]
    [Description("Scanner Settings")]
    public ScannerSettings ScannerSettings { get; set; } = new ScannerSettings();
    [Category("Player")]
    [Description("Contact Task Settings")]
    public TaskSettings TaskSettings { get; set; } = new TaskSettings();
    [Category("Player")]
    [Description("Respawn Settings")]
    public RespawnSettings RespawnSettings { get; set; } = new RespawnSettings();
    [Category("Player")]
    [Description("PedSwap Settings")]
    public PedSwapSettings PedSwapSettings { get; set; } = new PedSwapSettings();
    [Category("Player")]
    [Description("Cellphone Settings")]
    public CellphoneSettings CellphoneSettings { get; set; } = new CellphoneSettings();
    [Category("Player")]
    [Description("Player Speech Settings")]
    public PlayerSpeechSettings PlayerSpeechSettings { get; set; } = new PlayerSpeechSettings();
    [Category("Player")]
    [Description("Other Settings")]
    public PlayerOtherSettings PlayerOtherSettings { get; set; } = new PlayerOtherSettings();







    [Category("Player Activity")]
    [Description("Activity Settings")]
    public ActivitySettings ActivitySettings { get; set; } = new ActivitySettings();
    [Category("Player Activity")]
    [Description("Flashlight Settings")]
    public FlashlightSettings FlashlightSettings { get; set; } = new FlashlightSettings();
    [Category("Player Activity")]
    [Description("Drag Settings")]
    public DragSettings DragSettings { get; set; } = new DragSettings();
    [Category("Player Activity")]
    [Description("Binocular Settings")]
    public BinocularSettings BinocularSettings { get; set; } = new BinocularSettings();
    [Category("Player Activity")]
    [Description("Shovel Settings")]
    public ShovelSettings ShovelSettings { get; set; } = new ShovelSettings();
    [Category("Player Activity")]
    [Description("Door Toggle Settings")]
    public DoorToggleSettings DoorToggleSettings { get; set; } = new DoorToggleSettings();
    [Category("Player Activity")]
    [Description("Sprint Settings")]
    public SprintSettings SprintSettings { get; set; } = new SprintSettings();



    [Category("Player Weapon")]
    [Description("Recoil Settings")]
    public RecoilSettings RecoilSettings { get; set; } = new RecoilSettings();
    [Category("Player Weapon")]
    [Description("Sway Settings")]
    public SwaySettings SwaySettings { get; set; } = new SwaySettings();
    [Category("Player Weapon")]
    [Description("Selector Settings")]
    public SelectorSettings SelectorSettings { get; set; } = new SelectorSettings();





    [Category("Player AI Shared")]
    [Description("Damage Settings")]
    public DamageSettings DamageSettings { get; set; } = new DamageSettings();






    [Category("Police")]
    [Description("General Settings")]
    public PoliceSettings PoliceSettings { get; set; } = new PoliceSettings();
    [Category("Police")]
    [Description("Spawn Settings")]
    public PoliceSpawnSettings PoliceSpawnSettings { get; set; } = new PoliceSpawnSettings();
    [Category("Police")]
    [Description("Task Settings")]
    public PoliceTaskSettings PoliceTaskSettings { get; set; } = new PoliceTaskSettings();
    [Category("Police")]
    [Description("Speech Settings")]
    public PoliceSpeechSettings PoliceSpeechSettings { get; set; } = new PoliceSpeechSettings();
    [Category("Police")]
    [Description("Roadblock Settings")]
    public RoadblockSettings RoadblockSettings { get; set; } = new RoadblockSettings();

    [Category("Civilian")]
    [Description("Gang Settings")]
    public GangSettings GangSettings { get; set; } = new GangSettings();
    [Category("Civilian")]
    [Description("Security Settings")]
    public SecuritySettings SecuritySettings { get; set; } = new SecuritySettings();
    [Category("Civilian")]
    [Description("EMS Settings")]
    public EMSSettings EMSSettings { get; set; } = new EMSSettings();
    [Category("Civilian")]
    [Description("Fire Settings")]
    public FireSettings FireSettings { get; set; } = new FireSettings();
    [Category("Civilian")]
    [Description("Regular Civilian Settings")]
    public CivilianSettings CivilianSettings { get; set; } = new CivilianSettings();
    [Category("Civilian")]
    [Description("Taxi Settings")]
    public TaxiSettings TaxiSettings { get; set; } = new TaxiSettings();


    [Category("World")]
    [Description("World General Settings")]
    public WorldSettings WorldSettings { get; set; } = new WorldSettings();
    [Category("World")]
    [Description("Time Settings")]
    public TimeSettings TimeSettings { get; set; } = new TimeSettings();
    [Category("World")]
    [Description("Weather Settings")]
    public WeatherSettings WeatherSettings { get; set; } = new WeatherSettings();


    [Category("UI")]
    [Description("General Settings")]
    public UIGeneralSettings UIGeneralSettings { get; set; } = new UIGeneralSettings();
    [Category("UI")]
    [Description("LSR HUD Settings")]
    public LSRHUDSettings LSRHUDSettings { get; set; } = new LSRHUDSettings();
    [Category("UI")]
    [Description("Bar Display Settings")]
    public BarDisplaySettings BarDisplaySettings { get; set; } = new BarDisplaySettings();
    [Category("UI")]
    [Description("Action Wheel Settings")]
    public ActionWheelSettings ActionWheelSettings { get; set; } = new ActionWheelSettings();



    [Category("Key Bindings")]
    [Description("Key Bindings Settings")]
    public KeySettings KeySettings { get; set; } = new KeySettings();


    [Category("Vanilla")]
    [Description("Vanilla Settings")]
    public VanillaSettings VanillaSettings { get; set; } = new VanillaSettings();
    [Category("Other")]
    [Description("Performance Settings")]
    public PerformanceSettings PerformanceSettings { get; set; } = new PerformanceSettings();
    [Category("Other")]
    [Description("Debug Settings")]
    public DebugSettings DebugSettings { get; set; } = new DebugSettings();

    public void Setup()
    {
        AllDefaultableSettings = new List<ISettingsDefaultable>()
        {
            RespawnSettings, VehicleSettings, PedSwapSettings, ActivitySettings, SprintSettings, ViolationSettings, RecoilSettings, SwaySettings, SelectorSettings, InvestigationSettings, CriminalHistorySettings, ScannerSettings, KeySettings, PlayerOtherSettings, CellphoneSettings,
            PoliceSettings,GangSettings,CivilianSettings, EMSSettings,FireSettings, DamageSettings, WorldSettings, TaskSettings, TimeSettings, WeatherSettings, VanillaSettings,DebugSettings,PerformanceSettings,TaxiSettings,
            UIGeneralSettings,LSRHUDSettings,BarDisplaySettings,ActionWheelSettings, NeedsSettings, RoadblockSettings, PoliceSpawnSettings,PoliceTaskSettings,PoliceSpeechSettings,PlayerSpeechSettings,FlashlightSettings, SecuritySettings,DragSettings,BinocularSettings
            ,DoorToggleSettings,ShovelSettings
        };
    }
    public void SetDefault()
    {
        foreach(ISettingsDefaultable settingsDefaultable in AllDefaultableSettings)
        {
            settingsDefaultable.SetDefault();
        }
    }


}
