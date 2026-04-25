using System.Runtime.CompilerServices;
using DynamicPatcher;
using Extension.Ext;

namespace ExtensionHooks
{


    public static class NetworkBehaviors
    {
        [Hook(HookType.AresHook, Address = 0x4C6CB0, Size = 6)]
        // [UnmanagedCallersOnly(EntryPoint = "Network_RespondToEvent_Behaviors", CallConvs = [typeof(CallConvCdecl)])]
        public static unsafe uint Network_RespondToEvent_Behaviors(REGISTERS* R)
        {
            return Network.Network_RespondToEvent_Behaviors(R);
        }


        [Hook(HookType.AresHook, Address = 0x64BE7D, Size = 0x6)]
        // [UnmanagedCallersOnly(EntryPoint = "Network_GetEventSize1_Behaviors", CallConvs = [typeof(CallConvCdecl)])]
        public static unsafe uint Network_GetEventSize1_Behaviors(REGISTERS* R)
        {
            return Network.Network_GetEventSize1_Behaviors(R);
        }


        [Hook(HookType.AresHook, Address = 0x64C30E, Size = 0x6)]
        // [UnmanagedCallersOnly(EntryPoint = "Network_GetEventSize2_Behaviors", CallConvs = [typeof(CallConvCdecl)])]
        public static unsafe uint Network_GetEventSize2_Behaviors(REGISTERS* R)
        {
            return Network.Network_GetEventSize2_Behaviors(R);
        }


       
        [Hook(HookType.AresHook, Address = 0x64B6FE, Size = 0x6)]
        // [UnmanagedCallersOnly(EntryPoint = "Network_GetEventSize3_Behaviors", CallConvs = [typeof(CallConvCdecl)])]
        public static unsafe uint Network_GetEventSize3_Behaviors(REGISTERS* R)
        {
            return Network.Network_GetEventSize3_Behaviors(R);
        }

    }

}