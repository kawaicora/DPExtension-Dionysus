using DynamicPatcher;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Components;
using Extension.EventSystems;
using PatcherYRpp;
using Extension.Ext;
using Extension.CoraExtension;

namespace GeneralHooks
{
    public class General
    {
        static General()
        {
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioStartEvent, MathExHandler);
        }

        private static void MathExHandler(object sender, EventArgs e)
        {
            // ensure network synchronization
            MathEx.SetRandomSeed(new Random().Next(3200000,6400000));
            Logger.Log("set random seed!");
        }

        [Hook(HookType.AresHook, Address = 0x52BA60, Size = 5)]
        public static unsafe UInt32 YR_Boot(REGISTERS* R)
        {
            Logger.Log("YR Booted!");
            try
            {
                MemoryHelper.Write(0x4068E0, new byte[] { 0xE9, 0xDB, 0xE1, 0x09, 0x00 ,0x90,0x90,0x90,0x90},9); // 跳转到真实日志方法
                var _ = MainTools.Instance;
            }catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
            
            return 0;
        }

        // in progress: Initializing Tactical display
        [Hook(HookType.AresHook, Address = 0x6875F3, Size = 6)]
        public static unsafe UInt32 Scenario_Start1(REGISTERS* R)
        {
            EventSystem.General.Broadcast(EventSystem.General.ScenarioStartEvent, EventArgs.Empty);

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x55AFB3, Size = 6)]
        public static unsafe UInt32 LogicClass_Update(REGISTERS* R)
        {
            EventSystem.General.Broadcast(EventSystem.General.LogicClassUpdateEvent, new LogicClassUpdateEventArgs(true));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x55B719, Size = 5)]
        public static unsafe UInt32 LogicClass_Update_Late(REGISTERS* R)
        {
            EventSystem.General.Broadcast(EventSystem.General.LogicClassUpdateEvent, new LogicClassUpdateEventArgs(false));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x685659, Size = 0xA)]
        public static unsafe UInt32 Scenario_ClearClasses(REGISTERS* R)
        {
            EventSystem.General.Broadcast(EventSystem.General.ScenarioClearClassesEvent, EventArgs.Empty);
            
            return 0;
        }

          [Hook(HookType.AresHook, Address = 0x685582, Size = 0x6)]
            public static unsafe UInt32 Logic_Init(REGISTERS* R)
            {
                EventSystem.General.Broadcast(EventSystem.General.LogicInitEvent, EventArgs.Empty);
                
                return 0;
            }

    

        [Hook(HookType.AresHook, Address = 0x7CD8EF, Size = 9)]
        public static unsafe UInt32 ExeTerminate(REGISTERS* R)
        {
            Logger.Log("YR Terminated!");
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x7b3d6f, Size = 0x6)]
        static public unsafe UInt32 TunnelSendTo(REGISTERS* R)
        {
            return 0;
        }



        [Hook(HookType.AresHook, Address = 0x7b3f15, Size = 0x6)]
        static public unsafe UInt32 TunnelRecvFrom(REGISTERS* R)
        {
            return 0;
        }

        
        
        [Hook(HookType.AresHook, Address = 0x64CCBF, Size = 0x6)]
        static public unsafe UInt32 DoList_ReplaceReconMessage(REGISTERS* R)
        {
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x643C62, Size = 0x6)]
        
        static public unsafe UInt32 UpdateLoadProgress(REGISTERS* R)
        {
            return 0;
        }
       

        [Hook(HookType.AresHook, Address = 0x6931F1, Size = 0x8)]
        static public unsafe UInt32 LMouseDown(REGISTERS* R)
        {
            return GeneralExt.LMouseDown(R);
        }

        [Hook(HookType.AresHook, Address = 0x693296, Size = 0x8)]
        static public unsafe UInt32 LMouseUp(REGISTERS* R)
        {
            return GeneralExt.LMouseUp(R);
        }

        [Hook(HookType.AresHook, Address = 0x69335E, Size = 0x8)]
        static public unsafe UInt32 RMouseDown(REGISTERS* R)
        {
            return GeneralExt.RMouseDown(R);
        }
        [Hook(HookType.AresHook, Address = 0x6933DE, Size = 0x8)]
        static public unsafe UInt32 RMouseUp(REGISTERS* R)
        {
            return GeneralExt.RMouseUp(R);
        }
        
        
    }
}
