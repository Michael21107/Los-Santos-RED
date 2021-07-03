﻿using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WeaponInventory
{
    private Cop Cop;
    private uint GameTimeLastWeaponCheck;
    private bool IsSetDeadly;
    private bool IsSetLessLethal;
    private bool IsSetUnarmed;
    private bool IsSetDefault;
    private IssuableWeapon LongGun;
    private IssuableWeapon Sidearm;
    private bool HasHeavyWeaponOnPerson;
    private int DesiredAccuracy => IsSetLessLethal ? 30 : 10;

    public WeaponInventory(Cop cop)
    {
        Cop = cop;
    }

    public bool NeedsWeaponCheck => GameTimeLastWeaponCheck == 0 || Game.GameTime > GameTimeLastWeaponCheck + 750;
    public bool ShouldAutoSetWeaponState { get; set; } = true;
    public bool HasPistol => Sidearm != null;
    public void IssueWeapons()
    {
        Sidearm = Cop.AssignedAgency.GetRandomWeapon(true);
        //EntryPoint.WriteToConsole($"Issued: {Sidearm.ModelName} Variation: {string.Join(",", Sidearm.Variation.Components.Select(x => x.Name))}");
        LongGun = Cop.AssignedAgency.GetRandomWeapon(false);
        //EntryPoint.WriteToConsole($"Issued: {LongGun.ModelName} Variation: {string.Join(",", LongGun.Variation.Components.Select(x => x.Name))}");
    }
    public void UpdateLoadout(bool IsDeadlyChase, int WantedLevel)
    {
        if (ShouldAutoSetWeaponState)
        {
            if (WantedLevel == 0)
            {
                if (!IsSetDefault)
                {
                    SetDefault();
                }

            }
            else
            {
                if (IsDeadlyChase)
                {
                    if (Cop.IsInVehicle)
                    {
                        HasHeavyWeaponOnPerson = true;
                        if(WantedLevel < 4)
                        {
                            SetUnarmed();
                        }
                        else
                        {
                            SetDeadly();
                        }
                    }
                    else
                    {
                        SetDeadly();
                    }
                }
                else
                {
                    if (Cop.IsInVehicle)
                    {
                        SetUnarmed();
                    }
                    else
                    {
                        SetLessLethal();
                    }
                }
            }
            Cop.Pedestrian.Accuracy = DesiredAccuracy;
        }
    }
    private void SetDefault()
    {
        if ((!IsSetDefault || NeedsWeaponCheck) && Cop.Pedestrian.Exists() && Cop.Pedestrian.IsAlive)
        {
            EntryPoint.WriteToConsole($"COP EVENT {Cop.Pedestrian.Handle}: SETTING DEFAULT WEAPON LOADOUT", 3);
            if (Cop.Pedestrian.Inventory != null && !Cop.Pedestrian.Inventory.Weapons.Contains(WeaponHash.StunGun))
            {
                Cop.Pedestrian.Inventory.GiveNewWeapon(WeaponHash.StunGun, 100, false);
            }
            if (Cop.Pedestrian.Inventory != null && !Cop.Pedestrian.Inventory.Weapons.Contains(Sidearm.ModelName))
            {
                Cop.Pedestrian.Inventory.GiveNewWeapon(Sidearm.ModelName, -1, false);
                Sidearm.ApplyVariation(Cop.Pedestrian);
            }
            //if (setCurrent && Cop.Pedestrian.Inventory != null && Cop.Pedestrian.Inventory.EquippedWeapon != null)
            //{
                NativeFunction.CallByName<bool>("SET_CURRENT_PED_WEAPON", Cop.Pedestrian, 2725352035, true);
           // }
            NativeFunction.CallByName<bool>("SET_PED_CAN_SWITCH_WEAPON", Cop.Pedestrian, true);//was false, but might need them to switch in vehicles and if hanging outside vehicle
            NativeFunction.CallByName<bool>("SET_PED_COMBAT_ATTRIBUTES", Cop.Pedestrian, 2, true);//can do drivebys       
            IsSetLessLethal = false;
            IsSetUnarmed = false;
            IsSetDeadly = false;
            IsSetDefault = true;
            GameTimeLastWeaponCheck = Game.GameTime;
        }
    }
    private void SetDeadly()
    {
        if ((!IsSetDeadly || NeedsWeaponCheck) && Cop.Pedestrian.Exists() && Cop.Pedestrian.IsAlive)
        {
            if (Cop.Pedestrian.Inventory != null && !Cop.Pedestrian.Inventory.Weapons.Contains(Sidearm.ModelName))
            {
                Cop.Pedestrian.Inventory.GiveNewWeapon(Sidearm.ModelName, -1, true);
                Sidearm.ApplyVariation(Cop.Pedestrian);
            }
            if (Cop.Pedestrian.Inventory != null && !Cop.Pedestrian.Inventory.Weapons.Contains(LongGun.ModelName))
            {
                Cop.Pedestrian.Inventory.GiveNewWeapon(LongGun.ModelName, -1, true);
                LongGun.ApplyVariation(Cop.Pedestrian);
            }
            if (LongGun != null && HasHeavyWeaponOnPerson && !Cop.IsInVehicle)
            {
                NativeFunction.CallByName<bool>("SET_CURRENT_PED_WEAPON", Cop.Pedestrian, LongGun.GetHash(), true);
            }
            else
            {
                NativeFunction.CallByName<bool>("SET_CURRENT_PED_WEAPON", Cop.Pedestrian, Sidearm.GetHash(), true);
            }
            NativeFunction.CallByName<bool>("SET_PED_CAN_SWITCH_WEAPON", Cop.Pedestrian, true);//was false, but might need them to switch in vehicles and if hanging outside vehicle
            //NativeFunction.CallByName<bool>("SET_PED_COMBAT_ATTRIBUTES", Pedestrian, 1, true);//can use vehicle in combat
            NativeFunction.CallByName<bool>("SET_PED_COMBAT_ATTRIBUTES", Cop.Pedestrian, 2, true);//can do drivebys       
            IsSetLessLethal = false;
            IsSetUnarmed = false;
            IsSetDeadly = true;
            IsSetDefault = false;
            GameTimeLastWeaponCheck = Game.GameTime;
        }
    }
    private void SetLessLethal()
    {
        if ((!IsSetLessLethal || NeedsWeaponCheck) && Cop.Pedestrian.Exists() && Cop.Pedestrian.IsAlive)
        {
            if (Cop.Pedestrian.Inventory != null && !Cop.Pedestrian.Inventory.Weapons.Contains(WeaponHash.StunGun))
            {
                Cop.Pedestrian.Inventory.GiveNewWeapon(WeaponHash.StunGun, 100, true);
            }
            else if (Cop.Pedestrian.Inventory != null && Cop.Pedestrian.Inventory.EquippedWeapon != WeaponHash.StunGun)
            {
                Cop.Pedestrian.Inventory.EquippedWeapon = WeaponHash.StunGun;
            }
            NativeFunction.CallByName<bool>("SET_PED_CAN_SWITCH_WEAPON", Cop.Pedestrian, false);
            //NativeFunction.CallByName<bool>("SET_PED_COMBAT_ATTRIBUTES", Pedestrian, 1, false);//cant use vehicle in combat
            NativeFunction.CallByName<bool>("SET_PED_COMBAT_ATTRIBUTES", Cop.Pedestrian, 2, false);//cant do drivebys
            IsSetLessLethal = true;
            IsSetUnarmed = false;
            IsSetDeadly = false;
            IsSetDefault = false;
            GameTimeLastWeaponCheck = Game.GameTime;
        }
    }
    private void SetUnarmed()
    {
        if ((!IsSetUnarmed || NeedsWeaponCheck) && Cop.Pedestrian.Exists() && Cop.Pedestrian.IsAlive)
        {
            if (Cop.Pedestrian.Inventory != null && Cop.Pedestrian.Inventory.EquippedWeapon != null)
            {
                NativeFunction.CallByName<bool>("SET_CURRENT_PED_WEAPON", Cop.Pedestrian, 2725352035, true);
                NativeFunction.CallByName<bool>("SET_PED_CAN_SWITCH_WEAPON", Cop.Pedestrian, false);
            }
            // NativeFunction.CallByName<bool>("SET_PED_COMBAT_ATTRIBUTES", Pedestrian, 1, false);//cant use vehicle in combat
            NativeFunction.CallByName<bool>("SET_PED_COMBAT_ATTRIBUTES", Cop.Pedestrian, 2, false);//cant do drivebys
            IsSetLessLethal = false;
            IsSetUnarmed = true;
            IsSetDeadly = false;
            IsSetDefault = false;
            GameTimeLastWeaponCheck = Game.GameTime;
        }
    }
}

