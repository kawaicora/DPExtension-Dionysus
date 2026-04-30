using System;
using DynamicPatcher;
using PatcherYRpp;
namespace ExtensionHooks
{
    public class FactoryExtHooks
    {

        [Hook(HookType.AresHook, Address = 0x4C9B20, Size = 0x5)]  
        public static unsafe UInt32 FactoryClass_ProgressUpdate_Hook(REGISTERS* R)
        {
            return FactoryClass.FactoryClass_ProgressUpdate_Hook(R);
        }
    

        [Hook(HookType.AresHook, Address = 0x4C9DF6, Size = 0x6)] 
        public static unsafe UInt32 FactoryClass_Create_Hook(REGISTERS* R)
        {
            return FactoryClass.FactoryClass_Create_Hook(R);
        }
    }
}