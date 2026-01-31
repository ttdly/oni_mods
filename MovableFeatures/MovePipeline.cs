using System;
using System.Collections.Generic;
using HarmonyLib;
using MovableFeatures.Movables;
using CLog = GlobalUtil.Logger;
using Object = UnityEngine.Object;

namespace MovableFeatures
{
    public class MovePipeline
    {
        public static void Move(MoveMomentContext context)
        {
            SetBuildingDefCanMove(context);
            DeregisterComponents(context);
            RemoveWarpConduitSenderPorts(context);
            var transform = context.Movable.gameObject.transform;
            var layer = GridZLayerLookup.Lookup(transform.position.z);
            transform.SetPosition(Grid.CellToPosCBC(context.TargetCell, layer));
            RegisterComponents(context);
            RefreshMeter(context);
            RefreshWarpConduitStatues(context);
            AddWarpConduitPorts(context);
        }

        private static void DeregisterComponents(MoveMomentContext context)
        {
            var go = context.Movable.gameObject;
            var cell = Grid.PosToCell(go);
            if (context.StartManualDeliveryKg.IsNullOrDestroyed())
                context.StartManualDeliveryKg = new List<bool>();
            if (context.StartManualDeliveryKg.Count > 0)
                context.StartManualDeliveryKg.Clear();
            var building = go.GetComponent<Building>();
            if (building != null)
                building.Def.UnmarkArea(cell, building.Orientation, building.Def.ObjectLayer, go);
            var kSelectable = go.GetComponent<KSelectable>();
            kSelectable.IsSelectable = false;
            var buildingComplete = go.GetComponent<BuildingComplete>();
            if (buildingComplete != null)
                buildingComplete.UpdatePosition();
            if (SelectTool.Instance.selected == kSelectable)
                SelectTool.Instance.Select(null);
            var deconstructable = go.GetComponent<Deconstructable>();
            if (deconstructable != null)
                deconstructable.SetAllowDeconstruction(false);
            var handle = GameComps.StructureTemperatures.GetHandle(go);
            if (handle.IsValid())
                GameComps.StructureTemperatures.Disable(handle);
            var fakeFloorAdder = go.GetComponent<FakeFloorAdder>();
            if (fakeFloorAdder != null)
                fakeFloorAdder.SetFloor(false);
            var accessControl = go.GetComponent<AccessControl>();
            if (accessControl != null)
                accessControl.SetRegistered(false);
            foreach (var manualDeliveryKg in go.GetComponents<ManualDeliveryKG>())
            {
                DebugUtil.DevAssert(!manualDeliveryKg.IsPaused,
                    "RocketModule ManualDeliver chore was already paused, when go.rocket lands it will re-enable it.");
                context.StartManualDeliveryKg.Add(manualDeliveryKg.IsPaused);
                if (manualDeliveryKg.IsPaused) continue;
                manualDeliveryKg.Pause(true, "Rocket heading to space");
            }

            foreach (var buildingConduitEndpoints in go.GetComponents<BuildingConduitEndpoints>())
                buildingConduitEndpoints.RemoveEndPoint();
            var workable = go.GetComponent<Workable>();
            if (workable != null)
                workable.RefreshReachability();
            var structure = go.GetComponent<Structure>();
            if (structure != null)
                structure.UpdatePosition();
            var wireUtilitySemiVirtualNetworkLink = go.GetComponent<WireUtilitySemiVirtualNetworkLink>();
            if (wireUtilitySemiVirtualNetworkLink != null)
                wireUtilitySemiVirtualNetworkLink.SetLinkConnected(false);
            var partialLightBlocking = go.GetComponent<PartialLightBlocking>();
            if (partialLightBlocking == null)
                return;
            partialLightBlocking.ClearLightBlocking();
        }

        private static void RegisterComponents(MoveMomentContext context)
        {
            var go = context.Movable.gameObject;
            var cell = Grid.PosToCell(go);
            var building = go.GetComponent<Building>();
            if (building != null)
            {
                building.Def.MarkArea(cell, building.Orientation, building.Def.ObjectLayer, go);
                if (building.GetComponent<OccupyArea>() != null)
                    building.GetComponent<OccupyArea>().UpdateOccupiedArea();
                var logicPorts = building.GetComponent<LogicPorts>();
                if ((bool)(Object)logicPorts && go.GetComponent<BuildingComplete>() != null)
                    logicPorts.OnMove();
            }

            go.GetComponent<KSelectable>().IsSelectable = true;
            var buildingComplete = go.GetComponent<BuildingComplete>();
            if (buildingComplete != null)
                buildingComplete.UpdatePosition();
            var deconstructable = go.GetComponent<Deconstructable>();
            if (deconstructable != null)
                deconstructable.SetAllowDeconstruction(true);
            var handle = GameComps.StructureTemperatures.GetHandle(go.gameObject);
            if (handle.IsValid())
                GameComps.StructureTemperatures.Enable(handle);
            foreach (var storage in go.GetComponents<Storage>())
                storage.UpdateStoredItemCachedCells();
            var fakeFloorAdder = go.GetComponent<FakeFloorAdder>();
            if (fakeFloorAdder != null)
                fakeFloorAdder.SetFloor(true);
            var accessControl = go.GetComponent<AccessControl>();
            if (accessControl != null)
                accessControl.SetRegistered(true);
            if (context.StartManualDeliveryKg.Count > 0)
            {
                var enumerator = context.StartManualDeliveryKg.GetEnumerator();
                foreach (var manualDeliveryKg in go.GetComponents<ManualDeliveryKG>())
                {
                    enumerator.MoveNext();
                    if (enumerator.Current) continue;
                    manualDeliveryKg.Pause(false, "Landing on world");
                }
            }

            foreach (var buildingConduitEndpoints in go.GetComponents<BuildingConduitEndpoints>())
                buildingConduitEndpoints.AddEndpoint();
            var workable = go.GetComponent<Workable>();
            if (workable != null)
                workable.RefreshReachability();
            var structure = go.GetComponent<Structure>();
            if (structure != null)
                structure.UpdatePosition();
            var wireUtilitySemiVirtualNetworkLink = go.GetComponent<WireUtilitySemiVirtualNetworkLink>();
            if (wireUtilitySemiVirtualNetworkLink != null)
                wireUtilitySemiVirtualNetworkLink.SetLinkConnected(true);
            var partialLightBlocking = go.GetComponent<PartialLightBlocking>();
            if (partialLightBlocking == null)
                return;
            partialLightBlocking.SetLightBlocking();
        }

        private static void RefreshMeter(MoveMomentContext context)
        {
            var kBatchedAnimController = context.Movable.gameObject.GetComponent<KBatchedAnimController>();
            if (kBatchedAnimController == null) return;
            kBatchedAnimController.Play(kBatchedAnimController.GetCurrentAnim().name, kBatchedAnimController.GetMode());
        }

        private static void SetBuildingDefCanMove(MoveMomentContext context)
        {
            var go = context.Movable.gameObject;
            var buildingDef = go.GetComponent<Building>();
            if (buildingDef != null)
                buildingDef.Def.CanMove = true;
        }

        private static void RefreshWarpConduitStatues(MoveMomentContext context)
        {
            if (!context.Movable.isWarpConduit) return;
            var go = context.Movable.gameObject;
            if (go.TryGetComponent(out WarpConduitReceiver receiver))
            {
                var senderGameObject = receiver.senderGasStorage.gameObject;
                WarpConduitStatus.UpdateWarpConduitsOperational(senderGameObject, receiver.gameObject);
                return;
            }

            if (!go.TryGetComponent(out WarpConduitSender sender)) return;

            var receiverGameObject = sender.receiver.gameObject;
            WarpConduitStatus.UpdateWarpConduitsOperational(receiverGameObject, sender.gameObject);
        }

        private static void RemoveWarpConduitSenderPorts(MoveMomentContext context)
        {
            var go = context.Movable.gameObject;
            if (!context.Movable.isWarpConduit) return;
            var warpConduitSender = go.GetComponent<WarpConduitSender>();
            if (warpConduitSender == null) return;
            var liquidPort = WarpConduitPortAccess.GetLiquidPort(warpConduitSender);
            var gasPort = WarpConduitPortAccess.GetGasPort(warpConduitSender);
            if (!WarpConduitPortAccess.TryGetPortValues(liquidPort, out var liquidInputCell, out var liquidNetworkItem)
                || !WarpConduitPortAccess.TryGetPortValues(gasPort, out var gasInputCell, out var gasNetworkItem))
            {
                CLog.Warning($"无法获取 {go.GetProperName()} 的 WarpConduitSender 网络字段值");
                return;
            }
            Conduit.GetNetworkManager(warpConduitSender.liquidPortInfo.conduitType)
                .RemoveFromNetworks(liquidInputCell, liquidNetworkItem, true);
            Conduit.GetNetworkManager(warpConduitSender.gasPortInfo.conduitType)
                .RemoveFromNetworks(gasInputCell, gasNetworkItem, true);
            // Game.Instance.solidConduitSystem.RemoveFromNetworks(warpConduitSender.solidPort.inputCell, (object) warpConduitSender.solidPort.solidConsumer, true);
        }

        private static void AddWarpConduitPorts(MoveMomentContext context)
        {
            var go = context.Movable.gameObject;
            if (!context.Movable.isWarpConduit) return;
            var warpConduitSender = go.GetComponent<WarpConduitSender>();
            if (warpConduitSender == null) return;

            var liquidStorage = warpConduitSender.liquidStorage;
            var gasStorage = warpConduitSender.gasStorage;
            if (liquidStorage == null || gasStorage == null)
            {
                CLog.Warning($"无法获取 {go.GetProperName()} 的 WarpConduitSender Storage 字段");
                return;
            }

            if (!WarpConduitPortAccess.TryCreatePort(go, warpConduitSender.liquidPortInfo, 1, liquidStorage,
                    out var liquidPort)
                || !WarpConduitPortAccess.TryCreatePort(go, warpConduitSender.gasPortInfo, 2, gasStorage,
                    out var gasPort))
            {
                CLog.Warning($"无法为 {go.GetProperName()} 创建 WarpConduitSender ConduitPort 实例");
                return;
            }

            WarpConduitPortAccess.SetLiquidPort(warpConduitSender, liquidPort);
            WarpConduitPortAccess.SetGasPort(warpConduitSender, gasPort);
        }
        
        private static void RefreshConduitDispenser(MoveMomentContext context)
        {
            var go = context.Movable.gameObject;
            var conduitDispensers =  go.GetComponents<ConduitDispenser>();
            if (context.Movable.isWarpConduit && context.Movable.GetComponent<WarpConduitReceiver>() != null)
            {
                var buffer = new List<ConduitDispenser>();
                CLog.Info("1223123");
                var warpConduitReceiver = context.Movable.GetComponent<WarpConduitReceiver>();
                CLog.Info(warpConduitReceiver == null);
                if (warpConduitReceiver.senderGasStorage != null)
                {
                    var conduitDispenser = warpConduitReceiver.senderGasStorage.GetComponent<ConduitDispenser>();
                    if (conduitDispenser != null) buffer.Add(conduitDispenser);

                }
                CLog.Info("1");
                if (warpConduitReceiver.senderLiquidStorage != null)
                {
                    var conduitDispenser = warpConduitReceiver.senderLiquidStorage.GetComponent<ConduitDispenser>();
                    if (conduitDispenser != null) buffer.Add(conduitDispenser);
                }
                conduitDispensers = buffer.ToArray();
                CLog.Info("1qq");
            }
            
            CLog.Info(conduitDispensers.Length.ToString());
            if (conduitDispensers == null || conduitDispensers.Length == 0) return;
            var getInputCell = AccessTools.Method(typeof(ConduitDispenser), "GetInputCell");
            var utilityCell = AccessTools.Field(typeof(ConduitDispenser), "utilityCell");
            var partitionerEntryField = AccessTools.Field(typeof(ConduitDispenser), "partitionerEntry");
            var onConduitConnectionChanged = AccessTools.Method(typeof(ConduitDispenser), "OnConduitConnectionChanged");
            if (getInputCell == null || utilityCell == null || partitionerEntryField == null 
                || onConduitConnectionChanged == null)
            {
                CLog.Warning($"无法对 {go.GetProperName()} 的 ConduitDispenser 进行修改，因为无法获取私有属性与方法");
                return;
            }
            foreach (var conduitDispenser in go.GetComponents<ConduitDispenser>())
            {
                var cellObj = getInputCell.Invoke(conduitDispenser, new object[] { conduitDispenser.conduitType });
                if (!(cellObj is int newCell))
                {
                    CLog.Warning($"无法获取 {go.GetProperName()}_ConduitDispenser 在网络中的 cell 值");
                    continue;
                }
                var oldPartitionerEntry = partitionerEntryField.GetValue(conduitDispenser);
                if (!(oldPartitionerEntry is HandleVector<int>.Handle partitionerEntry))
                {
                    CLog.Warning($"无法获取 {go.GetProperName()}_ConduitDispenser 中 partitionerEntry 属性");
                    continue;
                }
                if (partitionerEntry.IsValid()) GameScenePartitioner.Instance.Free(ref partitionerEntry);
                utilityCell.SetValue(conduitDispenser, newCell);
                var layer = GameScenePartitioner.Instance.objectLayers[
                    (conduitDispenser.conduitType == ConduitType.Gas) ? 12 : 16
                ];
                var newPartitionerEntry = GameScenePartitioner.Instance.Add(
                    "ConduitConsumer.OnSpawn", 
                    go,
                    newCell,
                    layer, 
                    (Action<object>)Delegate.CreateDelegate(
                        typeof(Action<object>),
                        conduitDispenser.conduitType, 
                        onConduitConnectionChanged
                        )
                    );
                partitionerEntryField.SetValue(conduitDispenser, newPartitionerEntry);
                GameScenePartitioner.Instance.TriggerEvent(
                    newCell,
                    layer,
                    null);
            }
        }

        public class MoveMomentContext
        {
            public BaseMovable Movable { get; set; }
            public int TargetCell { get; set; }
            public List<bool> StartManualDeliveryKg { get; set; } = null;
        }
    }
}
