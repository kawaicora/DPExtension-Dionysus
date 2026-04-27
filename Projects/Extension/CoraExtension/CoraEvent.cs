using System;
using System.Runtime.ExceptionServices;
using Extension.Ext;
using PatcherYRpp;

namespace Extension.CoraExtension
{
    

    class CoraEvent
    {
        public static void RegisterEvent()
        {
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
            
            TimingEventRsp timingEventRsp  = new TimingEventRsp();
            Network.NetworkHandles.Add(timingEventRsp.Index,timingEventRsp);
            ProcessTimeEventRsp processTimeEventRsp = new ProcessTimeEventRsp();
            Network.NetworkHandles.Add(processTimeEventRsp.Index,processTimeEventRsp);
            #endregion
            #region 自定义事件
            CoraPlaceEventRsp coraPlaceEventRsp = new CoraPlaceEventRsp();
            Network.NetworkHandles.Add(coraPlaceEventRsp.Index,coraPlaceEventRsp);
            CoraDanmuEventRsp coraDanmuEventRsp = new CoraDanmuEventRsp();
            Network.NetworkHandles.Add(coraDanmuEventRsp.Index,coraDanmuEventRsp);
            CoraSpecialPlaceEventRsp coraSpecialPlaceEventRsp = new CoraSpecialPlaceEventRsp();
            Network.NetworkHandles.Add(coraSpecialPlaceEventRsp.Index,coraSpecialPlaceEventRsp);
            #endregion

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
            CoraUtils.Log($"Received event: {Name} with data: \nFrame:{frame} \nSenderHouseIndex:{senderHouseIndex} ");
            
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
            PlaceTechno(senderHouseIndex, data.Place.HeapID, data.Place.IsNaval == 0 ? false : true, data.Place.Location, data.Place.RTTIType);
        }

        protected bool PlaceTechno(int senderHouseIndex, int HeapID, bool IsNaval, CellStruct placeCoords, AbstractType RTTIType)
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

			
            var isPut = pTechno.Ref.Base.Put(CellClass.Cell2Coord(placeCoords), (Direction) 7u);
            if (isPut)
            {
                MainTools.PrintMessage($"创建{sUnitName}成功 在地图坐标  X:{placeCoords.X} Y:{placeCoords.Y}");
                return true;
            }
            else
            {
                pTechno.Ref.Base.UnInit();
                return false;
            }

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
            DoSpecialPlace(senderHouseIndex,data.SpecialPlace.ID,data.SpecialPlace.Location);
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
                    CoraUtils.Log($"发射失败：未找到超级武器类型 {superWeaponName}");
                    return;
                }
                Pointer<SuperClass> pSuper = pHouse.Ref.FindSuperWeapon(pSuperWeaponTypeClass);
                if (pSuper.IsNull)
                {
                    CoraUtils.Log($"发射失败：HouseClass未拥有超级武器 {superWeaponName}");
                    return;
                }
                
                CoraUtils.Log("FireSuperWeapon({2}):0x({3:X}) -> ({0}, {1})", location.X, location.Y, pSuperWeaponTypeClass.Ref.Base.ID, (int)pSuper);
                pSuper.Ref.SetCharge(100);
                pSuper.Ref.IsCharged = true;
                pSuper.Ref.Launch(location, pHouse.Ref.PlayerControl);
                pSuper.Ref.IsCharged = false;
            }
            catch (Exception ex)
            {
                _ = ex;
                CoraUtils.PrintException(ex);
            }
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

    #region 事件类型

    public enum CoraNetworkEvents : byte
    {
        CoraPlace = 0x65,
        CoraSpecialPlace = 0x66,
        CoraDanmu = 0x70,
      
    }

    
    #endregion
     
}