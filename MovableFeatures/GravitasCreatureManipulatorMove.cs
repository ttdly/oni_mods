using System;
using System.Reflection;
using HarmonyLib;
using CLog = GlobalUtil.Logger;

namespace MovableFeatures
{
    public class GravitasCreatureManipulatorMove
    {
        private static readonly AccessTools.FieldRef<
            GravitasCreatureManipulator.Instance, HandleVector<int>.Handle
        > GcmPartitionEntry =
            AccessTools.FieldRefAccess<GravitasCreatureManipulator.Instance, HandleVector<int>.Handle>(
                "m_partitionEntry");

        private static readonly AccessTools.FieldRef<
            GravitasCreatureManipulator.Instance, HandleVector<int>.Handle
        > GcmPartitionEntry2 =
            AccessTools.FieldRefAccess<GravitasCreatureManipulator.Instance,
                HandleVector<int>.Handle>("m_largeCreaturePartitionEntry");

        static readonly MethodInfo DetectCreatureMethod = AccessTools.Method(
            typeof(GravitasCreatureManipulator.Instance), "DetectCreature");

        static readonly MethodInfo DetectLargeCreatureMethod =
            AccessTools.Method(typeof(GravitasCreatureManipulator.Instance), "DetectLargeCreature");


    public static void Execute(MovePipeline.MoveMomentContext context)
    {

        var gameObject = context.Movable.gameObject;
        if (!gameObject.TryGetComponent(out BuildingComplete buildingComplete)) return;
        var def = gameObject.GetDef<MakeBaseSolid.Def>();
        var building = gameObject.GetComponent<Building>();
        var primaryElement = gameObject.GetComponent<PrimaryElement>();
        foreach (var solidOffset in def.solidOffsets)
        {
            // 先删除原来位置的固体
            var rotatedOffset = building.GetRotatedOffset(solidOffset);
            var originSolidCell = Grid.OffsetCell(context.Movable.originCell, rotatedOffset);
            SimMessages.ReplaceAndDisplaceElement(originSolidCell, SimHashes.Vacuum,
                CellEventLogger.Instance.SimCellOccupierOnSpawn,
                0.0f);
            Grid.Objects[originSolidCell, 9] = null;
            Grid.Foundation[originSolidCell] = false;
            Grid.SetSolid(originSolidCell, false, CellEventLogger.Instance.SimCellOccupierDestroy);
            SimMessages.ClearCellProperties(originSolidCell, 103);
            Grid.RenderedByWorld[originSolidCell] = true;
            World.Instance.OnSolidChanged(originSolidCell);
            GameScenePartitioner.Instance.TriggerEvent(originSolidCell,
                GameScenePartitioner.Instance.solidChangedLayer, null);
            // 在目标位置生成固体
            var targetSolidCell = Grid.OffsetCell(context.TargetCell, rotatedOffset);
            SimMessages.ReplaceAndDisplaceElement(targetSolidCell, primaryElement.ElementID,
                CellEventLogger.Instance.SimCellOccupierOnSpawn, primaryElement.Mass, primaryElement.Temperature);
            Grid.Objects[targetSolidCell, 9] = gameObject;
            Grid.Foundation[targetSolidCell] = true;
            Grid.SetSolid(targetSolidCell, true, CellEventLogger.Instance.SimCellOccupierForceSolid);
            SimMessages.SetCellProperties(targetSolidCell, (byte)103);
            Grid.RenderedByWorld[targetSolidCell] = false;
            World.Instance.OnSolidChanged(targetSolidCell);
            GameScenePartitioner.Instance.TriggerEvent(targetSolidCell,
                GameScenePartitioner.Instance.solidChangedLayer, (object)null);
        }

        // 刷新动物检测点
        var smi = gameObject.GetSMI<GravitasCreatureManipulator.Instance>();
        var def2 = gameObject.GetDef<GravitasCreatureManipulator.Def>();

        if (smi == null || def2 == null)
        {
            CLog.Warning("can not get GravitasCreatureManipulator instance");
            return;
        }
        smi.pickupCell = Grid.OffsetCell(context.TargetCell, def2.pickupOffset);
        GameScenePartitioner.Instance.Free(ref GcmPartitionEntry(smi));
        GameScenePartitioner.Instance.Free(ref GcmPartitionEntry2(smi));
        var action =
            (Action<object>)Delegate.CreateDelegate(
                typeof(Action<object>),
                smi,
                DetectCreatureMethod);
        var action2 =
            (Action<object>)Delegate.CreateDelegate(
                typeof(Action<object>),
                smi,
                DetectLargeCreatureMethod);
        GcmPartitionEntry(smi) = GameScenePartitioner.Instance.Add(nameof(GravitasCreatureManipulator),
            gameObject, smi.pickupCell, GameScenePartitioner.Instance.pickupablesChangedLayer,
            action);
        GcmPartitionEntry2(smi) = GameScenePartitioner.Instance.Add("GravitasCreatureManipulator.large",
            gameObject, Grid.CellLeft(smi.pickupCell),
            GameScenePartitioner.Instance.pickupablesChangedLayer,
            action2);
    }
    }

}
