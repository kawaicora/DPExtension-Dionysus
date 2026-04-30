using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using DynamicPatcher;
using Extension.EventSystems;

public static class CoraLogger
{


    // 函数地址
    private static readonly IntPtr DebugLogAddr = new IntPtr(0x4A4AC0); 

    /// <summary>
    /// 调用 Debug_Log，message 为 UTF8 字符串
    /// </summary>
    public unsafe static void Log(string message)
    {
        if (string.IsNullOrEmpty(message))
            message = "";

        byte[] utf8Bytes = Encoding.UTF8.GetBytes(message + "\0");
        fixed (byte* pMessage = utf8Bytes)
        {
            var func = (delegate* unmanaged[Cdecl]<IntPtr, void>)DebugLogAddr.ToPointer();
            func((IntPtr)pMessage);
        }
    }

  
    public static void Logln(string format,params object[] args)
    {
        Log(string.Format(format, args) + "\n");
    }


  
    public static void HookLogger()
    {
        // 获取Logger类型
        Type loggerType = typeof(DynamicPatcher.Logger);
        
        // 获取WriteLine属性
        PropertyInfo writeLineProp = loggerType.GetProperty("WriteLine", 
            BindingFlags.Public | BindingFlags.Static);
        
        if (writeLineProp != null)
        {
            // 创建自定义的委托
            DynamicPatcher.Logger.WriteLineDelegate WriteLineHandleDelegate = WriteLineHandle;
            
            // 设置属性值
            writeLineProp.SetValue(null, WriteLineHandleDelegate);
        }
    }
    
    public static void WriteLineHandle(string str)
    {
        Logln(str);
    }

    public static void LogEx(string format, params object[] args)
    {
        string str = string.Format(format, args);
        Logger.Log("################################################");
        Logger.Log(str);
        Logger.Log("################################################");
    }
   
  
}