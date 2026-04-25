using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

using PatcherYRpp;
using DynamicPatcher;

using Extension.Utilities;
using Extension.INI;
using System.Collections;
using Extension.Coroutines;

using System.Runtime.InteropServices;
using Extension.Ext;

using System.Linq;
using Extension.Components;
using System.Security.Cryptography.X509Certificates;
using Extension.Decorators;
using PatcherYRpp.Utilities;
using System.Threading;
using System.Reflection;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Extension.EventSystems;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Xml;
using System.Diagnostics;
using System.Drawing;

namespace Extension.CoraExtension
{
    public class MainTools
    {
        public GameObject gameObject;
        private static CoroutineSystem _coroutineSystem = new CoroutineSystem();

        private static MainTools _instance;
        public static MainTools Instance
        {
            get {
                if (_instance == null)
                {
                    _instance = new MainTools();
                }
                return _instance;
            }
            set {
                _instance = value;
            }
        }
        
        private static bool bIsAIControlEnable = false;
        List<Coroutine> coroutines = new List<Coroutine>(); 
        #region MainTools_TOR
        public MainTools()
        {
            #region 生命周期事件注册
            SessionExt.GameBattleFirstResume += GameBattleFirstResume;
            SessionExt.Start += Start;
            SessionExt.Update += Update;
            SessionExt.UpdateLate += UpdateLate;
            SessionExt.Destroy += Destroy;
            TechnoExt.Update += TechnoExtUpdate;
            TechnoExt.LateUpdate += TechnoExtLateUpdate;
            TechnoExt.Render += TechnoExtRender;
            GScreenExt.DrawOnTop_TheDarkSideOfTheMoonCallback += DrawOnTop_TheDarkSideOfTheMoonCallback;
            #endregion
            

            #region 网络事件注册
            PlaceEventRsp placeEventRsp = new PlaceEventRsp();
            Network.NetworkHandles.Add(placeEventRsp.Index, placeEventRsp);
            ProductEventRsp productEventRsp = new ProductEventRsp();
            Network.NetworkHandles.Add(productEventRsp.Index, productEventRsp);
            AbandonEventRsp abandonEventRsp = new AbandonEventRsp();
            Network.NetworkHandles.Add(abandonEventRsp.Index, abandonEventRsp);
            AbandonAllEventRsp abandonAllEventRsp = new AbandonAllEventRsp();
            Network.NetworkHandles.Add(abandonAllEventRsp.Index, abandonAllEventRsp);
            FrameEventRsp frameEventRsp = new FrameEventRsp();
            Network.NetworkHandles.Add(frameEventRsp.Index,frameEventRsp);
            SpecialPlaceEventRsp specialPlaceEventRsp = new SpecialPlaceEventRsp();
            Network.NetworkHandles.Add(specialPlaceEventRsp.Index,specialPlaceEventRsp);
            MegamissionEventRsp megamissionEventRsp = new MegamissionEventRsp();
            Network.NetworkHandles.Add(megamissionEventRsp.Index,megamissionEventRsp);
            DeployEventRsp deployEventRsp = new DeployEventRsp();
            Network.NetworkHandles.Add(deployEventRsp.Index,deployEventRsp);
            CoraPlaceEventRsp coraPlaceEventRsp = new CoraPlaceEventRsp();
            Network.NetworkHandles.Add(coraPlaceEventRsp.Index,coraPlaceEventRsp);
            CoraDanmuEventRsp coraDanmuEventRsp = new CoraDanmuEventRsp();
            Network.NetworkHandles.Add(coraDanmuEventRsp.Index,coraDanmuEventRsp);
            
            #endregion



            if (CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("StartDanmu",false))
            {
                _coroutineSystem.StartCoroutine(DanmuWebSocketInit());  //全局无需清除
            }
            #region 回调注册
            HouseExt.CreateCallback = (house) =>
            {
                if (house.IsNull)
                {
                    CoraUtils.Log("HouseExt CreateCallback 已经执行, 但是 house 是 null QAQ");
                    return;
                }
                if (house.Ref.Type.IsNull)
                {
                    CoraUtils.Log("HouseExt CreateCallback 已经执行, 但是 house的Type 是 null QAQ");
                    return;
                }
                if (house == HouseClass.Player)
                {
                    CoraUtils.Log($"玩家 {house.Ref.Type.Ref.Base.UIName}:{house.Ref.Type.Ref.Base.ID}的HouseClass实例被创建了！");
                }
                else
                {
                    string targetTag = house.Ref.ControlledByHuman() ? "远程玩家" : "AI";
                    string enemyTag = house.Ref.IsAlliedWith(HouseClass.Player)? "盟友":"敌人";
                    CoraUtils.Log($"{enemyTag} {targetTag} {house.Ref.Type.Ref.Base.UIName}:{house.Ref.Type.Ref.Base.ID}的HouseClass实例被创建了！");
                }
            };

            // 在工厂类的生产进度增加时触发的回调
            FactoryClass.ProgressAddCallback += (factory) =>
            {
                if (factory.IsNull)
                {
                    CoraUtils.Log("建造工厂类ProgressAddCallback 已经执行, 但是 factory 是 null QAQ");
                    return;
                }
                if (factory.Ref.Owner.IsNull)
                {
                    CoraUtils.Log("建造工厂类ProgressAddCallback 已经执行, 但是工厂的所有者是null QAQ");
                    
                    return;
                }
                
            };

            // 在工厂类被创建时触发的回调
            FactoryClass.CreateCallback += (factory) =>
            {
                if (factory.IsNull)
                {
                    CoraUtils.Log("建造工厂类CreateCallback 已经执行, 但是 factory 是 null QAQ");
                    return;
                }
                if (factory.Ref.Owner.IsNull)
                {
                    CoraUtils.Log("建造工厂类CreateCallback 已经执行, 但是工厂的所有者是null QAQ");
                    return;
                }
                

                if (factory.Ref.Owner == HouseClass.Player)
                {
                    
                    // CoraUtils.Log($"玩家的工厂类被创建了！所属阵营：{factory.Ref.Owner.Ref.Type.Ref.Base.ID} ^w^");
                    PrintMessage($"玩家 {factory.Ref.Owner.Ref.Type.Ref.Base.UIName}   开始建造 {factory.Ref.Object.Ref.Type.Convert<AbstractTypeClass>().Ref.UIName } ^w^", (ColorSchemeIndex)factory.Ref.Owner.Ref.Type.Ref.ColorSchemeIndex,3*60);
                    if (SessionClass.IsStandalone() || CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("IsOnlineBattleDangerFunctionEnable",false))
                    {
                        if (CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("InstantConstruction",false))
                        {
                            factory.Ref.Production.Step  = 54; //联机不可用
                        }
                    }
                }
                else
                {
                    
                    string targetTag = factory.Ref.Owner.Ref.ControlledByHuman() ? "远程玩家" : "AI";
                    string enemyTag = factory.Ref.Owner.Ref.IsAlliedWith(HouseClass.Player)? "盟友":"敌人";
                    // CoraUtils.Log($"{enemyTag} {targetTag} 的工厂类被创建了！所属阵营：{factory.Ref.Owner.Ref.Type.Ref.Base.ID} ^w^");
                    PrintMessage($"{enemyTag} {targetTag } {factory.Ref.Owner.Ref.Type.Ref.Base.UIName}  开始建造 {factory.Ref.Object.Ref.Type.Convert<AbstractTypeClass>().Ref.UIName}  ^w^", (ColorSchemeIndex)factory.Ref.Owner.Ref.Type.Ref.ColorSchemeIndex,3*60);
                    
                }

                
             
            };

            // 注册鼠标事件回调
            GeneralExt.LMouseDownCallback += () =>
            {
            };
           
            GeneralExt.RMouseDownCallback += () => {
                m_MouseDownCount += 1;

                if (m_MouseDownCount >= 3)
                {
                    m_MouseDownCount = 0;
                    
                  
                    
                }
            };
            #endregion
            
            CoraUtils.Log("MainTools CTOR");
        }

        private void Start()
        {
            try
            {
                CoraUtils.Log($"MainTools Started");
            }catch (Exception ex)
            {
                CoraUtils.PrintException(ex);
            }
        }

        private void UpdateLate()
        {
            _coroutineSystem.Update();
            DouyinDanmuWebSocket.instance.Update();
  
        }
        #endregion
        private void TechnoExtRender(TechnoExt ext)
        {
            if(bIsShowUnitName) CoraUtils.DrawUnitName(ext);
            
        }

        private void TechnoExtLateUpdate(TechnoExt ext)
        {
            
        }

        private void TechnoExtUpdate(TechnoExt ext)
        {
            
        }

         private void Update()
        {

            //不安全 在某些对象不存在的时候 这是UpdateBegin 使用 UpdateLate 减少奔溃
            
            List<Pointer<HouseClass>> pAllEnemyHouses = HouseClass.GetTargetAllEnemyHouse(HouseClass.Player);
            if (pAllEnemyHouses.Count > 0)
            {
                HouseClass.Player.Ref.Defeated = 0;
            }
            else
            {
                HouseClass.Player.Ref.Defeated = 1;
            }


            

        }
        private void DrawOnTop_TheDarkSideOfTheMoonCallback()
        {
            int i = 2;
            string text = string.Format("FPS: {0,-4} Avg: {1:F2} Frames: {2}", 
                CoraUtils.CurrentFrameRate, 
                CoraUtils.GetAverageFrameRate(),
                CoraUtils.TotalFramesElapsed);

            const int AdvCommBarHeight = 32;
            int h = Surface.Composite.Ref.Height;
            int w = Surface.Composite.Ref.Width;
            int x = 90;
            Point2D location = new Point2D(x,h- AdvCommBarHeight - 20 *  i);
            Surface.Composite.Ref.GetRect();

            Surface.Composite.Ref.DrawText(text,location, (ushort) DRAWTEXTCOLOR.COLOR_RED,0,-1);
            foreach (var item in HouseClass.Array)
            {
                if (item.Ref.Type == null)
                {
                    continue;
                }
                if (item  == HouseClass.FindNeutral())
                {
                    continue;
                }
                if (item  == HouseClass.FindSpecial())
                {
                    continue;
                }
                i++;
                location = new Point2D(x,h- AdvCommBarHeight - 20 *  i);
                Point2D screenPos = TacticalClass.Instance.Ref.CoordsToClient(CellClass.Cell2Coord(item.Ref.GetBaseCenter()));
                string isWinner = item.Ref.IsWinner ? "是":"否";
                string isLoser = item.Ref.IsLoser ? "是":"否";
                string isGameOver = item.Ref.IsGameOver ? "是":"否";
                string isDefeated = item.Ref.Defeated ? "是":"否";
                Surface.Composite.Ref.DrawText($"资金:{item.Ref.Available_Money()} / IsDefeated:{isDefeated} " , location, SystemUtils.ToRgb565(item.Ref.Color.R,item.Ref.Color.G,item.Ref.Color.B),0,-1);

                
            }
            i++;
            location = new Point2D(x,h- AdvCommBarHeight - 20 *  i);
            List<Pointer<HouseClass>> pAllEnemyHouses = HouseClass.GetTargetAllEnemyHouse(HouseClass.Player);

            Surface.Composite.Ref.DrawText($"敌方阵营数量:{pAllEnemyHouses.Count}" , location, SystemUtils.ToRgb565(255,0,0),0,-1);

        }

        private static Pointer<TechnoClass> GenUnit(string sUnitID,Pointer<HouseClass> onwer)
        {
            string sUnitName = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(sUnitID).Convert<AbstractTypeClass>().Ref.UIName;
            var obj = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(sUnitID).Ref.Base.CreateObject(onwer);
            if (obj.IsNull)
            {
                PrintMessage($"生成{sUnitName}失败 obj is NULL");
                return Pointer<TechnoClass>.Zero;
            }
            Pointer<TechnoClass> pTechno  = obj.Convert<TechnoClass>();

            if (pTechno.IsNull)
            {
                PrintMessage($"转换{sUnitName}失败 pTechno is NULL");
                return Pointer<TechnoClass>.Zero;
            }
            TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);



            var isCreated = TechnoPlacer.PlaceTechnoNear( pTechno,DisplayClass.Display_ZoneCell);
            if (isCreated)
            {
                PrintMessage($"创建{sUnitName}成功 在地图坐标  X:{DisplayClass.Display_ZoneCell.X} Y:{DisplayClass.Display_ZoneCell.Y}");
            }
            return pTechno;
        }

        /// <summary>
        /// 生成中心附近的随机坐标列表（无重复）
        /// </summary>
        /// <param name="center">中心单元格</param>
        /// <param name="range">范围</param>
        /// <param name="count">生成数量</param>
        /// <returns>无重复坐标列表</returns>
        private List<CellStruct> gen_range_cells(CellStruct center, int range, int count)
        {
            List<CellStruct> cells = new List<CellStruct>();
            Random _random = new Random();

            while (cells.Count < count)
            {
                // 生成随机偏移
                int offsetX = _random.Next(-range, range + 1);
                int offsetY = _random.Next(-range, range + 1);

                CellStruct newCell = center;
                newCell.X += (short)offsetX;
                newCell.Y += (short)offsetY;

                // ✅ 严格检查：如果列表里已经有这个坐标，就跳过，不添加
                bool isDuplicate = false;
                foreach (var exist in cells)
                {
                    if (exist.X == newCell.X && exist.Y == newCell.Y)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                // 不重复才添加
                if (!isDuplicate)
                {
                    cells.Add(newCell);
                }
            }

            return cells;
        }

        static int m_MouseDownCount = 0;

        #region GameBattleFirstResume
     
        [HandleProcessCorruptedStateExceptions]
        public void GameBattleFirstResume()
        {
            try
            {
                bIsAIControlEnable = CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("AIControl",false);
                
                bIsShowUnitName = CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("ShowUnitName",false);
                coroutines.Add(_coroutineSystem.StartCoroutine(GetAllHouseInfo()));
                coroutines.Add(_coroutineSystem.StartCoroutine(MouseCountClear(0.5f)));

                #region 注册弹幕事件
                DouyinDanmuWebSocket.instance.OnLike += OnLike;
                DouyinDanmuWebSocket.instance.OnChat += OnChat;
                DouyinDanmuWebSocket.instance.OnGift += OnGift;
                DouyinDanmuWebSocket.instance.JoinRoom += JoinRoom;
                #endregion
                coroutines.Add(_coroutineSystem.StartCoroutine(TEnumerator()));
                if (SessionClass.IsStandalone() || CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("IsOnlineBattleDangerFunctionEnable",false))
                {
                    HouseClass.Player.Ref.IQLevel = CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("IQLevel",(uint)0);  
                    HouseClass.Player.Ref.IQLevel2 = CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("IQLevel2",(uint)0);
                    if (CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("SuperweaponNoWait",false))
                    {
                        coroutines.Add(_coroutineSystem.StartCoroutine(CurrentPlayerAllSuperWeaponCharge())) ;  // 联机不可用
                    }
                    if (CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("InitialMoney",0) > 0)
                    {
                        HouseClass.Player.Ref.GiveMoney(CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("InitialMoney",0));    //联机不可用 延迟奔溃
                    }
                    
                    AIControl(HouseClass.Player,bIsAIControlEnable);

                }
                
                if(!SessionClass.IsStandalone()){
                    PrintMessage("联机模式!!",ColorSchemeIndex.Green,3*60);
                    int iPlayerCount = CoraUtils.GetPlayerCount();
                    PrintMessage($"联机玩家数量: {iPlayerCount}");
                    CoraUtils.Log($"联机玩家数量: {iPlayerCount}");
              
                    CoraUtils.GetPlayerNameList((sPlayerName) =>
                    {
                        PrintMessage($"玩家名字:{sPlayerName}");
                        CoraUtils.Log($"玩家名字:{sPlayerName}");
                    }
                    );
                }
                else
                {
                    switch(SessionClass.Instance.GameMode)
                    {
                        case GameMode.Campaign:
                            PrintMessage("战役模式!!",ColorSchemeIndex.Green,3*60);
                            break;
                        case GameMode.Skirmish:
                            PrintMessage("单人对战模式!!",ColorSchemeIndex.Green,3*60);
                            break;
                        default:
                            PrintMessage($"未知游戏模式 {SessionClass.Instance.GameMode}!!",ColorSchemeIndex.Green,3*60);
                            break;
                    }
                }
    
                coroutines.Add(GlobalHotkey.Start(_coroutineSystem)); //全局热键监听协程

                var hotkeyConfig = CoraUtils.MainToolsConfig("HotkeyConfig");
                CoraUtils.ParseHotkeyConfig(hotkeyConfig.GetBuffer().Unparsed);

                CoraUtils.Log($"MainTools GameBattleFirstResume");
                
                
            }
            catch(Exception ex)
            {
                CoraUtils.PrintException(ex);
            }
            
            //在组件被创建时执行一次
        }
        
        #endregion 

        private static readonly IntPtr GetPlayerNameAddr = new IntPtr(0x7350C0); 
        public unsafe static AnsiString GetPlayerName(IntPtr p)
        {

                var func = (delegate* unmanaged[Cdecl]<IntPtr, AnsiString>)GetPlayerNameAddr.ToPointer();
                return func(p);
        }


        
        public void Destroy()
        {

            // 注销弹幕事件
            DouyinDanmuWebSocket.instance.OnLike -= OnLike;
            DouyinDanmuWebSocket.instance.OnChat -= OnChat;
            DouyinDanmuWebSocket.instance.OnGift -= OnGift;
            DouyinDanmuWebSocket.instance.JoinRoom -= JoinRoom;
     
            foreach (var item in coroutines)
            {
                _coroutineSystem.StopCoroutine(item);
            }

            CoraUtils.Log($"MainTools Destroyed");

            //在组件被销毁时执行一次
        }


        private static void AIControl(Pointer<HouseClass> house, bool isAIControlEnable = true)
        {
            if (isAIControlEnable)
            {
                house.Ref.CurrentPlayer = false; // 必须在AiControl之前设置flase，否则会导致AI无法正常运作
                house.Ref.IQLevel = 5;  
                house.Ref.IQLevel2 = 5;  
                house.Ref.AutocreateAllowed = 1; // 允许自动生产
                PrintMessage("AIControl");
            }
            else
            {
                house.Ref.CurrentPlayer = true; // 必须在AiControl之前设置flase，否则会导致AI无法正常运作
                HouseClass.Player.Ref.IQLevel = CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("IQLevel",(uint)0);  
                HouseClass.Player.Ref.IQLevel2 = CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("IQLevel2",(uint)0);
                house.Ref.AutocreateAllowed = 0; // 禁止自动生产
                PrintMessage("PlayerControl");
            }
            
        }
        private IEnumerator MouseCountClear(float t)
        {
            while (true)
            {

                yield return CoroutineUtils.WaitForSeconds(t);

                if (m_MouseDownCount != 0)
                {
                    m_MouseDownCount = 0;
                }
            }
        }

        
        
 
        public IEnumerator TEnumerator()
        {
            while (true)
            {
                // foreach (var item in SidebarClass.Instance.Tabs)
                // {
                //     Logger.Log($"CemoCount: {item.CameoCount}");
                //     foreach (var cemo in item.Cameos)
                //     {
                //         Logger.Log($"CemoIndex: {cemo.ItemIndex}");
                //         Logger.Log($"CemoIndex: {cemo.ItemType}");
                        
                        
                //     }
                    
                // }
                // foreach (var item in SidebarClass.Instance.DiplomacyHouses)
                // {
                //     Logger.Log($"{item.Ref.Type.Ref.Base.UIName}");
                // }
                yield return new WaitForFrames(60);
            }
        }


        
        public static void PrintMessage(string msg,ColorSchemeIndex color = ColorSchemeIndex.White, int duration = 60, bool silent = false)
        {
            MessageListClass.Instance.MaxMessages = 100;
            MessageListClass.Instance.PrintMessage(msg, color, duration, silent);
        }

        

        /// <summary>
        /// 弹幕服务器初始化协程
        /// </summary>
        private static IEnumerator DanmuWebSocketInit()
        {
            if (!DouyinDanmuWebSocket.instance.isRun)
            {
                // 异步方法转协程等待
                DouyinDanmuWebSocket.instance.SetUrl("ws://127.0.0.1:8765");
                var runTask = DouyinDanmuWebSocket.instance.Run();
                while (!runTask.IsCompleted)
                {
                    yield return CoroutineUtils.WaitForSeconds(0.5);  // 等待直到任务完成
                }
            }
        }


        public static Pointer<SuperWeaponTypeClass> GetExistedSuperWeapon(Pointer<HouseClass> houseClass, SuperWeaponType superWeaponType)
        {
            Pointer<SuperWeaponTypeClass> pSWType = IntPtr.Zero;
            foreach (var super in houseClass.Ref.Supers)
            {
                if (super.Ref.Type.Ref.Type == superWeaponType)
                {
                    pSWType = super.Ref.Type;
                }
            }
            return pSWType;
        }

        [HandleProcessCorruptedStateExceptions]
        public static void LancherSuperWeapon(Pointer<HouseClass> houseClass, Pointer<AbstractClass> pTarget, string superWeaponName = "NukeSpecial")
        {
            try
            {
                
                CellStruct targetCell = CellClass.Coord2Cell(pTarget.Ref.GetCoords());
                LancherSuperWeapon(houseClass,targetCell,superWeaponName);
            }
            catch (Exception ex)
            {
                _ = ex;
                CoraUtils.PrintException(ex);
            }
        }


        /// <summary>
        /// 发射超级武器：让指定的阵营（houseClass）对指定的目标位置（pTarget）发射一个超级武器，超级武器的类型由superWeaponName指定。这个方法会自动查找玩家是否拥有该类型的超级武器，如果有则直接发射；如果没有则会记录日志并跳过发射。这个方法可以用来实现通过外部输入触发超级武器攻击的功能，例如通过弹幕指令或其他事件。
        /// </summary>
        /// <param name="houseClass"></param>
        /// <param name="pTarget"></param>
        /// <param name="superWeaponName"></param>
        [HandleProcessCorruptedStateExceptions]
        public static void LancherSuperWeapon(Pointer<HouseClass> houseClass, CellStruct pTarget, string superWeaponName = "NukeSpecial")
        {
            try
            {

                Pointer<SuperWeaponTypeClass> pSWType = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(superWeaponName);

                var parser = Parsers.GetParser<Pointer<SuperWeaponTypeClass>>();
                parser.Parse(superWeaponName, ref pSWType);

                if (pSWType.IsNull)
                {
                    CoraUtils.Log($"发射失败：未找到超级武器类型 {superWeaponName}");
                    return;
                }
                Pointer<SuperClass> pSuper = houseClass.Ref.FindSuperWeapon(pSWType);
                if (pSuper.IsNull)
                {
                    CoraUtils.Log($"发射失败：HouseClass未拥有超级武器 {superWeaponName}");
                    return;
                }
                
                CoraUtils.Log("FireSuperWeapon({2}):0x({3:X}) -> ({0}, {1})", pTarget.X, pTarget.Y, pSWType.Ref.Base.ID, (int)pSuper);
                pSuper.Ref.SetCharge(100);
                pSuper.Ref.IsCharged = true;
                pSuper.Ref.Launch(pTarget, houseClass.Ref.PlayerControl);
                pSuper.Ref.IsCharged = false;
            }
            catch (Exception ex)
            {
                _ = ex;
                CoraUtils.PrintException(ex);
            }
        }
        
        /// <summary>
        /// 攻击敌人
        /// </summary>
        /// <param name="superWeaponName"></param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        private static void AttackEnemyUseSuperWeapon(string superWeaponName = "NukeSpecial")
        {
            
            Random random = new Random();
            List<Pointer<HouseClass>> enemyHousePtrList = HouseClass.GetTargetAllEnemyHouse(HouseClass.Player,  bFilterNeutral: true ,bFilterSpecial: true);
            if (enemyHousePtrList.Count == 0)
            {
                CoraUtils.Log("当前无敌方阵营，无法攻击敌人");
                return;
            }
            
            int randomAiHouseNumber = random.Next(enemyHousePtrList.Count);   
            var targetHouse = enemyHousePtrList[randomAiHouseNumber];
            if (targetHouse.Ref.Buildings.Count == 0)
            {
                CoraUtils.Log($"目标阵营 {targetHouse.Ref.Type.Ref.Base.UIName} 没有建筑，无法攻击");
                return;
            }
            int buildingsCount = targetHouse.Ref.Buildings.Count();
            var targetBuilding = targetHouse.Ref.Buildings[random.Next(buildingsCount)];

            LancherSuperWeapon(HouseClass.Player, targetBuilding.Ref.BaseAbstract.GetThisPointer(), superWeaponName);
            return;
            
        }

    

        [HandleProcessCorruptedStateExceptions]
        private static IEnumerator CurrentPlayerAllSuperWeaponCharge()
        {
            while (true)
            {
                try
                {
                
                    foreach (Pointer<SuperClass> super in HouseClass.Player.Ref.Supers)
                    {
                        try
                        {
                  
                            super.Ref.SetCharge(100);
                            super.Ref.IsCharged = true;
                            SuperWeaponType superWeaponType = super.Ref.Type.Ref.Type;

                            if (super.Ref.CanFire() && super.Ref.IsCharged)
                            {
                                string superWeaponName = super.Ref.Type.Ref.Base.UIName;
                                
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            CoraUtils.PrintException(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CoraUtils.PrintException(ex);
                }
                yield return CoroutineUtils.WaitForSeconds(1);
            }
            
        }

        /// <summary>
        /// 充能AI超级武器：让指定的阵营的指定超级武器充能，超级武器的类型由superWeaponName指定
        /// </summary>
        /// <param name="house"></param>
        /// <param name="superWeaponName"></param>
        public static void ChargeHouseSuperWeapon(Pointer<HouseClass> house, string superWeaponName = "NukeSpecial")
        {
            try
            {
                SwizzleablePointer<SuperWeaponTypeClass> pSWType = new SwizzleablePointer<SuperWeaponTypeClass>(IntPtr.Zero);
                
                var parser = Parsers.GetParser<Pointer<SuperWeaponTypeClass>>();
                parser.Parse(superWeaponName, ref pSWType.Pointer);

                if (pSWType.IsNull)
                {
                    CoraUtils.Log($"充能失败：未找到超级武器类型 {superWeaponName}");
                    return;
                }
                Pointer<SuperClass> pSuper = house.Ref.FindSuperWeapon(pSWType);
                if (pSuper.IsNull)
                {
                    CoraUtils.Log($"充能失败：HouseClass未拥有超级武器 {superWeaponName}");
                    return;
                }
                
                pSuper.Ref.SetCharge(100);
                pSuper.Ref.IsCharged = true;
            }
            catch (Exception ex)
            {
                _ = ex;
                CoraUtils.PrintException(ex);
            }
        }


        

        /// <summary>
        /// 赠送当前玩家金钱：给当前玩家增加指定数量的金钱，金额由money参数指定。这个方法会检查money参数是否为正数且不超过int.MaxValue，如果不合法则会记录日志并跳过赠送；如果合法则会调用游戏内的方法增加玩家的金钱。这个方法可以用来实现通过外部输入触发增加金钱的功能，例如通过弹幕指令或其他事件。
        /// </summary>
        /// <param name="money"></param>
        [HandleProcessCorruptedStateExceptions]
        public static void GiveCurrentPlayerMoney(int money)
        {
            try
            {
                int giveMoney = money;
                if (giveMoney > int.MaxValue || giveMoney < 0) // 负数也直接跳过（防止溢出成负）
                {
                    CoraUtils.Log($"送金额溢出，跳过：{giveMoney}（最大值：{int.MaxValue}）");
                    PrintMessage($"赠送金额溢出，跳过：{giveMoney}（最大值：{int.MaxValue}）", ColorSchemeIndex.Red);
                    return;
                }
                if (HouseClass.Player.Ref.Available_Money() < 0)
                {
                    HouseClass.Player.Ref.GiveMoney(HouseClass.Player.Ref.Available_Money() * -1);//
                    CoraUtils.Log($"当前资金异常，已修正为0");
                    PrintMessage($"当前资金异常，已修正为0", ColorSchemeIndex.Red);
                }
                
                HouseClass.Player.Ref.GiveMoney(giveMoney);
            
            }
            catch (Exception ex)
            {
                CoraUtils.PrintException(ex);
            }
        }

        
        

        [HandleProcessCorruptedStateExceptions]
        
        private static IEnumerator GetAllHouseInfo()
        {
            
            var allHouses = HouseClass.Array;
            if (allHouses.Count <= 0)
            {
                CoraUtils.Log("当前无活跃阵营（游戏可能未进入对局）");
                yield return null;
            }
            
            Logger.Log($"=== 开始遍历所有活跃阵营（共 {allHouses.Count} 个）===");
            try
            {
                foreach (var house in HouseClass.Array)
                {
                    if (house.IsNull)
                    {
                        CoraUtils.Log("  - 空指针");
                        continue;
                    }

                    if (house.Ref.Type.IsNull)
                    {
                        CoraUtils.Log("GetAllAIHouseIndex: house type is null, skipping this house");
                        continue;
                    }

                    Logger.Log($"  --------------Side Index {house.Ref.Type.Ref.SideIndex}  Array Index {house.Ref.ArrayIndex}  ----------------");
                    Logger.Log($"  - 玩家：{house.Ref.Type.Ref.Base.UIName}");
                    Logger.Log($"  - 阵营ID：{house.Ref.Type.Ref.Base.ID}");
                    

                    Logger.Log($"  - 是否当前玩家：{(house.Ref.CurrentPlayer ? "是" : "否")}");


                    Logger.Log($"  - AI难度：{house.Ref.AIDifficulty}");
                    Logger.Log($"  - IQLevel：{house.Ref.IQLevel}");
                    Logger.Log($"  - IQLevel2：{house.Ref.IQLevel2}");
                    Logger.Log($"  - 建筑数量：{house.Ref.Buildings.Count}");
                    Logger.Log($"  - 所属势力：{house.Ref.Type.Ref.Suffix}");
                    Logger.Log($"  - 当前资金：{house.Ref.Available_Money()}");
                    Logger.Log($"  - 控制类型：{(house.Ref.ControlledByHuman() ? "玩家" : "AI")}");
                    string isWinner = house.Ref.IsWinner ? "是" : "否";
                    string isLoser = house.Ref.IsLoser ? "是" : "否";

                    Logger.Log($"  - IsWin：{isWinner}");
                    Logger.Log($"  - IsLose：{isLoser}");
                    // Logger.Log($"  - 正在建造:{factoryProducing.Ref.Production.Value.ToString() ?? "无"}");
                    Logger.Log($"  -----------------------------------------------------");

                }
                
            }
            catch (Exception ex)
            {
                _ = ex;
                CoraUtils.PrintException(ex);
            }
            yield return null;
        }

        [HandleProcessCorruptedStateExceptions]
        
        private void JoinRoom(string userName)
        {
            string join_msg = $"{userName} 加入了直播间!";
            PrintMessage(join_msg, ColorSchemeIndex.Orange, 150, true);
        }


        [HandleProcessCorruptedStateExceptions]
        
        private static void OnGift(string userName, string giftName, string count)
        {
            switch (giftName)
            {
                case "小花花":
                    {
                        
                        // 启动子协程处理铁幕逻辑
                        string give_msg = $"{userName} 送的 {giftName},获得铁幕!";
                        PrintMessage(give_msg, ColorSchemeIndex.Orange);
                        _coroutineSystem.StartCoroutine(IronCurtainGiftCoroutine(count));
                    }
                    break;

                case "嘉年华":
                    {
                        foreach (var item in HouseClass.Array)
                        {
                            if (item.IsNotNull)
                            {
                                continue;
                            }
                            else
                            {
                                foreach (var super in item.Ref.Supers)
                                {
                                    if(super.Ref.Type.Convert<AbstractTypeClass>().Ref.ID  == "NukeSpecial")
                                    {
                                        super.Ref.SetCharge(100);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "人气票":
                case "助力票":
                    {
                        try
                        {
                            int giveMoney = 10 * int.Parse(count);
                            string give_msg = $"{userName} 送的 {giftName},获得资金: {giveMoney}!";
                            PrintMessage(give_msg, ColorSchemeIndex.Orange, 150);
                            GiveCurrentPlayerMoney(giveMoney);
                            int currentMoney = HouseClass.Player.Ref.Available_Money();
                            string total_msg = $"当前资金: {currentMoney}";
                        }
                        catch (Exception ex)
                        {
                            _ = ex;
                            CoraUtils.PrintException(ex);
                        }
                    }
                    break;
                default:
                    PrintMessage($"未处理的礼物: {giftName} x {count} from {userName}", ColorSchemeIndex.Yellow, 150);
                    break;
            }
        }

        /// <summary>
        /// 小花花礼物处理协程
        /// </summary>
        private static IEnumerator IronCurtainGiftCoroutine(string count)
        {
                int buildingsCount = 0;
                Random random = new Random();
                try
                {
                    buildingsCount = HouseClass.Player.Ref.Buildings.Count;
                }
                catch (Exception ex)
                {
                    CoraUtils.PrintException(ex);
                }
                for (int i = 0; i < int.Parse(count); i++)
                {
                    try
                    {
                        int randomNumber = random.Next(buildingsCount);
                        LancherSuperWeapon(HouseClass.Player, HouseClass.Player.Ref.Buildings[0].Ref.BaseAbstract.GetThisPointer(), "IronCurtain");
                    }
                    catch (Exception ex)
                    {
                        CoraUtils.PrintException(ex);
                    }
                     yield return CoroutineUtils.WaitForSeconds(0.5);  // 等待下一帧再继续执行下一个铁幕
                }
            
        }

        [HandleProcessCorruptedStateExceptions]
        
        private static void OnChat(string userName, string content)
        {
            try
            {
                PrintMessage($"{userName} 说: {content}", ColorSchemeIndex.White, 150, true);
            }
            catch (Exception ex)
            {
                _ = ex;
            }

            switch (content)
            {
                case "666":
                    {
                        try
                        {
                            int giveMoney = 666;
                            string give_msg = $"{userName}发送666,获得资金: {giveMoney.ToString()}!";
                            GiveCurrentPlayerMoney(giveMoney);
                            int currentMoney = HouseClass.Player.Ref.Available_Money();
                            string total_msg = $"当前资金: {currentMoney}";
                            PrintMessage(give_msg, ColorSchemeIndex.Orange, 150);
                        }
                        catch (Exception ex)
                        {
                            _ = ex;
                            CoraUtils.PrintException(ex);
                        }
                    }
                    break;
                case "6":
                    {
                        try
                        {
                            int giveMoney = 60;
                            string give_msg = $"{userName}发送6,获得资金: {giveMoney.ToString()}!";
                            GiveCurrentPlayerMoney(giveMoney);
                            int currentMoney = HouseClass.Player.Ref.Available_Money();
                            string total_msg = $"当前资金: {currentMoney}";
                            PrintMessage(give_msg, ColorSchemeIndex.Orange, 150);
                        }
                        catch (Exception ex)
                        {
                            _ = ex;
                            CoraUtils.PrintException(ex);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        
        private static void OnLike(string userName, string count, string total)
        {
            try
            {
                int giveMoney = int.Parse(count) * 100;
                string give_msg = $"{userName}点赞{count}次,获得资金: {giveMoney * int.Parse(count)}!";
                GiveCurrentPlayerMoney(giveMoney);
                int currentMoney = HouseClass.Player.Ref.Available_Money();
                string total_msg = $"当前资金: {currentMoney}";
                PrintMessage(give_msg, ColorSchemeIndex.Orange, 150, true);
            }
            catch (Exception ex)
            {
                _ = ex;
                CoraUtils.PrintException(ex);
            }
        }

        #region  热键方法直接执行
        public static void GivePlayerMoney(int count)
        {
        
            PrintMessage($"给玩家资金{count}");
            HouseClass.Player.Ref.TransactMoney(count);
        } 
        private static bool bIsShowUnitName = false;
        public static void SwitchDisplayUnitNames()
        {
            bIsShowUnitName = !bIsShowUnitName;
        }


        public static void LauncherSuperWeaponAtMousePos(string sSuperWeaponName)
        {
            LancherSuperWeapon(HouseClass.Player, DisplayClass.Display_ZoneCell, sSuperWeaponName);
           
            PrintMessage($"发射{SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(sSuperWeaponName).Convert<AbstractTypeClass>().Ref.UIName} 地图坐标  X:{DisplayClass.Display_ZoneCell.X} Y:{DisplayClass.Display_ZoneCell.Y}");
        }
       
        public static void SwitchAIControl()
        {
            if (SessionClass.IsStandalone() || CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("IsOnlineBattleDangerFunctionEnable",false)){
                bIsAIControlEnable = !bIsAIControlEnable;
                AIControl(HouseClass.Player, bIsAIControlEnable);
            }else{
                CoraUtils.Log("已经禁止联机使用");
            }
           
        }
    
        public static void PlayVox()
        {
            VocClass.Speak("EVA_UnitReady");
        }
      
        public static void GenUnitRandom(bool GiveToEnemy = false)
        {
            int count = CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("GenUnitCount",5);
            string[] unitIDs = CoraUtils.MainToolsConfig("PlayerBaseConfig").GetList<string>("RandomInfantryUnitID");
            string[] unitIDs2 = CoraUtils.MainToolsConfig("PlayerBaseConfig").GetList<string>("RandomInfantryUnitID2");
            Random random = new Random();
            if (SessionClass.IsStandalone() || CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("IsOnlineBattleDangerFunctionEnable",false)){
                if (!GiveToEnemy)
                {
                    for (int i = 0; i < count; i++)
                    {
                        GenUnit(unitIDs[random.Next(unitIDs.Length)],HouseClass.Player);
                        GenUnit(unitIDs2[random.Next(unitIDs2.Length)],HouseClass.Player);
                    }
                    
                }else
                {
                    List<Pointer<HouseClass>> pAllEnemyHouses = HouseClass.GetTargetAllEnemyHouse(HouseClass.Player,  bFilterNeutral: true ,bFilterSpecial: true,bFilterDefeated: true);
                    if (pAllEnemyHouses.Count > 0)
                    {
                        Pointer<HouseClass> randomEnemyHouse = pAllEnemyHouses[new Random().Next(pAllEnemyHouses.Count)];
                        GenUnit(unitIDs[random.Next(unitIDs.Length)],randomEnemyHouse);
                        GenUnit(unitIDs2[random.Next(unitIDs2.Length)],randomEnemyHouse);
                    }
                }
            }else{
                CoraUtils.Log("已经禁止联机使用");
            }
            

        }

        
        #endregion


        #region 热键方法使用事件
        public static void SendLauncherSuperWeaponAtMousePosEvent(string sSuperWeaponName)
        {
            var cell = DisplayClass.Display_ZoneCell;
            Pointer<SuperWeaponTypeClass> pSWType = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(sSuperWeaponName);
            EventClass eventClass = EventClass.EventClass_CTOR();
            Pointer<EventClass> pEvent1 =  eventClass.EventClass_SpecialPlace(HouseClass.Player.Data.ArrayIndex,NetworkEvents.SpecialPlace,(int)pSWType.Ref.Type,ref cell);
            EventClass.AddEvent(pEvent1.Ref);
            PrintMessage($"发送发射{SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(sSuperWeaponName).Convert<AbstractTypeClass>().Ref.UIName} 地图坐标  X:{DisplayClass.Display_ZoneCell.X} Y:{DisplayClass.Display_ZoneCell.Y}的事件");
        }

        public static void SendProduceEvent(string ID){
            Pointer<TechnoTypeClass> pTechnoType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(ID);
            AbstractType abstractType = pTechnoType.Convert<AbstractClass>().Ref.WhatAmI();
            int nIsNaval = pTechnoType.Ref.IsNaval ? 1 : 0;
            string sUnitName = pTechnoType.Convert<AbstractTypeClass>().Ref.UIName;
           
            

            
            var pEvent1 = EventClass.EventClass_CTOR().EventClass_ProduceAbandonSuspend(
                HouseClass.Player.Data.ArrayIndex,
                NetworkEvents.Produce,
                abstractType,
                TechnoTypeClass.GetIndexByAbstractTypeAndID(abstractType, ID),
                pTechnoType.Ref.IsNaval 
            );
            EventClass.AddEvent(pEvent1.Ref);
            
            CoraUtils.Log($"发送生产事件: \nUnitName:{sUnitName} \nIsNaval:{nIsNaval} \nRTTIType:{abstractType}");
            
        }
        public static void SendPlaceEvent(string ID){
            Pointer<TechnoTypeClass> pTechnoType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(ID);
            AbstractType abstractType = pTechnoType.Convert<AbstractClass>().Ref.WhatAmI();
            var cell = DisplayClass.Display_ZoneCell;
            int nIsNaval = pTechnoType.Ref.IsNaval ? 1 : 0;
            string sUnitName = pTechnoType.Convert<AbstractTypeClass>().Ref.UIName;
            switch (abstractType)
            {
                case AbstractType.Unit:
                case AbstractType.UnitType:
                case AbstractType.Aircraft:
                case AbstractType.AircraftType:
                case AbstractType.Infantry:
                case AbstractType.InfantryType:
                case AbstractType.Building:
                case AbstractType.BuildingType:
                    {

                        
                        
                        var pEvent2 = EventClass.EventClass_CTOR().EventClass_Place(
                            HouseClass.Player.Data.ArrayIndex,
                            NetworkEvents.Place,
                            abstractType,
                            TechnoTypeClass.GetIndexByAbstractTypeAndID(abstractType, ID),
                            nIsNaval,
                            ref cell
                        );
                        
                        EventClass.AddEvent(pEvent2.Ref);
                        
                    }
                break;

                default:
                    CoraUtils.Log($"发送放置事件失败: \nUnitName:{sUnitName} \nIsNaval:{nIsNaval} \nRTTIType:{abstractType} 被忽略的类型:{abstractType}");
                break;
            }
            CoraUtils.Log($"发送放置事件: \nUnitName:{sUnitName} \nIsNaval:{nIsNaval} \nRTTIType:{abstractType}");
        }

     
        public static void SendCoraPlaceEventEx(string ID)
        {
            try
            {
                var cell = DisplayClass.Display_ZoneCell;
                Pointer<TechnoTypeClass> pTechnoType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(ID);
                AbstractType abstractType = pTechnoType.Convert<AbstractClass>().Ref.WhatAmI();
                int nIsNaval = pTechnoType.Ref.IsNaval ? 1 : 0;
                string sUnitName = pTechnoType.Convert<AbstractTypeClass>().Ref.UIName;
                switch (abstractType)
                {
                    case AbstractType.Unit:
                    case AbstractType.UnitType:
                    case AbstractType.Aircraft:
                    case AbstractType.AircraftType:
                    case AbstractType.Infantry:
                    case AbstractType.InfantryType:
                    case AbstractType.Building:
                    case AbstractType.BuildingType:
           
                        var event1 = EventClass.EventClass_CTOR();
                        event1.Type = (NetworkEvents)CoraNetworkEvents.CoraPlace;
                        event1.HouseIndex = (byte)HouseClass.Player.Data.ArrayIndex;
                        event1.Data = new EventData
                        {
                            Place = new Place
                            {
                                RTTIType = abstractType,
                                HeapID = TechnoTypeClass.GetIndexByAbstractTypeAndID(abstractType, ID),
                                IsNaval = nIsNaval,
                                Location = cell,
                                ExtraData = 0
                            }
                        };
                        event1.Frame = CoraUtils.TotalFramesElapsed;
                        EventClass.AddEvent(event1);
                       
                    break;
                    default:
                        CoraUtils.Log($"发送自定义放置事件失败: \nUnitName:{sUnitName} \nIsNaval:{nIsNaval} \nRTTIType:{abstractType} 被忽略的类型:{abstractType}");
                        break;
                }

                CoraUtils.Log($"发送自定义放置事件: \nUnitName:{sUnitName} \nIsNaval:{nIsNaval} \nRTTIType:{abstractType}");
            }catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
            
        }


        public static void SendDanmuEvent()
        {
            try
            {
                string nickname = "可乐";
                string msg = "咕咕嘎嘎";
                var event1 = EventClass.EventClass_CTOR();
                event1.Type = (NetworkEvents)CoraNetworkEvents.CoraDanmu;
                event1.HouseIndex = (byte)HouseClass.Player.Data.ArrayIndex;
                event1.Data = new EventData
                {
                    Danmu = new Danmu
                    {
                        Type = DanmuType.Message,
                        Nickname = new UniString(nickname),
                        Message = new UniString(msg),

                    }
                };
                event1.Frame = CoraUtils.TotalFramesElapsed;
                EventClass.AddEvent(event1);
            }catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
            
        }

        #endregion


    }
    

    #region 事件类
    unsafe class PlaceEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)NetworkEvents.Place;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "放置事件";

        public override uint Lenth => (uint)sizeof(EventData);


        // 实现事件响应逻辑
        protected override void Respond(Pointer<EventClass> pEvent, Pointer<EventData> pArg)
        {
            // 在这里处理接收到的事件
            var data = pArg.Ref;
            // pFactory.Ref.CompletedProduction();
            // 可以通过 pEvent 获取发送方信息
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            CoraUtils.LogEx($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} \n HeapID: {data.Place.HeapID} \nIsNaval:{data.Place.IsNaval} \nLocation:{data.Place.Location.X},{data.Place.Location.Y} \nRTTIType:{data.Place.RTTIType}");
            //游戏里面会创建FactoryClass 根据类型设置到HouseClass 的 primaryFactoryForXXX  
        }
    }


    unsafe class SpecialPlaceEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)NetworkEvents.SpecialPlace;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "超武放置事件";

        public override uint Lenth => (uint)sizeof(EventData);


        // 实现事件响应逻辑
        protected override void Respond(Pointer<EventClass> pEvent, Pointer<EventData> pArg)
        {
            // 在这里处理接收到的事件
            var data = pArg.Ref;
            // pFactory.Ref.CompletedProduction();
            // 可以通过 pEvent 获取发送方信息
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            
   
            Pointer<SuperWeaponTypeClass> pSuperWeaponTypeClass = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Array[data.SpecialPlace.ID]; 
            string superWeaponName = pSuperWeaponTypeClass.Convert<AbstractTypeClass>().Ref.UIName;
            string superWeaponID = pSuperWeaponTypeClass.Convert<AbstractTypeClass>().Ref.ID;
            CoraUtils.LogEx($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} \nID: {superWeaponID} \nName: {superWeaponName} \nLocation:{data.SpecialPlace.Location.X},{data.SpecialPlace.Location.Y}");
        }
    }
    //点击建筑栏的一个单位  
    //发送 SendProduceEvent  
    //处理事件 Produce   
    // 使用 TechnoTypeClass.GetByTypeAndIndex(abs,index)获取具体单位类型
    //创建FactoryClass并设置正在生产的单位类型为TechnoTypeClass.GetByTypeAndIndex(abs,index)
    //当生产完成时，FactoryClass调用CompletedProduction
    //
    unsafe class ProductEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)NetworkEvents.Produce;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "开始生产事件";

        public override uint Lenth => (uint)sizeof(EventData);


        // 实现事件响应逻辑
        protected override void Respond(Pointer<EventClass> pEvent, Pointer<EventData> pArg)
        {
            // 在这里处理接收到的事件
            var data = pArg.Ref;
            
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            var pTechnoType = TechnoTypeClass.GetByTypeAndIndex((AbstractType)data.Production.RTTI_ID, data.Production.Heap_ID);
            var pAbstractTypeClass = pTechnoType.Convert<AbstractTypeClass>();
            CoraUtils.LogEx($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} \n HeapID: {data.Production.Heap_ID} \nName: {pAbstractTypeClass.Ref.UIName} \nID: {pAbstractTypeClass.Ref.ID} \nIsNaval:{data.Production.IsNaval} \nRTTI_ID:{(AbstractType)data.Production.RTTI_ID}");
            
        }
    }

    unsafe class AbandonEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)NetworkEvents.Abandon;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "放弃生产事件";

        public override uint Lenth => (uint)sizeof(EventData);


        // 实现事件响应逻辑
        protected override void Respond(Pointer<EventClass> pEvent, Pointer<EventData> pArg)
        {
            // 在这里处理接收到的事件
            var data = pArg.Ref;
            
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            var pTechnoType = TechnoTypeClass.GetByTypeAndIndex((AbstractType)data.Production.RTTI_ID, data.Production.Heap_ID);
            var pAbstractTypeClass = pTechnoType.Convert<AbstractTypeClass>();
            CoraUtils.LogEx($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} \n HeapID: {data.Production.Heap_ID} \nName: {pAbstractTypeClass.Ref.UIName} \nID: {pAbstractTypeClass.Ref.ID} \nIsNaval:{data.Production.IsNaval} \nRTTI_ID:{(AbstractType)data.Production.RTTI_ID}");
            
        }
    }
   unsafe class AbandonAllEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)NetworkEvents.AbandonAll;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "放弃所有生产事件";

        public override uint Lenth => (uint)sizeof(EventData);


        // 实现事件响应逻辑
        protected override void Respond(Pointer<EventClass> pEvent, Pointer<EventData> pArg)
        {
            // 在这里处理接收到的事件
            var data = pArg.Ref;
            
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            var pTechnoType = TechnoTypeClass.GetByTypeAndIndex((AbstractType)data.Production.RTTI_ID, data.Production.Heap_ID);
            var pAbstractTypeClass = pTechnoType.Convert<AbstractTypeClass>();
            CoraUtils.LogEx($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} \n HeapID: {data.Production.Heap_ID} \nName: {pAbstractTypeClass.Ref.UIName} \nID: {pAbstractTypeClass.Ref.ID} \nIsNaval:{data.Production.IsNaval} \nRTTI_ID:{(AbstractType)data.Production.RTTI_ID}");
            
        }
    }
    unsafe class FrameEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)NetworkEvents.FrameSync;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "帧信息事件";

        public override uint Lenth => (uint)sizeof(EventData);


        // 实现事件响应逻辑
        protected override void Respond(Pointer<EventClass> pEvent, Pointer<EventData> pArg)
        {
            // 在这里处理接收到的事件
            var data = pArg.Ref;
            
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            CoraUtils.LogEx($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} \n CommandCount: {data.FrameInfo.CommandCount} \nCRC:{data.FrameInfo.CRC} \nDelay:{data.FrameInfo.Delay}");
            
        }
    }
    unsafe class AnimationEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)NetworkEvents.Animation;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "动画事件";

        public override uint Lenth => (uint)sizeof(EventData);


        // 实现事件响应逻辑
        protected override void Respond(Pointer<EventClass> pEvent, Pointer<EventData> pArg)
        {
            // 在这里处理接收到的事件
            var data = pArg.Ref;
            
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            CoraUtils.LogEx($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} \n ID: {data.Animation.ID} \nAnimOwner:{data.Animation.AnimOwner} \nLocation: {data.Animation.Location.X},{data.Animation.Location.Y}");
            
        }
    }


    unsafe class MegamissionEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)NetworkEvents.MegaMission;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "Megamission事件";

        public override uint Lenth => (uint)sizeof(EventData);


        // 实现事件响应逻辑
        protected override void Respond(Pointer<EventClass> pEvent, Pointer<EventData> pArg)
        {
            // 在这里处理接收到的事件
            var data = pArg.Ref;
            
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            
            if (data.MegaMission.Whom.m_RTTI != 0)
            {
                CoraUtils.Log("##########################################################");
                CoraUtils.Log($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} Whom \nm_ID:{data.MegaMission.Whom.m_ID} \nm_RTTI:{(AbstractType)data.MegaMission.Whom.m_RTTI}");
                DProcess(data.MegaMission.Whom);
                CoraUtils.Log("##########################################################");
            }
            if (data.MegaMission.Target.m_RTTI != 0)
            {
                CoraUtils.Log("##########################################################");
                CoraUtils.Log($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} Target \nm_ID:{data.MegaMission.Target.m_ID} \nm_RTTI:{(AbstractType)data.MegaMission.Target.m_RTTI}");
                DProcess(data.MegaMission.Target);
                CoraUtils.Log("##########################################################");
            }
            if (data.MegaMission.Destination.m_RTTI != 0)
            {
                CoraUtils.Log("##########################################################");
                CoraUtils.Log($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} Destination \nm_ID:{data.MegaMission.Destination.m_ID} \nm_RTTI:{(AbstractType)data.MegaMission.Destination.m_RTTI}");
                DProcess(data.MegaMission.Destination);
                CoraUtils.Log("##########################################################");
            }
                
        }       
        
        [HandleProcessCorruptedStateExceptions]
        void DProcess(TargetClass target)
        {
            AbstractType abstractType = (AbstractType)target.m_RTTI;
            switch (abstractType)
            {
                case AbstractType.Cell:
                    try
                    {
                        CellClass cell = target.UNPACK_Cell().Ref;
                        CoraUtils.Log($"Cell X:{cell.MapCoords.X} Y:{cell.MapCoords.Y}");
                    }
                    catch (Exception ex)
                    {
                        CoraUtils.PrintException(ex);
                    }
                    
                break;
                case AbstractType.Abstract:
                    try
                    {
                        AbstractClass  pAbstractClass = target.UNPACK_Abstract().Ref;
                        Pointer<TechnoClass> pTechnoClass = target.UNPACK_Abstract().Convert<TechnoClass>();
                        Pointer<AbstractTypeClass>  pAbstractTypeClass = pTechnoClass.Ref.Type.Convert<AbstractTypeClass>();
                        
                        CoraUtils.Log($"UIName:{pAbstractTypeClass.Ref.UIName}");
                    }
                    catch (Exception ex)
                    {
                        CoraUtils.PrintException(ex);
                    }
                break;
                default:
                break;
            }
        }
    }

    unsafe class DeployEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)NetworkEvents.Deploy;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "部署事件";

        public override uint Lenth => (uint)sizeof(EventData);


        // 实现事件响应逻辑
        protected override void Respond(Pointer<EventClass> pEvent, Pointer<EventData> pArg)
        {
            // 在这里处理接收到的事件
            var data = pArg.Ref;
            
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            CoraUtils.Log($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} ");
            
        }
    }

    //自定义事件，放置单位事件，包含单位类型、位置等信息
    unsafe class CoraPlaceEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)CoraNetworkEvents.CoraPlace;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "生成单位事件";

        public override uint Lenth => (uint)sizeof(EventData);
        

        // 实现事件响应逻辑
        protected override void Respond(Pointer<EventClass> pEvent, Pointer<EventData> pArg)
        {
            // 在这里处理接收到的事件
            var data = pArg.Ref;
            // pFactory.Ref.CompletedProduction();
            // 可以通过 pEvent 获取发送方信息
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            CoraUtils.LogEx($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} \n HeapID: {data.Place.HeapID} \nIsNaval:{data.Place.IsNaval} \nLocation:{data.Place.Location.X},{data.Place.Location.Y} \nRTTIType:{data.Place.RTTIType}");
            GenObj(senderHouseIndex, data.Place.HeapID, data.Place.IsNaval == 0 ? false : true, data.Place.Location, data.Place.RTTIType);
        }

        protected bool GenObj(int senderHouseIndex, int HeapID, bool IsNaval, CellStruct Location, AbstractType RTTIType)
        {
            var pTechnoType = TechnoTypeClass.GetByTypeAndIndex(RTTIType, HeapID);
            string sUnitName = pTechnoType.Convert<AbstractTypeClass>().Ref.UIName;
            Pointer<HouseClass> pHouse = HouseClass.Array[senderHouseIndex];
            var obj = pTechnoType.Ref.Base.CreateObject(pHouse);
            if (obj.IsNull)
            {
                MainTools.PrintMessage($"生成{sUnitName}失败 obj is NULL");
                return false;
            }
            Pointer<TechnoClass> pTechno  = obj.Convert<TechnoClass>();

            if (pTechno.IsNull)
            {
                MainTools.PrintMessage($"转换{sUnitName}失败 pTechno is NULL");
                return false;
            }
            TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);



            var isCreated = TechnoPlacer.PlaceTechnoNear( pTechno,Location);
            if (isCreated)
            {
                MainTools.PrintMessage($"创建{sUnitName}成功 在地图坐标  X:{Location.X} Y:{Location.Y}");
            }
            return true;
        }
    }

    unsafe class CoraDanmuEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)CoraNetworkEvents.CoraDanmu;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "弹幕事件";

        public override uint Lenth => (uint)sizeof(EventData);
        

        // 实现事件响应逻辑
        protected override void Respond(Pointer<EventClass> pEvent, Pointer<EventData> pArg)
        {
            // 在这里处理接收到的事件
            var data = pArg.Ref;
            // pFactory.Ref.CompletedProduction();
            // 可以通过 pEvent 获取发送方信息
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            ColorSchemeIndex color = (ColorSchemeIndex)HouseClass.Array[senderHouseIndex].Ref.Type.Ref.ColorSchemeIndex;
            switch(data.Danmu.Type){
                case DanmuType.Message:
                    MainTools.PrintMessage($"{data.Danmu.Nickname}: {data.Danmu.Message} ",color,150,true);
                break;
                case DanmuType.Gift:
                    MainTools.PrintMessage($"{data.Danmu.Nickname} 赠送 {data.Danmu.GiftName} * {data.Danmu.Count}  ",color,150,true);
                break;
                case DanmuType.Like:
                    MainTools.PrintMessage($"{data.Danmu.Nickname} 点赞 * {data.Danmu.Count}",ColorSchemeIndex.Green,150,true);
                break;

                case DanmuType.SuperChat:
                    MainTools.PrintMessage($"SuperChat{data.Danmu.Nickname}: {data.Danmu.Message} 金额: {data.Danmu.Count} ",color,150,true);
                break;
                case DanmuType.Private:
                    MainTools.PrintMessage($"{data.Danmu.Nickname}: @{data.Danmu.TergetNickname} {data.Danmu.Message} ",color,150,true);
                break;

                case DanmuType.EnterRoom:
                    MainTools.PrintMessage($"{data.Danmu.Nickname} 进入房间",color,150,true);
                    break;
                default:
                    break;

            }
            CoraUtils.LogEx($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} \nDanmuType:{data.Danmu.Type} \nNikename: {data.Danmu.Nickname} \nTargetNikename:{data.Danmu.TergetNickname} \nMessage:{data.Danmu.Message} \nGiftName:{data.Danmu.GiftName} \nCount:{data.Danmu.Count}");
            
        }

    }

    #endregion

    #region 事件数据结构

    public enum CoraNetworkEvents : byte
    {
        CoraPlace = 0x65,

        CoraDanmu = 0x70,
      
    }

    
    #endregion
     
}