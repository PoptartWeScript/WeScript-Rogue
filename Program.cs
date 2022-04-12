using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics;
using SharpDX.XInput;
using WeScriptWrapper;
using WeScript.SDK.UI;
using WeScript.SDK.UI.Components;
using System.Diagnostics;

namespace RogueCompany
{
    class Program
    {
        public static IntPtr processHandle = IntPtr.Zero;
        public static IntPtr wndHnd = IntPtr.Zero;
        public static IntPtr GWorldPtr = IntPtr.Zero;
        public static IntPtr GNamesPtr = IntPtr.Zero;
        public static IntPtr FNamePool = IntPtr.Zero;
        public static IntPtr ULocalPlayerControler = IntPtr.Zero;
        public static IntPtr bKickbackEnabled = IntPtr.Zero;
        public static IntPtr LTeamNum = IntPtr.Zero;
        public static IntPtr TeamNum = IntPtr.Zero;
        public static IntPtr LAKSTeamState = IntPtr.Zero;
        public static IntPtr GameBase = IntPtr.Zero;
        public static IntPtr GameSize = IntPtr.Zero;
        public static IntPtr FindWindow = IntPtr.Zero;
        public static IntPtr ULevel = IntPtr.Zero;
        public static IntPtr AActors = IntPtr.Zero;
        public static IntPtr AActor = IntPtr.Zero;
        public static IntPtr USceneComponent = IntPtr.Zero;
        public static IntPtr actor_pawn = IntPtr.Zero;
        public static IntPtr Playerstate = IntPtr.Zero;
        public static IntPtr AKSTeamState = IntPtr.Zero;
        public static IntPtr UplayerState = IntPtr.Zero; 
        public static IntPtr UGameInstance = IntPtr.Zero;
        public static IntPtr localPlayerArray = IntPtr.Zero;
        public static IntPtr ULocalPlayer = IntPtr.Zero;
        public static IntPtr Upawn = IntPtr.Zero; 
        public static IntPtr APlayerCameraManager = IntPtr.Zero;

        public static bool gameProcessExists = false; //avoid drawing if the game process is dead, or not existent
        public static bool isWow64Process = false; //we all know the game is 32bit, but anyway...
        public static bool isGameOnTop = false; //we should avoid drawing while the game is not set on top
        public static bool isOverlayOnTop = false; //we might allow drawing visuals, while the user is working with the "menu"
        public static bool IsDowned = false;
        public static bool bKickbackEnableds = false;

        public static uint PROCESS_ALL_ACCESS = 0x1FFFFF; //hardcoded access right to OpenProcess (even EAC strips some of the access flags)
        public static uint calcPid = 0x1FFFFF;
        public static uint Health = 0;
        public static uint EnemyID = 0;
        public static uint ActorCnt = 0; 
        public static uint AActorID = 0;

        public static int Offset_AKSTeamState = 0x398;
        public static int Offset_r_TeamNum = 0x220;
        public static int dist = 0;

        public static Vector2 wndMargins = new Vector2(0, 0); //if the game window is smaller than your desktop resolution, you should avoid drawing outside of it
        public static Vector2 wndSize = new Vector2(0, 0); //get the size of the game window ... to know where to draw       
        public static Vector2 GameCenterPos = new Vector2(0, 0);
        public static Vector2 GameCenterPos2 = new Vector2(0, 0);
        public static Vector2 AimTarg2D = new Vector2(0, 0); //for aimbot

        public static Vector3 AimTarg3D = new Vector3(0, 0, 0);
        public static Vector3 FMinimalViewInfo_Location = new Vector3(0, 0, 0);
        public static Vector3 FMinimalViewInfo_Rotation = new Vector3(0, 0, 0);
        public static Vector3 tempVec = new Vector3(0, 0, 0);

        public static float FMinimalViewInfo_FOV = 0;                
        public static float CurrentActorHP = 0;
        public static float CurrentActorHPMax = 0;
        
        public static Dictionary<UInt32, string> CachedID = new Dictionary<UInt32, string>();

        public static WeScript.SDK.UI.Menu RootMenu { get; private set; }
        public static WeScript.SDK.UI.Menu VisualsMenu { get; private set; }
        public static Menu AimbotMenu { get; private set; }
        class Components
        {
            public static readonly MenuKeyBind MainAssemblyToggle = new MenuKeyBind("mainassemblytoggle", "Toggle the whole assembly effect by pressing key:", VirtualKeyCode.Delete, KeybindType.Toggle, true);
            public static class VisualsComponent
            {
                public static readonly MenuBool DrawTheVisuals = new MenuBool("drawthevisuals", "Enable all of the Visuals", true);
                public static readonly MenuColor EnemyColor = new MenuColor("enecolor", "Enemy Color", new SharpDX.Color(0, 255, 0, 60));
                public static readonly MenuBool DrawBox = new MenuBool("box", "DrawBox", true);
                public static readonly MenuSlider DrawBoxThic = new MenuSlider("boxthickness", "Draw Box Thickness", 0, 0, 10);
                public static readonly MenuBool DrawBoxBorder = new MenuBool("drawboxborder", "Draw Border around Box and Text?", true);                
            }

            public static class AimbotComponent
            {
                public static readonly MenuBool AimGlobalBool = new MenuBool("enableaim", "Enable Aimbot Features", true);
                public static readonly MenuKeyBind AimKey = new MenuKeyBind("aimkey", "Aimbot HotKey (HOLD)", VirtualKeyCode.LeftMouse, KeybindType.Hold, false);
                public static readonly MenuSlider AimSpeed = new MenuSlider("aimspeed", "Aimbot Speed %", 12, 1, 100);
                public static readonly MenuSlider Distance = new MenuSlider("Distance", "Distance kill%", 12, 1, 100);
                public static readonly MenuSlider AimFov = new MenuSlider("aimfov", "Aimbot FOV", 100, 4, 1000);
                public static readonly MenuBool DrawFov = new MenuBool("DrawFOV", "Enable FOV Circle Features", true);
                public static readonly MenuColor AimFovColor = new MenuColor("aimfovcolor", "FOV Color", new SharpDX.Color(255, 255, 255, 30));
                public static readonly MenuBool NoRecoil = new MenuBool("noRecoil", "Enable No Recoil RISKY not tested if bannable", false);
            }
        }
        public static void InitializeMenu()
        {
            VisualsMenu = new WeScript.SDK.UI.Menu("visualsmenu", "Visuals Menu")
            {
                Components.VisualsComponent.DrawTheVisuals,
                Components.VisualsComponent.EnemyColor,
                Components.VisualsComponent.DrawBox,
                Components.VisualsComponent.DrawBoxThic.SetToolTip("Setting thickness to 0 will let the assembly auto-adjust itself depending on model distance"),
                Components.VisualsComponent.DrawBoxBorder.SetToolTip("Drawing borders may take extra performance (FPS) on low-end computers"),
            };

            AimbotMenu = new Menu("aimbotmenu", "Aimbot Menu")
            {
                Components.AimbotComponent.AimGlobalBool,
                Components.AimbotComponent.AimKey,
                Components.AimbotComponent.AimSpeed,
                Components.AimbotComponent.AimFov,
                Components.AimbotComponent.DrawFov,
                Components.AimbotComponent.AimFovColor,
                Components.AimbotComponent.NoRecoil,
            };
            RootMenu = new WeScript.SDK.UI.Menu("Rogue", "WeScript.app Rogue Company --Poptart--", true)
            {
                Components.MainAssemblyToggle.SetToolTip("The magical boolean which completely disables/enables the assembly!"),
                VisualsMenu,
                AimbotMenu,
            };
            RootMenu.Attach();
        }
        private static double GetDistance2D(Vector2 pos1, Vector2 pos2)
        {
            Vector2 vector = new Vector2(pos1.X - pos2.X, pos1.Y - pos2.Y);
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        }        
        private static string GetNameFromFName(uint key)
        {
            if (GNamesPtr == IntPtr.Zero)
                return "NULL";

            var chunkOffset = (uint)((int)(key) >> 16);
            var nameOffset = (ushort)key;
            ulong namePoolChunk = Memory.ZwReadUInt64(processHandle, (IntPtr)(GNamesPtr.ToInt64() + ((chunkOffset + 2) * 8)));
            ulong entryOffset = namePoolChunk + (ulong)(2 * nameOffset);
            short nameEntry = Memory.ZwReadInt16(processHandle, (IntPtr)entryOffset);
            int nameLength = nameEntry >> 6;
            string result = Memory.ZwReadString(processHandle, (IntPtr)entryOffset + 2, false);
            return result;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Rogue Company Assembly by Poptart");
            InitializeMenu();
            if (!Memory.InitDriver(DriverName.nsiproxy))
            {
                Console.WriteLine("[ERROR] Failed to initialize driver for some reason...");
            }
            Renderer.OnRenderer += OnRenderer;
            Memory.OnTick += OnTick;
        }
        public static double dims = 0.01905f;
        private static double GetDistance3D(Vector3 myPos, Vector3 enemyPos)
        {
            Vector3 vector = new Vector3(myPos.X - enemyPos.X, myPos.Y - enemyPos.Y, myPos.Z - enemyPos.Z);
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z) * dims;
        }
        private static void OnTick(int counter, EventArgs args)
        {
            
            if (processHandle == IntPtr.Zero)
            {
                wndHnd = Memory.FindWindowName("Rogue Company  ");
                if (wndHnd != IntPtr.Zero) //if it exists
                {
                    calcPid = Memory.GetPIDFromHWND(wndHnd);
                    if (calcPid > 0)
                    {
                        processHandle = Memory.ZwOpenProcess(PROCESS_ALL_ACCESS, calcPid);
                        if (processHandle != IntPtr.Zero)
                        {
                            isWow64Process = Memory.IsProcess64Bit(processHandle);
                        }
                        else
                        {
                            Console.WriteLine("failed to get handle");
                        }
                    }
                }
            }
            else
            {
                wndHnd = Memory.FindWindowName("Rogue Company  ");
                if (wndHnd != IntPtr.Zero)
                {
                    gameProcessExists = true;
                    wndMargins = Renderer.GetWindowMargins(wndHnd);
                    wndSize = Renderer.GetWindowSize(wndHnd);
                    isGameOnTop = Renderer.IsGameOnTop(wndHnd);
                    isOverlayOnTop = Overlay.IsOnTop();
                    if (GameBase == IntPtr.Zero)
                    {
                        GameBase = Memory.ZwGetModule(processHandle, null, isWow64Process);
                        Console.WriteLine("Got GAMEBASE of RogueCompany!");
                    }
                    else
                    {
                        if (GameSize == IntPtr.Zero)
                        {
                            GameSize = Memory.ZwGetModuleSize(processHandle, null, isWow64Process);
                        }
                        else
                        {
                            if (GWorldPtr == IntPtr.Zero)
                            {
                                GWorldPtr = Memory.ZwReadPointer(processHandle, GameBase + 0x6ADA478, isWow64Process);
                            }

                            if (GNamesPtr == IntPtr.Zero)
                            {
                                GNamesPtr = GameBase + 0x696B480;
                            }
                        }
                    }
                }
                else
                {
                    Memory.CloseHandle(processHandle);
                    processHandle = IntPtr.Zero;
                    gameProcessExists = false;
                    GameBase = IntPtr.Zero;
                    GameSize = IntPtr.Zero;
                    GWorldPtr = IntPtr.Zero;
                    GNamesPtr = IntPtr.Zero;
                }
            }
        }        
        private static void OnRenderer(int fps, EventArgs args)
        {
            if (!gameProcessExists) return;
            if ((!isGameOnTop) && (!isOverlayOnTop)) return;
            if (!Components.MainAssemblyToggle.Enabled) return;                      
            double fClosestPos = 999999;
            GameCenterPos = new Vector2(wndSize.X / 2 + wndMargins.X, wndSize.Y / 2 + wndMargins.Y);
            GameCenterPos2 = new Vector2(wndSize.X / 2 + wndMargins.X, wndSize.Y / 2 + wndMargins.Y + 750.0f);

            if (GWorldPtr != IntPtr.Zero)
            {
                Functions.Ppc();
                ULevel = Memory.ZwReadPointer(processHandle, GWorldPtr + 0x30, isWow64Process);
                if (GWorldPtr != IntPtr.Zero)
                {
                    AActors = Memory.ZwReadPointer(processHandle, (IntPtr)(ULevel.ToInt64() + Offsets.UE.ULevel.AActors), isWow64Process);
                    ActorCnt = Memory.ZwReadUInt32(processHandle, (IntPtr)(ULevel.ToInt64() + Offsets.UE.ULevel.AActorsCount));

                    if ((AActors != IntPtr.Zero) && (ActorCnt > 0))
                    {
                        for (uint i = 0; i <= ActorCnt; i++)
                        {
                            AActor = Memory.ZwReadPointer(processHandle, (IntPtr)(AActors.ToInt64() + i * 8),isWow64Process);
                            if (AActor != IntPtr.Zero)
                            {                                                                   
                                USceneComponent = Memory.ZwReadPointer(processHandle,(IntPtr)(AActor.ToInt64() + Offsets.UE.AActor.USceneComponent), isWow64Process);
                                if (USceneComponent != IntPtr.Zero)
                                {
                                    tempVec = Memory.ZwReadVector3(processHandle,(IntPtr)(USceneComponent.ToInt64() + Offsets.UE.AActor.tempVec));
                                    AActorID = Memory.ZwReadUInt32(processHandle,(IntPtr)AActor.ToInt64() + 0x18);
                                    if (!CachedID.ContainsKey(AActorID))
                                    {
                                        var retname = GetNameFromFName(AActorID);
                                        CachedID.Add(AActorID, retname);
                                    }

                                    CurrentActorHP = Memory.ZwReadFloat(processHandle, (IntPtr)(AActor.ToInt64() + Offsets.UE.AActor.Health));
                                    CurrentActorHPMax = Memory.ZwReadFloat(processHandle, (IntPtr)(AActor.ToInt64() + Offsets.UE.AActor.CurrentActorHPMax));

                                    if ((AActorID > 0))
                                    {
                                        var retname = CachedID[AActorID];
                                        retname = GetNameFromFName(AActorID);
                                        if (retname.Contains("MainCharacter_C") || retname.Contains("DefaultPVPBotCharacter_C") || retname.Contains("DefaultBotCharacter_C")) EnemyID = AActorID;

                                        //Team Info////////////////////////////////////////////////////////////////////////////////////////////////////////
                                        actor_pawn = Memory.ZwReadPointer(processHandle, (IntPtr)(AActor.ToInt64() + Offsets.UE.AActor.actor_pawn), isWow64Process);
                                        Playerstate = Memory.ZwReadPointer(processHandle, (IntPtr)(actor_pawn.ToInt64() + Offsets.UE.APawn.PlayerState), isWow64Process);
                                        bKickbackEnableds = Memory.ZwReadBool(processHandle, (IntPtr)(ULocalPlayerControler.ToInt64() + Offsets.UE.AActor.bKickbackEnableds));
                                        LAKSTeamState = Memory.ZwReadPointer(processHandle, (IntPtr)(UplayerState.ToInt64() + Offset_AKSTeamState), isWow64Process);
                                        LTeamNum = Memory.ZwReadPointer(processHandle, (IntPtr)(LAKSTeamState.ToInt64() + Offset_r_TeamNum), isWow64Process);
                                        AKSTeamState = Memory.ZwReadPointer(processHandle, (IntPtr)(Playerstate.ToInt64() + Offset_AKSTeamState), isWow64Process);
                                        TeamNum = Memory.ZwReadPointer(processHandle, (IntPtr)(AKSTeamState.ToInt64() + Offset_r_TeamNum), isWow64Process);
                                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                                        if (Components.AimbotComponent.NoRecoil.Enabled)
                                        {
                                            bKickbackEnableds = Memory.ZwWriteBool(processHandle, (IntPtr)(ULocalPlayerControler.ToInt64() + Offsets.UE.AActor.bKickbackEnableds), true);
                                        }
                                        else
                                        {
                                            bKickbackEnableds = Memory.ZwWriteBool(processHandle, (IntPtr)(ULocalPlayerControler.ToInt64() + Offsets.UE.AActor.bKickbackEnableds), false);
                                        }

                                    }
                                    if (Components.VisualsComponent.DrawTheVisuals.Enabled)
                                    {
                                        dist = (int)(GetDistance3D(FMinimalViewInfo_Location, tempVec));

                                        if (AActorID == EnemyID && CurrentActorHP > 0)
                                        {
                                            Vector2 vScreen_h3ad = new Vector2(0, 0);
                                            Vector2 vScreen_f33t = new Vector2(0, 0);
                                            if (Renderer.WorldToScreenUE4(new Vector3(tempVec.X, tempVec.Y, tempVec.Z + 70.0f), out vScreen_h3ad, FMinimalViewInfo_Location, FMinimalViewInfo_Rotation, FMinimalViewInfo_FOV, wndMargins, wndSize))
                                            {

                                                Renderer.WorldToScreenUE4(new Vector3(tempVec.X, tempVec.Y, tempVec.Z - 70.0f), out vScreen_f33t, FMinimalViewInfo_Location, FMinimalViewInfo_Rotation, FMinimalViewInfo_FOV, wndMargins, wndSize);
                                                if (Components.VisualsComponent.DrawBox.Enabled)
                                                {
                                                    if(LTeamNum != TeamNum)
                                                    Renderer.DrawFPSBox(vScreen_h3ad, vScreen_f33t, Components.VisualsComponent.EnemyColor.Color, BoxStance.standing, Components.VisualsComponent.DrawBoxThic.Value, Components.VisualsComponent.DrawBoxBorder.Enabled, true, CurrentActorHP, CurrentActorHPMax);
                                                    Renderer.DrawText("[" + dist + "m]", vScreen_f33t.X, vScreen_f33t.Y, Components.VisualsComponent.EnemyColor.Color, 12, TextAlignment.centered, false);
                                                }
                                                else
                                                {                        
                                                    Renderer.DrawFPSBox(vScreen_h3ad, vScreen_f33t, Color.Blue, BoxStance.standing, Components.VisualsComponent.DrawBoxThic.Value, Components.VisualsComponent.DrawBoxBorder.Enabled);
                                                    Renderer.DrawText("[" + dist + "m]", vScreen_f33t.X, vScreen_f33t.Y, Color.Black, 12, TextAlignment.centered, false);                                                   
                                                }                                                   
                                            }

                                            if (Components.AimbotComponent.DrawFov.Enabled) //draw fov circle
                                            {
                                                Renderer.DrawCircle(GameCenterPos, Components.AimbotComponent.AimFov.Value, Components.AimbotComponent.AimFovColor.Color);
                                            }
                                            var AimDist2D = GetDistance2D(vScreen_h3ad, GameCenterPos);
                                            if (Components.AimbotComponent.AimFov.Value < AimDist2D) continue;

                                            if (AimDist2D < fClosestPos)
                                            {
                                                fClosestPos = AimDist2D;
                                                AimTarg2D = vScreen_h3ad;


                                                if (Components.AimbotComponent.AimKey.Enabled && Components.AimbotComponent.AimGlobalBool.Enabled && dist > 5 && LTeamNum != TeamNum )
                                                {                                                    
                                                    double DistX = AimTarg2D.X - GameCenterPos.X;
                                                    double DistY = (AimTarg2D.Y) - GameCenterPos.Y;
                                                    double slowDistX = DistX / (0.5f + (Math.Abs(DistX) / (1.0f + Components.AimbotComponent.AimSpeed.Value)));
                                                    double slowDistY = DistY / (0.5f + (Math.Abs(DistY) / (1.0f + Components.AimbotComponent.AimSpeed.Value)));
                                                    Input.mouse_eventWS(MouseEventFlags.MOVE, (int)slowDistX, (int)slowDistY, MouseEventDataXButtons.NONE, IntPtr.Zero);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
