// 引入系统基础库
using System;
// 引入非泛型集合接口
using System.Collections;
// 引入泛型集合（List、Dictionary 等）
using System.Collections.Generic;
// 引入向量计算库（用于 3D 坐标插值）
using System.Numerics;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using PatcherYRpp.Utilities.Clusters;


// 脚本所在命名空间
namespace Scripts
{
    // 可序列化特性：允许这个类被游戏引擎序列化存储
    [Serializable]
    // 定义一个名为“杀生樱”的类，实现集群接口 ICanCluster<SesshoSakura>
    public class SesshoSakura : ICanCluster<SesshoSakura>
    {
        // 集群接口属性：当前对象所属的集群
        public ICluster<SesshoSakura> Cluster { get; set; }

        // 坐标属性：返回当前杀生樱的坐标（未实现）
        public CoordStruct Point => throw new NotImplementedException();

        // 归属阵营属性：返回杀生樱的所属玩家（未实现）
        public Pointer<HouseClass> Owner => throw new NotImplementedException();

        // 每帧更新方法：游戏会每帧调用这个方法（未实现）
        public void Update()
        {
            throw new NotImplementedException();
        }
    }

    // 可序列化
    [Serializable]
    // 杀生樱集群类：管理一组杀生樱
    public class SesshoSakuraCluster : Cluster<SesshoSakura>
    {
        // 重写集群更新方法：每帧更新整个集群逻辑（未实现）
        public override void Update()
        {
        }
    }

    // 可序列化
    [Serializable]
    // 杀生樱集群发现器：自动把附近的杀生樱分成组
    public class SesshoSakuraClusterDiscover : ClusterDiscoverer<SesshoSakura>
    {
        // 集群生效半径：10 格距离
        public override int ClusterRange => 10 * Game.CellSize;
        // 每个集群最多容纳 3 个杀生樱
        public override int ClusterCapacity => 3;
        // 最少 2 个才会形成集群
        public override int ClusterStartNum => 2;
        // 所有杀生樱对象列表（未实现具体内容）
        public override List<SesshoSakura> ObjectList { get; }

        // 创建一个新的杀生樱集群（未实现）
        public override ICluster<SesshoSakura> CreateCluster(IEnumerable<SesshoSakura> objects)
        {
            throw new NotImplementedException();
        }
    }

    // 可序列化
    [Serializable]
    // 八重神子/玉藻妖狐 单位逻辑脚本（继承 TechnoScriptable 单位脚本基类）
    public class YaeHageatama : TechnoScriptable
    {
        // 构造方法：创建脚本实例，传入单位主体
        public YaeHageatama(TechnoExt owner) : base(owner) { }

        // 协程方法：单位跳跃到目标坐标（抛物线跳跃）
        // IEnumerator 是 YR 引擎的协程，可分帧执行
        public IEnumerator Jump(CoordStruct dest)
        {
            // 尝试把当前单位转换成 FootClass（步兵/载具基础类）
            if (Owner.OwnerObject.CastToFoot(out Pointer<FootClass> pFoot))
            {
                // 获取单位的移动控制器（Locomotor）
                ILocomotion locomotion = pFoot.Ref.Locomotor;

                // 在目标点附近找一个可站立的单元格
                if (MapClass.Instance.TryGetCellAt(TechnoPlacer.FindPlaceableCellNear(Owner.OwnerObject, dest), out Pointer<CellClass> pDestCell))
                {
                    // 重新计算目标点：使用单元格内步兵合适的落脚点
                    dest = pDestCell.Ref.FindInfantrySubposition(dest, true, false, false);
                }

                // 记录跳跃起点坐标
                CoordStruct start = Owner.OwnerObject.Ref.BaseAbstract.GetCoords();
                // 尝试获取起点所在单元格
                if (MapClass.Instance.TryGetCellAt(start, out Pointer<CellClass> pStartCell))
                {
                    // 把单位从起点单元格移除（不播放动画）
                    pStartCell.Ref.RemoveContent(Owner.OwnerObject.Convert<ObjectClass>(), false);
                }

                // 跳跃进度：0~1
                float percent = 0f;
                // 跳跃速度：每帧增加 0.1
                float speed = 0.1f;

                // 记录单位是否被玩家选中
                bool selected = Owner.OwnerObject.Ref.Base.IsSelected;
                // 跳跃期间临时取消选中，防止界面干扰
                Owner.OwnerObject.Ref.Base.Deselect();
                // 锁定移动控制器：禁止引擎自动移动单位
                locomotion.Lock();

                // 循环执行：直到进度 100%
                while (percent <= 1f)
                {
                    // 线性插值：根据当前进度计算中间坐标
                    CoordStruct cur = Vector3.Lerp(start.ToVector3(), dest.ToVector3(), percent).ToCoordStruct();
                    // 叠加抛物线高度：sin(π*percent) 形成向上再向下的弧线
                    cur += new CoordStruct(0, 0, (int)(Math.Sin(percent * Math.PI) * 500));

                    // 清空单位目标点
                    Owner.OwnerObject.Ref.SetDestination(Pointer<AbstractClass>.Zero);
                    // 停止移动
                    locomotion.Stop_Moving();
                    // 停止移动动画
                    locomotion.Stop_Movement_Animation();
                    // 清空所有占用标记
                    locomotion.Mark_All_Occupation_Bits(0);

                    // 获取当前位置所在单元格
                    if (MapClass.Instance.TryGetCellAt(Owner.OwnerObject.Ref.Base.Location, out Pointer<CellClass> pCurCell))
                    {
                        // 如果单位高度低于桥梁高度
                        if (Owner.OwnerObject.Ref.Base.Location.Z - pCurCell.Ref.GetCenterCoords().Z < Game.BridgeHeight)
                        {
                            // 取消当前单元格的占用标记
                            Owner.OwnerObject.Ref.Base.UnmarkAllOccupationBits(Owner.OwnerObject.Ref.Base.Location);
                        }
                    }

                    // 设置单位当前坐标为计算出的抛物线坐标
                    Owner.OwnerObject.Ref.Base.Location = cur;

                    // 重新获取当前坐标所在单元格
                    if (MapClass.Instance.TryGetCellAt(cur, out pCurCell))
                    {
                        // 如果高度低于桥梁
                        if (cur.Z - pCurCell.Ref.GetCenterCoords().Z < Game.BridgeHeight)
                        {
                            // 标记新位置为占用
                            Owner.OwnerObject.Ref.Base.MarkAllOccupationBits(cur);
                        }
                    }

                    // 进度增加
                    percent += speed;
                    // 等待一帧，继续下一次循环
                    yield return null;
                }

                // 跳跃结束：解锁移动控制器
                locomotion.Unlock();
                // 取消最后位置的占用标记
                Owner.OwnerObject.Ref.Base.UnmarkAllOccupationBits(Owner.OwnerObject.Ref.Base.Location);
                // 把单位添加到目标单元格
                pDestCell.Ref.AddContent(Owner.OwnerObject.Convert<ObjectClass>(), false);

                // 如果之前是选中状态，恢复选中
                if (selected)
                {
                    Owner.OwnerObject.Ref.Base.Select();
                }

                // 跳跃落地后执行创建（杀生樱）
                Create();
            }
        }

        // 创建杀生樱（未实现逻辑）
        void Create()
        {
            // TODO
        }

        // 爆发/爆炸效果（未实现逻辑）
        void Brust()
        {
            // TODO
        }

        // 重写开火事件：单位攻击时触发
        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            // 0 号武器：跳跃攻击
            if (weaponIndex == 0)
            {
                // 判断目标是否是单元格
                if (pTarget.CastToCell(out Pointer<CellClass> pCell))
                {
                    // 启动跳跃协程，跳到目标单元格坐标
                    GameObject.StartCoroutine(Jump(pCell.Ref.Base.GetCoords()));
                }
            }

            // 1 号武器：爆发技能
            if (weaponIndex == 1)
            {
                // 执行爆发
                Brust();
            }
        }
    }
}