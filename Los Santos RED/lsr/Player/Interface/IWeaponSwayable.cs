﻿using LosSantosRED.lsr.Locations;
using LSR.Vehicles;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface IWeaponSwayable
    {
        Equipment Equipment { get; }
        bool IsInVehicle { get; }
        Ped Character { get; }
        string DebugLine4 { get; set; }
        bool IsRagdoll { get; }
        bool IsStunned { get; }
        bool IsInFirstPerson { get; }
    }
}
