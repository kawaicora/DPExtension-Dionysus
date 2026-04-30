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
using System.Runtime.InteropServices;

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
            MathEx.SetRandomSeed(0);
            Logger.Log("set random seed!");
        }

        [Hook(HookType.AresHook, Address = 0x52BA60, Size = 5)]
        public static unsafe UInt32 YR_Boot(REGISTERS* R)
        {
            Logger.Log("YR Booted!");
            try
            {
                // MemoryHelper.Write(0x4068E0, new byte[] { 0xE9, 0xDB, 0xE1, 0x09, 0x00 ,0x90,0x90,0x90,0x90},9); // 跳转到真实日志方法
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
        
        [Hook(HookType.AresHook, Address = 0x4068E0, Size = 0x0)]
        static public unsafe UInt32 Debug_Log(REGISTERS* R)
        {   
            return 0x4A4AC0;  //跳转到日志
        }

        // [Hook(HookType.AresHook, Address = 0x4A4AC0, Size = 0x1)]   // 所有日志出口包括自己的
        // static public unsafe UInt32 Debug_Log2(REGISTERS* R)
        // {
          
        //     // 模拟printf格式化
        //     // unsafe string FormatPrintfMessage(string format, REGISTERS* R)
        //     // {
        //     //     System.Text.StringBuilder result = new System.Text.StringBuilder();
        //     //     int argIndex = 0;
        //     //     int stackOffset = 8; // ESP+8 是第一个参数
                
        //     //     for (int i = 0; i < format.Length; i++)
        //     //     {
        //     //         if (format[i] == '%')
        //     //         {
        //     //             if (i + 1 < format.Length)
        //     //             {
        //     //                 if (format[i + 1] == '%')
        //     //                 {
        //     //                     // 转义的%
        //     //                     result.Append('%');
        //     //                     i++;
        //     //                     continue;
        //     //                 }
                            
        //     //                 // 解析格式说明符
        //     //                 i++;
        //     //                 char specifier = format[i];
                            
        //     //                 // 读取参数值
        //     //                 uint argValue = 0;
        //     //                 try
        //     //                 {
        //     //                     argValue = R->Stack32(stackOffset);
        //     //                     stackOffset += 4; // 每个参数4字节
        //     //                 }
        //     //                 catch
        //     //                 {
        //     //                     return result.ToString() + "[参数读取错误]";
        //     //                 }
                            
        //     //                 // 根据说明符格式化
        //     //                 switch (specifier)
        //     //                 {
        //     //                     case 'd':
        //     //                     case 'i':
        //     //                         result.Append((int)argValue);
        //     //                         break;
        //     //                     case 'u':
        //     //                         result.Append(argValue);
        //     //                         break;
        //     //                     case 'x':
        //     //                         result.Append($"{argValue:x}");
        //     //                         break;
        //     //                     case 'X':
        //     //                         result.Append($"{argValue:X}");
        //     //                         break;
        //     //                     case 's':
        //     //                         // 字符串指针
        //     //                         if (argValue != 0)
        //     //                         {
        //     //                             try
        //     //                             {
        //     //                                 string str = Marshal.PtrToStringAnsi(new IntPtr(argValue));
        //     //                                 result.Append(str ?? "[空字符串]");
        //     //                             }
        //     //                             catch
        //     //                             {
        //     //                                 result.Append($"[字符串:0x{argValue:X}]");
        //     //                             }
        //     //                         }
        //     //                         else
        //     //                         {
        //     //                             result.Append("[空指针]");
        //     //                         }
        //     //                         break;
        //     //                     case 'c':
        //     //                         result.Append((char)(argValue & 0xFF));
        //     //                         break;
        //     //                     case 'p':
        //     //                         result.Append($"0x{argValue:X8}");
        //     //                         break;
        //     //                     case 'f':
        //     //                         // 浮点数
        //     //                         float floatValue = *(float*)&argValue;
        //     //                         result.Append(floatValue.ToString("F6"));
        //     //                         break;
        //     //                     default:
        //     //                         result.Append($"[未知格式:{specifier}]");
        //     //                         break;
        //     //                 }
                            
        //     //                 argIndex++;
        //     //             }
        //     //         }
        //     //         else
        //     //         {
        //     //             result.Append(format[i]);
        //     //         }
        //     //     }
                
        //     //     return result.ToString();
        //     // }
        //     // uint formatPtr = R->Stack32(4);
        //     // try
        //     // {
        //     //     if (formatPtr != 0)
        //     //     {
        //     //         string format = Marshal.PtrToStringAnsi(new IntPtr(formatPtr));
                    
        //     //         if (!string.IsNullOrEmpty(format))
        //     //         {
        //     //             // 直接模拟printf格式化
        //     //             string formatted = FormatPrintfMessage(format, R);
        //     //             Console.Write($"[Debug_Log] {formatted}");
        //     //         }
        //     //     }
        //     // }
        //     // catch (Exception ex)
        //     // {
        //     //     _ = ex;  //不要输出否则异常
        //     // }
        //     return 0x4A4AF9;
        // }
    }
}
