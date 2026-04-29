using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.ExceptionServices;
using DynamicPatcher;
using Extension.Coroutines;
using Extension.Ext;
using PatcherYRpp;
using PatcherYRpp.Utilities;

namespace Extension.CoraExtension
{
    class CoraEventUtils
    {
       
        public static void RegisterEvent()
        {
            #region 自定义事件注册
            Network.NetworkHandles.Add((byte)CoraNetworkEvents.CoraPlace,new CoraPlaceEventRsp());
            Network.NetworkHandles.Add((byte)CoraNetworkEvents.CoraSpecialPlace,new CoraSpecialPlaceEventRsp());
            Network.NetworkHandles.Add((byte)CoraNetworkEvents.CoraProduceComple,new CoraProduceCompleEventRsp());
            Network.NetworkHandles.Add((byte)CoraNetworkEvents.CoraMoneyChange,new CoraMoneyChangeEventRsp());
            Network.NetworkHandles.Add((byte)CoraNetworkEvents.CoraSpecialCharge,new CoraSpecialChargeEventRsp());
            #endregion

        }
          public static void UnregisterEvent()
        {
            Network.NetworkHandles.Clear();
        }

    }
  
    

    #region 自定义事件

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
            // 可以通过 pEvent 获取发送方信息
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            CoraUtils.LogEx($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} \n HeapID: {data.Place.HeapID} \nIsNaval:{data.Place.IsNaval} \nLocation:{data.Place.Location.X},{data.Place.Location.Y} \nRTTIType:{data.Place.RTTIType}");
            try
            {
                DoPlace(senderHouseIndex, data.Place.HeapID, data.Place.IsNaval == 0 ? false : true, data.Place.Location, data.Place.RTTIType);
            }catch (Exception ex){
                Logger.PrintException(ex);
            }
            
        }

        protected bool DoPlace(int senderHouseIndex, int HeapID, bool IsNaval, CellStruct placeCoords, AbstractType RTTIType)
        {
            var pTechnoType = TechnoTypeClass.GetByTypeAndIndex(RTTIType, HeapID);
            string sUnitName = pTechnoType.Convert<AbstractTypeClass>().Ref.UIName;
            Pointer<HouseClass> pHouse = HouseClass.Array[senderHouseIndex];
            var obj = pTechnoType.Ref.Base.CreateObject(pHouse);
            if (obj.IsNull)
            {
                Logger.Log($"生成{sUnitName}失败 obj is NULL");
                return false;
            }
            Pointer<TechnoClass> pTechno  = obj.Convert<TechnoClass>();
            if (pTechno.IsNull)
            {
                Logger.Log($"生成{sUnitName}失败 pTechno is NULL");
                return false;
            }
            return TechnoPlacer.PlaceTechnoNear(pTechno,placeCoords,true);
            
        }
    }

    unsafe class CoraMoneyChangeEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)CoraNetworkEvents.CoraMoneyChange;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "自定义生产完成事件";

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

            int money = data.Unknown_Tuple.Unknown_0;
            try
            {
                DoMoneyChange(pEvent.Ref.HouseIndex,money);
            }catch (Exception ex){
                Logger.PrintException(ex);
            }
            
            
            
   
        }
        protected bool DoMoneyChange(int senderHouseIndex, int money)
        {
            
            Pointer<HouseClass> pHouse = HouseClass.Array[senderHouseIndex];
            pHouse.Ref.TransactMoney(money);
            return true;

        }
    }
    unsafe class CoraSpecialPlaceEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)CoraNetworkEvents.CoraSpecialPlace;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "自定义超武放置事件";

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
            try
            {
                DoSpecialPlace(senderHouseIndex,data.SpecialPlace.ID,data.SpecialPlace.Location);
            }catch (Exception ex){
                Logger.PrintException(ex);
            }
            
            CoraUtils.LogEx($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} \nID: {superWeaponID} \nName: {superWeaponName} \nLocation:{data.SpecialPlace.Location.X},{data.SpecialPlace.Location.Y}");
        }

        protected void DoSpecialPlace(int senderHouseIndex,int id,CellStruct location)
        {
            try
            {
                Pointer<SuperWeaponTypeClass> pSuperWeaponTypeClass = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Array[id]; 
                string superWeaponName = pSuperWeaponTypeClass.Convert<AbstractTypeClass>().Ref.UIName;
                string superWeaponID = pSuperWeaponTypeClass.Convert<AbstractTypeClass>().Ref.ID;
          
                Pointer<HouseClass> pHouse = HouseClass.Array[senderHouseIndex];
          
                if (pSuperWeaponTypeClass.IsNull)
                {
                    Logger.Log($"发射失败：未找到超级武器类型 {superWeaponName}");
                    return;
                }
                Pointer<SuperClass> pSuper = pHouse.Ref.FindSuperWeapon(pSuperWeaponTypeClass);
                if (pSuper.IsNull)
                {
                    Logger.Log($"发射失败：HouseClass未拥有超级武器 {superWeaponName}");
                    return;
                }
                
                pSuper.Ref.SetCharge(100);
                pSuper.Ref.IsCharged = true;
                pSuper.Ref.Launch(location, pHouse.Ref.PlayerControl);
                pSuper.Ref.IsCharged = false;
            }
            catch (Exception ex)
            {
                _ = ex;
                Logger.PrintException(ex);
            }
        }
    }


    

    unsafe class CoraSpecialChargeEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)CoraNetworkEvents.CoraSpecialCharge;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "自定义生产完成事件";

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

            try
            {
                DoSpecialCharge(senderHouseIndex,data.Unknown_Tuple.Unknown_0);
            }catch (Exception ex){
                Logger.PrintException(ex);
            }
        }
        protected bool DoSpecialCharge(int senderHouseIndex,int id)
        {
            Pointer<SuperWeaponTypeClass> pSuperWeaponTypeClass = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Array[id]; 
            string superWeaponName = pSuperWeaponTypeClass.Convert<AbstractTypeClass>().Ref.UIName;
            string superWeaponID = pSuperWeaponTypeClass.Convert<AbstractTypeClass>().Ref.ID;
        
            Pointer<HouseClass> pHouse = HouseClass.Array[senderHouseIndex];
        
            if (pSuperWeaponTypeClass.IsNull)
            {
                Logger.Log($"充能失败：未找到超级武器类型 {superWeaponName}");
                return false;
            }
            Pointer<SuperClass> pSuper = pHouse.Ref.FindSuperWeapon(pSuperWeaponTypeClass);
            if (pSuper.IsNull)
            {
                Logger.Log($"充能失败：HouseClass未拥有超级武器 {superWeaponName}");
                return false;
            }
            pSuper.Ref.SetCharge(100);
            pSuper.Ref.IsCharged = true;
            return true;
        }
    }

    unsafe class CoraProduceCompleEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)CoraNetworkEvents.CoraProduceComple;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "自定义生产完成事件";

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

            try
            {
                DoProduceComple(pEvent.Ref.HouseIndex,(AbstractType)data.Production.RTTI_ID,data.Production.Heap_ID,(data.Production.IsNaval == 1 )?true:false);
            }catch (Exception ex){
                Logger.PrintException(ex);
            }
        }
        protected bool DoProduceComple(int senderHouseIndex, AbstractType abs, int heapId,bool isNaval)
        {
            Pointer<TechnoTypeClass> pTechnoType = TechnoTypeClass.GetByTypeAndIndex(abs,heapId);
            Pointer<BuildingTypeClass> pBuilding =  pTechnoType.Convert<BuildingTypeClass>();
            Pointer<HouseClass> pHouse = HouseClass.Array[senderHouseIndex];
            Pointer<FactoryClass> pFactory = Pointer<FactoryClass>.Zero ;
            switch (abs)
            {
                case AbstractType.Unit:
                case AbstractType.UnitType:
                    if (isNaval)
                    {
                        pFactory = pHouse.Ref.PrimaryForShips;
                    }
                    else
                    {
                        pFactory = pHouse.Ref.PrimaryForVehicles;   
                    }
                break;
                case AbstractType.Aircraft:
                case AbstractType.AircraftType:
                    pFactory = pHouse.Ref.PrimaryForAircraft;   
                break;
                case AbstractType.Building:
                case AbstractType.BuildingType:

                    if(pBuilding.Ref.BuildCat == BuildCat.Combat)
                    {
                        pFactory = pHouse.Ref.PrimaryForDefenses;
                    }
                    else
                    {
                        pFactory = pHouse.Ref.PrimaryForBuildings;
                    }
                break;
                case AbstractType.Infantry:
                case AbstractType.InfantryType:
                    pFactory = pHouse.Ref.PrimaryForInfantry;
                break;
                
                    
                    
            }

            if (pFactory.IsNull)
            {
                return false;   
            }
            // pFactory.Ref.Production.Step = 54;
            
            return true;

        }
    }

    
    

    #endregion
    

    #region 自定义事件类型

    public enum CoraNetworkEvents : byte
    {
        CoraPlace = 0x30,
        CoraSpecialPlace = 0x31,
        CoraProduceComple = 0x32,
        CoraMoneyChange = 0x33,
        CoraSpecialCharge = 0x34,
      
    }

    
    #endregion
     
}