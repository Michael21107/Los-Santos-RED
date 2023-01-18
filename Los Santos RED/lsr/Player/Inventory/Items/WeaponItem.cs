﻿using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using LSR.Vehicles;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;


public class WeaponItem : ModItem
{
    private bool IsProgramicallyChangingItems = false;
    private WeaponVariation CurrentWeaponVariation = new WeaponVariation();
    private WeaponInformation WeaponInformation;
    public bool RequiresDLC { get; set; } = false;
    public string ModelName { get; set; }
    public uint ModelHash { get; set; }

    public WeaponItem()
    {
    }
    public WeaponItem(string name, string description, bool requiresDLC, ItemType itemType) : base(name, description, itemType)
    {
        RequiresDLC = requiresDLC;
    }

    public override void Setup(PhysicalItems physicalItems, IWeapons weapons)
    {
        //ModelItem = physicalItems.Get(ModelItemID);
        //if (ModelItem == null)
        //{
            //ModelItem = new PhysicalItem(ModelItemID, Game.GetHashKey(ModelItemID), ePhysicalItemType.Weapon);
        //}
        ModelItem = new PhysicalItem(ModelName, ModelHash == 0 ? Game.GetHashKey(ModelName) : ModelHash, ePhysicalItemType.Weapon);
        WeaponInformation = weapons.GetWeapon(ModelItem?.ModelName);
        if (WeaponInformation == null)
        {
            MenuCategory = "Weapon";
        }
        else
        {
            MenuCategory = WeaponInformation.Category.ToString();
        }
    }

    public override void CreateSellMenuItem(Transaction Transaction, MenuItem menuItem, UIMenu sellMenuRNUI, ISettingsProvideable settings, ILocationInteractable player, bool isStealing, IEntityProvideable world)
    {

        string description;
        if (Description.Length >= 200)
        {
            description = Description.Substring(0, 200) + "...";//menu cant show more than 225?, need some for below
        }
        else
        {
            description = Description;
        }
        description += "~n~~s~";
        if (RequiresDLC)
        {
            description += $"~n~~b~DLC Weapon";
        }
        string formattedPurchasePrice = menuItem.SalesPrice.ToString("C0");
        //WeaponInformation myWeapon = Weapons.GetWeapon(ModelItem.ModelName);
        bool hasPedGotWeapon = false;
        if (NativeFunction.Natives.HAS_PED_GOT_WEAPON<bool>(player.Character, WeaponInformation.Hash, false))
        {
            hasPedGotWeapon = true;
        }
        else
        {
            hasPedGotWeapon = false;
        }

        if (ModelItem != null && ModelItem.ModelHash != 0)
        {
            NativeFunction.Natives.REQUEST_WEAPON_ASSET(ModelItem.ModelHash, 31, 0);
        }

        UIMenu WeaponMenu = null;
        bool FoundCategoryMenu = false;
        if (WeaponInformation != null)
        {
            UIMenu WeaponSubMenu = sellMenuRNUI.Children.Where(x => x.Value.SubtitleText.ToLower() == "weapons").FirstOrDefault().Value;
            UIMenu ToCheckFirst = sellMenuRNUI;
            if (WeaponSubMenu != null)
            {
                ToCheckFirst = WeaponSubMenu;
            }
            UIMenu CategoryMenu = ToCheckFirst.Children.Where(x => x.Value.SubtitleText == MenuCategory).FirstOrDefault().Value;
            if (CategoryMenu != null)
            {
                FoundCategoryMenu = true;
                WeaponMenu = Transaction.MenuPool.AddSubMenu(CategoryMenu, menuItem.ModItemName);
                CategoryMenu.MenuItems[CategoryMenu.MenuItems.Count() - 1].Description = description;
                CategoryMenu.MenuItems[CategoryMenu.MenuItems.Count() - 1].RightLabel = formattedPurchasePrice;
                CategoryMenu.MenuItems[CategoryMenu.MenuItems.Count() - 1].Enabled = hasPedGotWeapon;
                EntryPoint.WriteToConsole($"Added Weapon {Name} To SubMenu {CategoryMenu.SubtitleText}", 5);
            }
            //foreach (UIMenu uimen in Transaction.MenuPool.ToList())
            //{
            //    if (uimen.SubtitleText == WeaponInformation.Category.ToString() && uimen.ParentMenu == sellMenuRNUI)
            //    {
            //        FoundCategoryMenu = true;
            //        WeaponMenu = Transaction.MenuPool.AddSubMenu(uimen, menuItem.ModItemName);
            //        uimen.MenuItems[uimen.MenuItems.Count() - 1].Description = description;
            //        uimen.MenuItems[uimen.MenuItems.Count() - 1].RightLabel = formattedPurchasePrice;
            //        uimen.MenuItems[uimen.MenuItems.Count() - 1].Enabled = hasPedGotWeapon;
            //        EntryPoint.WriteToConsole($"Added Weapon {Name} To SubMenu {uimen.SubtitleText}", 5);
            //        break;
            //    }
            //}

        }
        if (!FoundCategoryMenu && WeaponMenu == null)
        {
            WeaponMenu = Transaction.MenuPool.AddSubMenu(sellMenuRNUI, menuItem.ModItemName);
            sellMenuRNUI.MenuItems[sellMenuRNUI.MenuItems.Count() - 1].Description = description;
            sellMenuRNUI.MenuItems[sellMenuRNUI.MenuItems.Count() - 1].RightLabel = formattedPurchasePrice;
            sellMenuRNUI.MenuItems[sellMenuRNUI.MenuItems.Count() - 1].Enabled = hasPedGotWeapon;
            EntryPoint.WriteToConsole($"Added Weapon {Name} To Main Buy Menu", 5);
        }

        if (Transaction.HasBannerImage)
        {
            WeaponMenu.SetBannerType(Transaction.BannerImage);
        }
        else if (Transaction.RemoveBanner)
        {
            WeaponMenu.RemoveBanner();
        }

        WeaponMenu.OnMenuOpen += (sender) =>
        {
            OnWeaponSellMenuOpen(sender, player);
        };

        UIMenuNumericScrollerItem<int> PurchaseAmmo = new UIMenuNumericScrollerItem<int>($"Sell Ammo", $"Select to sell ammo for this weapon.", menuItem.SubAmount, 500, menuItem.SubAmount) { Index = 0, Formatter = v => $"{v} - ${menuItem.SubPrice * v}" };
        UIMenuItem Purchase = new UIMenuItem($"Sell", "Select to sell this Weapon") { RightLabel = formattedPurchasePrice };
        if (hasPedGotWeapon)
        {
            Purchase.Enabled = true;
        }
        else
        {
            Purchase.Enabled = false;
        }
        if (WeaponInformation.Category != WeaponCategory.Melee && WeaponInformation.Category != WeaponCategory.Throwable && 1==0)
        {
            WeaponMenu.AddItem(PurchaseAmmo);
        }
        Purchase.Activated += (sender, selectedItem) =>
        {
            int TotalPrice = menuItem.SalesPrice;
            if (!SellWeapon(player,Transaction,menuItem))
            {
                return;
            }
            player.BankAccounts.GiveMoney(menuItem.SalesPrice);
            Transaction.MoneySpent += menuItem.SalesPrice;
            OnWeaponSellMenuOpen(sender, player);
        };
        WeaponMenu.AddItem(Purchase);
    }
    private void OnWeaponSellMenuOpen(UIMenu sender, ILocationInteractable player)
    {
        EntryPoint.WriteToConsole($"OnWeaponMenuOpen RAN!", 5);
        foreach (UIMenuItem uimen in sender.MenuItems)
        {
            if (uimen.GetType() == typeof(UIMenuListScrollerItem<MenuItemExtra>))
            {
                UIMenuListScrollerItem<MenuItemExtra> myItem = (UIMenuListScrollerItem<MenuItemExtra>)(object)uimen;
                foreach (MenuItemExtra stuff in myItem.Items)
                {
                    WeaponComponent myComponent = WeaponInformation.PossibleComponents.Where(x => x.Name == stuff.ExtraName).FirstOrDefault();
                    if (myComponent != null)
                    {
                        if (WeaponInformation.HasComponent(player.Character, myComponent))
                        {
                            myItem.SelectedItem = stuff;
                            stuff.HasItem = true;
                            EntryPoint.WriteToConsole($"OnWeaponMenuOpen RAN! {myComponent.Name} HAS COMPONENT {stuff.HasItem} {myItem.OptionText}", 5);
                            break;
                        }
                        else
                        {
                            IsProgramicallyChangingItems = true;
                            myItem.SelectedItem = myItem.Items[0];
                            IsProgramicallyChangingItems = false;
                            stuff.HasItem = false;
                            EntryPoint.WriteToConsole($"OnWeaponMenuOpen RAN! {myComponent.Name} DOES NOT HAVE COMPONENT  {stuff.HasItem} {myItem.OptionText}", 5);
                        }
                        // myItem.Formatter = v => v.HasItem ? $"{v.ExtraName} - Equipped" : v.PurchasePrice == 0 ? v.ExtraName : $"{v.ExtraName} - ${v.PurchasePrice}";
                    }
                }
                myItem.Reformat();
            }
            else if (uimen.Text == "Sell")
            {
                if (WeaponInformation.HasWeapon(player.Character) && WeaponInformation.Category != WeaponCategory.Throwable)
                {
                    uimen.Enabled = true;
                }
                else
                {
                    uimen.Enabled = false;
                    // uimen.RightLabel = "Owned";
                }
            }
            else if (uimen.Text == "Sell Ammo")
            {
                if (WeaponInformation.HasWeapon(player.Character))
                {
                    uimen.Enabled = true;
                }
                else
                {
                    uimen.Enabled = false;
                }
            }
            EntryPoint.WriteToConsole($"Full Below Level: {uimen.Text}", 5);
        }
    }
    private bool SellWeapon(ILocationInteractable player, Transaction transaction, MenuItem menuItem)
    {
        if (WeaponInformation != null)
        {
            if (NativeFunction.Natives.HAS_PED_GOT_WEAPON<bool>(player.Character, WeaponInformation.Hash, false))
            {
                //menuItem.ItemsBoughtFromPlayer++;
                NativeFunction.Natives.REMOVE_WEAPON_FROM_PED(player.Character, WeaponInformation.Hash);   
                player.WeaponEquipment.SetUnarmed();
                transaction.OnItemSold(this, menuItem, 1);
                return true;
            }
        }
        transaction.PlayErrorSound();
        transaction.DisplayMessage("~r~Sale Failed", "We are sorry, we are unable to complete this transation");
        return false;
    }


    public override void CreatePurchaseMenuItem(Transaction Transaction, MenuItem menuItem, UIMenu purchaseMenu, ISettingsProvideable settings, ILocationInteractable player, bool isStealing, IEntityProvideable world)
    {
        CurrentWeaponVariation = new WeaponVariation();
        EntryPoint.WriteToConsole($"Purchase Menu Add Weapon Entry ItemName: {Name}", 5);
        string description;
        if (Description.Length >= 200)
        {
            description = Description.Substring(0, 200) + "...";//menu cant show more than 225?, need some for below
        }
        else
        {
            description = Description;
        }
        description += "~n~~s~";
        if (RequiresDLC)
        {
            description += $"~n~~b~DLC Weapon";
        }
        string formattedPurchasePrice = menuItem.PurchasePrice.ToString("C0");

        if (menuItem.PurchasePrice == 0)
        {
            formattedPurchasePrice = "FREE";
        }
        if (ModelItem != null && ModelItem.ModelHash != 0)
        {
            NativeFunction.Natives.REQUEST_WEAPON_ASSET(ModelItem.ModelHash, 31, 0);
        }
        UIMenu WeaponMenu = null;
        bool FoundCategoryMenu = false;
        if (WeaponInformation != null)
        {
            UIMenu WeaponSubMenu = purchaseMenu.Children.Where(x => x.Value.SubtitleText.ToLower() == "weapons").FirstOrDefault().Value;
            UIMenu ToCheckFirst = purchaseMenu;
            if(WeaponSubMenu != null)
            {
                ToCheckFirst = WeaponSubMenu;
            }
            UIMenu CategoryMenu = ToCheckFirst.Children.Where(x => x.Value.SubtitleText == MenuCategory).FirstOrDefault().Value;
            if (CategoryMenu != null)
            {
                FoundCategoryMenu = true;
                WeaponMenu = Transaction.MenuPool.AddSubMenu(CategoryMenu, menuItem.ModItemName);
                CategoryMenu.MenuItems[CategoryMenu.MenuItems.Count() - 1].Description = description;
                CategoryMenu.MenuItems[CategoryMenu.MenuItems.Count() - 1].RightLabel = formattedPurchasePrice;
                EntryPoint.WriteToConsole($"Added Weapon {Name} To SubMenu {CategoryMenu.SubtitleText}", 5);
            }
            //foreach (UIMenu uimen in Transaction.MenuPool.ToList())
            //{
            //    if (uimen.SubtitleText == WeaponInformation.Category.ToString() && uimen.ParentMenu == purchaseMenu)
            //    {
            //        FoundCategoryMenu = true;
            //        WeaponMenu = Transaction.MenuPool.AddSubMenu(uimen, menuItem.ModItemName);
            //        uimen.MenuItems[uimen.MenuItems.Count() - 1].Description = description;
            //        uimen.MenuItems[uimen.MenuItems.Count() - 1].RightLabel = formattedPurchasePrice;
            //        EntryPoint.WriteToConsole($"Added Weapon {Name} To SubMenu {uimen.SubtitleText}", 5);
            //        break;
            //    }
            //}
        }
        if (!FoundCategoryMenu && WeaponMenu == null)
        {
            WeaponMenu = Transaction.MenuPool.AddSubMenu(purchaseMenu, menuItem.ModItemName);
            purchaseMenu.MenuItems[purchaseMenu.MenuItems.Count() - 1].Description = description;
            purchaseMenu.MenuItems[purchaseMenu.MenuItems.Count() - 1].RightLabel = formattedPurchasePrice;
            EntryPoint.WriteToConsole($"Added Weapon {Name} To Main Buy Menu", 5);
        }
        if (Transaction.HasBannerImage)
        {
            WeaponMenu.SetBannerType(Transaction.BannerImage);
        }
        else if (Transaction.RemoveBanner)
        {
            WeaponMenu.RemoveBanner();
        }
        WeaponMenu.OnMenuOpen += (sender) =>
        {
            OnWeaponMenuOpen(sender, player);
        };
        if (WeaponInformation != null)
        {
            foreach (ComponentSlot majorCOmponentSlot in WeaponInformation.PossibleComponents.Where(x => x.ComponentSlot != ComponentSlot.Coloring).GroupBy(x => x.ComponentSlot).Select(x => x.Key))
            {
                List<MenuItemExtra> stuffList = new List<MenuItemExtra>() { new MenuItemExtra("Default", 0) };
                bool AddedAny = false;
                foreach (WeaponComponent mywc in WeaponInformation.PossibleComponents.Where(x => x.ComponentSlot == majorCOmponentSlot))
                {
                    MenuItemExtra menuItemExtra = menuItem.Extras.FirstOrDefault(x => x.ExtraName == mywc.Name);
                    if (menuItemExtra != null)
                    {
                        AddedAny = true;
                        stuffList.Add(menuItemExtra);
                    }
                }
                if (AddedAny)
                {
                    UIMenuListScrollerItem<MenuItemExtra> componentScroller = new UIMenuListScrollerItem<MenuItemExtra>(majorCOmponentSlot.ToString(), majorCOmponentSlot.ToString(), stuffList) { Index = 0 };
                    componentScroller.IndexChanged += (sender,oldIndex,newIndex) => 
                    {
                        OnWeaponScrollerChange(WeaponMenu, componentScroller, oldIndex, newIndex, Transaction,player, menuItem);
                    };
                    componentScroller.Activated += (sender, selectedItem) =>
                    {
                        UIMenuListScrollerItem<MenuItemExtra> myItem = (UIMenuListScrollerItem<MenuItemExtra>)selectedItem;
                        bool isComponentSlot = false;
                        ComponentSlot selectedSlot;
                        foreach (ComponentSlot cs in Enum.GetValues(typeof(ComponentSlot)).Cast<ComponentSlot>().ToList())
                        {
                            if (cs.ToString() == selectedItem.Text)
                            {
                                selectedSlot = cs;
                                isComponentSlot = true;
                                if (myItem.SelectedItem.ExtraName == "Default")
                                {
                                    WeaponInformation.SetSlotDefault(player.Character, selectedSlot);
                                    Transaction.PlaySuccessSound();
                                    Transaction.DisplayMessage("Set Default", $"Set the {selectedSlot} slot to default");
                                    OnWeaponMenuOpen(sender,player);
                                    return;
                                }
                                break;
                            }
                        }
                        WeaponComponent myComponent = WeaponInformation.PossibleComponents.Where(x => x.Name == myItem.SelectedItem.ExtraName).FirstOrDefault();
                        if (myComponent != null && menuItem != null)
                        {
                            EntryPoint.WriteToConsole($"Weapon Component Purchase {menuItem.ModItemName} Player.Money {player.BankAccounts.Money} menuItem.PurchasePrice {menuItem.PurchasePrice} myComponent {myComponent.Name}", 5);
                            if (player.BankAccounts.Money < myItem.SelectedItem.PurchasePrice)
                            {
                                Transaction.DisplayInsufficientFundsMessage();
                                return;
                            }
                            if (WeaponInformation.HasComponent(player.Character, myComponent))
                            {
                                Transaction.PlayErrorSound();
                                Transaction.DisplayMessage("Already Owned", "We are sorry, we are unable to complete this transation, as the item is already owned");
                                return;
                            }
                            if (!PurchaseComponent(player, Transaction, menuItem, myComponent))
                            {
                                return;
                            }
                            player.BankAccounts.GiveMoney(-1 * myItem.SelectedItem.PurchasePrice);
                            Transaction.MoneySpent += myItem.SelectedItem.PurchasePrice;
                            OnWeaponMenuOpen(sender, player);
                        }
                    };
                    WeaponMenu.AddItem(componentScroller);
                }
            }
        }



        UIMenuNumericScrollerItem<int> PurchaseAmmoMenu = new UIMenuNumericScrollerItem<int>($"Purchase Ammo", $"Select to purchase ammo for this weapon.", menuItem.SubAmount, 500, menuItem.SubAmount) { Index = 0, Formatter = v => $"{v} - ${menuItem.SubPrice * v}" };
        PurchaseAmmoMenu.Activated += (sender, selectedItem) =>
        {
            int TotalItems = 1;
            if (selectedItem.GetType() == typeof(UIMenuNumericScrollerItem<int>))
            {
                UIMenuNumericScrollerItem<int> myItem = (UIMenuNumericScrollerItem<int>)selectedItem;
                TotalItems = myItem.Value;
            }
            if (menuItem != null)
            {
                int TotalPrice = menuItem.SubPrice * TotalItems;
                EntryPoint.WriteToConsole($"Weapon Purchase {menuItem.ModItemName} Player.Money {player.BankAccounts.Money} menuItem.PurchasePrice {1}", 5);
                if (player.BankAccounts.Money < TotalPrice)
                {
                    Transaction.DisplayInsufficientFundsMessage();
                    return;
                }
                if (!PurchaseAmmo(player,Transaction,menuItem,TotalItems))
                {
                    return;
                }
                player.BankAccounts.GiveMoney(-1 * TotalPrice);
                Transaction.MoneySpent += TotalPrice;
                OnWeaponMenuOpen(sender,player);
            }
        };
        if (WeaponInformation.Category != WeaponCategory.Melee && WeaponInformation.Category != WeaponCategory.Throwable)
        {
            WeaponMenu.AddItem(PurchaseAmmoMenu);
        }



        UIMenuItem Purchase = new UIMenuItem($"Purchase", "Select to purchase this Weapon") { RightLabel = formattedPurchasePrice };
        if (NativeFunction.Natives.HAS_PED_GOT_WEAPON<bool>(player.Character, WeaponInformation.Hash, false))
        {
            Purchase.Enabled = false;
        }
        Purchase.Activated += (sender, selectedItem) =>
        {
            int TotalPrice = menuItem.PurchasePrice;
            foreach (WeaponComponent wc in CurrentWeaponVariation?.Components)
            {
                MenuItemExtra mie = menuItem.Extras.FirstOrDefault(x => x.ExtraName == wc.Name);
                if (mie != null)
                {
                    TotalPrice += mie.PurchasePrice;
                }
            }
            EntryPoint.WriteToConsole($"Weapon Purchase {menuItem.ModItemName} Player.Money {player.BankAccounts.Money} menuItem.PurchasePrice {menuItem.PurchasePrice}", 5);
            if (player.BankAccounts.Money < TotalPrice)
            {
                Transaction.DisplayInsufficientFundsMessage();
                return;
            }
            if (!PurchaseWeapon(player,Transaction,menuItem))
            {
                return;
            }
            player.BankAccounts.GiveMoney(-1 * TotalPrice);
            Transaction.MoneySpent += TotalPrice;
            OnWeaponMenuOpen(sender, player);
        };
        WeaponMenu.AddItem(Purchase);
    }
    private void OnWeaponMenuOpen(UIMenu sender, ILocationInteractable player)
    {
        
        EntryPoint.WriteToConsole($"OnWeaponMenuOpen RAN! START", 5);
        foreach (UIMenuItem uimen in sender.MenuItems)
        {
            if (uimen.GetType() == typeof(UIMenuListScrollerItem<MenuItemExtra>))
            {
                UIMenuListScrollerItem<MenuItemExtra> myItem = (UIMenuListScrollerItem<MenuItemExtra>)(object)uimen;
                foreach (MenuItemExtra stuff in myItem.Items)
                {
                    WeaponComponent myComponent = WeaponInformation.PossibleComponents.Where(x => x.Name == stuff.ExtraName).FirstOrDefault();
                    if (myComponent != null)
                    {
                        if (WeaponInformation.HasComponent(player.Character, myComponent))
                        {
                            myItem.SelectedItem = stuff;
                            stuff.HasItem = true;
                            EntryPoint.WriteToConsole($"OnWeaponMenuOpen RAN! {myComponent.Name} HAS COMPONENT {stuff.HasItem} {myItem.OptionText}", 5);
                            break;
                        }
                        else
                        {
                            IsProgramicallyChangingItems = true;
                            myItem.SelectedItem = myItem.Items[0];
                            IsProgramicallyChangingItems = false;
                            //myItem.SelectedItem = stuff;
                            stuff.HasItem = false;
                            EntryPoint.WriteToConsole($"OnWeaponMenuOpen RAN! {myComponent.Name} DOES NOT HAVE COMPONENT  {stuff.HasItem} {myItem.OptionText}", 5);
                        }
                        // myItem.Formatter = v => v.HasItem ? $"{v.ExtraName} - Equipped" : v.PurchasePrice == 0 ? v.ExtraName : $"{v.ExtraName} - ${v.PurchasePrice}";
                    }
                }
                myItem.Reformat();
            }
            else if (uimen.Text == "Purchase")
            {
                if (WeaponInformation.HasWeapon(player.Character) && WeaponInformation.Category != WeaponCategory.Throwable)
                {
                    uimen.Enabled = false;
                    uimen.RightLabel = "Owned";
                }
                else
                {
                    uimen.Enabled = true;
                    // uimen.RightLabel = "Owned";
                }
            }
            else if (uimen.Text == "Purchase Ammo")
            {
                if (WeaponInformation.HasWeapon(player.Character))
                {
                    uimen.Enabled = true;
                }
                else
                {
                    uimen.Enabled = false;
                }
            }
            EntryPoint.WriteToConsole($"Full Below Level: {uimen.Text}", 5);
        }
    }
    private bool PurchaseAmmo(ILocationInteractable player, Transaction transaction, MenuItem menuItem, int TotalItems)
    {
        if (WeaponInformation != null && WeaponInformation.Category != WeaponCategory.Melee && NativeFunction.Natives.HAS_PED_GOT_WEAPON<bool>(player.Character, WeaponInformation.Hash, false))
        {
            NativeFunction.Natives.ADD_AMMO_TO_PED(player.Character, WeaponInformation.Hash, TotalItems);
            transaction.PlaySuccessSound();
            transaction.DisplayMessage("~g~Purchase", $"Thank you for your purchase of ~r~{TotalItems} ~s~rounds for ~o~{menuItem.ModItemName}~s~");
            return true;
        }
        transaction.PlayErrorSound();
        transaction.DisplayMessage("~r~Purchase Failed", "We are sorry, we are unable to complete this transation");
        return false;
    }
    private bool PurchaseComponent(ILocationInteractable player, Transaction transaction, MenuItem menuItem, WeaponComponent myComponent)
    {
        if (WeaponInformation != null && WeaponInformation.AddComponent(player.Character, myComponent))
        {
            transaction.PlaySuccessSound();
            transaction.DisplayMessage("~g~Purchase", $"Thank you for your purchase of ~r~{myComponent.Name}~s~ for ~o~{menuItem.ModItemName}~s~");
            return true;
        }
        transaction.PlayErrorSound();
        transaction.DisplayMessage("~r~Purchase Failed", "We are sorry, we are unable to complete this transation");
        return false;
    }   
    private bool PurchaseWeapon(ILocationInteractable player, Transaction transaction, MenuItem menuItem)
    {
        if (WeaponInformation != null)
        {
            if (WeaponInformation.Category == WeaponCategory.Throwable || !NativeFunction.Natives.HAS_PED_GOT_WEAPON<bool>(player.Character, WeaponInformation.Hash, false))
            {
                //menuItem.ItemsSoldToPlayer++;
                NativeFunction.Natives.GIVE_WEAPON_TO_PED(player.Character, WeaponInformation.Hash, WeaponInformation.AmmoAmount, false, false);
                if (CurrentWeaponVariation != null)
                {
                    WeaponInformation.ApplyWeaponVariation(player.Character, CurrentWeaponVariation);
                }
                player.WeaponEquipment.SetUnarmed();
                transaction.OnItemPurchased(this, menuItem, 1);
                return true;
            }
        }
        return false;
    }
    private void OnWeaponScrollerChange(UIMenu sender, UIMenuScrollerItem item, int oldIndex, int newIndex, Transaction transaction, ILocationInteractable player, MenuItem CurrentMenuItem)
    {
        try
        {
            if (WeaponInformation != null && !IsProgramicallyChangingItems)
            {
                //EntryPoint.WriteToConsole($"OnWeaponScrollerChange Weapon Start {CurrentModItem.ModelItem.ModelName} {CurrentWeapon.ModelName}", 5);
                string ExtraName = item.OptionText;
                int ExtraPrice = 0;
                if (item.GetType() == typeof(UIMenuListScrollerItem<MenuItemExtra>))
                {
                    UIMenuListScrollerItem<MenuItemExtra> test = (UIMenuListScrollerItem<MenuItemExtra>)item;
                    ExtraName = test.SelectedItem.ExtraName;
                    ExtraPrice = test.SelectedItem.PurchasePrice;
                }
                WeaponComponent myComponent = WeaponInformation.PossibleComponents.Where(x => x.Name == ExtraName).FirstOrDefault();
                if (myComponent != null)
                {
                    uint ComponentHash = NativeFunction.Natives.GET_WEAPON_COMPONENT_TYPE_MODEL<uint>(myComponent.Hash);
                    NativeFunction.Natives.REQUEST_MODEL(ComponentHash);
                    uint GameTimeRequested = Game.GameTime;
                    while (!NativeFunction.Natives.HAS_MODEL_LOADED<bool>(ComponentHash))
                    {
                        if (Game.GameTime - GameTimeRequested >= 500)
                        {
                            break;
                        }
                        GameFiber.Yield();
                    }
                    foreach (WeaponComponent wc in WeaponInformation.PossibleComponents.Where(x => x.ComponentSlot == myComponent.ComponentSlot))
                    {
                        if (NativeFunction.Natives.HAS_WEAPON_GOT_WEAPON_COMPONENT<bool>(transaction.SellingProp, wc.Hash))
                        {
                            NativeFunction.Natives.REMOVE_WEAPON_COMPONENT_FROM_WEAPON_OBJECT(transaction.SellingProp, wc.Hash);
                        }
                        if (CurrentWeaponVariation.Components.Contains(wc))
                        {
                            CurrentWeaponVariation.Components.Remove(wc);
                        }
                    }
                    if (item.Text != "Default")
                    {
                        if (NativeFunction.Natives.DOES_WEAPON_TAKE_WEAPON_COMPONENT<bool>(WeaponInformation.Hash, myComponent.Hash))
                        {
                            if (!NativeFunction.Natives.HAS_WEAPON_GOT_WEAPON_COMPONENT<bool>(transaction.SellingProp, myComponent.Hash))
                            {
                                NativeFunction.Natives.GIVE_WEAPON_COMPONENT_TO_WEAPON_OBJECT(transaction.SellingProp, myComponent.Hash);
                            }
                            if (!CurrentWeaponVariation.Components.Contains(myComponent))
                            {
                                CurrentWeaponVariation.Components.Add(myComponent);
                            }
                        }
                    }
                }
                else
                {
                    foreach (WeaponComponent wc in WeaponInformation.PossibleComponents.Where(x => x.ComponentSlot.ToString() == item.Text))
                    {
                        if (NativeFunction.Natives.HAS_WEAPON_GOT_WEAPON_COMPONENT<bool>(transaction.SellingProp, wc.Hash))
                        {
                            NativeFunction.Natives.REMOVE_WEAPON_COMPONENT_FROM_WEAPON_OBJECT(transaction.SellingProp, wc.Hash);
                        }
                        if (CurrentWeaponVariation.Components.Contains(wc))
                        {
                            CurrentWeaponVariation.Components.Remove(wc);
                        }
                    }
                }
            }
            if (WeaponInformation != null)
            {
                if (!WeaponInformation.HasWeapon(player.Character))
                {
                    int TotalPrice = CurrentMenuItem.PurchasePrice;
                    EntryPoint.WriteToConsole($"Current Weapon: {WeaponInformation.ModelName}", 5);
                    if (CurrentWeaponVariation != null)
                    {
                        foreach (WeaponComponent weaponComponents in CurrentWeaponVariation.Components)
                        {
                            MenuItemExtra mie = CurrentMenuItem.Extras.FirstOrDefault(x => x.ExtraName == weaponComponents.Name);
                            if (mie != null && !WeaponInformation.HasComponent(player.Character, weaponComponents))
                            {
                                TotalPrice += mie.PurchasePrice;
                            }
                            EntryPoint.WriteToConsole($"                Components On: {weaponComponents.Name}", 5);
                        }
                    }
                    foreach (UIMenuItem uimli in sender.MenuItems)
                    {
                        if (uimli.Text == "Purchase")
                        {
                            uimli.RightLabel = TotalPrice.ToString("C0");
                            break;
                        }
                    }
                    EntryPoint.WriteToConsole($"Current Weapon: {WeaponInformation.ModelName} Total Price {TotalPrice}", 5);
                }
            }
        }
        catch (Exception ex)
        {
            EntryPoint.WriteToConsole($"Weapon Preview Error {ex.Message} {ex.StackTrace}", 0);
        }
    }
    public override void CreatePreview(Transaction Transaction, Camera StoreCam, bool isPurchase, IEntityProvideable world, ISettingsProvideable settings)
    {
        try
        {
            if (ModelItem != null && ModelItem.ModelName != "")
            {
                Transaction.RotatePreview = true;
                EntryPoint.WriteToConsole($"WEAPON ITEM CREATE PREVIEW {ModelItem.ModelName} {ModelItem.ModelHash}");
                Vector3 Position = Vector3.Zero;
                if (StoreCam.Exists())
                {
                    Position = StoreCam.Position + StoreCam.Direction / 2f;
                }
                else
                {
                    Vector3 GPCamPos = NativeFunction.Natives.GET_GAMEPLAY_CAM_COORD<Vector3>();
                    Vector3 GPCamDir = NativeHelper.GetGameplayCameraDirection();
                    Position = GPCamPos + GPCamDir / 2f;
                }

               
                if (NativeFunction.Natives.HAS_WEAPON_ASSET_LOADED<bool>(ModelItem.ModelHash))
                {
                    EntryPoint.WriteToConsole($"WEAPON ITEM ASSET LOADED {ModelItem.ModelName} {ModelItem.ModelHash}");
                    Transaction.SellingProp = NativeFunction.Natives.CREATE_WEAPON_OBJECT<Rage.Object>(ModelItem.ModelHash, 60, Position.X, Position.Y, Position.Z, true, 1.0f, 0, 0, 1);
                }
                if (Transaction.SellingProp.Exists())
                {
                    float length = Transaction.SellingProp.Model.Dimensions.X;
                    float width = Transaction.SellingProp.Model.Dimensions.Y;
                    float height = Transaction.SellingProp.Model.Dimensions.Z;

                    float LargestSideLength = length;
                    if (width > LargestSideLength)
                    {
                        LargestSideLength = width;
                    }
                    if (height > LargestSideLength)
                    {
                        LargestSideLength = height;
                    }

                    if (StoreCam.Exists())
                    {
                        Position = StoreCam.Position + (StoreCam.Direction.ToNormalized() * 0.5f) + (StoreCam.Direction.ToNormalized() * LargestSideLength / 2f);//
                    }
                    else
                    {
                        Vector3 GPCamPos = NativeFunction.Natives.GET_GAMEPLAY_CAM_COORD<Vector3>();
                        Vector3 GPCamDir = NativeHelper.GetGameplayCameraDirection();
                        Position = GPCamPos + (GPCamDir.ToNormalized() * 0.5f) + (GPCamDir.ToNormalized() * LargestSideLength / 2f);
                    }
                    Transaction.SellingProp.Position = Position;
                    Transaction.SellingProp.SetRotationYaw(Transaction.SellingProp.Rotation.Yaw + 45f);
                    if (Transaction.SellingProp != null && Transaction.SellingProp.Exists())
                    {
                        NativeFunction.Natives.SET_ENTITY_HAS_GRAVITY(Transaction.SellingProp, false);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Game.DisplayNotification($"Error Displaying Model {ex.Message} {ex.StackTrace}");
        }
    }
}

