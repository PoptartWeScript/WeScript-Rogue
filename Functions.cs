using System;
using WeScriptWrapper;


namespace RogueCompany
{
    public class Functions
    {
        public static float Rad2Deg(float rad)
        {
            return (float)(rad * 180.0f / Math.PI);
        }

        public static float Deg2Rad(float deg)
        {
            return (float)(deg * Math.PI / 180.0f);
        }

        public static float atanf(float X)
        {
            return (float)Math.Atan(X);
        }

        public static float tanf(float X)
        {
            return (float)Math.Tan(X);
        }

        public static void Ppc()
        {

            
            if (Program.GWorldPtr != IntPtr.Zero)
            {                
                Program.UGameInstance = Memory.ZwReadPointer(Program.processHandle, (IntPtr)(Program.GWorldPtr.ToInt64() + Offsets.UE.UWorld.OwningGameInstance), true);
                if (Program.UGameInstance != IntPtr.Zero)
                {
                    Program.localPlayerArray = Memory.ZwReadPointer(Program.processHandle, (IntPtr)(Program.UGameInstance.ToInt64() + Offsets.UE.UGameInstance.LocalPlayers), true);
                    if (Program.localPlayerArray != IntPtr.Zero)
                    {
                        Program.ULocalPlayer = Memory.ZwReadPointer(Program.processHandle, Program.localPlayerArray, true);
                        if (Program.ULocalPlayer != IntPtr.Zero)
                        {
                            Program.ULocalPlayerControler = Memory.ZwReadPointer(Program.processHandle, (IntPtr)(Program.ULocalPlayer.ToInt64() + Offsets.UE.UPlayer.PlayerController), true);

                            if (Program.ULocalPlayerControler != IntPtr.Zero)
                            {
                                Program.Upawn = Memory.ZwReadPointer(Program.processHandle, (IntPtr)(Program.ULocalPlayerControler.ToInt64() + Offsets.UE.APlayerController.AcknowledgedPawn), true);
                                Program.UplayerState = Memory.ZwReadPointer(Program.processHandle, (IntPtr)(Program.Upawn.ToInt64() + Offsets.UE.APawn.PlayerState), true);
                                Program.APlayerCameraManager = Memory.ZwReadPointer(Program.processHandle, (IntPtr)Program.ULocalPlayerControler.ToInt64() + 0x2B8, true);
                                if (Program.APlayerCameraManager != IntPtr.Zero)
                                {
                                    Program.FMinimalViewInfo_Location = Memory.ZwReadVector3(Program.processHandle,
                                        (IntPtr)Program.APlayerCameraManager.ToInt64() + 0x1AB0 + 0x0000);
                                    Program.FMinimalViewInfo_Rotation = Memory.ZwReadVector3(Program.processHandle,
                                        (IntPtr)Program.APlayerCameraManager.ToInt64() + 0x1AB0 + 0x000C);
                                    float FMinimalViewInfo_FOV2 = Memory.ZwReadFloat(Program.processHandle,
                                        (IntPtr)Program.APlayerCameraManager.ToInt64() + 0x1AB0 + 0x0018);
                                    float RadFOV = (Program.wndSize.Y * 0.5f) / tanf(Deg2Rad(FMinimalViewInfo_FOV2 * 0.5f));
                                    Program.FMinimalViewInfo_FOV = (float)(2 * Rad2Deg(atanf(Program.wndSize.X * 0.5f / RadFOV)));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
