﻿using LosSantosRED.lsr.Interface;
using LSR.Vehicles;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;


public class WeaponStorage
{
    private ISettingsProvideable Settings;
    private IWeapons Weapons;
    public List<StoredWeapon> StoredWeapons { get; set; } = new List<StoredWeapon>();

    public WeaponStorage(ISettingsProvideable settings)
    {
        Settings = settings;
        StoredWeapons = new List<StoredWeapon>();
    }
    public void Reset()
    {
        StoredWeapons.Clear();
    }
    public void PlaceInStorage(uint hash, IWeapons weapons)
    {
        Weapons = weapons;
        foreach (WeaponDescriptor wd in Game.LocalPlayer.Character.Inventory.Weapons.Where(x=> (uint)x.Hash == hash))
        {
            StoredWeapons.Add(new StoredWeapon((uint)wd.Hash, Vector3.Zero, Weapons.GetWeaponVariation(Game.LocalPlayer.Character, (uint)wd.Hash), wd.Ammo));
        }
        foreach (WeaponDescriptor wd in Game.LocalPlayer.Character.Inventory.Weapons.Where(x => (uint)x.Hash == hash))
        {
            NativeFunction.Natives.REMOVE_WEAPON_FROM_PED(Game.LocalPlayer.Character, (uint)wd.Hash);
        }
    }
    public void PlaceInStorage(StoredWeapon storedWeapon, IWeapons weapons)
    {
        Weapons = weapons;
        if (storedWeapon == null)
        {
            return;
        }
        StoredWeapons.Add(storedWeapon);
        foreach (WeaponDescriptor wd in Game.LocalPlayer.Character.Inventory.Weapons.Where(x => (uint)x.Hash == storedWeapon.WeaponHash))
        {
            NativeFunction.Natives.REMOVE_WEAPON_FROM_PED(Game.LocalPlayer.Character, (uint)wd.Hash);
        }
    }
    public void RemoveFromStorage(StoredWeapon storedWeapon, IWeapons weapons)
    {
        Weapons = weapons;
        if (storedWeapon == null)
        {
            return;
        }
        if(!StoredWeapons.Contains(storedWeapon))
        {
            EntryPoint.WriteToConsole("CANNOT REMOVE STORED WEAPON FROM STORAGE, NOT IN LIST");
            return;
        }
        StoredWeapons.Remove(storedWeapon);
        storedWeapon.GiveToPlayer(Weapons);
    }
    public void CreateInteractionMenu(IInteractionable player, MenuPool menuPool, UIMenu menuToAdd, IWeapons weapons, IModItems modItems, bool withAnimations)
    {
        Weapons = weapons;
        UIMenu WeaponsHeaderMenu = menuPool.AddSubMenu(menuToAdd, "Stored Weapons");
        menuToAdd.MenuItems[menuToAdd.MenuItems.Count() - 1].Description = "Manage Stored Weapons. Place items within storage, or retreive them for use.";
        WeaponsHeaderMenu.SetBannerType(EntryPoint.LSRedColor);
        player.WeaponEquipment.StoreWeapons();
        List<StoredWeapon> PlayerStoredWeapons = player.WeaponEquipment.StoredWeapons.Where(x=> (int)x.WeaponHash != -72657034).ToList();//parachute?
        foreach (StoredWeapon storedWeapon in StoredWeapons)
        {
            storedWeapon.CreateManagementMenu(player, menuPool, this, WeaponsHeaderMenu, weapons, modItems, withAnimations);
        }
        foreach (StoredWeapon storedWeapon in PlayerStoredWeapons)
        {
            if (!StoredWeapons.Any(x => x.WeaponHash == storedWeapon.WeaponHash))
            {
                storedWeapon.CreateManagementMenu(player, menuPool, this, WeaponsHeaderMenu, weapons, modItems, withAnimations);
            }
        }
    }
    public void AddRandomWeapons(IModItems modItems,IWeapons weapons)
    {
        Weapons = weapons;
        if (Settings.SettingsManager.PlayerOtherSettings.MaxRandomWeaponsToGet <= 0)
        {
            EntryPoint.WriteToConsole("NoWeaponSetting");
            return;
        }
        int ItemsToGet = RandomItems.GetRandomNumberInt(1, Settings.SettingsManager.PlayerOtherSettings.MaxRandomWeaponsToGet);
        for (int i = 0; i < ItemsToGet; i++)
        {
            WeaponItem toGet = modItems.GetRandomWeapon(true);
            if (toGet == null)
            {
                EntryPoint.WriteToConsole("NoWeaponItem");
                continue;
            }
            WeaponInformation wi = weapons.GetWeapon(toGet.ModelName);
            if (wi == null)
            {
                EntryPoint.WriteToConsole("NoWeaponModel");
                continue;
            }
            WeaponVariation weaponVariation = new WeaponVariation(0);
            if(RandomItems.RandomPercent(Settings.SettingsManager.PlayerOtherSettings.PercentageToGetRandomWeaponVariation))
            {
                weaponVariation = weapons.GetRandomVariation((uint)wi.Hash, Settings.SettingsManager.PlayerOtherSettings.PercentageToGetComponentInRandomVariation);
            }
            StoredWeapons.Add(new StoredWeapon((uint)wi.Hash, Vector3.Zero, weaponVariation, wi.AmmoAmount));
            EntryPoint.WriteToConsole($"AddedStoredWeapon {wi.Hash}");
        }
    }

    public void OnImpounded()
    {
        RemoveIllegalWeapons();
    }
    private void RemoveIllegalWeapons()
    {
        StoredWeapons.Clear();
    }
}

