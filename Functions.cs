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
                Program.UGameInstance = Memory.ZwReadPointer(Program.processHandle, (IntPtr)(Program.GWorldPtr.ToInt64() + Offsets.UE.UWorld.OwningGameInstance), Program.isWow64Process);
                if (Program.UGameInstance != IntPtr.Zero)
                {
                    Program.localPlayerArray = Memory.ZwReadPointer(Program.processHandle, (IntPtr)(Program.UGameInstance.ToInt64() + Offsets.UE.UGameInstance.LocalPlayers), Program.isWow64Process);
                    if (Program.localPlayerArray != IntPtr.Zero)
                    {
                        Program.ULocalPlayer = Memory.ZwReadPointer(Program.processHandle, Program.localPlayerArray, Program.isWow64Process);
                        if (Program.ULocalPlayer != IntPtr.Zero)
                        {
                            Program.ULocalPlayerControler = Memory.ZwReadPointer(Program.processHandle, (IntPtr)(Program.ULocalPlayer.ToInt64() + Offsets.UE.UPlayer.PlayerController), Program.isWow64Process);

                            if (Program.ULocalPlayerControler != IntPtr.Zero)
                            {
                                Program.Upawn = Memory.ZwReadPointer(Program.processHandle, (IntPtr)(Program.ULocalPlayerControler.ToInt64() + Offsets.UE.APlayerController.AcknowledgedPawn), Program.isWow64Process);
                                Program.UplayerState = Memory.ZwReadPointer(Program.processHandle, (IntPtr)(Program.Upawn.ToInt64() + Offsets.UE.APawn.PlayerState), Program.isWow64Process);
                                Program.APlayerCameraManager = Memory.ZwReadPointer(Program.processHandle, (IntPtr)(Program.ULocalPlayerControler.ToInt64() + Offsets.UE.APlayerController.PlayerCameraManager), Program.isWow64Process);
                                if (Program.APlayerCameraManager != IntPtr.Zero)
                                {
                                    Program.FMinimalViewInfo_Location = Memory.ZwReadVector3(Program.processHandle,(IntPtr)(Program.APlayerCameraManager.ToInt64() + Offsets.UE.APlayerCameraManager.CameraCachePrivate) + 0x0000);
                                    Program.FMinimalViewInfo_Rotation = Memory.ZwReadVector3(Program.processHandle,(IntPtr)(Program.APlayerCameraManager.ToInt64() + Offsets.UE.APlayerCameraManager.CameraCachePrivate) + 0x000C);
                                    float FMinimalViewInfo_FOV2 = Memory.ZwReadFloat(Program.processHandle,(IntPtr)(Program.APlayerCameraManager.ToInt64() + Offsets.UE.APlayerCameraManager.CameraCachePrivate) + 0x0018);
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
