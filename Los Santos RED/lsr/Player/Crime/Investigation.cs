﻿using LosSantosRED.lsr;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage;
using Rage.Native;
using System;
using System.Drawing;
using System.Linq;

public class Investigation
{
    private uint GameTimeLastInvestigationExpired;
    private uint GameTimeStartedInvestigation;
    private Blip InvestigationBlip;
    private IPoliceRespondable Player;
    private ISettingsProvideable Settings;
    private IEntityProvideable World;
    private bool HavePlayerDescription = false;
    private uint GameTimeLastUpdatedInvestigation;
    private uint GameTimePoliceArrived;
    private uint GameTimeEMSArrived;
    private uint GameTimeFireArrived;
    private int PoliceToRespond(int wantedLevel)
    {
        {
            if(wantedLevel >= 6)
            {
                return Settings.SettingsManager.PoliceTaskSettings.InvestigationRespondingOfficers_Wanted6;
            }
            if(wantedLevel >= 5)
            {
                return Settings.SettingsManager.PoliceTaskSettings.InvestigationRespondingOfficers_Wanted5;
            }
            else if (wantedLevel >= 4)
            {
                return Settings.SettingsManager.PoliceTaskSettings.InvestigationRespondingOfficers_Wanted4;
            }
            else if (wantedLevel >= 3)
            {
                return Settings.SettingsManager.PoliceTaskSettings.InvestigationRespondingOfficers_Wanted3;
            }
            else if (wantedLevel >= 2)
            {
                return Settings.SettingsManager.PoliceTaskSettings.InvestigationRespondingOfficers_Wanted2;
            }
            else
            {
                return Settings.SettingsManager.PoliceTaskSettings.InvestigationRespondingOfficers_Wanted1;
            }
        }
    }
    public Color BlipColor => World.TotalWantedLevel > 0 ? Color.DarkOrange : RequiresPolice && IsSuspicious ? Color.Orange : RequiresPolice ? Color.Yellow : Color.White;
    public Investigation(IPoliceRespondable player, ISettingsProvideable settings, IEntityProvideable world)
    {
        Player = player;
        Settings = settings;
        World = world;
    }
    public string DebugText => $"Invest: IsActive {IsActive} IsSus {IsSuspicious} Distance {Distance} Position {Position}";
    public float Distance => Settings.SettingsManager.InvestigationSettings.ActiveDistance;
    public bool IsActive { get; private set; }
    public bool IsSuspicious => IsActive && IsNearPosition && HavePlayerDescription;
    public Vector3 Position { get; private set; }
    private bool IsOutsideInvestigationRange { get; set; }



    public bool HasPoliceInvestigated => HasPoliceArrived && Game.GameTime - GameTimePoliceArrived >= Settings.SettingsManager.InvestigationSettings.ExtraTimeAfterReachingInvestigationCenterBeforeExpiring;

    public bool HasEMSInvestigated => HasEMSArrived && Game.GameTime - GameTimeEMSArrived >= Settings.SettingsManager.InvestigationSettings.ExtraTimeAfterReachingInvestigationCenterBeforeExpiring;

    public bool HasFireInvestigated => HasFireArrived && Game.GameTime - GameTimeFireArrived >= Settings.SettingsManager.InvestigationSettings.ExtraTimeAfterReachingInvestigationCenterBeforeExpiring;



    public bool HasPoliceArrived { get; private set; }
    public bool HasEMSArrived { get; private set; }
    public bool HasFireArrived { get; private set; }

    public int InvestigationWantedLevel { get; private set; }

    public bool RequiresPolice { get; set; }
    public bool RequiresEMS { get; set; }
    public bool RequiresFirefighters { get; set; }
    public int RespondingPolice { get; private set; }

    private bool IsTimedOut => GameTimeStartedInvestigation != 0 && Game.GameTime - GameTimeStartedInvestigation >= Settings.SettingsManager.InvestigationSettings.TimeLimit;//60000;//short for testing was 180000

    private bool IsMinTimedOut => GameTimeStartedInvestigation != 0 && Game.GameTime - GameTimeStartedInvestigation >= Settings.SettingsManager.InvestigationSettings.MinTimeLimit;//60000;//short for testing was 180000

    public bool IsNearPosition { get; private set; }
    public int CurrentRespondingPoliceCount { get; private set; }

    public void Dispose()
    {
        if (InvestigationBlip.Exists())
        {
            InvestigationBlip.Delete();
        }
    }
    public void Reset()
    {
        IsActive = false;
        HavePlayerDescription = false;
        GameTimeStartedInvestigation = 0;
        GameTimeLastInvestigationExpired = 0;
        RequiresPolice = false;
        RequiresEMS = false;
        RequiresFirefighters = false;
        IsNearPosition = false;

        HasPoliceArrived = false;
        HasFireArrived = false;
        HasEMSArrived = false;
        GameTimeEMSArrived = 0;
        GameTimeFireArrived = 0;
        GameTimePoliceArrived = 0;
        foreach (Cop cop in World.Pedestrians.AllPoliceList.Where(x => x.IsRespondingToInvestigation))
        {
            cop.IsRespondingToInvestigation = false;
        }
        CurrentRespondingPoliceCount = 0;
        InvestigationWantedLevel = 0;
        if (InvestigationBlip.Exists())
        {
            InvestigationBlip.Delete();
        }
    }
    public void Start(Vector3 postionToInvestigate, bool havePlayerDescription, bool requiresPolice, bool requiresEMS, bool requiresFirefighters)
    {
        if(Player.IsWanted)
        {
            return;
        }
        if(requiresPolice)
        {
            RequiresPolice = true;
        }
        if(requiresEMS)
        {
            RequiresEMS = true;
        }
        if(requiresFirefighters)
        {
            RequiresFirefighters = true;
        }
        Position = NativeHelper.GetStreetPosition(postionToInvestigate, true);
        GameFiber.Yield();
        if (havePlayerDescription)
        {
            HavePlayerDescription = havePlayerDescription;
        }
        if (!IsActive)
        {
            IsActive = true;
            GameTimeStartedInvestigation = Game.GameTime;
            if (Settings.SettingsManager.InvestigationSettings.CreateBlip)
            {
                InvestigationBlip = new Blip(Position, 250f)
                {
                    Name = "Investigation Center",
                    Color = BlipColor,
                    Alpha = 0.25f
                };
                NativeFunction.Natives.BEGIN_TEXT_COMMAND_SET_BLIP_NAME("STRING");
                NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME("Investigation Center");
                NativeFunction.Natives.END_TEXT_COMMAND_SET_BLIP_NAME(InvestigationBlip);
                NativeFunction.Natives.SET_BLIP_AS_SHORT_RANGE((uint)InvestigationBlip.Handle, true);
                GameFiber.Yield();
            }
            EntryPoint.WriteToConsole($"PLAYER EVENT: INVESTIGATION START {RequiresPolice} {RequiresEMS} {RequiresFirefighters}");
        }
        
    }
    public void Update()
    {
        IsNearPosition = Position != Vector3.Zero && Player.Character.DistanceTo2D(Position) <= Settings.SettingsManager.InvestigationSettings.SuspiciousDistance;
        IsOutsideInvestigationRange = Position == Vector3.Zero || Game.LocalPlayer.Character.DistanceTo2D(Position) > Settings.SettingsManager.InvestigationSettings.MaxDistance;
        if (IsActive && Player.IsNotWanted)
        {
            AssignCops();
            GameFiber.Yield();
            CheckExpired();
            if (IsSuspicious && Player.AnyPoliceCanRecognizePlayer && Player.PoliceResponse.HasBeenNotWantedFor >= 5000)
            {
                Player.PoliceResponse.ApplyReportedCrimes();
            }
        }
        UpdateBlip();
        if (Settings.SettingsManager.PerformanceSettings.PrintUpdateTimes)
        {
            EntryPoint.WriteToConsole($"Investigation Update (Secondary Tasks) 250ms? Ran Time Since {Game.GameTime - GameTimeLastUpdatedInvestigation}",5);
        }
        GameTimeLastUpdatedInvestigation = Game.GameTime;
    }


    public void OnPoliceArrived()
    {
        if(HasPoliceArrived || !IsActive)
        {
            return;
        }
        GameTimePoliceArrived = Game.GameTime;
        HasPoliceArrived = true;
        EntryPoint.WriteToConsole("Investigation OnPoliceArrived");
    }
    public void OnEMSArrived()
    {
        if (HasEMSArrived || !IsActive)
        {
            return;
        }
        GameTimeEMSArrived = Game.GameTime;
        HasEMSArrived = true;
        EntryPoint.WriteToConsole("Investigation OnEMSArrived");
    }
    public void OnFireFightersArrived()
    {
        if (HasFireArrived || !IsActive)
        {
            return;
        }
        GameTimeFireArrived = Game.GameTime;
        HasFireArrived = true;
        EntryPoint.WriteToConsole("Investigation OnFireFightersArrived");
    }

    private void CheckExpired()
    {
        if(IsOutsideInvestigationRange)
        {
            EntryPoint.WriteToConsole("Investigation Expire OUTSIDE RANGE");
            Expire();
        }
        else if(IsTimedOut)
        {
            EntryPoint.WriteToConsole("Investigation Expire TIMED OUT");
            Expire();
        }
        else if (IsMinTimedOut && CheckCriteria())
        {
            EntryPoint.WriteToConsole("Investigation Expire MIN TIME AND CRITERIA EXPIRE");
            Expire();
        }
    }
    private bool CheckCriteria()
    {
        bool CanPoliceExpire;//rewrite, no need for so many IFS!
        bool CanFireExpire;
        bool CanEMSExpire;

        if(!RequiresPolice)
        {
            CanPoliceExpire = true;
        }
        else 
        {
           // bool isWantedLevelOK = ;
            //bool HasArrivedOK = World.Pedestrians.AllPoliceList.Any(x =>
            //            x.GameTimeReachedInvestigationPosition > 0 &&
            //            Game.GameTime - x.GameTimeReachedInvestigationPosition >= Settings.SettingsManager.InvestigationSettings.ExtraTimeAfterReachingInvestigationCenterBeforeExpiring);

            CanPoliceExpire = World.TotalWantedLevel <= 1 && HasPoliceInvestigated;

            //EntryPoint.WriteToConsole($"INVESTIGATION CheckCriteria {isWantedLevelOK} {HasArrivedOK} RequiresPolice{RequiresPolice} RequiresFirefighters{RequiresFirefighters} RequiresEMS{RequiresEMS}");
        }

        
        if(!RequiresFirefighters)
        {
            CanFireExpire = true;
        }
        else
        { 
            CanFireExpire = !World.AnyFiresNearPlayer && HasFireInvestigated;
        }

        if(!RequiresEMS)
        {
            CanEMSExpire = true;
        }
        else
        {
            CanEMSExpire = !World.Pedestrians.AnyInjuredPeopleNearPlayer && !HasEMSInvestigated;
        }

        if(CanPoliceExpire)
        {
            RequiresPolice = false;
        }
        if (CanFireExpire)
        {
            RequiresFirefighters = false;
        }
        if (CanEMSExpire)
        {
            RequiresEMS = false;
        }
        return CanPoliceExpire && CanFireExpire && CanEMSExpire;
    }
    private void AssignCops()
    {
        if (RequiresPolice)
        {
            CrimeEvent HighestCrimeEvent = Player.PoliceResponse.CrimesReported.OrderBy(x => x.AssociatedCrime?.Priority).FirstOrDefault();
            if (World.TotalWantedLevel > 0)
            {
                InvestigationWantedLevel = World.TotalWantedLevel;
                RespondingPolice = PoliceToRespond(World.TotalWantedLevel);
            }    
            else if (HighestCrimeEvent != null)
            {
                InvestigationWantedLevel = HighestCrimeEvent.AssociatedCrime.ResultingWantedLevel;
                RespondingPolice = PoliceToRespond(HighestCrimeEvent.AssociatedCrime.ResultingWantedLevel);
            }
            else
            {
                InvestigationWantedLevel = 1;
                RespondingPolice = PoliceToRespond(1);
            }
            int tasked = 0;
            foreach (Cop cop in World.Pedestrians.AllPoliceList.Where(x => x.Pedestrian.Exists() && (InvestigationWantedLevel >= 3 || x.AssignedAgency?.Classification == Classification.Police || x.AssignedAgency?.Classification == Classification.Sheriff)).OrderBy(x => x.Pedestrian.DistanceTo2D(Position)))//first pass, only want my police and whatever units?
            {
                if(!cop.IsInVehicle && cop.Pedestrian.DistanceTo2D(Position) >= 150f)
                {
                    cop.IsRespondingToInvestigation = false;
                }
                else if (!cop.IsDead && !cop.IsUnconscious && tasked < RespondingPolice)
                {
                    cop.IsRespondingToInvestigation = true;
                    tasked++;
                }
                else
                {
                    cop.IsRespondingToInvestigation = false;
                }
            }
            if(tasked < RespondingPolice)
            {
                foreach (Cop cop in World.Pedestrians.AllPoliceList.Where(x => x.Pedestrian.Exists() && !x.IsRespondingToInvestigation && x.AssignedAgency?.Classification != Classification.Police && x.AssignedAgency?.Classification != Classification.Sheriff).OrderBy(x => x.Pedestrian.DistanceTo2D(Position)))
                {
                    if (!cop.IsInVehicle && cop.Pedestrian.DistanceTo2D(Position) >= 150f)
                    {
                        cop.IsRespondingToInvestigation = false;
                    }
                    else if (!cop.IsDead && !cop.IsUnconscious && tasked < RespondingPolice)
                    {
                        cop.IsRespondingToInvestigation = true;
                        tasked++;
                    }
                }
            }
            CurrentRespondingPoliceCount = tasked;
            // EntryPoint.WriteToConsole($"Investigation Active, RespondingPolice {RespondingPolice} Total Tasked {tasked}");
        }
        else
        {
            foreach (Cop cop in World.Pedestrians.AllPoliceList.Where(x => x.IsRespondingToInvestigation))
            {
                cop.IsRespondingToInvestigation = false;
            }
            CurrentRespondingPoliceCount = 0;
            InvestigationWantedLevel = 0;
        }
    }
    private void UpdateBlip()
    {
        if (IsActive && Player.IsNotWanted && Settings.SettingsManager.InvestigationSettings.CreateBlip)
        {
            if (!InvestigationBlip.Exists())
            {
                InvestigationBlip = new Blip(Position, 250f)
                {
                    Name = "Investigation Center",
                    Color = BlipColor,//Color.Yellow,
                    Alpha = 0.25f
                };
                NativeFunction.Natives.BEGIN_TEXT_COMMAND_SET_BLIP_NAME("STRING");
                NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME("Investigation Center");
                NativeFunction.Natives.END_TEXT_COMMAND_SET_BLIP_NAME(InvestigationBlip);
                NativeFunction.Natives.SET_BLIP_AS_SHORT_RANGE((uint)InvestigationBlip.Handle, true);
                GameFiber.Yield();
            }
            else
            {
                InvestigationBlip.Position = Position;
                InvestigationBlip.Color = BlipColor;
            }
        }
        else
        {
            if (InvestigationBlip.Exists())
            {
                InvestigationBlip.Delete();
            }
        }
    }
    public void Expire()
    {
        EntryPoint.WriteToConsole($"Investigation Expiring RequiresPolice{RequiresPolice} RequiresEMS{RequiresEMS} RequiresFirefighters{RequiresFirefighters}");
        IsActive = false;
        HavePlayerDescription = false;
        GameTimeStartedInvestigation = 0;
        GameTimeLastInvestigationExpired = Game.GameTime;
        RequiresPolice = false;
        RequiresEMS = false;
        RequiresFirefighters = false;
        HasPoliceArrived = false;
        HasFireArrived = false;
        HasEMSArrived = false;
        GameTimeEMSArrived = 0;
        GameTimeFireArrived = 0;
        GameTimePoliceArrived = 0;
        if (InvestigationBlip.Exists())
        {
            InvestigationBlip.Delete();
        }
        foreach (Cop cop in World.Pedestrians.AllPoliceList.Where(x => x.IsRespondingToInvestigation))
        {
            cop.IsRespondingToInvestigation = false;
            cop.GameTimeReachedInvestigationPosition = 0;
        }
        InvestigationWantedLevel = 0;
        CurrentRespondingPoliceCount = 0;
        RespondingPolice = 0;
        Player.OnInvestigationExpire();
    }
}

