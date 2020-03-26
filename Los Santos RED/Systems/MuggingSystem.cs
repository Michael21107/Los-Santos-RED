﻿using ExtensionsMethods;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class MuggingSystem
{
    
    private static Ped LastAimedAtPed;
   // private static Vector3 PlaceLastMugged;
    private static uint GameTimeStartedAimingAtTarget;
    public static bool IsRunning { get; set; }
    public static bool IsMugging { get; set; }
    public static bool CanMugFromBehind
    {
        get
        {
            if (GameTimeStartedAimingAtTarget == 0)
                return false;
            else if (Game.GameTime - GameTimeStartedAimingAtTarget >= 2000)
                return true;
            else
                return false;
        }
    }
    public static void Initialize()
    {
        IsRunning = true;
    }
    public static void Dispose()
    {
        IsRunning = false;
    }
    public static void Tick()
    {

        if (!IsMugging && Game.LocalPlayer.Character.IsAiming && Game.LocalPlayer.IsFreeAimingAtAnyEntity)
        {
            Entity Target = Game.LocalPlayer.GetFreeAimingTarget();

            if (!(Target is Ped))
                return;

            if (PedScanning.CopPeds.Any(x => x.Pedestrian.Handle == Target.Handle))
                return;//aiming at cop

            GTAPed GTAPedTarget = PedScanning.Civilians.FirstOrDefault(x => x.Pedestrian.Handle == Target.Handle);

            if (GTAPedTarget == null)
                GTAPedTarget = new GTAPed((Ped)Target, false, Target.Health);

            bool CanSee = GTAPedTarget.Pedestrian.CanSeePlayer();
            // 

            if (!GTAPedTarget.HasBeenMugged && GTAPedTarget.DistanceToPlayer <= 15f && !GTAPedTarget.Pedestrian.IsInAnyVehicle(false))
            {
                if (CanSee)
                    MugTarget(GTAPedTarget,false);
                else
                {
                    if (LastAimedAtPed == GTAPedTarget.Pedestrian)
                    {
                        StartedAimingAtTarget();
                    }
                    else
                    {
                        StoppedAimingAtTarget();
                        LastAimedAtPed = GTAPedTarget.Pedestrian;
                    }
                }
                if (GameTimeStartedAimingAtTarget > 0 && Game.GameTime - GameTimeStartedAimingAtTarget > 1500)
                    MugTarget(GTAPedTarget,false);
            }
            else
            {
                StoppedAimingAtTarget();
            }
        }
        else if (!IsMugging && !Game.LocalPlayer.Character.IsAiming && NativeFunction.CallByName<bool>("IS_PLAYER_TARGETTING_ANYTHING",Game.LocalPlayer))
        {
            GTAWeapon MyWeapon = LosSantosRED.GetCurrentWeapon();
            if (MyWeapon == null || MyWeapon.Category != GTAWeapon.WeaponCategory.Melee)
                return;

            int TargetEntity;
            bool Found;
            unsafe
            {
                Found = NativeFunction.CallByName<bool>("GET_PLAYER_TARGET_ENTITY", Game.LocalPlayer, &TargetEntity);
            }
            if (!Found)
                return;
     
            int Handle = TargetEntity;
            Debugging.WriteToLog("Muggin Melee", string.Format("Middle Handle: {0}", Handle));



            if (PedScanning.CopPeds.Any(x => x.Pedestrian.Handle == Handle))
                return;//aiming at cop

            GTAPed GTAPedTarget = PedScanning.Civilians.FirstOrDefault(x => x.Pedestrian.Handle == Handle);

            if (GTAPedTarget == null)
                return;

            if(!GTAPedTarget.HasBeenMugged)
                MugTarget(GTAPedTarget,true);

            Debugging.WriteToLog("Muggin Melee", string.Format("Made it to the End Ped Handle: {0}", Handle));
        }

        if (!Game.LocalPlayer.Character.IsAiming)
            StoppedAimingAtTarget();
    }
    private static void MugTarget(GTAPed MuggingTarget,bool IsMelee)
    {
        GameFiber.StartNew(delegate
        {
            IsMugging = true;
            MuggingTarget.CanFlee = false;
            uint GameTimeStartedMugging = Game.GameTime;
            MuggingTarget.Pedestrian.BlockPermanentEvents = true;

            LosSantosRED.RequestAnimationDictionay("ped");

            if (!Game.LocalPlayer.Character.IsAnySpeechPlaying)
                Game.LocalPlayer.Character.PlayAmbientSpeech("CHALLENGE_THREATEN");
            
            NativeFunction.CallByName<bool>("TASK_PLAY_ANIM", MuggingTarget.Pedestrian, "ped", "handsup_enter", 2.0f, -2.0f, -1, 2, 0, false, false, false);
            GameFiber.Sleep(750);
            MuggingTarget.Pedestrian.PlayAmbientSpeech("GUN_BEG");
            while (!NativeFunction.CallByName<bool>("IS_ENTITY_PLAYING_ANIM", MuggingTarget.Pedestrian, "ped", "handsup_enter", 1))
            {
                GameFiber.Sleep(100);


                if(Game.GameTime - GameTimeStartedMugging >= 2500)
                {
                    IsMugging = false;
                    MuggingTarget.CanFlee = true;
                    break;
                }
            }

            if (!IsMugging)
                return;

            GameFiber.Sleep(500);

            GameTimeStartedMugging = Game.GameTime;
            bool Intimidated = false;
            while (Game.GameTime - GameTimeStartedMugging <= 1500)
            {      
                if (!IsMelee && !Game.LocalPlayer.IsFreeAiming)
                {
                    Intimidated = false;
                    break;
                }
                else if (IsMelee && !NativeFunction.CallByName<bool>("IS_PLAYER_TARGETTING_ANYTHING", Game.LocalPlayer))
                {
                    Intimidated = false;
                    break;
                }
                Intimidated = true;
                GameFiber.Yield();
            }

            MuggingTarget.Pedestrian.BlockPermanentEvents = false;
            if (Intimidated)
            {
                NativeFunction.CallByName<bool>("SET_PED_MONEY", MuggingTarget.Pedestrian, 0);
                Vector3 MoneyPos = MuggingTarget.Pedestrian.Position.Around2D(0.5f, 1.5f);
                NativeFunction.CallByName<bool>("CREATE_AMBIENT_PICKUP", Game.GetHashKey("PICKUP_MONEY_VARIABLE"), MoneyPos.X, MoneyPos.Y, MoneyPos.Z, 0, LosSantosRED.MyRand.Next(15, 100), 1, false, true);
                MuggingTarget.HasBeenMugged = true;
                MuggingTarget.AddCrime(Police.CurrentCrimes.Mugging,MuggingTarget.Pedestrian.Position);
            }
            MuggingTarget.CanFlee = true;
            IsMugging = false;      
        });
    }
    public static void StartedAimingAtTarget()
    {
        GameTimeStartedAimingAtTarget = Game.GameTime;
    }
    public static void StoppedAimingAtTarget()
    {
        GameTimeStartedAimingAtTarget = 0;
    }
}
