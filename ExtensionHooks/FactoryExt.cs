using System;
using DynamicPatcher;
using PatcherYRpp;
namespace ExtensionHooks
{
    public class FactoryExtHooks
    {

        [Hook(HookType.AresHook, Address = 0x4C9B85, Size = 0x7)]  //要在  mov     [esi+24h], edx 之后
        public static unsafe UInt32 FactoryClass_ProgressAdd_Hook(REGISTERS* R)
        {
            return FactoryClass.FactoryClass_ProgressAdd_Hook(R);
        }


        [Hook(HookType.AresHook, Address = 0x4C9DF6, Size = 0x6)] 
        public static unsafe UInt32 FactoryClass_Create_Hook(REGISTERS* R)
        {
            return FactoryClass.FactoryClass_Create_Hook(R);
        }
    }
}