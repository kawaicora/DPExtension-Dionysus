using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Drawing;

using System.Drawing.Imaging;
using System.Drawing.Text;

using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DynamicPatcher;
using Extension.Components;
using Extension.Coroutines;
using Extension.Ext;
using Microsoft.CSharp;
using PatcherYRpp;


namespace Extension.CoraExtension {
    public class CoraUtils
    {
        private static string sMainToolsConfigFilename = "__main_tools.ini";
        private static Dictionary<string,INIComponent> uMainToolsConfigs = new(); 
        /// <summary>
        /// 获取配置单例
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public static INIComponent MainToolsConfig(string section)
        {
            INIComponent ini;
            if (uMainToolsConfigs.TryGetValue(section,out ini))
            {
                return ini;
            }
            else
            {
                ini = new INIComponent(sMainToolsConfigFilename,section);
                uMainToolsConfigs.Add(section,ini);
                return ini;
            }
        }
        private static int hotkeyId = 1000;
        

        // 静态解析方法
        public static void ParseHotkeyConfig(Dictionary<string,string> keyValues)
        {   
            hotkeyId = 1000;
            foreach (string item in keyValues.Keys)
            {
                hotkeyId+=1;
                if(String.IsNullOrEmpty(item)) continue;

                string keyPart = item;
                string methodPart = keyValues[item];

                // 2. 解析 KEY(...) 里面所有 MOD_* 和 VK_*
                var match = Regex.Match(keyPart, @"KEY\((.+?)\)");
                string[] tokens = match.Groups[1].Value.Split(',');

                int modifiers = 0;
                int vk = 0;
                foreach (string t in tokens)
                {
                    string token = t.Trim();

                    if (token.StartsWith("MOD_"))
                    {
                        
                        
                        var field = MOD.GetKey(token);
                        modifiers += field;
                    }
                    else if (token.StartsWith("VK_"))
                    {
                        var field = VK.GetKey(token);
                        vk += field;
                    }
                }

                // ===================== 核心：解析静态方法（支持字符串、int、bool、double） =====================
                Action execute = () =>
                {
                    try
                    {
                        // 匹配：类.方法(参数)
                        var reg = Regex.Match(methodPart, @"^([\w\.]+)\(?([^)]*)\)?$");
                        string fullName = reg.Groups[1].Value;
                        string paramStr = reg.Groups[2].Value.Trim();

                        // 拆分 类名 + 方法名
                        int dot = fullName.LastIndexOf('.');
                        string className = fullName.Substring(0, dot);
                        string methodName = fullName.Substring(dot + 1);

                        // 找到类型
                        Type type = Type.GetType(className)
                                ?? Assembly.GetExecutingAssembly().GetType(className);

                        if (type == null) return;

                        // ===================== 自动解析参数（支持字符串、int、bool、double） =====================
                        object[] args = null;
                        if (!string.IsNullOrWhiteSpace(paramStr))
                        {
                            string raw = paramStr;

                            // 字符串：去掉引号
                            if (raw.StartsWith("\"") && raw.EndsWith("\""))
                            {
                                args = new object[] { raw.Substring(1, raw.Length - 2) };
                            }
                            // 数字 / bool / 其他
                            else
                            {
                                var method = type.GetMethod(methodName);
                                if (method == null || method.GetParameters().Length == 0) return;

                                Type paramType = method.GetParameters()[0].ParameterType;
                                object val = Convert.ChangeType(raw, paramType);
                                args = new object[] { val };
                            }
                        }

                        // 【安全调用】自动匹配方法，不会空指针
                        MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
                        methodInfo?.Invoke(null, args);
                    }
                    catch(Exception ex)
                    {
                        Logger.Log($"出现错误: id = {hotkeyId} func = {methodPart}");
                        Logger.PrintException(ex);
                    }
                };
                Logger.Log($"AddHotKeyEvent id = {hotkeyId} func = {methodPart}");
                GlobalHotkey.AddHotKeyEvent(hotkeyId, execute, modifiers, vk);
            }
        }

        /// <summary>
        /// 执行一段完整代码，调用里面的 static void entryFuncName()
        /// </summary>
        public static string DoExecute(string code,string entryFuncName = "Entry")
        {
            try
            {
                // 1. 编译代码
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters
                {
                    GenerateInMemory = true,    // 直接在内存运行
                    GenerateExecutable = false, // 不生成exe
                };

                // 引用你自己的程序集（必须，否则找不到你的类）
                parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);

                // 编译
                CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

                // 报错直接返回
                if (results.Errors.HasErrors)
                {
                    string err = "";
                    foreach (CompilerError e in results.Errors)
                        err += e.ErrorText + "\n";
                    return err;
                }

                // 2. 找到所有类，执行 entryFuncName
                foreach (Type type in results.CompiledAssembly.GetTypes())
                {
                    MethodInfo method = type.GetMethod(entryFuncName, BindingFlags.Static | BindingFlags.Public);
                    if (method != null)
                    {
                        method.Invoke(null, null);
                        return $"执行成功:  {type.FullName}.{entryFuncName}()";
                    }
                }

                return $"未找到 static void {entryFuncName}()";
            }
            catch (Exception ex)
            {
                return "执行失败: " + ex.Message;
            }
        }
        

        
        /// <summary>
        /// 根据文本生成 Bitmap 图片
        /// </summary>
        /// <param name="text">要绘制的文本</param>
        /// <returns>包含文本的 Bitmap 对象</returns>
        public static Bitmap GenBitMapUseTex(string text,Color textColor ,Color backgroundColor  ,int padding = 1,int fontSize = 16,FontStyle fontStyle =FontStyle.Bold,string font = "微软雅黑")
        {
            // 1. 基础配置：字体、字号、文字颜色
            Font textFont = new Font(font, fontSize, fontStyle); // 可修改字体、大小、样式
            Brush textBrush = new SolidBrush(textColor ); // 文字颜色
            Color bgColor = backgroundColor; // 背景颜色（如需透明改为 Color.Transparent）

            // 2. 创建临时画布，测量文本实际宽高（避免文字被截断）
            using (Bitmap tempBmp = new Bitmap(1, 1))
            using (Graphics tempG = Graphics.FromImage(tempBmp))
            {
                // 设置高质量渲染（文字更清晰）
                tempG.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                SizeF textSize = tempG.MeasureString(text, textFont);

                // 3. 创建最终画布（宽高 = 文本尺寸 + 内边距，防止文字贴边）
                 // 内边距
                int width = (int)Math.Ceiling(textSize.Width) + padding ;
                int height = (int)Math.Ceiling(textSize.Height) + padding ;
                Bitmap bitmap = new Bitmap(width, height);

                // 4. 绘制文本到画布
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // 高质量绘图设置
                    g.Clear(bgColor); // 填充背景色
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    // 计算居中坐标
                    float x = padding;
                    float y = padding;

                    // 绘制文本
                    g.DrawString(text, textFont, textBrush, x, y);
                }

                // 5. 释放非托管资源（必须释放，防止内存泄漏）
                textFont.Dispose();
                textBrush.Dispose();

                return bitmap;
            }
        }

        
        // public static YRClassHandle<BSurface> GenSurface(string text,Color textColor ,Color backgroundColor  ,int padding = 1,int fontSize = 16,FontStyle fontStyle =FontStyle.Bold,string font = "微软雅黑")
        // {
        //     try
        //     {  
        //         YRClassHandle<BSurface> surface;
               
                
                
                
        //         //
        //         // 生成文字图片：小灰熊坦克
        //         Bitmap textBmp = CoraUtils.GenBitMapUseTex(
        //             text,
        //             textColor,
        //             backgroundColor,
        //             padding,
        //             fontSize,
        //             fontStyle,
        //             font
        //         );
                
        //         // 必须转换成 YR 支持的 16位 RGB565 格式
        //         Rectangle rect = new Rectangle(0, 0, textBmp.Width, textBmp.Height);
        //         Bitmap formatBmp = textBmp.Clone(rect, PixelFormat.Format16bppRgb565);
        //         textBmp.Dispose(); // 释放原图

        //         // 创建 YR 绘图表面
        //         surface = new YRClassHandle<BSurface>(formatBmp.Width, formatBmp.Height);
        //         surface.Ref.Allocate(2);

        //         // // 把图片数据复制到 YR 表面
        //         BitmapData data = formatBmp.LockBits(rect, ImageLockMode.ReadOnly, formatBmp.PixelFormat);
        //         Helpers.Copy(data.Scan0, surface.Ref.BaseSurface.Buffer, data.Stride * data.Height);
        //         formatBmp.UnlockBits(data);

        //         formatBmp.Dispose(); // 释放资源
              
        //         return surface;
        //     }
        //     catch(Exception ex)
        //     {
        //         Logger.PrintException(ex);
        //         // 防止绘制失败导致游戏崩溃
        //         return null;
        //     }
        // }

        

        [HandleProcessCorruptedStateExceptions]
        public static void DrawAllUnitName()
        {
            foreach (var obj in ObjectClass.Array)
            {
                
                DrawUnitName(obj);
                
            }
            
        }


        public static IEnumerator DrawAllUnitNameCo()
        {
            while (true)
            {
                
                foreach (var obj in ObjectClass.Array)
                {
                
                    DrawUnitName(obj);
                
                
                    yield return new WaitForFrames(1);
                }
            }
            
            
        }



        [HandleProcessCorruptedStateExceptions]
        public static void DrawUnitName(Pointer<ObjectClass> obj)
        {
            try
            {
                if (obj.IsNull )
                {
                    return;
                }
                Pointer<TechnoClass> pTechno  = obj.Convert<TechnoClass>();          
                if (pTechno.IsNull)
                {
                    return;
                }


                TechnoExt pTechnoExt = TechnoExt.ExtMap.Find(pTechno);
                if(pTechnoExt == null)
                {
                    return;
                }
                
                DrawUnitName(pTechnoExt);

            }catch(Exception ex)
            {
                CoraUtils.PrintException(ex);
            }
        }

        
        public static void ExportAllUnitUiNameAndID()
        {
            try
            {
                List<string> lines = new List<string>();
                lines.Add("------------------------SuperWeaponList------------------------------");
                for (int index = 0; index < SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Array.Count; index++)
                {
                    var item = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Array[index];
                    
                    string line = $"{index} {item.Convert<AbstractTypeClass>().Ref.ID} {item.Convert<AbstractTypeClass>().Ref.UIName} {item.Ref.Base.Base.WhatAmI()}";
                    lines.Add(line);
                }

                lines.Add("------------------------BuildingList------------------------------");
                for (int index = 0; index < BuildingTypeClass.ABSTRACTTYPE_ARRAY.Array.Count; index++)
                {
                    var item = BuildingTypeClass.ABSTRACTTYPE_ARRAY.Array[index];
                    
                    string line = $"{index} {item.Convert<AbstractTypeClass>().Ref.ID} {item.Convert<AbstractTypeClass>().Ref.UIName} {item.Ref.BaseAbstractType.Base.WhatAmI()}";
                    lines.Add(line);
                }

                

                lines.Add("------------------------InfantryList------------------------------");
                for (int index = 0; index < InfantryTypeClass.ABSTRACTTYPE_ARRAY.Array.Count; index++)
                {
                    var item = InfantryTypeClass.ABSTRACTTYPE_ARRAY.Array[index];
                    
                    string line = $"{index} {item.Convert<AbstractTypeClass>().Ref.ID} {item.Convert<AbstractTypeClass>().Ref.UIName} {item.Ref.BaseAbstractType.Base.WhatAmI()}";
                    lines.Add(line);
                }

                lines.Add("------------------------UnitList------------------------------");
                for (int index = 0; index < UnitTypeClass.ABSTRACTTYPE_ARRAY.Array.Count; index++)
                {
                    var item = UnitTypeClass.ABSTRACTTYPE_ARRAY.Array[index];
                    
                    string line = $"{index} {item.Convert<AbstractTypeClass>().Ref.ID} {item.Convert<AbstractTypeClass>().Ref.UIName} {item.Ref.BaseAbstractType.Base.WhatAmI()}";
                    lines.Add(line);
                }
                
                lines.Add("------------------------AircraftList------------------------------");
                for (int index = 0; index < AircraftTypeClass.ABSTRACTTYPE_ARRAY.Array.Count; index++)
                {
                    var item = AircraftTypeClass.ABSTRACTTYPE_ARRAY.Array[index];
                    
                    string line = $"{index} {item.Convert<AbstractTypeClass>().Ref.ID} {item.Convert<AbstractTypeClass>().Ref.UIName} {item.Ref.BaseAbstractType.Base.WhatAmI()}";
                    lines.Add(line);
                }
                
                

                System.IO.File.WriteAllLines("UnitIDAndUIName.txt", lines);
            }
            catch(Exception ex)
            {
                CoraUtils.PrintException(ex);
            }
        }


        [HandleProcessCorruptedStateExceptions]
        public static void DrawUnitName(TechnoExt owner)
        {
            try{
                var pTechno = owner.OwnerObject;
                if (pTechno.Ref.owner.Equals(IntPtr.Zero))  //有概率奔溃
                {
                    return;
                }
                // 把游戏3D坐标 → 屏幕2D坐标
                Point2D screenPos = TacticalClass.Instance.Ref.CoordsToClient(pTechno.Ref.BaseAbstract.GetCoords());
                Pointer<HouseClass> pOwnerHouse = (Pointer<HouseClass>)pTechno.Ref.owner;
                screenPos.Y -= 60;
                ushort color = SystemUtils.ToRgb565(pOwnerHouse.Ref.Color.R, pOwnerHouse.Ref.Color.G, pOwnerHouse.Ref.Color.B);
                if (pTechno.Ref.Owner.Ref.IsAlliedWith(HouseClass.FindNeutral())) 
                {
                    color = SystemUtils.ToRgb565(255,222,173);
                    return;
                }
                if (pTechno.Ref.Owner.Ref.IsAlliedWith(HouseClass.FindSpecial())){
                    color = SystemUtils.ToRgb565(173,216,200);
                    return;
                }
                
                Surface.Composite.Ref.DrawText($"{pTechno.Ref.Type.Convert<AbstractTypeClass>().Ref.UIName} {pTechno.Convert<ObjectClass>().Ref.Health}",screenPos, color,0,-1);

            }catch(Exception ex)
            {
                CoraUtils.PrintException(ex);
            }
        }

        public static int GetPlayerCount()
        {
            Pointer<int> pPlayerCount = (IntPtr)0xA8DA84;
                    
                  
            int iPlayerCount = pPlayerCount.Ref;
            return iPlayerCount;
        }
        
        public static List<string> GetPlayerNameList(Action<string> action = null)
        {
            List<string> result = new List<string>();
      
            for (int i = 0;i< GetPlayerCount(); i++)
            {
        

                IntPtr p = (IntPtr)0xA8DA78;
                Logger.Log($"p = 0x{p.ToInt32():X8}");
                IntPtr p2 = Marshal.ReadIntPtr(p);
                Logger.Log($"p2 = 0x{p2.ToInt32():X8}");
                UniStringPointer p3 = Marshal.ReadIntPtr(p2+4 * i);  
                Logger.Log($"p3 = 0x{((IntPtr)p3).ToInt32():X8}");
    
                string sPlayerName = p3.ToString();
                result.Add(sPlayerName);
                action?.Invoke(sPlayerName);   
            }
            return result;
        }
       
        #region 日志工具
        public static void Log(string format, params object[] args)
        {
            string str = string.Format(format, args);
            Logger.Log(str);
            DebugLog.Logln(format,args);
        }

        public static void LogEx(string format, params object[] args)
        {
            string str = string.Format(format, args);
            Logger.Log("################################################");
            DebugLog.Logln("################################################");
            Logger.Log(str);
            DebugLog.Logln(format,args);
            Logger.Log("################################################");
            DebugLog.Logln("################################################");
        }
        public static void LogError(string format, params object[] args)
        {
            string str = string.Format(format, args);
            LogError(str);
            DebugLog.Logln(format, args);
        }

        public static void LogError(string str)
        {
            Logger.LogWithColor("[Error] " + str, ConsoleColor.Red);
            DebugLog.Logln(str);
        }

        public static void PrintException(Exception ex)
        {

            Logger.PrintException(ex);

            DebugLog.Logln("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", ConsoleColor.DarkRed);
            PrintExceptionBase(ex);
            DebugLog.Logln("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", ConsoleColor.DarkRed);

        }
        private static void PrintExceptionBase(Exception e)
        {
             DebugLog.Logln("{0} info: ", e.GetType().FullName);
             DebugLog.Logln("Message: " + e.Message);
             DebugLog.Logln("Source: " + e.Source);
            DebugLog.Logln("TargetSite.Name: " + e.TargetSite?.Name);
            DebugLog.Logln("Stacktrace: " + e.StackTrace);
            if (e is ReflectionTypeLoadException { LoaderExceptions: var loaderExceptions })
            {
                foreach (Exception e2 in loaderExceptions)
                {
                    DebugLog.Logln("--------------------------------------------------------", ConsoleColor.DarkRed);
                    PrintExceptionBase(e2);
                }
            }

            if (e.InnerException != null)
            {
                DebugLog.Logln("--------------------------------------------------------", ConsoleColor.DarkRed);
                PrintExceptionBase(e.InnerException);
            }
        }
        #endregion

    }
}
