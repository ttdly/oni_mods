using System.Collections.Generic;
using System.Linq;
using MovableFeatures.Movables;
using CLog = GlobalUtil.Logger;
using Object = UnityEngine.Object;

namespace MovableFeatures
{
    public class MovePipeline
    {
        public static void Move(MoveMomentContext context)
        {
            GetLayer(context);
            if (context.Movable.flag.HasFlag(MovableFlags.JustCreateNew))
            {
                CreateNewPipeline(context);
            }
            else
            {
                MoveExistsPipeline(context);
            }
            ReActiveObject(context);
            MoveNeutronium(context);
        }

        private static void CreateNewPipeline(MoveMomentContext context)
        {
            var newGo =
                GameUtil.KInstantiate(context.Movable.gameObject,
                    Grid.CellToPosCBC(context.TargetCell, context.TargetLayer), context.TargetLayer);
            var loreBearer = context.Movable.gameObject.GetComponent<LoreBearer>();
            if (loreBearer != null) ComponentAttrToggle.ToggleLoreBearer(loreBearer, newGo.AddOrGet<LoreBearer>());
            var setLocker = context.Movable.gameObject.GetComponent<SetLocker>();
            if (setLocker != null) ComponentAttrToggle.ToggleSetLocker(setLocker, newGo.AddOrGet<SetLocker>());
            var activatable = context.Movable.gameObject.GetComponent<Activatable>();
            if (activatable != null) ComponentAttrToggle.ToggleActivated(activatable, newGo.AddOrGet<Activatable>());
            // RefreshWarpConduitStatues(context);
            newGo.SetActive(false);
            newGo.SetActive(true);
            context.Movable.gameObject.SetActive(false);
            context.Movable.gameObject.DeleteObject();

        }

        private static void MoveExistsPipeline(MoveMomentContext context)
        {
            SetBuildingDefCanMove(context);
            DeregisterComponents(context);
            // RemoveWarpConduitSenderPorts(context);
            SetTransformPosition(context);
            RegisterComponents(context);
            RefreshMeter(context);
            // RefreshWarpConduitStatues(context);
            // AddWarpConduitPorts(context);
        }

        private static void GetLayer(MoveMomentContext context)
        {
            var transform = context.Movable.gameObject.transform;
            context.TargetLayer = GridZLayerLookup.Lookup(transform.position.z);
        }

        private static void ReActiveObject(MoveMomentContext context)
        {
            if (!context.Movable.gameObject.HasTag(GameTags.GeyserFeature)) return;
            context.Movable.gameObject.SetActive(false);
            context.Movable.gameObject.SetActive(true);
        }

        private static void MoveNeutronium(MoveMomentContext context)
        {
            if (!context.HaveNeutronium) return;
            var offsets = new[] { 0 };
            if (context.Movable.gameObject.HasTag(GameTags.GeyserFeature))
            {
                var buffer = new HashSet<int>();
                for (var i = 0; i < 3; i++)
                {
                    if (IsMoveNeutronium(Grid.OffsetCell(context.Movable.originCell, i, -1)))
                        buffer.Add(i);
                    if (IsMoveNeutronium(Grid.OffsetCell(context.Movable.originCell, -i, -1)))
                        buffer.Add(-i);
                }

                offsets = buffer.ToArray();
            }

            if (context.Movable.gameObject.PrefabID() == new Tag(GeothermalControllerConfig.ID))
                offsets = new[] { -4, -3, -2, -1, 0, 1, 2, 3, 4 };

            if (context.Movable.gameObject.PrefabID() == new Tag(GeothermalVentConfig.ID))
                offsets = new[] { -1, 0, 1 };

            foreach (var offset in offsets)
            {
                var cell = Grid.OffsetCell(context.Movable.originCell, offset, -1);
                if (!(Grid.Element.Length < cell || Grid.Element[cell] == null || !IsMoveNeutronium(cell)))
                    SimMessages.ReplaceElement(cell, SimHashes.Vacuum, CellEventLogger.Instance.DebugTool, 0);
                cell = Grid.OffsetCell(context.TargetCell, offset, -1);
                if (Grid.IsValidCell(cell))
                    SimMessages.ReplaceElement(cell, SimHashes.Unobtanium, CellEventLogger.Instance.DebugTool,
                        float.PositiveInfinity);
            }

            return;

            bool IsMoveNeutronium(int cell)
            {
                return Grid.Element[cell].id == SimHashes.Unobtanium;
            }
        }

        private static void SetTransformPosition(MoveMomentContext context)
        {
            context.Movable.gameObject.transform.SetPosition(Grid.CellToPosCBC(context.TargetCell,
                context.TargetLayer));
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
            {
                building.Def.UnmarkArea(cell, building.Orientation, building.Def.ObjectLayer, go);
                GameComps.GetKComponentManager(typeof(RequiresFoundation)).Remove(go);
            }

            var kSelectable = go.GetComponent<KSelectable>();
            kSelectable.IsSelectable = false;
            var buildingComplete = go.GetComponent<BuildingComplete>();
            if (buildingComplete != null)
                buildingComplete.UpdatePosition();
            if (SelectTool.Instance.selected == kSelectable)
                SelectTool.Instance.Select(null);
            var deconstructable = go.GetComponent<Deconstructable>();
            if (deconstructable != null)
            {
                deconstructable.SetAllowDeconstruction(false);
                context.CanDeconstructable = deconstructable.allowDeconstruction;
            }

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
                context.StartManualDeliveryKg.Add(manualDeliveryKg.IsPaused);
                if (manualDeliveryKg.IsPaused) continue;
                manualDeliveryKg.Pause(true, "Object is moving");
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
            CLog.Info($"{go.GetProperName()}--{go.GetComponent<OccupyArea>() != null}");
            var cell = Grid.PosToCell(go);
            var building = go.GetComponent<Building>();
            if (building != null)
            {
                building.Def.MarkArea(cell, building.Orientation, building.Def.ObjectLayer, go);
                if (building.GetComponent<OccupyArea>() != null)
                    building.GetComponent<OccupyArea>().UpdateOccupiedArea();
                var logicPorts = building.GetComponent<LogicPorts>();
                GameComps.GetKComponentManager(typeof(RequiresFoundation)).Add(go);
                if ((bool)(Object)logicPorts && go.GetComponent<BuildingComplete>() != null)
                    logicPorts.OnMove();
            }
            else
            {
                if (go.GetComponent<OccupyArea>() != null)
                {
                    go.GetComponent<OccupyArea>().UpdateOccupiedArea();
                }

            }

            go.GetComponent<KSelectable>().IsSelectable = true;
            var buildingComplete = go.GetComponent<BuildingComplete>();
            if (buildingComplete != null)
                buildingComplete.UpdatePosition();
            var deconstructable = go.GetComponent<Deconstructable>();
            if (deconstructable != null && context.CanDeconstructable)
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
                    manualDeliveryKg.Pause(false, "move over");
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

        public class MoveMomentContext
        {
            public BaseMovable Movable { get; set; }
            public int TargetCell { get; set; }
            public List<bool> StartManualDeliveryKg { get; set; }
            public Grid.SceneLayer TargetLayer { get; set; } = Grid.SceneLayer.Background;
            public bool CanDeconstructable;

            public bool HaveNeutronium => (Movable.flag & MovableFlags.HaveNeutronium) != 0;
        }
    }
}
