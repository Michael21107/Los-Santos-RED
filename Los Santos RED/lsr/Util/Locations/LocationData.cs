﻿using LosSantosRED.lsr.Interface;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Locations
{
    public class LocationData
    {
        private Vector3 ClosestNode;
        private IStreets Streets;
        private IZones Zones;
        private Zone PreviousZone;
        private uint GameTimeEnteredZone;
        private int InteriorID;
        private IInteriors Interiors;

        public LocationData(Ped characterToLocate, IStreets streets, IZones zones, IInteriors interiors)
        {
            Streets = streets;
            Zones = zones;
            Interiors = interiors;
            CharacterToLocate = characterToLocate;
        }
        public Interior CurrentInterior { get; private set; }
        public Ped CharacterToLocate { get; set; }
        public Street CurrentStreet { get; private set; }
        public Street CurrentCrossStreet { get; private set; }
        public Zone CurrentZone { get; private set; }
        public bool IsOffroad { get; private set; }
        public bool IsInside => CurrentInterior != null && CurrentInterior.ID != 0;
        public uint GameTimeInZone => GameTimeEnteredZone == 0 ? 0 : Game.GameTime - GameTimeEnteredZone;
        public void Update(Ped characterToLocate)
        {
            if(characterToLocate.Exists())
            {
                CharacterToLocate = characterToLocate;
            }
            if (CharacterToLocate.Exists())
            {
                GetZone();
                GameFiber.Yield();
                GetInterior();
                GameFiber.Yield();
                GetNode();
                GameFiber.Yield();
                GetStreets();
                GameFiber.Yield();
                
            }
            else
            {
                CurrentZone = null;
                CurrentStreet = null;
                CurrentCrossStreet = null;
                CurrentInterior = null;
            }
        }
        private void GetZone()
        {
            if (CharacterToLocate.Exists())
            {
                CurrentZone = Zones.GetZone(CharacterToLocate.Position);
                if(PreviousZone == null || CurrentZone != PreviousZone)
                {
                    GameTimeEnteredZone = Game.GameTime;
                    PreviousZone = CurrentZone;
                }
            }
        }
        private void GetNode()
        {
            if (CharacterToLocate.Exists() && !IsInside)
            {
                ClosestNode = Rage.World.GetNextPositionOnStreet(CharacterToLocate.Position);
                if (ClosestNode.DistanceTo2D(CharacterToLocate) >= 15f)//was 15f
                {
                    IsOffroad = true;
                }
                else
                {
                    IsOffroad = false;
                }
            }
            else
            {
                IsOffroad = true;
            }
        }
        private void GetStreets()
        {
            if (IsOffroad || IsInside)
            {
                CurrentStreet = null;
                CurrentCrossStreet = null;
                return;
            }

            int StreetHash = 0;
            int CrossingHash = 0;
            string CurrentStreetName;
            string CurrentCrossStreetName;
            unsafe
            {
                NativeFunction.CallByName<uint>("GET_STREET_NAME_AT_COORD", ClosestNode.X, ClosestNode.Y, ClosestNode.Z, &StreetHash, &CrossingHash);
            }
            string StreetName = string.Empty;
            if (StreetHash != 0)
            {
                unsafe
                {
                    IntPtr ptr = NativeFunction.CallByName<IntPtr>("GET_STREET_NAME_FROM_HASH_KEY", StreetHash);
                    StreetName = Marshal.PtrToStringAnsi(ptr);
                }
                CurrentStreetName = StreetName;
                GameFiber.Yield();
            }
            else
            {
                CurrentStreetName = "";
            }

            string CrossStreetName = string.Empty;
            if (CrossingHash != 0)
            {
                unsafe
                {
                    IntPtr ptr = NativeFunction.CallByName<IntPtr>("GET_STREET_NAME_FROM_HASH_KEY", CrossingHash);
                    CrossStreetName = Marshal.PtrToStringAnsi(ptr);
                }
                CurrentCrossStreetName = CrossStreetName;
                GameFiber.Yield();
            }
            else
            {
                CurrentCrossStreetName = "";
            }

            CurrentStreet = Streets.GetStreet(CurrentStreetName);
            CurrentCrossStreet = Streets.GetStreet(CurrentCrossStreetName);
            GameFiber.Yield();

            if (CurrentStreet == null)
            {
                CurrentStreet = new Street("Calle Sin Nombre", 60f, "MPH");
            }
        }
        private void GetInterior()
        {
            InteriorID = NativeFunction.Natives.GET_INTERIOR_FROM_ENTITY<int>(CharacterToLocate);
            if(InteriorID == 0)
            {
                CurrentInterior = new Interior(0,"");
            }
            else
            {
                CurrentInterior = Interiors.GetInterior(InteriorID);
                if(CurrentInterior == null)
                {
                    CurrentInterior = new Interior(InteriorID, "");
                }
            }
        }
    }
}
