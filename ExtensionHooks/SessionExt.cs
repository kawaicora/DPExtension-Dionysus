using System;
using DynamicPatcher;
using Extension.Components;
using Extension.Ext;
using PatcherYRpp;

namespace ExtensionHooks
{
    public class SessionExtHook {
        
        
        [Hook(HookType.AresHook, Address = 0x69BAB0, Size = 6)]
        static public unsafe UInt32 Resume(REGISTERS* R)  //对局开始的时候会调用这个函数
        {

            return SessionExt.Resume(R);
        }

        [Hook(HookType.AresHook, Address = 0x55de4f, Size = 5)]
        static public unsafe UInt32 GameLoop2(REGISTERS* R)  
        {

            return 0;
        }


        [Hook(HookType.AresHook, Address = 0x72dfb0, Size = 0x6)]
        static public unsafe UInt32 ExitGameLoop(REGISTERS* R)
        {
            return SessionExt.ExitGameLoop(R);
        }

        [Hook(HookType.AresHook, Address = 0x55D360, Size = 0xB)]  //建造 等 在里面
        static public unsafe UInt32 GameLoop(REGISTERS* R)
        {
            return 0;
        }


  


    }
}