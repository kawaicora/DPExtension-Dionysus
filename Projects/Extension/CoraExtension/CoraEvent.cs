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
            // #region 网络事件注册
            PlaceEventRsp placeEventRsp = new PlaceEventRsp();
            Network.NetworkHandles.Add((byte)NetworkEvents.Place, new PlaceEventRsp());
            Network.NetworkHandles.Add((byte)NetworkEvents.Produce, new ProductEventRsp());
            Network.NetworkHandles.Add((byte)NetworkEvents.Abandon, new AbandonEventRsp());
            Network.NetworkHandles.Add((byte)NetworkEvents.AbandonAll,  new AbandonAllEventRsp());
            Network.NetworkHandles.Add((byte)NetworkEvents.SpecialPlace,new SpecialPlaceEventRsp());
            Network.NetworkHandles.Add((byte)NetworkEvents.MegaMission,new MegamissionEventRsp());
            Network.NetworkHandles.Add((byte)NetworkEvents.Deploy,new DeployEventRsp());
            
            TimingEventRsp timingEventRsp  = new TimingEventRsp();
            Network.NetworkHandles.Add(timingEventRsp.Index,timingEventRsp);
            ProcessTimeEventRsp processTimeEventRsp = new ProcessTimeEventRsp();
            Network.NetworkHandles.Add(processTimeEventRsp.Index,processTimeEventRsp);
            // #endregion
            #region 自定义事件
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
                Logger.Log("##########################################################");
                Logger.Log($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} Whom \nm_ID:{data.MegaMission.Whom.m_ID} \nm_RTTI:{(AbstractType)data.MegaMission.Whom.m_RTTI}");
                DProcess(data.MegaMission.Whom);
                Logger.Log("##########################################################");
            }
            if (data.MegaMission.Target.m_RTTI != 0)
            {
                Logger.Log("##########################################################");
                Logger.Log($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} Target \nm_ID:{data.MegaMission.Target.m_ID} \nm_RTTI:{(AbstractType)data.MegaMission.Target.m_RTTI}");
                DProcess(data.MegaMission.Target);
                Logger.Log("##########################################################");
            }
            if (data.MegaMission.Destination.m_RTTI != 0)
            {
                Logger.Log("##########################################################");
                Logger.Log($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} Destination \nm_ID:{data.MegaMission.Destination.m_ID} \nm_RTTI:{(AbstractType)data.MegaMission.Destination.m_RTTI}");
                DProcess(data.MegaMission.Destination);
                Logger.Log("##########################################################");
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
                        Logger.Log($"Cell X:{cell.MapCoords.X} Y:{cell.MapCoords.Y}");
                    }
                    catch (Exception ex)
                    {
                        Logger.PrintException(ex);
                    }
                    
                break;
                case AbstractType.Abstract:
                    try
                    {
                        AbstractClass  pAbstractClass = target.UNPACK_Abstract().Ref;
                        Pointer<TechnoClass> pTechnoClass = target.UNPACK_Abstract().Convert<TechnoClass>();
                        Pointer<AbstractTypeClass>  pAbstractTypeClass = pTechnoClass.Ref.Type.Convert<AbstractTypeClass>();
                        
                        Logger.Log($"UIName:{pAbstractTypeClass.Ref.UIName}");
                    }
                    catch (Exception ex)
                    {
                        Logger.PrintException(ex);
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
            Logger.Log($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} ");
            
        }
    }

    unsafe class ProcessTimeEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)NetworkEvents.ProcessTime;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "进度事件";

        public override uint Lenth => (uint)sizeof(EventData);


        // 实现事件响应逻辑
        protected override void Respond(Pointer<EventClass> pEvent, Pointer<EventData> pArg)
        {
            // 在这里处理接收到的事件
            var data = pArg.Ref;
            
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            Logger.Log($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} ");
            
        }
    }
    unsafe class TimingEventRsp : NetworkHandle<EventData>
    {
        // 定义事件索引（确保唯一）
        public override byte Index => (byte)NetworkEvents.Timing;
        
        // 定义数据长度
        
        // 定义事件名称（用于调试）
        public override string Name => "定时事件";

        public override uint Lenth => (uint)sizeof(EventData);


        // 实现事件响应逻辑
        protected override void Respond(Pointer<EventClass> pEvent, Pointer<EventData> pArg)
        {
            // 在这里处理接收到的事件
            var data = pArg.Ref;
            
            var senderHouseIndex = pEvent.Ref.HouseIndex;
            var frame = pEvent.Ref.Frame; 
            Logger.Log($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} ");
            
        }
    }

    
    #endregion

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
            pTechno.Convert<AbstractClass>().CastIf(AbstractType.Building, out Pointer<BuildingClass> pBuilding);
            if (!pBuilding.IsNull)
            {
                pBuilding.Ref.Base.BaseMission.QueueMission(Mission.Construction, false);
            }
            else
            {
                var Guard = !pBuilding.IsNull || pHouse.Ref.ControlledByHuman();
                var mission = Guard ? Mission.Guard : Mission.Hunt;
                pTechno.Ref.BaseMission.QueueMission(mission, false);
            }
           
            
            var isPut = pTechno.Ref.Base.Put(CellClass.Cell2Coord(placeCoords), (Direction)7u);
            if (isPut)
            {
                Logger.Log($"创建{sUnitName}成功 在地图坐标  X:{placeCoords.X} Y:{placeCoords.Y}");
                return true;
            }
            else
            {
                pTechno.Ref.Base.UnInit();
                return false;
            }
           
            
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
                
                Logger.Log("FireSuperWeapon({2}):0x({3:X}) -> ({0}, {1})", location.X, location.Y, pSuperWeaponTypeClass.Ref.Base.ID, (int)pSuper);
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
            pFactory.Ref.Production.Step = 54;
            
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