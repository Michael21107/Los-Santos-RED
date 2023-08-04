﻿using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage.Native;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using Mod;
using System.Runtime;
using static DispatchScannerFiles;
using LosSantosRED.lsr.Player;
using System.ComponentModel;

[Serializable()]
[XmlInclude(typeof(ClothingItem))]
[XmlInclude(typeof(BinocularsItem))]
[XmlInclude(typeof(BongItem))]
[XmlInclude(typeof(ConsumableItem))]
[XmlInclude(typeof(DrillItem))]
[XmlInclude(typeof(DrinkItem))]
[XmlInclude(typeof(FlashlightItem))]
[XmlInclude(typeof(FoodItem))]
[XmlInclude(typeof(HammerItem))]
[XmlInclude(typeof(HotelStayItem))]
[XmlInclude(typeof(ValuableItem))]
[XmlInclude(typeof(IngestItem))]
[XmlInclude(typeof(InhaleItem))]
[XmlInclude(typeof(InjectItem))]
[XmlInclude(typeof(LicensePlateItem))]
[XmlInclude(typeof(PipeSmokeItem))]
[XmlInclude(typeof(PliersItem))]
[XmlInclude(typeof(ScrewdriverItem))]
[XmlInclude(typeof(ShovelItem))]
[XmlInclude(typeof(SmokeItem))]
[XmlInclude(typeof(TapeItem))]
[XmlInclude(typeof(UmbrellaItem))]
[XmlInclude(typeof(VehicleItem))]
[XmlInclude(typeof(WeaponItem))]
[XmlInclude(typeof(EquipmentItem))]
[XmlInclude(typeof(BodyArmorItem))]
public class ModItem
{
    private UIMenuNumericScrollerItem<int> sellScroller;
    private UIMenuNumericScrollerItem<int> purchaseScroller;
    private UIMenuNumericScrollerItem<int> takeScroller;
    private UIMenu inventoryItemSubMenu;
    private UIMenuItem inventoryItemSubMenuItem;
    private UIMenuNumericScrollerItem<int> giveScroller;
   // private Func<int, string> Formatter = v => $"{(v == 1 && MeasurementName == "Item" ? "" : v.ToString() + " ")}{(MeasurementName != "Item" || v > 1 ? MeasurementName : "")}{(v > 1 ? "(s)" : "")}{(MeasurementName != "Item" || v > 1 ? " - " : "")}${(v * menuItem.SalesPrice)}"

    public ModItem()
    {

    }
    public ModItem(string name, ItemType itemType)
    {
        Name = name;
        ItemType = itemType;
    }
    public ModItem(string name, string description, ItemType itemType)
    {
        Name = name;
        Description = description;
        ItemType = itemType;
    }


    [XmlIgnore]
    public string MenuCategory { get; set; }
    [XmlIgnore]
    public PhysicalItem ModelItem { get; set; }
    [XmlIgnore]
    public PhysicalItem PackageItem { get; set; }
    public string Name { get; set; }
    public string Description { get; set; } = "";
    public ItemType ItemType { get; set; } = ItemType.None;
    public ItemSubType ItemSubType { get; set; } = ItemSubType.None;
    public string MeasurementName { get; set; } = "Item";
    public int AmountPerPackage { get; set; } = 1;
    public string ModelItemID { get; set; }
    public string PackageItemID { get; set; }
    public bool IsPossessionIllicit { get; set; } = false;
    public float PercentLostOnUse { get; set; } = 0.0f;
    public bool ConsumeOnPurchase { get; set; } = false;
    public virtual bool CanConsume { get; set; } = false;//no no
    public int FindPercentage { get; set; } = 0;
    public int PoliceFindDuringPlayerSearchPercentage { get; set; } = 85;
    public virtual bool IsDLC => false;
    public virtual string DisplayName => Name;
    public virtual string DisplayDescription => Description;
    public virtual void Setup(PhysicalItems physicalItems, IWeapons weapons)
    {
        if (ModelItemID != "")
        {
            ModelItem = physicalItems.Get(ModelItemID);
        }
        if (PackageItemID != "")
        {
            PackageItem = physicalItems.Get(PackageItemID);
        }
        MenuCategory = ItemType.ToString();
    }
    public virtual string FullDescription(ISettingsProvideable Settings)
    {
        return $"{Description}~n~" 
            + GetTypeDescription(Settings)
            + GetExtendedDescription(Settings)
            + (MeasurementName != "Item" ? " " + MeasurementName + "(s)" : "");      
    }
    public virtual bool UseItem(IActionable actionable, ISettingsProvideable settings, IEntityProvideable world, ICameraControllable cameraControllable, IIntoxicants intoxicants, ITimeControllable time)
    {
        return false;
    }
    public virtual bool ConsumeItem(IActionable actionable, bool applyNeeds)
    {
        return false;
    }
    public virtual string GetTypeDescription(ISettingsProvideable settings)
    {
        return $"~n~Type: ~p~{ItemType}~s~" + (ItemSubType != ItemSubType.None ? $" - ~p~{ItemSubType}~s~" : "");
    }
    public virtual string GetExtendedDescription(ISettingsProvideable settings)
    {
        return "";
    }
    public virtual string PurchaseMenuDescription(ISettingsProvideable settings)
    {
        return "";
    }
    public virtual string SellMenuDescription(ISettingsProvideable settings)
    {
        return "";
    }
    public virtual void AddNewItem(IModItems modItems)
    {

    }
    public virtual void CreateSellMenuItem(Transaction Transaction, MenuItem menuItem, UIMenu sellMenu, ISettingsProvideable settings, ILocationInteractable player, bool isStealing, IEntityProvideable world)
    {
        sellScroller = new UIMenuNumericScrollerItem<int>(menuItem.ModItemName, "", 1, 1, 1) { Formatter = v => $"{(v == 1 && MeasurementName == "Item" ? "" : v.ToString() + " ")}{(MeasurementName != "Item" || v > 1 ? MeasurementName : "")}{(v > 1 ? "(s)" : "")}{(MeasurementName != "Item" || v > 1 ? " - " : "")}${(v * menuItem.SalesPrice)}", Value = 1 };
        UpdateSellMenuItem(Transaction, menuItem, settings, player, isStealing);
        sellScroller.Activated += (sender, selectedItem) =>
        {
            SellItem(Transaction, player, menuItem, sellScroller.Value, isStealing);
            UpdateSellMenuItem(Transaction, menuItem, settings, player, isStealing);
        };
        UIMenu CategoryMenu = sellMenu.Children.Where(x => x.Value.SubtitleText == MenuCategory).FirstOrDefault().Value;
        if (CategoryMenu != null)
        {
            CategoryMenu.AddItem(sellScroller);

        }
        else
        {
            sellMenu.AddItem(sellScroller);
        }
    }
    public void UpdateSellMenuItem(Transaction Transaction, MenuItem menuItem, ISettingsProvideable settings, ILocationInteractable player, bool isStealing)
    {
        bool isEnabled = true;
        InventoryItem PlayerInventoryItem = player.Inventory.ItemsList.Where(x => x.ModItem.Name == menuItem.ModItemName).FirstOrDefault();
        int MaxSell = 1;
        int PlayerItems = 0;
        if (PlayerInventoryItem != null)
        {
            PlayerItems = PlayerInventoryItem.Amount;
            MaxSell = PlayerInventoryItem.Amount;
        }
        int RemainingToSell = MaxSell;
        int itemsBoughtFromPlayer = 0;
        if (menuItem.NumberOfItemsToPurchaseFromPlayer != -1)
        {
            DesiredItem di = Transaction.PersonTransaction?.TransactionPed?.PedDesires.Get(this);
            if(di != null)
            {
                itemsBoughtFromPlayer = di.ItemsBoughtFromPlayer;
            }
            RemainingToSell = menuItem.NumberOfItemsToPurchaseFromPlayer - itemsBoughtFromPlayer;
            if(RemainingToSell < 0)
            {
                RemainingToSell = 0;
            }
            if (RemainingToSell >= 1 && PlayerItems >= 1)
            {
                MaxSell = Math.Min(MaxSell, RemainingToSell);
            }
            else
            {
                MaxSell = 1;
                isEnabled = false;
            }
        }
        else
        {
            if (PlayerItems <= 0)
            {
                MaxSell = 1;
                isEnabled = false;
            }
        }
        string formattedPurchasePrice = menuItem.SalesPrice.ToString("C0");
        string description = Description;
        description += "~n~~s~";
        description += $"~n~Price: ~g~{formattedPurchasePrice}~s~";
        description += $"~n~Type: ~p~{ItemType}~s~" + (ItemSubType != ItemSubType.None ? $" - ~p~{ItemSubType}~s~" : "");
        description += $"~n~~b~{AmountPerPackage}~s~ Item(s) per Package";
        if (AmountPerPackage > 1)
        {
            description += $"~n~~b~{((float)menuItem.SalesPrice / (float)AmountPerPackage).ToString("C2")} ~s~per Item";
        }
        description += SellMenuDescription(settings);
        description += $"~n~{RemainingToSell} {MeasurementName}(s) Wanted~s~";

        if (!ConsumeOnPurchase)
        {
            description += $"~n~Player Inventory: {PlayerItems}~s~ {MeasurementName}(s)";
        }
        if (sellScroller == null)
        {
            return;
        }
        sellScroller.Maximum = MaxSell;
        sellScroller.Enabled = isEnabled;
        sellScroller.Description = description;
        //EntryPoint.WriteToConsoleTestLong($"SELL Item: {Name} formattedSalesPrice {formattedPurchasePrice} NumberOfItemsToPurchaseFromPlayer: {menuItem.NumberOfItemsToPurchaseFromPlayer} ItemsBoughtFromPlayer {itemsBoughtFromPlayer}");
    }
    private bool SellItem(Transaction Transaction, ILocationInteractable player, MenuItem menuItem, int TotalItems, bool isStealing)
    {
        int TotalPrice = menuItem.SalesPrice * TotalItems;
        if (player.Inventory.Remove(this, TotalItems))
        {
            player.BankAccounts.GiveMoney(TotalPrice);
            Transaction.MoneySpent += TotalPrice;
            Transaction.PersonTransaction?.TransactionPed?.PedInventory.Add(this, TotalItems);
            //menuItem.ItemsBoughtFromPlayer += TotalItems;
            Transaction.OnAmountChanged(this);
            Transaction.OnItemSold(this, menuItem, TotalItems);
            while (player.ActivityManager.IsPerformingActivity)
            {
                GameFiber.Sleep(500);
            }
            return true;
        }
        return false;
    }
    public virtual void CreatePurchaseMenuItem(Transaction Transaction, MenuItem menuItem, UIMenu purchaseMenu, ISettingsProvideable settings, ILocationInteractable player, bool isStealing, IEntityProvideable world)
    {
        int maxscroller = 10;
        if(ConsumeOnPurchase)
        {
            maxscroller = 1;
        }
        purchaseScroller = new UIMenuNumericScrollerItem<int>(menuItem.ModItemName, "", 1, maxscroller, 1)
        {
            Formatter = v => $"{(v == 1 && MeasurementName == "Item" ? "" : v.ToString() + " ")}" +
            $"{(MeasurementName != "Item" || v > 1 ? MeasurementName : "")}" +
            $"{(v > 1 ? "(s)" : "")}" +
            $"{(MeasurementName != "Item" || v > 1 ? " - " : "")}" +
            $"{(menuItem.PurchasePrice == 0 ? "" : $"${(v * menuItem.PurchasePrice)}")}",
            Value = 1
        };
        UpdatePurchaseMenuItem(Transaction, menuItem, settings, player, isStealing);
        purchaseScroller.Activated += (sender,selectedItem) =>
        {
            PurchaseItem(Transaction, player, menuItem, purchaseScroller.Value, isStealing);
            UpdatePurchaseMenuItem(Transaction, menuItem, settings, player, isStealing);
        };
        UIMenu CategoryMenu = purchaseMenu.Children.Where(x => x.Value.SubtitleText == MenuCategory).FirstOrDefault().Value;
        if (CategoryMenu != null)
        {
            CategoryMenu.AddItem(purchaseScroller);

        }
        else
        {
            purchaseMenu.AddItem(purchaseScroller);
        }
    }
    public void UpdatePurchaseMenuItem(Transaction Transaction, MenuItem menuItem, ISettingsProvideable settings, ILocationInteractable player, bool isStealing)
    {
        if (menuItem != null && purchaseScroller != null)
        {
            InventoryItem PlayerInventoryItem = player.Inventory.ItemsList.Where(x => x.ModItem.Name == menuItem.ModItemName).FirstOrDefault();
            int PlayerItems = 0;
            if (PlayerInventoryItem != null)
            {
                PlayerItems = PlayerInventoryItem.Amount;
            }

            string formattedPurchasePrice = menuItem.PurchasePrice.ToString("C0");

            if (isStealing)
            {
                formattedPurchasePrice = "";
            }
            if (menuItem.PurchasePrice == 0)
            {
                formattedPurchasePrice = "";
            }

            string description = Description;
            description += "~n~~s~";
            description += $"~n~Price: ~r~{formattedPurchasePrice}~s~";
            description += $"~n~Type: ~p~{ItemType}~s~" + (ItemSubType != ItemSubType.None ? $" - ~p~{ItemSubType}~s~" : "");
            description += $"~n~~b~{AmountPerPackage}~s~ Item(s) per Package";
            if (AmountPerPackage > 1)
            {
                description += $"~n~~b~{((float)menuItem.PurchasePrice / (float)AmountPerPackage).ToString("C2")} ~s~per Item";
            }

            description += PurchaseMenuDescription(settings);

            bool enabled = true;
            int RemainingToBuy = 10;
            int MaxBuy = 10;
            int itemsSoldToPlayer = 0;
            if (menuItem.NumberOfItemsToSellToPlayer != -1)
            {
                DesiredItem di = Transaction.PersonTransaction?.TransactionPed?.PedDesires.Get(this);
                if (di != null)
                {
                    itemsSoldToPlayer = di.ItemsSoldToPlayer;
                }
                RemainingToBuy = menuItem.NumberOfItemsToSellToPlayer - itemsSoldToPlayer;// menuItem.ItemsSoldToPlayer;
                if (RemainingToBuy <= 0)
                {
                    MaxBuy = 0;
                    RemainingToBuy = 1;
                    enabled = false;
                }
                else
                {
                    MaxBuy = RemainingToBuy;
                }
                description += $"~n~{MaxBuy} {MeasurementName}(s) For Purchase~s~";
            }
            else if (ConsumeOnPurchase)
            {
                RemainingToBuy = 1;
            }
            if (!ConsumeOnPurchase)
            { 
                description += $"~n~Player Inventory: {PlayerItems}~s~ {MeasurementName}(s)";
            }
            if (purchaseScroller == null)
            {
                return;
            }
            purchaseScroller.Maximum = RemainingToBuy;
            purchaseScroller.Enabled = enabled;
            purchaseScroller.Description = description;
            //EntryPoint.WriteToConsoleTestLong($"PURCHASE Item: {Name} formattedPurchasePrice {formattedPurchasePrice} NumberOfItemsToSellToPlayer: {menuItem.NumberOfItemsToSellToPlayer} ItemsSoldToPlayer {itemsSoldToPlayer}");
        }
    }
    private bool PurchaseItem(Transaction Transaction, ILocationInteractable player, MenuItem menuItem, int TotalItems, bool isStealing)
    {
        int TotalPrice = menuItem.PurchasePrice * TotalItems;
        if (player.BankAccounts.Money >= TotalPrice || isStealing)
        {      
            Transaction?.PersonTransaction?.TransactionPed?.PedInventory.Remove(this, TotalItems);
            //menuItem.ItemsSoldToPlayer += TotalItems;
            if (ConsumeOnPurchase)
            {
                player.ActivityManager.UseInventoryItem(this, false);
            }
            else
            {
                player.Inventory.Add(this, TotalItems * AmountPerPackage);
            }
            if (!isStealing)
            {
                player.BankAccounts.GiveMoney(-1 * TotalPrice);
                Transaction.MoneySpent += TotalPrice;
            }
            Transaction.OnItemPurchased(this, menuItem, TotalItems);
            return true;
        }
        Transaction.DisplayInsufficientFundsMessage();
        return false;
    }
    public virtual void CreatePreview(Transaction Transaction, Camera StoreCam, bool isPurchase, IEntityProvideable world, ISettingsProvideable settings)
    {
        try
        {
            string ModelToSpawn = "";
            bool useClose = true;
            if (PackageItem != null)
            {
                ModelToSpawn = PackageItem.ModelName;
                useClose = !PackageItem.IsLarge;
            }
            if (ModelToSpawn == "" && ModelItem != null)
            {
                ModelToSpawn = ModelItem.ModelName;
                useClose = !ModelItem.IsLarge;
            }


           // Transaction.RotatePreview = true;

            Vector3 Position = Vector3.Zero;
            if (StoreCam.Exists())
            {
                if (useClose)
                {
                    Position = StoreCam.Position + StoreCam.Direction;
                }
                else
                {
                    Position = StoreCam.Position + (StoreCam.Direction.ToNormalized() * 3f);
                }
            }
            else
            {
                Vector3 GPCamPos = NativeFunction.Natives.GET_GAMEPLAY_CAM_COORD<Vector3>();
                Vector3 GPCamDir = NativeHelper.GetGameplayCameraDirection();
                if (useClose)
                {
                    Position = GPCamPos + GPCamDir / 2;
                }
                else
                {
                    Position = GPCamPos + GPCamDir.ToNormalized() * 3f;
                }
            }
            if (ModelToSpawn != "" && NativeFunction.Natives.IS_MODEL_VALID<bool>(Game.GetHashKey(ModelToSpawn)))
            {
                try
                {
                   Transaction.SellingProp = new Rage.Object(ModelToSpawn, Position);
                }
                catch (Exception ex)
                {
                    //EntryPoint.WriteToConsoleTestLong($"Error Spawning Model {ex.Message} {ex.StackTrace}");
                }
                if (Transaction.SellingProp.Exists())
                {
                    Transaction.SellingProp.SetRotationYaw(Transaction.SellingProp.Rotation.Yaw + 45f);
                    if (Transaction.SellingProp != null && Transaction.SellingProp.Exists())
                    {
                        NativeFunction.Natives.SET_ENTITY_HAS_GRAVITY(Transaction.SellingProp, false);
                    }
                }
                //EntryPoint.WriteToConsole("SIMPLE TRANSACTION: PREVIEW ITEM RAN");
            }
            else
            {
                if (Transaction.SellingProp.Exists())
                {
                    Transaction.SellingProp.Delete();
                }
            }
        }
        catch (Exception ex)
        {
            Game.DisplayNotification($"Error Displaying Model {ex.Message} {ex.StackTrace}");
        }
    }
    public override string ToString()
    {
        return Name;
    }
    public bool DropItem(IActionable actionable, ISettingsProvideable settings, int amount)
    {
        ItemDroppingActivity activity = new ItemDroppingActivity(actionable, settings, this, amount);
        if (activity.CanPerform(actionable))
        {
            actionable.ActivityManager.StartUpperBodyActivity(activity);
            return true;
        }
        return false;
    }
    public virtual void CreateInventoryManageMenu(IInteractionable player, MenuPool menuPool, SimpleInventory simpleInventory, UIMenu headerMenu, bool withAnimations, ISettingsProvideable settings)
    {
        inventoryItemSubMenu = menuPool.AddSubMenu(headerMenu, Name);
        inventoryItemSubMenuItem = headerMenu.MenuItems[headerMenu.MenuItems.Count() - 1];
        inventoryItemSubMenuItem.Description = Description;
        inventoryItemSubMenu.SetBannerType(EntryPoint.LSRedColor);

        takeScroller = new UIMenuNumericScrollerItem<int>("Take", "", 1, 1, 1) { Value = 1, Enabled = true, Formatter = v => v.ToString() + " " + MeasurementName + (v > 1 ? "(s)" : "") };
        takeScroller.Activated += (sender, selectedItem) =>
        {
            if(simpleInventory.Remove(this, takeScroller.Value))
            {
                if(withAnimations)
                {
                    player.ActivityManager.PerformItemAnimation(true);
                }
                player.Inventory.Add(this,takeScroller.Value);
            }
            UpdateInventoryScrollers(player, simpleInventory, settings);
        };
        inventoryItemSubMenu.AddItem(takeScroller);

        giveScroller = new UIMenuNumericScrollerItem<int>("Deposit", "", 1, 1, 1) { Value = 1, Enabled = true, Formatter = v => v.ToString() + " " + MeasurementName + (v > 1 ? "(s)" : "") };
        giveScroller.Activated += (sender, selectedItem) =>
        {
            if(player.Inventory.Remove(this, giveScroller.Value))
            {
                if(withAnimations)
                {
                    player.ActivityManager.PerformItemAnimation(false);
                }
                simpleInventory.Add(this, giveScroller.Value);
            }
            UpdateInventoryScrollers(player, simpleInventory, settings);
        };
        inventoryItemSubMenu.AddItem(giveScroller);
        UpdateInventoryScrollers(player, simpleInventory, settings);
    }
    public void UpdateInventoryScrollers(IInteractionable player, SimpleInventory simpleInventory, ISettingsProvideable settings)
    {
        int storedItems = 0;
        int playerItems = 0;
        InventoryItem currentInventoryItem = simpleInventory.Get(this);
        if (currentInventoryItem == null || currentInventoryItem.Amount <= 0)
        {
            takeScroller.Maximum = 1;
            takeScroller.Value = 1;
            takeScroller.Enabled = false;
            storedItems = 0;
        }
        else
        {
            storedItems = currentInventoryItem.Amount;
            takeScroller.Maximum = currentInventoryItem.Amount;
            takeScroller.Value = currentInventoryItem.Amount;
            takeScroller.Enabled = true;
        }
        InventoryItem playerInventoryItem = player.Inventory.Get(this);
        if (playerInventoryItem == null || playerInventoryItem.Amount <= 0)
        {
            playerItems = 0;
            giveScroller.Maximum = 1;
            giveScroller.Value = 1;
            giveScroller.Enabled = false;
        }
        else
        {
            playerItems = playerInventoryItem.Amount;
            giveScroller.Maximum = playerInventoryItem.Amount;
            giveScroller.Value = playerInventoryItem.Amount;
            giveScroller.Enabled = true;
        }

        string descriptionToUse = $"{Description}" +
            $"~n~{PurchaseMenuDescription(settings)}~n~" +
            $"~n~{storedItems} {MeasurementName}(s) ~o~Stored~s~" +
            $"~n~{playerItems} {MeasurementName}(s) In ~y~Player Inventory~s~."; ;

        takeScroller.Description = descriptionToUse;
        giveScroller.Description = descriptionToUse;

        inventoryItemSubMenuItem.RightLabel = $"{storedItems} Stored";
        inventoryItemSubMenuItem.Description = descriptionToUse;
    }

    public virtual void CreateSimpleSellMenu(ILocationInteractable player, UIMenu sellPlateSubMenu,GameLocation gameLocation, int defaultPrice, int altPrice)
    {
        UIMenuItem MenuItem = new UIMenuItem(Name,Description);
        MenuItem.Activated += (sender, e) =>
        {
            if (!player.Inventory.Remove(this, 1))
            {
                return;
            }
            player.BankAccounts.GiveMoney(defaultPrice);
            MenuItem.Enabled = false;
            if (gameLocation == null)
            {
                return;
            }
            gameLocation.PlaySuccessSound();
            gameLocation.DisplayMessage("~g~Sale", $"You have sold your {ItemSubType}.");
        };
        sellPlateSubMenu.AddItem(MenuItem);
    }
}

