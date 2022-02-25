﻿using ExtensionsMethods;
using iFruitAddon2;
using LosSantosRED.lsr;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

public class Gangs : IGangs
{
    private readonly string ConfigFileName = "Plugins\\LosSantosRED\\Gangs.xml";
    private bool UseVanillaConfig = true;
    private List<Gang> GangsList;
    private Gang DefaultGang;
    public Gangs()
    {

    }
    public List<Gang> AllGangs => GangsList;
    public void ReadConfig()
    {
#if DEBUG
        UseVanillaConfig = true;
#else
            UseVanillaConfig = true;
#endif
        if (File.Exists(ConfigFileName))
        {
            GangsList = Serialization.DeserializeParams<Gang>(ConfigFileName);
        }
        else
        {
            if (UseVanillaConfig)
            {
                DefaultConfig();
            }
            else
            {
                //CustomConfig();
            }
            Serialization.SerializeParams(GangsList, ConfigFileName);
        }
    }
    public List<Gang> GetAllGangs()
    {
        return GangsList;
    }
    public Gang GetGang(string GangInitials)
    {
        return GangsList.Where(x => x.ID.ToLower() == GangInitials.ToLower()).FirstOrDefault();
    }
    public Gang GetGangByContact(string contactName)
    {
        return GangsList.Where(x => x.ContactName.ToLower() == contactName.ToLower()).FirstOrDefault();
    }
    public List<Gang> GetGangs(Ped ped)
    {
        return GangsList.Where(x => x.Personnel != null && x.Personnel.Any(b => b.ModelName.ToLower() == ped.Model.Name.ToLower())).ToList();
    }
    public List<Gang> GetGangs(Vehicle vehicle)
    {
        return GangsList.Where(x => x.Vehicles != null && x.Vehicles.Any(b => b.ModelName.ToLower() == vehicle.Model.Name.ToLower())).ToList();
    }
    public List<Gang> GetSpawnableGangs(int WantedLevel)
    {
        return GangsList.Where(x => x.CanSpawnAnywhere && x.CanSpawn(WantedLevel)).ToList();
    }
    private void DefaultConfig()
    {
        List<DispatchableVehicle> GenericVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("buccaneer", 15, 15),
            new DispatchableVehicle("manana", 15, 15),
            new DispatchableVehicle("tornado", 15, 15),};

        List<DispatchableVehicle> AllVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("buccaneer", 15, 15),
            new DispatchableVehicle("buccaneer2", 15, 15),
            new DispatchableVehicle("manana", 15, 15),
            new DispatchableVehicle("chino", 15, 15),
            new DispatchableVehicle("chino2", 15, 15),
            new DispatchableVehicle("faction", 15, 15),
            new DispatchableVehicle("faction2", 15, 15),
            new DispatchableVehicle("primo", 15, 15),
            new DispatchableVehicle("primo2", 15, 15),
            new DispatchableVehicle("voodoo", 15, 15),
            new DispatchableVehicle("voodoo2", 15, 15),
        };

        List<IssuableWeapon> AllSidearms = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_pistol", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_pistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_pistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight"), new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_pistol_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_pistol_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_pistol_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight"), new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_combatpistol", new WeaponVariation()),
            new IssuableWeapon("weapon_combatpistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_combatpistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_combatpistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight"), new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_heavypistol", new WeaponVariation()),
            new IssuableWeapon("weapon_heavypistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Etched Wood Grip Finish" )})),
            new IssuableWeapon("weapon_heavypistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight"), new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_heavypistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_revolver_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_revolver_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_snspistol_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_ceramicpistol", new WeaponVariation()),
            new IssuableWeapon("weapon_snspistol", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol50", new WeaponVariation()),
            new IssuableWeapon("weapon_appistol", new WeaponVariation()),
            new IssuableWeapon("weapon_snspistol_mk2", new WeaponVariation()),
        };
        List<IssuableWeapon> AllLongGuns = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_pumpshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_pumpshotgun", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_pumpshotgun_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_pumpshotgun_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_pumpshotgun_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Holographic Sight" )})),
            new IssuableWeapon("weapon_pumpshotgun_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight"), new WeaponComponent("Holographic Sight" )})),
            new IssuableWeapon("weapon_carbinerifle", new WeaponVariation()),
            new IssuableWeapon("weapon_carbinerifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Grip"), new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_carbinerifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Scope"), new WeaponComponent("Grip"), new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_carbinerifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Scope"), new WeaponComponent("Grip"), new WeaponComponent("Flashlight"), new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_carbinerifle_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_carbinerifle_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Holographic Sight"), new WeaponComponent("Grip"), new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_carbinerifle_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Holographic Sight"), new WeaponComponent("Grip"), new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_carbinerifle_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Large Scope"), new WeaponComponent("Grip"), new WeaponComponent("Flashlight"), new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_sawnoffshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_dbshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_microsmg", new WeaponVariation()),
            new IssuableWeapon("weapon_microsmg", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation()),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
            new IssuableWeapon("weapon_minismg", new WeaponVariation()),
            new IssuableWeapon("weapon_minismg", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_autoshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation()),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
            new IssuableWeapon("weapon_assaultrifle_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_assaultrifle_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation()),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
            new IssuableWeapon("weapon_mg", new WeaponVariation()),
            new IssuableWeapon("weapon_mg", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_combatmg", new WeaponVariation()),
            new IssuableWeapon("weapon_combatmg", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_combatmg_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_combatmg_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
        };

        //Peds
        List<DispatchablePerson> LostMCPEds = new List<DispatchablePerson>() {
            new DispatchablePerson("g_m_y_lost_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_lost_02",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_lost_03",30,30,5,15,500,800,0,1),
            new DispatchablePerson("ig_clay",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_f_y_lost_01",10,10,5,15,500,800,0,1) };

        List<DispatchablePerson> VagosPeds = new List<DispatchablePerson>() {
            new DispatchablePerson("g_m_y_mexgoon_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_mexgoon_02",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_mexgoon_03",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_f_y_vagos_01",10,10,5,15,500,800,0,1) };

        List<DispatchablePerson> FamiliesPeds = new List<DispatchablePerson>() {
            new DispatchablePerson("g_m_y_famca_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_famdnf_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_famfor_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_f_y_families_01",10,10,5,15,500,800,0,1) };

        List<DispatchablePerson> BallasPeds = new List<DispatchablePerson>() {
            new DispatchablePerson("g_m_y_ballasout_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_ballaeast_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_ballaorig_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_f_y_ballas_01",10,10,5,15,500,800,0,1) };

        List<DispatchablePerson> MarabuntaPeds = new List<DispatchablePerson>() {
            new DispatchablePerson("g_m_y_salvaboss_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_salvagoon_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_salvagoon_02",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_salvagoon_03",10,10,5,15,500,800,0,1) };

        List<DispatchablePerson> AltruistPeds = new List<DispatchablePerson>() {
            new DispatchablePerson("a_m_m_acult_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("a_m_o_acult_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("a_m_o_acult_02",30,30,5,15,500,800,0,1),
            new DispatchablePerson("a_m_y_acult_01",10,10,5,15,500,800,0,1),
            new DispatchablePerson("a_m_y_acult_02",10,10,5,15,500,800,0,1),
            new DispatchablePerson("a_f_m_fatcult_01",10,10,5,15,500,800,0,1), };

        List<DispatchablePerson> VarriosPeds = new List<DispatchablePerson>() {
            new DispatchablePerson("g_m_y_azteca_01",100,100,5,15,500,800,0,1),
            new DispatchablePerson("ig_ortega",20,20,5,15,500,800,0,1), };

        List<DispatchablePerson> TriadsPeds = new List<DispatchablePerson>() {
            new DispatchablePerson("g_m_m_chigoon_01",33,33,5,15,500,800,0,1),
            new DispatchablePerson("g_m_m_chigoon_02",33,33,5,15,500,800,0,1),
            new DispatchablePerson("g_m_m_korboss_01",33,33,5,15,500,800,0,1),
            new DispatchablePerson("ig_hao",33,33,5,15,500,800,0,1),  };

        List<DispatchablePerson> KoreanPeds = new List<DispatchablePerson>() {
            new DispatchablePerson("g_m_y_korean_01",33,33,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_korean_02",33,33,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_korlieut_01",33,33,5,15,500,800,0,1) };

        List<DispatchablePerson> RedneckPeds = new List<DispatchablePerson>() {
            new DispatchablePerson("a_m_m_hillbilly_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("a_m_m_hillbilly_02",30,30,5,15,500,800,0,1),
            new DispatchablePerson("a_m_m_hillbilly_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("a_m_m_hillbilly_02",10,10,5,15,500,800,0,1) };

        List<DispatchablePerson> ArmenianPeds = new List<DispatchablePerson>() {
            new DispatchablePerson("g_m_m_armboss_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_m_armgoon_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_m_armlieut_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_armgoon_02",10,10,5,15,500,800,0,1) };

        List<DispatchablePerson> CartelPeds = new List<DispatchablePerson>() {
            new DispatchablePerson("g_m_m_mexboss_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_m_mexboss_02",30,30,5,15,500,800,0,1),
            new DispatchablePerson("g_m_y_mexgang_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("a_m_y_mexthug_01",30,30,5,15,500,800,0,1), };

        List<DispatchablePerson> MafiaPeds = new List<DispatchablePerson>() {
            new DispatchablePerson("s_m_m_highsec_01",30,30,5,15,500,800,0,1) { RequiredVariation = new PedVariation(new List<PedComponent>() { new PedComponent(0, 0, 0, 0) },new List<PedPropComponent>() {  }) },//not good, bad heads
            new DispatchablePerson("s_m_m_highsec_01",30,30,5,15,500,800,0,1) { RequiredVariation = new PedVariation(new List<PedComponent>() { new PedComponent(0, 0, 1, 0) },new List<PedPropComponent>() { }) },//not good, bad heads
            new DispatchablePerson("s_m_m_highsec_01",30,30,5,15,500,800,0,1) { RequiredVariation = new PedVariation(new List<PedComponent>() { new PedComponent(0, 0, 2, 0) },new List<PedPropComponent>() { }) },//not good, bad heads
            new DispatchablePerson("s_m_m_highsec_02",30,30,5,15,500,800,0,1),
            new DispatchablePerson("u_m_m_jewelsec_01",30,30,5,15,500,800,0,1),
             new DispatchablePerson("u_m_m_aldinapoli",30,30,5,15,500,800,0,1) { RequiredVariation = new PedVariation(new List<PedComponent>() { new PedComponent(4, 0, 0, 0) },new List<PedPropComponent>() { }) },//not good, bad heads
                                                                                                                                                                                                   };
        //u_m_m_jewelsec_01//has some bad heads
        //u_m_m_aldinapoli
        //s_m_y_devinsec_01//nope has bad alts, no no no no
        //s_m_y_casino_01//has some bad heads
        

        List<DispatchablePerson> YardiesPeds = new List<DispatchablePerson>() {
            new DispatchablePerson("a_m_m_og_boss_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("a_m_o_soucent_01",30,30,5,15,500,800,0,1),
            new DispatchablePerson("a_m_y_soucent_02",30,30,5,15,500,800,0,1),};

        //Vehicles
        List<DispatchableVehicle> LostMCVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("daemon", 70, 70) { MaxOccupants = 1 },
            new DispatchableVehicle("slamvan2", 15, 15) { MaxOccupants = 1 },
            new DispatchableVehicle("gburrito", 15, 15) { MaxOccupants = 1 },};

        List<DispatchableVehicle> VarriosVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("buccaneer", 50, 50){ RequiredPrimaryColorID = 42,RequiredSecondaryColorID = 42},
            new DispatchableVehicle("buccaneer2", 50, 50){RequiredPrimaryColorID = 42,RequiredSecondaryColorID = 42 },//yellow
        };

        List<DispatchableVehicle> BallasVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("baller", 50, 50){ RequiredPrimaryColorID = 145,RequiredSecondaryColorID = 145 },
            new DispatchableVehicle("baller2", 50, 50){ RequiredPrimaryColorID = 145,RequiredSecondaryColorID = 145 },//purp[le
        };

        List<DispatchableVehicle> VagosVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("chino", 50, 50){ RequiredPrimaryColorID = 38,RequiredSecondaryColorID = 38 },
            new DispatchableVehicle("chino2", 50, 50){ RequiredPrimaryColorID = 38,RequiredSecondaryColorID = 38 },//orange
        };

        List<DispatchableVehicle> MarabuntaVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("faction", 50, 50){ RequiredPrimaryColorID = 70,RequiredSecondaryColorID = 70 },
            new DispatchableVehicle("faction2", 50, 50){ RequiredPrimaryColorID = 70,RequiredSecondaryColorID = 70 },//blue
        };

        List<DispatchableVehicle> KoreanVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("feltzer2", 33, 33){ RequiredPrimaryColorID = 4,RequiredSecondaryColorID = 4 },//silver
            new DispatchableVehicle("comet2", 33, 33){ RequiredPrimaryColorID = 4,RequiredSecondaryColorID = 4 },//silver
            new DispatchableVehicle("dubsta2", 33, 33){ RequiredPrimaryColorID = 4,RequiredSecondaryColorID = 4 },//silver
        };

        List<DispatchableVehicle> TriadVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("fugitive", 50, 50){ RequiredPrimaryColorID = 111,RequiredSecondaryColorID = 111 },//white
            new DispatchableVehicle("oracle", 50, 50){ RequiredPrimaryColorID = 111,RequiredSecondaryColorID = 111 },//white
           // new DispatchableVehicle("cavalcade", 33, 33){ RequiredPrimaryColorID = 111,RequiredSecondaryColorID = 111 },//white
        };

        List<DispatchableVehicle> YardieVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("virgo", 33, 33){ RequiredPrimaryColorID = 55,RequiredSecondaryColorID = 55 },//matte lime green
            new DispatchableVehicle("voodoo", 33, 33){ RequiredPrimaryColorID = 55,RequiredSecondaryColorID = 55 },//matte lime green
            new DispatchableVehicle("voodoo2", 33, 33){ RequiredPrimaryColorID = 55,RequiredSecondaryColorID = 55 },//matte lime green
        };

        List<DispatchableVehicle> DiablosVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("stalion", 100, 100){ RequiredPrimaryColorID = 28,RequiredSecondaryColorID = 28 },//red
        };

        List<DispatchableVehicle> MafiaVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("sentinel", 50, 50) { RequiredPrimaryColorID = 0,RequiredSecondaryColorID = 0 },//black
            new DispatchableVehicle("sentinel2", 50, 50) { RequiredPrimaryColorID = 0,RequiredSecondaryColorID = 0 },//black
        };

        List<DispatchableVehicle> ArmeniaVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("schafter2", 100, 100) { RequiredPrimaryColorID = 0,RequiredSecondaryColorID = 0 },//black
        };

        List<DispatchableVehicle> CartelVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("cavalcade2", 50, 50) { RequiredPrimaryColorID = 0,RequiredSecondaryColorID = 0 },//black
            new DispatchableVehicle("cavalcade", 50, 50) { RequiredPrimaryColorID = 0,RequiredSecondaryColorID = 0 },//black

        };

        List<DispatchableVehicle> RedneckVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("rumpo", 33, 33),
            new DispatchableVehicle("bison", 33, 33),
            new DispatchableVehicle("sanchez2",33,33) {MaxOccupants = 1 },
        };

        List<DispatchableVehicle> FamiliesVehicles = new List<DispatchableVehicle>() {
            new DispatchableVehicle("emperor ",15,15) { RequiredPrimaryColorID = 53,RequiredSecondaryColorID = 53 },//green
            new DispatchableVehicle("peyote ",15,15) { RequiredPrimaryColorID = 53,RequiredSecondaryColorID = 53 },//green
            new DispatchableVehicle("nemesis",15,15) {MaxOccupants = 1 },
            new DispatchableVehicle("buccaneer",15,15) { RequiredPrimaryColorID = 53,RequiredSecondaryColorID = 53 },//green
            new DispatchableVehicle("manana",15,15)  { RequiredPrimaryColorID = 53,RequiredSecondaryColorID = 53 },//green
            new DispatchableVehicle("tornado",15,15)  { RequiredPrimaryColorID = 53,RequiredSecondaryColorID = 53 },//green
        };

        //Weapon
        List<IssuableWeapon> MeleeWeapons = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_hatchet", new WeaponVariation()),
            new IssuableWeapon("weapon_knuckle", new WeaponVariation()),
            new IssuableWeapon("weapon_knife", new WeaponVariation()),
            new IssuableWeapon("weapon_machete", new WeaponVariation()),
            new IssuableWeapon("weapon_switchblade", new WeaponVariation()),
            new IssuableWeapon("weapon_nightstick", new WeaponVariation()),
            new IssuableWeapon("weapon_bat", new WeaponVariation()),
            new IssuableWeapon("weapon_crowbar", new WeaponVariation()),
            new IssuableWeapon("weapon_hammer", new WeaponVariation()),
        };


        List<IssuableWeapon> FamiliesSidearms = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_pistol", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_pistol_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_combatpistol", new WeaponVariation()),
            new IssuableWeapon("weapon_combatpistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_heavypistol", new WeaponVariation()),
            new IssuableWeapon("weapon_revolver_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_snspistol_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_ceramicpistol", new WeaponVariation()),
            new IssuableWeapon("weapon_snspistol", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol50", new WeaponVariation()),
            new IssuableWeapon("weapon_appistol", new WeaponVariation()),
            new IssuableWeapon("weapon_snspistol_mk2", new WeaponVariation()),
        };
        List<IssuableWeapon> FamiliesLongGuns = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_pumpshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_sawnoffshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_dbshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_microsmg", new WeaponVariation()),
            new IssuableWeapon("weapon_microsmg", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation()),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
            new IssuableWeapon("weapon_minismg", new WeaponVariation()),
            new IssuableWeapon("weapon_minismg", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_autoshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation()),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation()),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
        };
        List<IssuableWeapon> LostSidearms = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_pistol", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_pistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_pistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight"), new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_pistol_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_pistol_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_pistol_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight"), new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_combatpistol", new WeaponVariation()),
            new IssuableWeapon("weapon_combatpistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_combatpistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_combatpistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight"), new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_heavypistol", new WeaponVariation()),
            new IssuableWeapon("weapon_heavypistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Etched Wood Grip Finish" )})),
            new IssuableWeapon("weapon_heavypistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight"), new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_heavypistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_revolver_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_revolver_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_snspistol_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_ceramicpistol", new WeaponVariation()),
            new IssuableWeapon("weapon_snspistol", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol50", new WeaponVariation()),
            new IssuableWeapon("weapon_appistol", new WeaponVariation()),
            new IssuableWeapon("weapon_snspistol_mk2", new WeaponVariation()),
        };
        List<IssuableWeapon> LostLongGuns = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_pumpshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_pumpshotgun", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_pumpshotgun_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_pumpshotgun_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_pumpshotgun_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Holographic Sight" )})),
            new IssuableWeapon("weapon_pumpshotgun_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Flashlight"), new WeaponComponent("Holographic Sight" )})),
            new IssuableWeapon("weapon_carbinerifle", new WeaponVariation()),
            new IssuableWeapon("weapon_carbinerifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Grip"), new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_carbinerifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Scope"), new WeaponComponent("Grip"), new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_carbinerifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Scope"), new WeaponComponent("Grip"), new WeaponComponent("Flashlight"), new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_carbinerifle_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_carbinerifle_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Holographic Sight"), new WeaponComponent("Grip"), new WeaponComponent("Flashlight" )})),
            new IssuableWeapon("weapon_carbinerifle_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Holographic Sight"), new WeaponComponent("Grip"), new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_carbinerifle_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Large Scope"), new WeaponComponent("Grip"), new WeaponComponent("Flashlight"), new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_sawnoffshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_dbshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_microsmg", new WeaponVariation()),
            new IssuableWeapon("weapon_microsmg", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation()),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
            new IssuableWeapon("weapon_minismg", new WeaponVariation()),
            new IssuableWeapon("weapon_minismg", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_autoshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation()),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
            new IssuableWeapon("weapon_assaultrifle_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_assaultrifle_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation()),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
            new IssuableWeapon("weapon_mg", new WeaponVariation()),
            new IssuableWeapon("weapon_mg", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_combatmg", new WeaponVariation()),
            new IssuableWeapon("weapon_combatmg", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_combatmg_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_combatmg_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
        };
        List<IssuableWeapon> VagosSidearms = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_combatpistol", new WeaponVariation()),
            new IssuableWeapon("weapon_combatpistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip" )})),
            new IssuableWeapon("weapon_heavypistol", new WeaponVariation()),
            new IssuableWeapon("weapon_appistol", new WeaponVariation()),
            new IssuableWeapon("weapon_snspistol_mk2", new WeaponVariation()),
        };
        List<IssuableWeapon> VagosLongGuns = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_autoshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation()),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation()),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
        };
        List<IssuableWeapon> BallasSidearms = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_pistol", new WeaponVariation()),
            new IssuableWeapon("weapon_ceramicpistol", new WeaponVariation()),
            new IssuableWeapon("weapon_snspistol", new WeaponVariation()),
            new IssuableWeapon("weapon_snspistol_mk2", new WeaponVariation()),
        };
        List<IssuableWeapon> BallasLongGuns = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_pumpshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_sawnoffshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_dbshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation()),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
        };
        List<IssuableWeapon> MarabuntaSidearms = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_pistol_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_appistol", new WeaponVariation()),
            new IssuableWeapon("weapon_snspistol_mk2", new WeaponVariation()),
        };
        List<IssuableWeapon> MarabuntaLongGuns = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation()),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_assaultrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
            new IssuableWeapon("weapon_assaultrifle_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_assaultrifle_mk2", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation()),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_compactrifle", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
        };
        List<IssuableWeapon> VarriosSidearms = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_pistol", new WeaponVariation()),
            new IssuableWeapon("weapon_snspistol", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol50", new WeaponVariation()),
        };
        List<IssuableWeapon> VarriosLongGuns = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_sawnoffshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_dbshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_microsmg", new WeaponVariation()),
            new IssuableWeapon("weapon_microsmg", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation()),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Extended Clip") })),
            new IssuableWeapon("weapon_machinepistol", new WeaponVariation(new List<WeaponComponent> {  new WeaponComponent("Drum Magazine") })),
        };
        List<IssuableWeapon> TriadsSidearms = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_appistol", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol", new WeaponVariation()),
        };
        List<IssuableWeapon> TriadsLongGuns = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_bullpuprifle_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_bullpuprifle", new WeaponVariation()),
            new IssuableWeapon("weapon_bullpupshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_combatpdw", new WeaponVariation()),
        };
        List<IssuableWeapon> KkangpaeSidearms = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_appistol", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol", new WeaponVariation()),
        };
        List<IssuableWeapon> KkangpaeLongGuns = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_bullpuprifle_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_bullpuprifle", new WeaponVariation()),
            new IssuableWeapon("weapon_bullpupshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_combatpdw", new WeaponVariation()),
        };
        List<IssuableWeapon> MafiaSidearms = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_heavypistol", new WeaponVariation()),
            new IssuableWeapon("weapon_pistol_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_ceramicpistol", new WeaponVariation()),
            new IssuableWeapon("weapon_combatpistol", new WeaponVariation()),
        };
        List<IssuableWeapon> MafiaLongGuns = new List<IssuableWeapon>()
        {
            new IssuableWeapon("weapon_carbinerifle_mk2", new WeaponVariation()),
            new IssuableWeapon("weapon_sawnoffshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_bullpupshotgun", new WeaponVariation()),
            new IssuableWeapon("weapon_carbinerifle", new WeaponVariation()),
        };

        DefaultGang = new Gang("~s~", "UNK", "Unknown Gang","Unk", "White", null, null, "", null, null,null) { MaxWantedLevelSpawn = 0 };
        GangsList = new List<Gang>
        {
            new Gang("~w~", "AMBIENT_GANG_LOST", "The Lost MC","LOST MC", "White", LostMCPEds, LostMCVehicles, "LOST ",MeleeWeapons,LostSidearms,LostLongGuns, "LOST MC President","CHAR_MP_BIKER_BOSS") { 
                DenName = "Clubhouse",AmbientMemberMoneyMin = 1000, AmbientMemberMoneyMax = 5000,EnemyGangs = new List<string>() { "AMBIENT_GANG_MADRAZO", "AMBIENT_GANG_GAMBETTI", "AMBIENT_GANG_ANCELOTTI" } } ,//Meth
            new Gang("~o~", "AMBIENT_GANG_MEXICAN", "Vagos","Vagos", "Orange", VagosPeds, VagosVehicles, "",MeleeWeapons,VagosSidearms,VagosLongGuns,"Vagos O.G.", "CHAR_MP_MEX_BOSS") { 
                DenName = "Hangout",AmbientMemberMoneyMin = 500, AmbientMemberMoneyMax = 2000,EnemyGangs = new List<string>() { "AMBIENT_GANG_SALVA" } } ,//marijuana
            new Gang("~g~", "AMBIENT_GANG_FAMILY", "The Families","Families", "Green", FamiliesPeds, FamiliesVehicles, "",MeleeWeapons,FamiliesSidearms,FamiliesLongGuns,"Families O.G.","CHAR_MP_FAM_BOSS") { 
                DenName = "Hangout",AmbientMemberMoneyMin = 1000, AmbientMemberMoneyMax = 5000,EnemyGangs = new List<string>() { "AMBIENT_GANG_BALLAS" } } ,//marijuana
            new Gang("~p~", "AMBIENT_GANG_BALLAS", "Ballas","Ballas", "Purple", BallasPeds, BallasVehicles, "",MeleeWeapons,BallasSidearms,BallasLongGuns,"Ballas O.G.","CHAR_MP_JULIO") { 
                DenName = "Hangout",AmbientMemberMoneyMin = 2000, AmbientMemberMoneyMax = 7000,EnemyGangs = new List<string>() { "AMBIENT_GANG_FAMILY" } } ,//crack
            new Gang("~b~", "AMBIENT_GANG_MARABUNTE", "Marabunta Grande","Marabunta", "Blue", MarabuntaPeds, MarabuntaVehicles, "",MeleeWeapons,MarabuntaSidearms,MarabuntaLongGuns,"Marabunta O.G.","CHAR_MP_MEX_LT") { 
                DenName = "Hangout",AmbientMemberMoneyMin = 2000, AmbientMemberMoneyMax = 7000,EnemyGangs = new List<string>() { "AMBIENT_GANG_MADRAZO", "AMBIENT_GANG_SALVA" } } ,//marijuana
            new Gang("~w~", "AMBIENT_GANG_CULT", "Altruist Cult","Altruist", "White", AltruistPeds, GenericVehicles, "",MeleeWeapons,FamiliesSidearms,FamiliesLongGuns,"Altruist Leader","CHAR_PA_MALE") { 
                DenName = "Gathering Location",AmbientMemberMoneyMin = 200, AmbientMemberMoneyMax = 1000,EnemyGangs = new List<string>() { "AMBIENT_GANG_HILLBILLY" } } ,
            new Gang("~y~", "AMBIENT_GANG_SALVA", "Varrios Los Aztecas","Varrios", "Yellow", VarriosPeds, VarriosVehicles, "",MeleeWeapons,VarriosSidearms,VarriosLongGuns,"Varrios O.G.","CHAR_ORTEGA") { 
                DenName = "Hangout",AmbientMemberMoneyMin = 1000, AmbientMemberMoneyMax = 5000,EnemyGangs = new List<string>() { "AMBIENT_GANG_MARABUNTE" } } ,//crack
            new Gang("~r~", "AMBIENT_GANG_WEICHENG", "Triads","Triads", "Red", TriadsPeds, TriadVehicles, "",MeleeWeapons,TriadsSidearms,TriadsLongGuns,"Triad Leader","CHAR_CHENGSR") { 
                DenName = "Meeting Spot",AmbientMemberMoneyMin = 2000, AmbientMemberMoneyMax = 7000,EnemyGangs = new List<string>() { "AMBIENT_GANG_MESSINA" } } ,//heroin
            new Gang("~b~", "AMBIENT_GANG_HILLBILLY", "Rednecks","Rednecks", "Black", RedneckPeds, RedneckVehicles, "",MeleeWeapons,FamiliesSidearms,FamiliesLongGuns,"Redneck Leader","CHAR_ONEIL") { 
                DenName = "Clubhouse",AmbientMemberMoneyMin = 200, AmbientMemberMoneyMax = 1000,EnemyGangs = new List<string>() { "AMBIENT_GANG_CULT" } } ,//TOILET CLEANER
            new Gang("~q~", "AMBIENT_GANG_KKANGPAE", "Kkangpae","Kkangpae", "Pink", KoreanPeds, KoreanVehicles, "",MeleeWeapons,KkangpaeSidearms,KkangpaeLongGuns,"Kkangpae Leader","CHAR_CHENG") { 
                DenName = "Hangout",AmbientMemberMoneyMin = 2000, AmbientMemberMoneyMax = 7000,EnemyGangs = new List<string>() { "AMBIENT_GANG_YARDIES" } } ,//heroin

            new Gang("~g~", "AMBIENT_GANG_GAMBETTI", "Gambetti Crime Family","Gambetti", "Green", MafiaPeds, MafiaVehicles, "",MeleeWeapons,MafiaSidearms,MafiaLongGuns,"Gambetti Boss","CHAR_TOM") { 
                DenName = "Social Club",AmbientMemberMoneyMin = 1000, AmbientMemberMoneyMax = 10000,EnemyGangs = new List<string>() { "AMBIENT_GANG_LOST" } } ,//cocaine
            new Gang("~g~", "AMBIENT_GANG_PAVANO", "Pavano Crime Family","Pavano", "Green", MafiaPeds, MafiaVehicles, "",MeleeWeapons,MafiaSidearms,MafiaLongGuns,"Pavano Boss","CHAR_DOM") { 
                DenName = "Social Club",AmbientMemberMoneyMin = 1000, AmbientMemberMoneyMax = 10000,EnemyGangs = new List<string>() { "AMBIENT_GANG_ARMENIAN" } } ,//cocaine
            new Gang("~g~", "AMBIENT_GANG_LUPISELLA", "Lupisella Crime Family","Lupisella", "Green", MafiaPeds, MafiaVehicles, "",MeleeWeapons,MafiaSidearms,MafiaLongGuns,"Lupisella Boss","CHAR_AGENT14") { 
                DenName = "Social Club",AmbientMemberMoneyMin = 1000, AmbientMemberMoneyMax = 10000,EnemyGangs = new List<string>() { "AMBIENT_GANG_KKANGPAE" } } ,//cocaine
            new Gang("~g~", "AMBIENT_GANG_MESSINA", "Messina Crime Family","Messina", "Green", MafiaPeds, MafiaVehicles, "",MeleeWeapons,MafiaSidearms,MafiaLongGuns,"Messina Boss","CHAR_BARRY") { 
                DenName = "Social Club",AmbientMemberMoneyMin = 1000, AmbientMemberMoneyMax = 10000,EnemyGangs = new List<string>() { "AMBIENT_GANG_WEICHENG" } } ,//cocaine
            new Gang("~g~", "AMBIENT_GANG_ANCELOTTI", "Ancelotti Crime Family","Ancelotti", "Green", MafiaPeds, MafiaVehicles, "",MeleeWeapons,MafiaSidearms,MafiaLongGuns,"Ancelotti Boss","CHAR_DREYFUSS") { 
                DenName = "Social Club",AmbientMemberMoneyMin = 1000, AmbientMemberMoneyMax = 10000,EnemyGangs = new List<string>() { "AMBIENT_GANG_LOST" } } ,//cocaine



            new Gang("~r~", "AMBIENT_GANG_MADRAZO", "Madrazo Cartel","Cartel", "Red", CartelPeds, CartelVehicles, "",MeleeWeapons,FamiliesSidearms,FamiliesLongGuns,"Madrazo","CHAR_MANUEL") { 
                DenName = "Den",AmbientMemberMoneyMin = 200, AmbientMemberMoneyMax = 1000,EnemyGangs = new List<string>() { "AMBIENT_GANG_MARABUNTE" } } ,//Meth
            new Gang("~b~", "AMBIENT_GANG_ARMENIAN", "Armenian Mob","Armenian", "Black", ArmenianPeds, ArmeniaVehicles, "",MeleeWeapons,FamiliesSidearms,FamiliesLongGuns,"Armenian Leader","CHAR_MP_PROF_BOSS") { 
                DenName = "Hangout",AmbientMemberMoneyMin = 200, AmbientMemberMoneyMax = 1000,EnemyGangs = new List<string>() { "AMBIENT_GANG_PAVANO" } } ,//heroin
            new Gang("~g~", "AMBIENT_GANG_YARDIES", "Yardies","Yardies", "Green", YardiesPeds, YardieVehicles, "",MeleeWeapons,FamiliesSidearms,FamiliesLongGuns,"Yardie O.G.","CHAR_MP_GERALD") { 
                DenName = "Chill Spot",AmbientMemberMoneyMin = 200, AmbientMemberMoneyMax = 1000,EnemyGangs = new List<string>() { "AMBIENT_GANG_KKANGPAE", "AMBIENT_GANG_DIABLOS" } } ,//marijuana
            new Gang("~r~", "AMBIENT_GANG_DIABLOS", "Diablos","Diablos", "Red", VagosPeds, DiablosVehicles, "",MeleeWeapons,FamiliesSidearms,FamiliesLongGuns,"Diablo Leader","CHAR_TW") { 
                DenName = "Hangout",AmbientMemberMoneyMin = 200, AmbientMemberMoneyMax = 1000,EnemyGangs = new List<string>() { "AMBIENT_GANG_YARDIES" } } ,//SPANK

            //DefaultGang
        };
    }


   
}