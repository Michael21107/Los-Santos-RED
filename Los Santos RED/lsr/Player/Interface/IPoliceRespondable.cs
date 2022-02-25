﻿using LosSantosRED.lsr.Locations;
using LSR.Vehicles;
using Rage;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;

namespace LosSantosRED.lsr.Interface
{
    public interface IPoliceRespondable
    {
        bool AnyPoliceCanHearPlayer { get; set; }
        bool AnyPoliceCanRecognizePlayer { get; set; }
        bool AnyPoliceCanSeePlayer { get; set; }
        bool AnyPoliceRecentlySeenPlayer { get; set; }
        LocationData CurrentLocation { get; set; }
        PoliceResponse PoliceResponse { get;  }//should not be
        VehicleExt CurrentSeenVehicle { get; }
        WeaponInformation CurrentSeenWeapon { get; }
        VehicleExt CurrentVehicle { get; }
        WeaponInformation CurrentWeapon { get; }
        WeaponCategory CurrentWeaponCategory { get; }
        Investigation Investigation { get; }
        bool IsAliveAndFree { get; }
        bool IsBustable { get; }
        bool IsBusted { get; }
        bool IsVisiblyArmed { get; }
        bool IsDead { get; }
        bool IsInSearchMode { get; set; }

        bool IsInVehicle { get; }
        bool IsNotWanted { get; }
        bool IsWanted { get; }
        int MaxWantedLastLife { get; }
        Vector3 PlacePoliceLastSeenPlayer { get; set; }
        bool RecentlyBusted { get; }
        bool RecentlyStartedPlaying { get; }
        List<VehicleExt> ReportedStolenVehicles { get; }
        int WantedLevel { get; }
        uint TimeToRecognize { get; }
        uint TimeInSearchMode { get; }
        //bool StarsRecentlyGreyedOut { get; }
        //bool StarsRecentlyActive { get; }
        Violations Violations { get; }//not good comrade
        Vector3 Position { get; }
        Ped Character { get; }
        float SearchModePercentage { get; }
        //uint HasBeenWantedFor { get; }
        //Vector3 RootPosition { get; }
        bool IsAttemptingToSurrender { get; }
        bool IsCop { get; }
        float ClosestPoliceDistanceToPlayer { get; set; }
        bool RecentlyShot { get; }

        void Arrest();
      //  void StoreCriminalHistory();
        void AddCrime(Crime crime, bool ByPolice, Vector3 positionLastSeenCrime, VehicleExt vehicleLastSeenPlayerIn, WeaponInformation weaponLastSeenPlayerWith, bool HaveDescription, bool announceCrime, bool IsForPlayer);
        void ResetScanner();
        void OnAppliedWantedStats(int wantedLevel);
        void OnWantedActiveMode();
        void OnWantedSearchMode();
        void OnInvestigationExpire();
        void OnRequestedBackUp();
        void OnWeaponsFree();
        void OnLethalForceAuthorized();
        void OnPoliceNoticeVehicleChange();
        void OnSuspectEluded();
        void SetWantedLevel(int resultingWantedLevel, string name, bool v);
        //void AddInjured(PedExt myPed, bool wasShot, bool wasMeleeAttacked, bool wasHitByVehicle);
        //void AddKilled(PedExt myPed, bool wasShot, bool wasMeleeAttacked, bool wasHitByVehicle);
        GangRelationships GangRelationships { get; }
        BigMessageHandler BigMessage { get; }

        void YellInPain();
        //void ChangeReputation(Gang gang, int v, bool sendNotification);
    }
}