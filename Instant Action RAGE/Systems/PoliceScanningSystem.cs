﻿using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExtensionsMethods;
using System.Drawing;
using System.IO;

namespace Instant_Action_RAGE.Systems
{
    internal static class PoliceScanningSystem
    {
        private static uint GameTimeInterval;
        private static uint VehicleReplaceInterval;
        private static uint LOSInterval;
        private static int CopDogRelationShip;
        private static Random rnd;
        private static List<Vehicle> ReplacedVehicles = new List<Vehicle>();
        public static List<PoliceTask> CopsToTask = new List<PoliceTask>();

        static PoliceScanningSystem()
        {
            rnd = new Random();
        }
        enum PoliceState
        {
            Normal = 0,
            UnarmedChase = 1,
            CautiousChase = 2,
            DeadlyChase = 3,
            ArrestedWait = 4,
        }
        public static List<GTACop> CopPeds { get; private set; }
        public static List<GTACop> K9Peds { get; private set; }
        public static int ScanningInterval { get; private set; }
        public static float ScanningRange { get; private set; }
        public static float InnocentScanningRange { get; private set; }
        public static int CopsKilled { get; set; } = 0;
        public static bool InnocentsNear { get; private set; }
        public static bool Enabled { get; set; } = true;
        public static bool PlayerHurtPolice { get; set; } = false;
        public static bool PlayerKilledPolice { get; set; } = false;
        public static Vector3 PlacePlayerLastSeen
        {
            get
            {
                if (!CopPeds.Any(x => x.GameTimeLastSeenPlayer > 0))
                    return new Vector3(0f, 0f, 0f);
                else
                    return CopPeds.Where(x => x.GameTimeLastSeenPlayer > 0).OrderByDescending(x => x.GameTimeLastSeenPlayer).FirstOrDefault().PositionLastSeenPlayer;
            }
        }
        public static bool IsRunning { get; set; } = true;
        public static void Initialize()
        {
            ScanningInterval = 5000;
            ScanningRange = 200f;
            InnocentScanningRange = 10f;
            CopPeds = new List<GTACop>();
            K9Peds = new List<GTACop>();
            TaskQueue();
            MainLoop();
        }
        public static void MainLoop()
        {
            GameFiber.StartNew(delegate
            {
                while (IsRunning)
                {
                    CopPeds.RemoveAll(x => !x.CopPed.Exists() ||  x.CopPed.IsDead);

                    if (Game.GameTime > GameTimeInterval + ScanningInterval)
                    {
                        ScanForPolice();                        
                        GameTimeInterval = Game.GameTime;
                    }
                    if (Game.GameTime > VehicleReplaceInterval + 5000)
                    {
                        //CheckVehiclesToReplace();
                        VehicleReplaceInterval = Game.GameTime;
                    }
                    if(Game.GameTime > LOSInterval + 500) // was 2000
                    {
                        CheckLOS();
                        SetPrimaryPursuer();
                        LOSInterval = Game.GameTime;
                    }


                    if (!PlayerHurtPolice || !PlayerKilledPolice)
                        CheckDamage();


                    //if (Game.IsKeyDown(Keys.NumPad5)) // Our menu on/off switch.
                    //{
                    //    Game.LocalPlayer.Character.GiveCash(5000, "Michael");
                    //}

                    if (Game.IsKeyDown(Keys.NumPad7)) // Our menu on/off switch.
                    {
                        //Game.LocalPlayer.WantedLevel = 0;
                        InstantAction.CurrentPoliceState = InstantAction.PoliceState.Normal;
                        InstantAction.ResetPlayer(true, true);
                    }
                    if (Game.IsKeyDown(Keys.NumPad8)) // Our menu on/off switch.
                    {
                        Game.LocalPlayer.WantedLevel--;
                    }
                    if (Game.IsKeyDown(Keys.NumPad9)) // Our menu on/off switch.
                    {
                        Game.LocalPlayer.WantedLevel++;
                    }
                    if (Settings.Debug)
                    {
                        foreach (GTACop Cop in CopPeds.Where(x => x.CopPed.Exists() && !x.CopPed.IsDead))
                        {
                            //if (Cop.TaskFiber != null && Cop.TaskFiber.Name == "Chase")
                            //    Debug.DrawArrowDebug(new Vector3(Cop.CopPed.Position.X, Cop.CopPed.Position.Y, Cop.CopPed.Position.Z + 2f), Vector3.Zero, Rage.Rotator.Zero, 1f, Color.Green);
                            //else if(Cop.TaskFiber != null && Cop.TaskFiber.Name == "Arrest")
                            //    Debug.DrawArrowDebug(new Vector3(Cop.CopPed.Position.X, Cop.CopPed.Position.Y, Cop.CopPed.Position.Z + 2f), Vector3.Zero, Rage.Rotator.Zero, 1f, Color.Black);
                            //else
                            //    Debug.DrawArrowDebug(new Vector3(Cop.CopPed.Position.X, Cop.CopPed.Position.Y, Cop.CopPed.Position.Z + 2f), Vector3.Zero, Rage.Rotator.Zero, 1f, Color.Yellow);



                            if (Cop.CopPed.Tasks.CurrentTaskStatus == Rage.TaskStatus.InProgress)
                                Debug.DrawArrowDebug(new Vector3(Cop.CopPed.Position.X, Cop.CopPed.Position.Y, Cop.CopPed.Position.Z + 2f), Vector3.Zero, Rage.Rotator.Zero, 1f, Color.Green);
                            else if (Cop.CopPed.Tasks.CurrentTaskStatus == Rage.TaskStatus.Interrupted)
                                Debug.DrawArrowDebug(new Vector3(Cop.CopPed.Position.X, Cop.CopPed.Position.Y, Cop.CopPed.Position.Z + 2f), Vector3.Zero, Rage.Rotator.Zero, 1f, Color.Purple);
                            else if (Cop.CopPed.Tasks.CurrentTaskStatus == Rage.TaskStatus.None)
                                Debug.DrawArrowDebug(new Vector3(Cop.CopPed.Position.X, Cop.CopPed.Position.Y, Cop.CopPed.Position.Z + 2f), Vector3.Zero, Rage.Rotator.Zero, 1f, Color.White);

                            else if (Cop.CopPed.Tasks.CurrentTaskStatus == Rage.TaskStatus.NoTask)
                                Debug.DrawArrowDebug(new Vector3(Cop.CopPed.Position.X, Cop.CopPed.Position.Y, Cop.CopPed.Position.Z + 2f), Vector3.Zero, Rage.Rotator.Zero, 1f, Color.Orange);

                            else if (Cop.CopPed.Tasks.CurrentTaskStatus == Rage.TaskStatus.Preparing)
                                Debug.DrawArrowDebug(new Vector3(Cop.CopPed.Position.X, Cop.CopPed.Position.Y, Cop.CopPed.Position.Z + 2f), Vector3.Zero, Rage.Rotator.Zero, 1f, Color.Red);


                            else if (Cop.CopPed.Tasks.CurrentTaskStatus == Rage.TaskStatus.Unknown)
                                Debug.DrawArrowDebug(new Vector3(Cop.CopPed.Position.X, Cop.CopPed.Position.Y, Cop.CopPed.Position.Z + 2f), Vector3.Zero, Rage.Rotator.Zero, 1f, Color.Black);



                            else
                                Debug.DrawArrowDebug(new Vector3(Cop.CopPed.Position.X, Cop.CopPed.Position.Y, Cop.CopPed.Position.Z + 2f), Vector3.Zero, Rage.Rotator.Zero, 1f, Color.Yellow);


                            //if (Cop.CopPed.Exists())
                            //    Cop.CopPed.BlockPermanentEvents = true;

                        }
                        Debug.DrawArrowDebug(new Vector3(PlacePlayerLastSeen.X, PlacePlayerLastSeen.Y, PlacePlayerLastSeen.Z + 2f), Vector3.Zero, Rage.Rotator.Zero, 1f, Color.Yellow);
                    }
                    GameFiber.Yield();
                }
            });
        }
        private static void ScanForPolice()
        {
            Ped[] Pedestrians = Array.ConvertAll(World.GetEntities(Game.LocalPlayer.Character.Position, 250f, GetEntitiesFlags.ConsiderHumanPeds | GetEntitiesFlags.ExcludePlayerPed).Where(x => x is Ped).ToArray(), (x => (Ped)x));
            foreach (Ped Cop in Pedestrians.Where(s => s.Exists() && !s.IsDead && s.isPoliceArmy() && s.IsVisible))
            {
                if (!CopPeds.Any(x => x.CopPed == Cop))
                {
                    bool canSee = false;
                    if (Cop.PlayerIsInFront() && Cop.IsInRangeOf(Game.LocalPlayer.Character.Position, 55f) && NativeFunction.CallByName<bool>("HAS_ENTITY_CLEAR_LOS_TO_ENTITY_IN_FRONT", Cop, Game.LocalPlayer.Character))
                        canSee = true;

                    GTACop myCop = new GTACop(Cop, canSee, canSee ? Game.GameTime : 0, canSee ? Game.LocalPlayer.Character.Position : new Vector3(0f, 0f, 0f));
                    if (rnd.Next(0, 100) <= 15 && Game.LocalPlayer.WantedLevel > 0 && myCop.CopPed.IsInAnyVehicle(false) && K9Peds.Count < 2)
                        CreateK9(myCop);
                    Cop.IsPersistent = false;
                    CopPeds.Add(myCop);

                    if (InstantAction.CurrentPoliceState == InstantAction.PoliceState.DeadlyChase && PlayerHurtPolice)
                    {
                        if (rnd.Next(0, 100) <= 85)
                            Cop.Inventory.GiveNewWeapon(WeaponHash.CarbineRifle, 90, true); // AR-15
                        Cop.Health = 100;
                    }
                }
            }
            foreach (GTACop Cop in CopPeds)
            {
                if (Cop.CopPed.Exists() && Cop.CopPed.IsDead)
                {
                    if (PlayerKilledPed(Cop))
                    {
                        PlayerKilledPolice = true;
                        break;
                    }
                }
            }
            CopPeds.RemoveAll(x => !x.CopPed.Exists() || x.CopPed.IsDead);
            K9Peds.RemoveAll(x => !x.CopPed.Exists() || x.CopPed.IsDead);
        }
        private static void CreateK9(GTACop Cop)
        {
            if (!Cop.CopPed.IsInAnyVehicle(false) || Cop.CopPed.IsOnBike || Cop.CopPed.IsInBoat || Cop.CopPed.IsInHelicopter)
                return;

            Ped Doggo = new Ped("a_c_shepherd", Game.LocalPlayer.Character.GetOffsetPosition(new Vector3(0f, -4f, 0f)), Game.LocalPlayer.Character.Heading);

            Doggo.BlockPermanentEvents = true;
            Doggo.IsPersistent = true;

            if(Cop.CopPed.CurrentVehicle.IsSeatFree(1))
                Doggo.WarpIntoVehicle(Cop.CopPed.CurrentVehicle, 1);
            else
                Doggo.WarpIntoVehicle(Cop.CopPed.CurrentVehicle, 2);
            Doggo.RelationshipGroup = "COPDOGS";


            Game.SetRelationshipBetweenRelationshipGroups("COPDOGS", "COP", Relationship.Like);
            Game.SetRelationshipBetweenRelationshipGroups("COP","COPDOGS",  Relationship.Like);

            GTACop DoggoCop = new GTACop(Doggo, false);
            K9Peds.Add(DoggoCop);
            WriteToLog("CreateK9", String.Format("Created K9 ", Doggo.Handle));

        }
        private static void CheckDamage()
        {
            foreach (GTACop Cop in CopPeds)
            {
                if (NativeFunction.CallByName<bool>("HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY", Cop.CopPed, Game.LocalPlayer.Character, true))
                {
                    PlayerHurtPolice = true;
                    Cop.HurtByPlayer = true;
                }
                if(Game.LocalPlayer.Character.IsInAnyVehicle(false) && NativeFunction.CallByName<bool>("HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY", Cop.CopPed, Game.LocalPlayer.Character.CurrentVehicle, true))
                {
                    PlayerHurtPolice = true;
                    Cop.HurtByPlayer = true;
                }
                if (PlayerKilledPed(Cop))
                    PlayerKilledPolice = true;

                //if (Cop.CopPed.IsDead && Cop.HurtByPlayer)
                //{
                //    //PlayerKilledPolice = true;
                //    Ped killer = NativeFunction.Natives.GetPedSourceOfDeath<Entity>(Cop.CopPed);//Cop.CopPed.GetKiller();//GET_PED_SOURCE_OF_DEATH
                //    if (NativeFunction.CallByName<bool>("IS_ENTITY_A_PED", killer))
                //    {
                //        if (Game.LocalPlayer.Character == (Ped)killer || Cop.CopPed == (Ped)killer)
                //        {
                //            PlayerKilledPolice = true;
                //        }
                //    }
                //    else if (NativeFunction.CallByName<bool>("IS_ENTITY_A_VEHICLE", killer))
                //    {
                //        if (Game.LocalPlayer.Character.CurrentVehicle == killer)
                //        {
                //            PlayerKilledPolice = true;
                //        }
                //    }
                //}
            }
        }
        private static bool PlayerKilledPed(GTACop Cop)
        {
            if (Cop.CopPed.IsDead && Cop.HurtByPlayer)
            {
                Ped killer = NativeFunction.Natives.GetPedSourceOfDeath<Entity>(Cop.CopPed);//Cop.CopPed.GetKiller();//GET_PED_SOURCE_OF_DEATH
                if (NativeFunction.CallByName<bool>("IS_ENTITY_A_PED", killer))
                {
                    if (Game.LocalPlayer.Character == (Ped)killer || Cop.CopPed == (Ped)killer)
                    {
                        return true;
                    }
                    else
                        return false;
                }
                else if (NativeFunction.CallByName<bool>("IS_ENTITY_A_VEHICLE", killer))
                {
                    if (Game.LocalPlayer.Character.CurrentVehicle == killer)
                    {
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }
        private static void CheckLOS()
        {
            int i = 0;
            Entity EntityToCheck;
            if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
                EntityToCheck = Game.LocalPlayer.Character.CurrentVehicle;
            else
                EntityToCheck = Game.LocalPlayer.Character;
            foreach (GTACop Cop in CopPeds.Where(x => x.CopPed.Exists() && !x.CopPed.IsDead && !x.CopPed.IsInHelicopter))
            {
                if (Cop.CopPed.PlayerIsInFront() && Cop.CopPed.IsInRangeOf(Game.LocalPlayer.Character.Position, 55f) && !Cop.CopPed.IsDead && NativeFunction.CallByName<bool>("HAS_ENTITY_CLEAR_LOS_TO_ENTITY_IN_FRONT", Cop.CopPed, EntityToCheck)) //was 55f
                {
                    Cop.canSeePlayer = true;
                    Cop.GameTimeLastSeenPlayer = Game.GameTime;
                    Cop.PositionLastSeenPlayer = Game.LocalPlayer.Character.Position;
                    i++;
                }
                else
                {
                    Cop.canSeePlayer = false;
                }
            }
            foreach (GTACop Cop in CopPeds.Where(x => x.CopPed.Exists() && !x.CopPed.IsDead && x.CopPed.IsInHelicopter))
            {
                if (Cop.CopPed.IsInRangeOf(Game.LocalPlayer.Character.Position, 250f) && !Cop.CopPed.IsDead && NativeFunction.CallByName<bool>("HAS_ENTITY_CLEAR_LOS_TO_ENTITY", Cop.CopPed, EntityToCheck,17)) //was 55f
                {
                    Cop.canSeePlayer = true;
                    Cop.GameTimeLastSeenPlayer = Game.GameTime;
                    Cop.PositionLastSeenPlayer = Game.LocalPlayer.Character.Position;
                    i++;
                }
                else
                {
                    Cop.canSeePlayer = false;
                }
            }
        }
        private static void SetPrimaryPursuer()
        {
            if (CopPeds.Count == 0)
                return;
            foreach (GTACop Cop in CopPeds.Where(x => x.CopPed.Exists() && !x.CopPed.IsDead && !x.CopPed.IsInHelicopter))
            {
                Cop.isPursuitPrimary = false;
            }
            GTACop PursuitPrimary = CopPeds.Where(x => x.CopPed.Exists() && !x.CopPed.IsDead && !x.CopPed.IsInAnyVehicle(false)).OrderBy(x => x.CopPed.Position.DistanceTo2D(Game.LocalPlayer.Character.Position)).FirstOrDefault();
            if (PursuitPrimary == null)
                return;
            else
                PursuitPrimary.isPursuitPrimary = true;
        }
        public static void AddItemToQueue(PoliceTask _MyTask)
        {
            if (!CopsToTask.Contains(_MyTask))
                CopsToTask.Add(_MyTask);
        }
    
        public static void TaskChasing(GTACop Cop)
        {
            //if (!Cop.CopPed.IsInRangeOf(Game.LocalPlayer.Character.Position, 100f) || Cop.TaskFiber != null)
            //{
            //    Cop.CopPed.Tasks.Clear();
            //    Cop.CopPed.BlockPermanentEvents = false;
            //    Cop.TaskFiber = null;
            //    WriteToLog("Task Chasing", string.Format("Initial Return: {0}", Cop.CopPed.Handle));
            //    return;
            //}

            if (Cop.CopPed.IsInRangeOf(Game.LocalPlayer.Character.Position, 100f) && Cop.TaskFiber != null && Cop.TaskFiber.Name == "Chase" && !Cop.RecentlySeenPlayer())
            {
                return;
            }
            if (!Cop.CopPed.IsInRangeOf(Game.LocalPlayer.Character.Position, 100f) && Cop.TaskFiber != null)
            {
                Cop.CopPed.Tasks.Clear();
                Cop.CopPed.BlockPermanentEvents = false;
                Cop.TaskFiber.Abort();
                Cop.TaskFiber = null;
                WriteToLog("Task Chasing", string.Format("Initial Return: {0}", Cop.CopPed.Handle));
                return;
            }

            Cop.TaskFiber =
            GameFiber.StartNew(delegate
            {
                WriteToLog("Task Chasing", string.Format("Started Chase: {0}", Cop.CopPed.Handle));
                uint TaskTime = Game.GameTime;
                string LocalTaskName = "GoTo";
                NativeFunction.CallByName<bool>("SET_PED_PATH_CAN_USE_CLIMBOVERS", Cop.CopPed, true);
                NativeFunction.CallByName<bool>("SET_PED_PATH_CAN_USE_LADDERS", Cop.CopPed, true);
                NativeFunction.CallByName<bool>("SET_PED_PATH_CAN_DROP_FROM_HEIGHT", Cop.CopPed, true);
                Cop.CopPed.BlockPermanentEvents = true;

                //Main Loop
                while (Cop.CopPed.Exists() && !Cop.CopPed.IsDead)
                {
                    Cop.CopPed.BlockPermanentEvents = true;

                    if (InstantAction.CurrentPoliceState == InstantAction.PoliceState.UnarmedChase)
                        InstantAction.SetCopTazer(Cop);
                    else
                        InstantAction.SetCopDeadly(Cop);

                    NativeFunction.CallByName<uint>("SET_PED_MOVE_RATE_OVERRIDE", Cop.CopPed, 1.1f);

                    if (Game.GameTime - TaskTime >= 250)
                    {
                        bool _locIsInAnyVehicle = Game.LocalPlayer.Character.IsInAnyVehicle(false);
                        float _locrangeTo = Cop.CopPed.RangeTo(Game.LocalPlayer.Character.Position);
                        Rage.TaskStatus _locTaskStatus = Cop.CopPed.Tasks.CurrentTaskStatus;
                        if (_locrangeTo > 100f)
                            break;

                    if (_locIsInAnyVehicle && Game.LocalPlayer.Character.CurrentVehicle.Speed <= 25)
                    {
                        if (Cop.isPursuitPrimary && _locrangeTo <= 25f && LocalTaskName != "CarJack")
                        {
                            Cop.CopPed.CanRagdoll = false;
                            NativeFunction.CallByName<bool>("TASK_ENTER_VEHICLE", Cop.CopPed, Game.LocalPlayer.Character.CurrentVehicle, -1, -1, 2f, 9);
                            Cop.CopPed.KeepTasks = true;
                            TaskTime = Game.GameTime;
                            LocalTaskName = "CarJack";
                            WriteToLog("TaskChasing", "Primary Cop SubTasked with CarJack");
                        }
                        else if (!Cop.isPursuitPrimary && _locrangeTo <= 25 && LocalTaskName != "Arrest")
                        {
                            NativeFunction.CallByName<bool>("TASK_GOTO_ENTITY_AIMING", Cop.CopPed, Game.LocalPlayer.Character, 4f, 20f);
                            Cop.CopPed.KeepTasks = true;
                            TaskTime = Game.GameTime;
                            LocalTaskName = "Arrest";
                        }

                    }
                    else
                    {
                        if (LocalTaskName != "Arrest" && (InstantAction.CurrentPoliceState == InstantAction.PoliceState.ArrestedWait || (InstantAction.CurrentPoliceState == InstantAction.PoliceState.CautiousChase && _locrangeTo <= 15f)))
                            {
                                unsafe
                                {
                                    int lol = 0;
                                    NativeFunction.CallByName<bool>("OPEN_SEQUENCE_TASK", &lol);
                                    NativeFunction.CallByName<bool>("TASK_GO_TO_ENTITY", 0, Game.LocalPlayer.Character, -1, 20f, 500f, 1073741824, 1); //Original and works ok
                                    NativeFunction.CallByName<bool>("TASK_GOTO_ENTITY_AIMING", 0, Game.LocalPlayer.Character, 4f, 20f);
                                    NativeFunction.CallByName<bool>("TASK_AIM_GUN_AT_ENTITY", 0, Game.LocalPlayer.Character, 10000, false);
                                    NativeFunction.CallByName<bool>("SET_SEQUENCE_TO_REPEAT", lol, true);
                                    NativeFunction.CallByName<bool>("CLOSE_SEQUENCE_TASK", lol);
                                    NativeFunction.CallByName<bool>("TASK_PERFORM_SEQUENCE", Cop.CopPed, lol);
                                    NativeFunction.CallByName<bool>("CLEAR_SEQUENCE_TASK", &lol);
                                }

                                TaskTime = Game.GameTime;

                                Cop.CopPed.KeepTasks = true;
                                LocalTaskName = "Arrest";
                                //WriteToLog("TaskChasing", "Cop SubTasked with Arresting");
                            }
                            else if (LocalTaskName != "GotoShooting" && InstantAction.CurrentPoliceState == InstantAction.PoliceState.UnarmedChase && _locrangeTo <= 7f)
                            {
                                Cop.CopPed.CanRagdoll = true;
                                NativeFunction.CallByName<bool>("TASK_GO_TO_ENTITY_WHILE_AIMING_AT_ENTITY", Cop.CopPed, Game.LocalPlayer.Character, Game.LocalPlayer.Character, 200f, true, 4.0f, 200f, false, false, (uint)FiringPattern.DelayFireByOneSecond);
                                Cop.CopPed.KeepTasks = true;
                                TaskTime = Game.GameTime;
                                LocalTaskName = "GotoShooting";
                            }
                            else if (LocalTaskName != "Goto" && (InstantAction.CurrentPoliceState == InstantAction.PoliceState.UnarmedChase || InstantAction.CurrentPoliceState == InstantAction.PoliceState.CautiousChase) && _locrangeTo >= 15) //was 15f
                            {
                                Cop.CopPed.CanRagdoll = true;
                                NativeFunction.CallByName<bool>("TASK_GO_TO_ENTITY", Cop.CopPed, Game.LocalPlayer.Character, -1, 5.0f, 500f, 1073741824, 1); //Original and works ok
                                Cop.CopPed.KeepTasks = true;
                                TaskTime = Game.GameTime;
                                LocalTaskName = "Goto";
                            }

                        }

                        if (LocalTaskName == "CarJack" && Cop.CopPed.IsInAnyVehicle(true))
                        {
                            WriteToLog("TaskChasing", "GET OUT DUMMY!");
                            Cop.CopPed.Tasks.Clear();
                            //Cop.CopPed.Tasks.LeaveVehicle(Cop.CopPed.CurrentVehicle, LeaveVehicleFlags.LeaveDoorOpen); // Get Out
                            LocalTaskName = "";
                        }

                        if ((InstantAction.areHandsUp || Game.LocalPlayer.Character.IsStunned) && !InstantAction.isBusted && _locrangeTo <= 4f)
                            InstantAction.SurrenderBust = true;

                        if (Cop.CanSpeak && rnd.Next(0, 100) <= 10)
                        {
                            Cop.GameTimeLastSpoke = Game.GameTime;
                            Cop.CopPed.PlayAmbientSpeech("CHALLENGE_THREATEN");
                            WriteToLog("TaskChasing", "Cop Spoke!");
                        }

                        if (Game.LocalPlayer.Character.IsInAnyVehicle(false) && (_locrangeTo >= 45f || Game.LocalPlayer.Character.CurrentVehicle.Speed >= 25))
                        {
                            GameFiber.Sleep(rnd.Next(900, 1500));//reaction time?
                            break;
                        }
                        Cop.CopPed.KeepTasks = true;
                        TaskTime = Game.GameTime;
                    }

                    GameFiber.Yield();
                    if (InstantAction.CurrentPoliceState == InstantAction.PoliceState.Normal || InstantAction.CurrentPoliceState == InstantAction.PoliceState.DeadlyChase)
                    {
                        GameFiber.Sleep(rnd.Next(900, 1500));//reaction time?
                        break;
                    }
                }
                if (Cop.CopPed.Exists() && !Cop.CopPed.IsDead)
                {
                    Cop.CopPed.BlockPermanentEvents = false;
                    Cop.CopPed.Tasks.Clear();
                }
                WriteToLog("Task Chasing", string.Format("Loop End: {0}", Cop.CopPed.Handle));
                Cop.TaskFiber = null;
                Cop.isTasked = false;
                if (Cop.CopPed.Exists() && !Cop.CopPed.IsDead)
                    Cop.CopPed.CanRagdoll = true;

            }, "Chase");
        }
        //public static void TaskArresting(GTACop Cop)
        //{
        //    if (Cop.CopPed.IsInRangeOf(Game.LocalPlayer.Character.Position, 100f) && Cop.TaskFiber != null && Cop.TaskFiber.Name == "Arrest" && !Cop.RecentlySeenPlayer())
        //    {
        //        return;
        //    }
        //    if (!Cop.CopPed.IsInRangeOf(Game.LocalPlayer.Character.Position, 100f) && Cop.TaskFiber != null)
        //    {
        //        Cop.CopPed.Tasks.Clear();
        //        Cop.CopPed.BlockPermanentEvents = false;
        //        Cop.TaskFiber.Abort();
        //        Cop.TaskFiber = null;
        //        WriteToLog("TaskArresting", "Cop Cleared");
        //    }

        //    Cop.TaskFiber =
        //    GameFiber.StartNew(delegate
        //    {
        //        WriteToLog("Task Arresting", string.Format("Started Arrest: {0}", Cop.CopPed.Handle));
        //        uint TaskTime = Game.GameTime;
        //        string LocalTaskName = "GoTo";
        //        NativeFunction.CallByName<bool>("SET_PED_PATH_CAN_USE_CLIMBOVERS", Cop.CopPed, true);
        //        NativeFunction.CallByName<bool>("SET_PED_PATH_CAN_USE_LADDERS", Cop.CopPed, true);
        //        NativeFunction.CallByName<bool>("SET_PED_PATH_CAN_DROP_FROM_HEIGHT", Cop.CopPed, true);
        //        Cop.CopPed.BlockPermanentEvents = true;

        //        while (Cop.CopPed.Exists() && !Cop.CopPed.IsDead)
        //        {
        //            Cop.CopPed.BlockPermanentEvents = true;
        //            if (Game.GameTime - TaskTime >= 500)
        //            {
        //                float _locrangeTo = Cop.CopPed.RangeTo(Game.LocalPlayer.Character.Position);
        //                if (LocalTaskName != "AimAt" && _locrangeTo <= 5f)
        //                {
        //                    Cop.CopPed.Tasks.AimWeaponAt(Game.LocalPlayer.Character, -1);
        //                    LocalTaskName = "AimAt";
        //                }
        //                else if (LocalTaskName != "GoToAiming" || _locrangeTo >= 5f)
        //                {
        //                    Cop.CopPed.Tasks.GoToWhileAiming(Game.LocalPlayer.Character, 4f, 20f);
        //                    LocalTaskName = "GoToAiming";
        //                }

        //                if ((InstantAction.areHandsUp || Game.LocalPlayer.Character.IsStunned) && !InstantAction.isBusted && _locrangeTo <= 4f) // THis was outside the task loop
        //                    InstantAction.SurrenderBust = true;

        //                Cop.CopPed.KeepTasks = true;
        //                TaskTime = Game.GameTime;
        //            }

        //            if (InstantAction.CurrentPoliceState == InstantAction.PoliceState.Normal || InstantAction.CurrentPoliceState == InstantAction.PoliceState.DeadlyChase || (InstantAction.CurrentPoliceState == InstantAction.PoliceState.UnarmedChase && !InstantAction.areHandsUp))
        //            {
        //                GameFiber.Sleep(rnd.Next(750, 1500));//reaction time?
        //                break;
        //            }

        //            GameFiber.Yield();
        //        }
        //        if (Cop.CopPed.Exists() && !Cop.CopPed.IsDead)
        //        {
        //            Cop.CopPed.BlockPermanentEvents = false;
        //            Cop.CopPed.Tasks.Clear();
        //        }
        //        WriteToLog("Task Arresting", string.Format("Loop End: {0}", Cop.CopPed.Handle));
        //        Cop.TaskFiber = null;
        //    }, "Arrest");
        //}
        public static void TaskSimpleChase(GTACop Cop)
        {
            Cop.CopPed.BlockPermanentEvents = true;
            Cop.SimpleTaskName = "SimpleChase";
            //Cop.CopPed.Tasks.AimWeaponAt(Game.LocalPlayer.Character.Position,-1);
            Cop.CopPed.Tasks.GoToWhileAiming(Game.LocalPlayer.Character, 10f, 40f);
            Cop.CopPed.KeepTasks = true;
            WriteToLog("TaskSimpleChase", "How many times i this getting called?");
        }
        public static void TaskSimpleArrest(GTACop Cop)
        {
            Cop.CopPed.BlockPermanentEvents = true;
            Cop.SimpleTaskName = "SimpleArrest";
            unsafe
            {
                int lol = 0;
                NativeFunction.CallByName<bool>("OPEN_SEQUENCE_TASK", &lol);
                NativeFunction.CallByName<bool>("TASK_GO_TO_ENTITY", 0, Game.LocalPlayer.Character, -1, 20f, 500f, 1073741824, 1); //Original and works ok
                NativeFunction.CallByName<bool>("TASK_GOTO_ENTITY_AIMING", 0, Game.LocalPlayer.Character, 4f, 20f);
                NativeFunction.CallByName<bool>("TASK_AIM_GUN_AT_ENTITY", 0, Game.LocalPlayer.Character, -1, false);
                NativeFunction.CallByName<bool>("SET_SEQUENCE_TO_REPEAT", lol, true);
                NativeFunction.CallByName<bool>("CLOSE_SEQUENCE_TASK", lol);
                NativeFunction.CallByName<bool>("TASK_PERFORM_SEQUENCE", Cop.CopPed, lol);
                NativeFunction.CallByName<bool>("CLEAR_SEQUENCE_TASK", &lol);
            }
            Cop.CopPed.KeepTasks = true;
            WriteToLog("TaskSimpleArrest", string.Format("Started SimpleArrest: {0}", Cop.CopPed.Handle));
        }
        public static void TaskK9(GTACop Cop)
        {

            Cop.TaskFiber =
            GameFiber.StartNew(delegate
            {
                WriteToLog("Task K9 Chasing", string.Format("Started Chase: {0}", Cop.CopPed.Handle));
                uint TaskTime = Game.GameTime;
                string LocalTaskName = "GoTo";
                //NativeFunction.CallByName<bool>("SET_PED_PATH_CAN_USE_CLIMBOVERS", Cop.CopPed, true);
                //NativeFunction.CallByName<bool>("SET_PED_PATH_CAN_USE_LADDERS", Cop.CopPed, true);
                //NativeFunction.CallByName<bool>("SET_PED_PATH_CAN_DROP_FROM_HEIGHT", Cop.CopPed, true);
                //NativeFunction.CallByName<bool>("TASK_GO_TO_ENTITY", Cop.CopPed, Game.LocalPlayer.Character, -1, 1.0f, 500f, 1073741824, 1); //Original and works ok

                Cop.CopPed.BlockPermanentEvents = true;
                while (Cop.CopPed.Exists() && !Cop.CopPed.IsDead && !Cop.CopPed.IsInRangeOf(Game.LocalPlayer.Character.Position, 65f) && Cop.CopPed.IsInAnyVehicle(false) && !Cop.CopPed.CurrentVehicle.IsSeatFree(-1))
                    GameFiber.Sleep(2000);


                WriteToLog("Task K9 Chasing", string.Format("Near Player Chase: {0}", Cop.CopPed.Handle));

                while (Cop.CopPed.Exists() && !Cop.CopPed.IsDead)
                {
                    if (Game.GameTime - TaskTime >= 1000)
                    {
                        float _locrangeTo = Cop.CopPed.RangeTo(Game.LocalPlayer.Character.Position);
                        if (_locrangeTo >= 65f)
                            break;

                        NativeFunction.CallByName<uint>("SET_PED_MOVE_RATE_OVERRIDE", Cop.CopPed, 1.3f);
                        if (LocalTaskName != "Exit" && Cop.CopPed.IsInAnyVehicle(false) && Cop.CopPed.CurrentVehicle.Speed <= 5 && !Cop.CopPed.CurrentVehicle.HasDriver && _locrangeTo <= 75f)
                        {
                            NativeFunction.CallByName<bool>("TASK_LEAVE_VEHICLE", Cop.CopPed, Cop.CopPed.CurrentVehicle, 16);
                            TaskTime = Game.GameTime;
                            LocalTaskName = "Exit";
                            WriteToLog("TaskK9Chasing", "Cop SubTasked with Exit");
                        }
                        else if (InstantAction.CurrentPoliceState == InstantAction.PoliceState.ArrestedWait && LocalTaskName != "Arrest")
                        {
                            NativeFunction.CallByName<bool>("TASK_GO_TO_ENTITY", Cop.CopPed, Game.LocalPlayer.Character, -1, 5.0f, 500f, 1073741824, 1); //Original and works ok
                            TaskTime = Game.GameTime;
                            LocalTaskName = "Arrest";
                            WriteToLog("TaskK9Chasing", "Cop SubTasked with Arresting");
                        }
                        else if ((InstantAction.CurrentPoliceState == InstantAction.PoliceState.UnarmedChase || InstantAction.CurrentPoliceState == InstantAction.PoliceState.CautiousChase || InstantAction.CurrentPoliceState == InstantAction.PoliceState.DeadlyChase) && LocalTaskName != "GotoFighting" && _locrangeTo <= 15f) //was 10f
                        {
                            NativeFunction.CallByName<bool>("TASK_COMBAT_PED", Cop.CopPed, Game.LocalPlayer.Character, 0, 16);
                            TaskTime = Game.GameTime;
                            LocalTaskName = "GotoFighting";
                            WriteToLog("TaskK9Chasing", "Cop SubTasked with Fighting");
                        }
                        else if ((InstantAction.CurrentPoliceState == InstantAction.PoliceState.UnarmedChase || InstantAction.CurrentPoliceState == InstantAction.PoliceState.CautiousChase || InstantAction.CurrentPoliceState == InstantAction.PoliceState.DeadlyChase) && LocalTaskName != "Goto" && _locrangeTo >= 50f) //was 15f
                        {
                            NativeFunction.CallByName<bool>("TASK_GO_TO_ENTITY", Cop.CopPed, Game.LocalPlayer.Character, -1, 5.0f, 500f, 1073741824, 1); //Original and works ok
                            TaskTime = Game.GameTime;
                            LocalTaskName = "Goto";
                            WriteToLog("TaskK9Chasing", "Cop SubTasked with GoTo");
                        }
                    }
                    GameFiber.Yield();
                    //if (InstantAction.CurrentPoliceState == InstantAction.PoliceState.Normal)
                    //    break;
                }
                WriteToLog("Task K9 Chasing", string.Format("Loop End: {0}", Cop.CopPed.Handle));
                Cop.TaskFiber = null;
                

                if (Cop.CopPed.Exists() && !Cop.CopPed.IsDead)
                {
                    Cop.CopPed.IsPersistent = false;
                    Cop.CopPed.BlockPermanentEvents = false;
                    if (!Cop.CopPed.IsInAnyVehicle(false))
                        Cop.CopPed.Tasks.ReactAndFlee(Game.LocalPlayer.Character);
                }

            }, "K9");
        }
     
        public static void UntaskAll()
        {
            foreach (GTACop Cop in CopPeds)
            {
                //if (Cop.TaskFiber != null && Cop.TaskFiber.IsAlive)
                //{
                //    if (Cop.CopPed.Exists())
                //    { 
                //        Cop.CopPed.Tasks.Clear();
                //        Cop.CopPed.BlockPermanentEvents = false;
                //    }
                if(Cop.isTasked)
                    AddItemToQueue(new PoliceTask(Cop, PoliceTask.Task.Untask));
                   
                //}
            }
            WriteToLog("UntaskAll", "");
        }
        public static void Untask(GTACop Cop)
        {
            if (Cop.CopPed.Exists())
            {
                if (Cop.TaskFiber != null)
                {
                    Cop.TaskFiber.Abort();
                    Cop.TaskFiber = null;
                }
                Cop.CopPed.Tasks.Clear();
                Cop.CopPed.BlockPermanentEvents = false;
                Cop.CopPed.IsPersistent = false;
                Cop.SimpleTaskName = "";
                Cop.isTasked = false;
                WriteToLog("Untask", string.Format("Untasked: {0}", Cop.CopPed.Handle));
            }
        }
        public static void TaskQueue()
        {
            GameFiber.StartNew(delegate
            {
                while(true)
                {
                    int _ToTask = CopsToTask.Count;
                    
                    if (_ToTask > 0)
                    {
                        WriteToLog("TaskQueue", string.Format("Cops To Task: {0}", _ToTask));
                        PoliceTask _policeTask = CopsToTask[0];
                        _policeTask.CopToAssign.isTasked = true;
                        if (_policeTask.TaskToAssign == PoliceTask.Task.Arrest)
                            TaskChasing(_policeTask.CopToAssign);
                        else if (_policeTask.TaskToAssign == PoliceTask.Task.Chase)
                            TaskChasing(_policeTask.CopToAssign);
                        else if (_policeTask.TaskToAssign == PoliceTask.Task.Untask)
                            Untask(_policeTask.CopToAssign);
                        else if (_policeTask.TaskToAssign == PoliceTask.Task.SimpleArrest)
                            TaskSimpleArrest(_policeTask.CopToAssign);
                        else if (_policeTask.TaskToAssign == PoliceTask.Task.SimpleChase)
                            TaskSimpleChase(_policeTask.CopToAssign);

                        _policeTask.CopToAssign.TaskIsQueued = false;
                        CopsToTask.RemoveAt(0);
                    }
                    GameFiber.Sleep(100);
                }
            });
        }
        private static void WriteToLog(String ProcedureString, String TextToLog)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ": " + ProcedureString + ": " + TextToLog + System.Environment.NewLine);
            File.AppendAllText("Plugins\\InstantAction\\" + "log.txt", sb.ToString());
            sb.Clear();
        }
    }
}
