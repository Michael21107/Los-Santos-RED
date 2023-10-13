﻿using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SpawnError
{
    public SpawnError(uint modelHash, Vector3 spawnLocation, uint gameTimeSpawned)
    {
        ModelHash = modelHash;
        SpawnLocation = spawnLocation;
        GameTimeSpawned = gameTimeSpawned;
    }

    public uint ModelHash { get; set; } 
    public Vector3 SpawnLocation { get; set; }
    public uint GameTimeSpawned { get; set; }
    public bool HasCleared { get; set; } = false;
    public bool CheckVehicle(Vehicle vehicle)
    {
        if(!vehicle.Exists())
        {
            return false;
        }
        if (Game.GameTime - GameTimeSpawned >= 5000 && vehicle.Model.Hash == ModelHash && vehicle.IsPersistent && vehicle.Position.DistanceTo2D(SpawnLocation) <= 5.0f)
        {
            EntryPoint.WriteToConsole($"SPAWN ERRORS CLEANED UP VEHICLE {vehicle.Handle} {Game.GameTime} {vehicle.Position} spawnError: {ModelHash} {SpawnLocation} {GameTimeSpawned}");
            vehicle.Delete();
            HasCleared = true;
            return true;
        }
        if(Game.GameTime - GameTimeSpawned >= 15000)
        {
            HasCleared = true;
            EntryPoint.WriteToConsole($"SPAWN ERRORS DID NOT FIND VEHICLE TO CLEANUP, REMOVING spawnError: {ModelHash} {SpawnLocation} {GameTimeSpawned}");
        }
        return false;
    }
}

