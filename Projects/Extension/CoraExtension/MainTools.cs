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
            CoraLogger.HookLogger();
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
            
           
           


            if (CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("StartDanmu",false))
            {
                _coroutineSystem.StartCoroutine(DanmuWebSocketInit());  //全局无需清除
            }
            #region 回调注册
            HouseExt.CreateCallback = (house) =>
            {
                if (house.IsNull)
                {
                    Logger.Log("HouseExt CreateCallback 已经执行, 但是 house 是 null QAQ");
                    return;
                }
                if (house.Ref.Type.IsNull)
                {
                    Logger.Log("HouseExt CreateCallback 已经执行, 但是 house的Type 是 null QAQ");
                    return;
                }
                if (house == HouseClass.Player)
                {
                    Logger.Log($"玩家 {house.Ref.Type.Ref.Base.UIName}:{house.Ref.Type.Ref.Base.ID}的HouseClass实例被创建了！");
                }
                else
                {
                    string targetTag = house.Ref.ControlledByHuman() ? "远程玩家" : "AI";
                    string enemyTag = house.Ref.IsAlliedWith(HouseClass.Player)? "盟友":"敌人";
                    Logger.Log($"{enemyTag} {targetTag} {house.Ref.Type.Ref.Base.UIName}:{house.Ref.Type.Ref.Base.ID}的HouseClass实例被创建了！");
                }
            };

         
            FactoryClass.ProgressUpdateCallback += (factory) =>
            {
                if (factory.IsNull)
                {
                    Logger.Log("建造工厂类ProgressAddCallback 已经执行, 但是 factory 是 null QAQ");
                    return;
                }
                if (factory.Ref.Owner.IsNull)
                {
                    Logger.Log("建造工厂类ProgressAddCallback 已经执行, 但是工厂的所有者是null QAQ");
                    
                    return;
                }

                if (factory.Ref.Owner  == HouseClass.Player)
                {
                    if (CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("InstantConstruction", false))
                    {
                        AbstractType abstractType = factory.Ref.Object.Ref.BaseAbstract.WhatAmI();
                        string id = factory.Ref.Object.Ref.Type.Convert<AbstractTypeClass>().Ref.ID;
                        bool isNaval = factory.Ref.Object.Ref.Type.Ref.IsNaval;
                        int index = TechnoTypeClass.GetIndexByAbstractTypeAndID(abstractType, id);
                        NetworkHandle<Production>.Send(
                            (byte)CoraNetworkEvents.CoraProdutionComplete,
                            new Production
                            {
                                RTTI_ID = (int)abstractType,
                                Heap_ID = index,
                                IsNaval = isNaval ? 1 : 0,
                            }
                        );
                    }
                    // Logger.Log($"玩家工厂Duration: {factory.Ref.Production.Timer.Duration}");
                }else
                {
                    string targetTag = factory.Ref.Owner.Ref.ControlledByHuman() ? "远程玩家" : "AI";
                    string enemyTag = factory.Ref.Owner.Ref.IsAlliedWith(HouseClass.Player)? "盟友":"敌人";
                    // Logger.Log($"{targetTag} {enemyTag} 工厂Duration: {factory.Ref.Production.Timer.Duration}");
                }
            };
         
            // 在工厂类被创建时触发的回调
            FactoryClass.CreateCallback += (factory) =>
            {
                if (factory.IsNull)
                {
                    Logger.Log("建造工厂类CreateCallback 已经执行, 但是 factory 是 null QAQ");
                    return;
                }
                if (factory.Ref.Owner.IsNull)
                {
                    Logger.Log("建造工厂类CreateCallback 已经执行, 但是工厂的所有者是null QAQ");
                    return;
                }
                

                if (factory.Ref.Owner == HouseClass.Player)
                {
                    // Logger.Log($"玩家的工厂类被创建了！所属阵营：{factory.Ref.Owner.Ref.Type.Ref.Base.ID} ^w^");
                    MessageListClass.Instance.PrintMessage($"玩家 {factory.Ref.Owner.Ref.Type.Ref.Base.UIName}   开始建造 {factory.Ref.Object.Ref.Type.Convert<AbstractTypeClass>().Ref.UIName } ^w^", (ColorSchemeIndex)factory.Ref.Owner.Ref.Type.Ref.ColorSchemeIndex,3*60);
                    
                }
                else
                {
                    
                    string targetTag = factory.Ref.Owner.Ref.ControlledByHuman() ? "远程玩家" : "AI";
                    string enemyTag = factory.Ref.Owner.Ref.IsAlliedWith(HouseClass.Player)? "盟友":"敌人";
                    // Logger.Log($"{enemyTag} {targetTag} 的工厂类被创建了！所属阵营：{factory.Ref.Owner.Ref.Type.Ref.Base.ID} ^w^");
                    MessageListClass.Instance.PrintMessage($"{enemyTag} {targetTag } {factory.Ref.Owner.Ref.Type.Ref.Base.UIName}  开始建造 {factory.Ref.Object.Ref.Type.Convert<AbstractTypeClass>().Ref.UIName}  ^w^", (ColorSchemeIndex)factory.Ref.Owner.Ref.Type.Ref.ColorSchemeIndex,3*60);
                    
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
            
            Logger.Log("MainTools CTOR");
        }

        private void Start()
        {
            try
            {
                Logger.Log($"MainTools Started");
            }catch (Exception ex)
            {
                Logger.PrintException(ex);
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
                Game.CurrentFrameRate, 
                Game.GetAverageFrameRate(),
                Game.TotalFramesElapsed);

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
        static int m_MouseDownCount = 0;

        #region GameBattleFirstResume
     
        [HandleProcessCorruptedStateExceptions]
        public void GameBattleFirstResume()
        {
            try
            {
                bIsAIControlEnable = CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("AIControl",false);
                
                bIsShowUnitName = CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("ShowUnitName",false);
                CoraEventUtils.RegisterEvent();
                #region 注册弹幕事件
                DouyinDanmuWebSocket.instance.OnLike += OnLike;
                DouyinDanmuWebSocket.instance.OnChat += OnChat;
                DouyinDanmuWebSocket.instance.OnGift += OnGift;
                DouyinDanmuWebSocket.instance.JoinRoom += JoinRoom;
                #endregion
                if (SessionClass.IsStandalone() || CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("IsOnlineBattleDangerFunctionEnable",false))
                {
                    if (CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("SuperweaponNoWait",false))
                    {
                        coroutines.Add(_coroutineSystem.StartCoroutine(ChargeSuperWeapon()));
                    }
                    if (CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("InitialMoney",0) > 0)
                    {
                        HouseClass.Player.Ref.GiveMoney(CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("InitialMoney",0));    //联机不可用 延迟奔溃
                    }
                    
                    AIControl(HouseClass.Player,bIsAIControlEnable);

                }
                
                if(!SessionClass.IsStandalone()){
                    MessageListClass.Instance.PrintMessage("联机模式!!",ColorSchemeIndex.Green,3*60);
                    int iPlayerCount = CoraUtils.GetPlayerCount();
                    MessageListClass.Instance.PrintMessage($"联机玩家数量: {iPlayerCount}");
                    Logger.Log($"联机玩家数量: {iPlayerCount}");
              
                    CoraUtils.GetPlayerNameList((sPlayerName) =>
                    {
                        MessageListClass.Instance.PrintMessage($"玩家名字:{sPlayerName}");
                        Logger.Log($"玩家名字:{sPlayerName}");
                    }
                    );
                }
                else
                {
                    switch(SessionClass.Instance.GameMode)
                    {
                        case GameMode.Campaign:
                            MessageListClass.Instance.PrintMessage("战役模式!!",ColorSchemeIndex.Green,3*60);
                            break;
                        case GameMode.Skirmish:
                            MessageListClass.Instance.PrintMessage("单人对战模式!!",ColorSchemeIndex.Green,3*60);
                            break;
                        default:
                            MessageListClass.Instance.PrintMessage($"未知游戏模式 {SessionClass.Instance.GameMode}!!",ColorSchemeIndex.Green,3*60);
                            break;
                    }
                }
    
                coroutines.Add(GlobalHotkey.Start(_coroutineSystem)); //全局热键监听协程

                var hotkeyConfig = CoraUtils.MainToolsConfig("HotkeyConfig");
                CoraUtils.ParseHotkeyConfig(hotkeyConfig.GetBuffer().Unparsed);

                Logger.Log($"MainTools GameBattleFirstResume");
                
                
            }
            catch(Exception ex)
            {
                Logger.PrintException(ex);
            }
            
            //在组件被创建时执行一次
        }

        private IEnumerator ChargeSuperWeapon()
        {
            while (true)
            {
                for (var index = 0 ; index < HouseClass.Player.Ref.Supers.Count;index++)
                {
                    if (HouseClass.Player.Ref.Supers[index].IsNull)
                    {
                        yield return new WaitForFrames(5);
                        continue;
                    }
                    if (HouseClass.Player.Ref.Supers[index].Ref.IsCharged)
                    {
                        yield return new WaitForFrames(5);
                        continue;
                    }
                    NetworkHandle<UnknownTuple>.Send(
                        (byte)CoraNetworkEvents.CoraSpecialCharge,
                        new UnknownTuple
                        {
                            Unknown_0 = index,

                        }
                    );
                    
                    yield return new WaitForFrames(5);
                }
               
            }
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
            CoraEventUtils.UnregisterEvent();
            // 注销弹幕事件
            DouyinDanmuWebSocket.instance.OnLike -= OnLike;
            DouyinDanmuWebSocket.instance.OnChat -= OnChat;
            DouyinDanmuWebSocket.instance.OnGift -= OnGift;
            DouyinDanmuWebSocket.instance.JoinRoom -= JoinRoom;
     
            foreach (var item in coroutines)
            {
                _coroutineSystem.StopCoroutine(item);
            }

            Logger.Log($"MainTools Destroyed");

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
                MessageListClass.Instance.PrintMessage("AIControl");
            }
            else
            {
                house.Ref.CurrentPlayer = true; // 必须在AiControl之前设置flase，否则会导致AI无法正常运作
                HouseClass.Player.Ref.IQLevel = CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("IQLevel",(uint)0);  
                HouseClass.Player.Ref.IQLevel2 = CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("IQLevel2",(uint)0);
                house.Ref.AutocreateAllowed = 0; // 禁止自动生产
                MessageListClass.Instance.PrintMessage("PlayerControl");
            }
            
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
                Logger.PrintException(ex);
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
                    Logger.Log($"发射失败：未找到超级武器类型 {superWeaponName}");
                    return;
                }
                Pointer<SuperClass> pSuper = houseClass.Ref.FindSuperWeapon(pSWType);
                if (pSuper.IsNull)
                {
                    Logger.Log($"发射失败：HouseClass未拥有超级武器 {superWeaponName}");
                    return;
                }
                
                Logger.Log("FireSuperWeapon({2}):0x({3:X}) -> ({0}, {1})", pTarget.X, pTarget.Y, pSWType.Ref.Base.ID, (int)pSuper);
                pSuper.Ref.SetCharge(100);
                pSuper.Ref.IsCharged = true;
                pSuper.Ref.Launch(pTarget, houseClass.Ref.PlayerControl);
                pSuper.Ref.IsCharged = false;
            }
            catch (Exception ex)
            {
                _ = ex;
                Logger.PrintException(ex);
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
                Logger.Log("当前无敌方阵营，无法攻击敌人");
                return;
            }
            
            int randomAiHouseNumber = random.Next(enemyHousePtrList.Count);   
            var targetHouse = enemyHousePtrList[randomAiHouseNumber];
            if (targetHouse.Ref.Buildings.Count == 0)
            {
                Logger.Log($"目标阵营 {targetHouse.Ref.Type.Ref.Base.UIName} 没有建筑，无法攻击");
                return;
            }
            int buildingsCount = targetHouse.Ref.Buildings.Count();
            var targetBuilding = targetHouse.Ref.Buildings[random.Next(buildingsCount)];

            LancherSuperWeapon(HouseClass.Player, targetBuilding.Ref.BaseAbstract.GetThisPointer(), superWeaponName);
            return;
            
        }

    

       
        #region 弹幕相关方法
        [HandleProcessCorruptedStateExceptions]
        
        private void JoinRoom(string userName)
        {
           
        }


        [HandleProcessCorruptedStateExceptions]
        
        private static void OnGift(string userName, string giftName, string count)
        {
           
        }

        

        [HandleProcessCorruptedStateExceptions]
        
        private static void OnChat(string userName, string content)
        {
           
        }

        [HandleProcessCorruptedStateExceptions]
        
        private static void OnLike(string userName, string count, string total)
        {
            
        }


        #endregion


        #region  热键方法直接执行

        private static bool bIsShowUnitName = false;
        public static void SwitchDisplayUnitNames()
        {
            bIsShowUnitName = !bIsShowUnitName;
        }



        public static void SwitchAIControl()
        {
            if (SessionClass.IsStandalone() || CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("IsOnlineBattleDangerFunctionEnable",false)){
                bIsAIControlEnable = !bIsAIControlEnable;
                AIControl(HouseClass.Player, bIsAIControlEnable);
            }else{
                Logger.Log("已经禁止联机使用");
            }
           
        }
    
        
        public static void GenUnitRandom(bool GiveToEnemy = false)
        {
           
            int count = CoraUtils.MainToolsConfig("PlayerBaseConfig").Get("GenUnitCount",5);
            
            string[] unitIDs = CoraUtils.MainToolsConfig("PlayerBaseConfig").GetList<string>("RandomInfantryUnitID");
            string[] unitIDs2 = CoraUtils.MainToolsConfig("PlayerBaseConfig").GetList<string>("RandomInfantryUnitID2");
            string[] MargedUnitList = unitIDs.Concat(unitIDs2).ToArray();
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                SendCoraPlaceEvent(MargedUnitList[random.Next(MargedUnitList.Length)]);
              
            }
        
           
        }
      
        #endregion

        #region 热键方法使用事件


        public static void SendChangeMoneyEvent(int money)
        {
            NetworkHandle<UnknownTuple>.Send(
                (byte)CoraNetworkEvents.CoraMoneyChange,
                new UnknownTuple
                {
                    Unknown_0 = money      
                }
            );
            MessageListClass.Instance.PrintMessage($"发送资金修改事件: {money}");
        }
        public static void SendCoraSpecialPlaceEvent(string id)
        {
            var cell = DisplayClass.Display_ZoneCell;
            NetworkHandle<SpecialPlace>.Send(
                
                (byte)CoraNetworkEvents.CoraSpecialPlace,
                new SpecialPlace
                {
                    ID = SuperWeaponTypeClass.GetIndexByID(id),
                    Location = cell
                }
            );
            MessageListClass.Instance.PrintMessage($"发送放置超武事件:{SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(id).Convert<AbstractTypeClass>().Ref.UIName} 地图坐标  X:{DisplayClass.Display_ZoneCell.X} Y:{DisplayClass.Display_ZoneCell.Y}");
        }

        public static void SendCoraPlaceEvent(string ID)
        {
            
            try
            {
                var cell = DisplayClass.Display_ZoneCell;
                Pointer<TechnoTypeClass> pTechnoType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(ID);
                AbstractType abstractType = pTechnoType.Convert<AbstractClass>().Ref.WhatAmI();
                int nIsNaval = pTechnoType.Ref.IsNaval ? 1 : 0;
                string sUnitName = pTechnoType.Convert<AbstractTypeClass>().Ref.UIName;
              
                NetworkHandle<Place>.Send(
                    (byte)CoraNetworkEvents.CoraPlace,
                    new Place
                    {
                        RTTIType = abstractType,
                        HeapID = TechnoTypeClass.GetIndexByAbstractTypeAndID(abstractType, ID),
                        IsNaval = nIsNaval,
                        Location = cell
                    }
                );
                

                Logger.Log($"发送自定义放置事件: \nUnitName:{sUnitName} \nIsNaval:{nIsNaval} \nRTTIType:{abstractType}");
            }catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        
        public static void SendProduceEvent(string ID){
            Pointer<TechnoTypeClass> pTechnoType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(ID);
            AbstractType abstractType = pTechnoType.Convert<AbstractClass>().Ref.WhatAmI();
            int nIsNaval = pTechnoType.Ref.IsNaval ? 1 : 0;
            string sUnitName = pTechnoType.Convert<AbstractTypeClass>().Ref.UIName;
           
            NetworkHandle<Production>.Send(
                (byte)NetworkEvents.Produce,
                new Production
                {
                    RTTI_ID = (int)abstractType,
                    Heap_ID = TechnoTypeClass.GetIndexByAbstractTypeAndID(abstractType, ID),
                    IsNaval = nIsNaval
                }
            );
            Logger.Log($"发送生产事件: \nUnitName:{sUnitName} \nIsNaval:{nIsNaval} \nRTTIType:{abstractType}");
            
        }
     


        #endregion


    }
    

}