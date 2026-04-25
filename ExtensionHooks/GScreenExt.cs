using System;
using DynamicPatcher;
using Extension.Ext;
using PatcherYRpp;
namespace ExtensionHooks
{    
    class GScreenExtHooks
    {
        [Hook(HookType.AresHook, Address = 0x4F4583, Size = 0x6)]
        static public unsafe UInt32 GScreenClass_DrawOnTop_TheDarkSideOfTheMoon(REGISTERS* R)
        {
            return GScreenExt.DrawOnTop_TheDarkSideOfTheMoon(R);
        }

       
    }
}