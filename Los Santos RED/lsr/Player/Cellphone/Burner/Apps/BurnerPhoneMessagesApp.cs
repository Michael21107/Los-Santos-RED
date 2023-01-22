﻿using LosSantosRED.lsr.Interface;
using Rage.Native;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

public class BurnerPhoneMessagesApp : BurnerPhoneApp
{
    private bool IsDisplayingTextMessage;
    private int CurrentRow;
    private int CurrentIndex;

    public BurnerPhoneMessagesApp(BurnerPhone burnerPhone, ICellPhoneable player, ITimeReportable time, ISettingsProvideable settings, int index) : base(burnerPhone, player, time, settings, index, "Messages", 2)
    {
    }
    public override void Update()
    {
        Notifications = Player.CellPhone.TextList.Where(x => !x.IsRead).Count();
        base.Update();
    }
    public override void Open(bool Reset)
    {
        IsDisplayingTextMessage = false;
        if (Reset)
        {
            CurrentRow = 0;
        }
        BurnerPhone.SetHeader(Name);
        NativeFunction.Natives.BEGIN_SCALEFORM_MOVIE_METHOD(BurnerPhone.GlobalScaleformID, "SET_DATA_SLOT_EMPTY");
        NativeFunction.Natives.xC3D0841A0CC546A6(6);//2
        NativeFunction.Natives.END_SCALEFORM_MOVIE_METHOD();
        foreach (PhoneText text in Player.CellPhone.TextList.OrderBy(x => x.Index))
        {
            DrawMessage(text);
        }
        NativeFunction.Natives.BEGIN_SCALEFORM_MOVIE_METHOD(BurnerPhone.GlobalScaleformID, "DISPLAY_VIEW");
        NativeFunction.Natives.xC3D0841A0CC546A6(6);
        NativeFunction.Natives.xC3D0841A0CC546A6(CurrentRow);
        NativeFunction.Natives.END_SCALEFORM_MOVIE_METHOD();
    }
    public override void HandleInput()
    {
        if (NativeFunction.Natives.x91AEF906BCA88877<bool>(3, 172) && !IsDisplayingTextMessage)//UP
        {
            BurnerPhone.MoveFinger(1);
            BurnerPhone.NavigateMenu(1);
            CurrentRow = CurrentRow - 1;
        }
        else if (NativeFunction.Natives.x91AEF906BCA88877<bool>(3, 173) && !IsDisplayingTextMessage)//DOWN
        {
            BurnerPhone.MoveFinger(2);
            BurnerPhone.NavigateMenu(3);
            CurrentRow = CurrentRow + 1;
        }
        int TotalMessages = Player.CellPhone.TextList.Count();
        if (TotalMessages > 0)
        {
            if (CurrentRow > TotalMessages - 1)
            {
                CurrentRow = 0;
            }
        }
        if (CurrentRow < 0)
        {
            CurrentRow = 0;
        }
        if (NativeFunction.Natives.x91AEF906BCA88877<bool>(3, 176) && !IsDisplayingTextMessage)//SELECT
        {
            BurnerPhone.MoveFinger(5);
            BurnerPhone.PlayAcceptedSound();
            IsDisplayingTextMessage = true;
            DisplayTextUI(Player.CellPhone.TextList.Where(x => x.Index == CurrentRow).FirstOrDefault());
        }
        if (NativeFunction.Natives.x305C8DCD79DA8B0F<bool>(3, 177))//CLOSE
        {
            BurnerPhone.PlayBackSound();
            if (IsDisplayingTextMessage)
            {
                IsDisplayingTextMessage = false;
                Open(false);
            }
            else
            {
                //GameFiber.Sleep(200);
                BurnerPhone.ReturnHome(Index);
            }
        }
        if (!IsDisplayingTextMessage)
        {
            BurnerPhone.SetSoftKey((int)SoftKey.Left, SoftKeyIcon.Select, Color.LightBlue);
            BurnerPhone.SetSoftKey((int)SoftKey.Middle, SoftKeyIcon.Blank, Color.Black);
            BurnerPhone.SetSoftKey((int)SoftKey.Right, SoftKeyIcon.Back, Color.Purple);
        }
        else
        {
            BurnerPhone.SetSoftKey((int)SoftKey.Left, SoftKeyIcon.Delete, Color.Red);
            BurnerPhone.SetSoftKey((int)SoftKey.Middle, SoftKeyIcon.Call, Color.LightBlue);
            BurnerPhone.SetSoftKey((int)SoftKey.Right, SoftKeyIcon.Back, Color.Purple);
        }
    }
    private void DrawMessage(PhoneText text)
    {
        NativeFunction.Natives.BEGIN_SCALEFORM_MOVIE_METHOD(BurnerPhone.GlobalScaleformID, "SET_DATA_SLOT");
        NativeFunction.Natives.xC3D0841A0CC546A6(6);//2
        NativeFunction.Natives.xC3D0841A0CC546A6(text.Index);
        NativeFunction.Natives.xC3D0841A0CC546A6(text.HourSent);
        NativeFunction.Natives.xC3D0841A0CC546A6(text.MinuteSent);
        if (text.IsRead)
        {
            NativeFunction.Natives.xC3D0841A0CC546A6(34);
        }
        else
        {
            NativeFunction.Natives.xC3D0841A0CC546A6(33);
        }
        NativeFunction.Natives.BEGIN_TEXT_COMMAND_SCALEFORM_STRING("STRING");
        NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(text.ContactName);
        NativeFunction.Natives.END_TEXT_COMMAND_SCALEFORM_STRING();
        NativeFunction.Natives.BEGIN_TEXT_COMMAND_SCALEFORM_STRING("STRING");
        NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(text.Message);
        NativeFunction.Natives.END_TEXT_COMMAND_SCALEFORM_STRING();
        NativeFunction.Natives.END_SCALEFORM_MOVIE_METHOD();
    }
    public void DisplayTextUI(PhoneText text)
    {
        if (text != null)
        {
            text.IsRead = true;
            NativeFunction.Natives.BEGIN_SCALEFORM_MOVIE_METHOD(BurnerPhone.GlobalScaleformID, "SET_DATA_SLOT");
            NativeFunction.Natives.xC3D0841A0CC546A6(7);
            NativeFunction.Natives.xC3D0841A0CC546A6(0);

            NativeFunction.Natives.BEGIN_TEXT_COMMAND_SCALEFORM_STRING("STRING");
            NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(text.ContactName);       //UI::_ADD_TEXT_COMPONENT_APP_TITLE
            NativeFunction.Natives.END_TEXT_COMMAND_SCALEFORM_STRING();

            NativeFunction.Natives.BEGIN_TEXT_COMMAND_SCALEFORM_STRING("STRING");
            NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(text.Message);       //UI::_ADD_TEXT_COMPONENT_APP_TITLE
            NativeFunction.Natives.END_TEXT_COMMAND_SCALEFORM_STRING();

            NativeFunction.Natives.BEGIN_TEXT_COMMAND_SCALEFORM_STRING("STRING");
            NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME("CHAR_BLANK_ENTRY");       //UI::_ADD_TEXT_COMPONENT_APP_TITLE
            NativeFunction.Natives.END_TEXT_COMMAND_SCALEFORM_STRING();

            NativeFunction.Natives.END_SCALEFORM_MOVIE_METHOD();

            NativeFunction.Natives.BEGIN_SCALEFORM_MOVIE_METHOD(BurnerPhone.GlobalScaleformID, "DISPLAY_VIEW");
            NativeFunction.Natives.xC3D0841A0CC546A6(7);
            NativeFunction.Natives.END_SCALEFORM_MOVIE_METHOD();

            SetTextApp();
        }
    }
    private void SetTextApp()
    {
        NativeFunction.Natives.BEGIN_SCALEFORM_MOVIE_METHOD(BurnerPhone.GlobalScaleformID, "SET_DATA_SLOT");
        NativeFunction.Natives.xC3D0841A0CC546A6(1);
        NativeFunction.Natives.xC3D0841A0CC546A6(0);
        NativeFunction.Natives.xC3D0841A0CC546A6(2);
        NativeFunction.Natives.xC3D0841A0CC546A6(Player.CellPhone.TextList.Where(x => !x.IsRead).Count());
        NativeFunction.Natives.BEGIN_TEXT_COMMAND_SCALEFORM_STRING("STRING");
        NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME("Texts");
        NativeFunction.Natives.END_TEXT_COMMAND_SCALEFORM_STRING();
        NativeFunction.Natives.xC3D0841A0CC546A6(100);
        NativeFunction.Natives.END_SCALEFORM_MOVIE_METHOD();
    }
}

