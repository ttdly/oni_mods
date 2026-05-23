using System;
using STRINGS;
using UnityEngine;
using CLog = GlobalUtil.Logger;

namespace MovableFeatures
{
    public class BaseMovable : KMonoBehaviour
    {
        private static readonly EventSystem.IntraObjectHandler<BaseMovable> OnRefreshUserMenuDelegate =
            new EventSystem.IntraObjectHandler<BaseMovable>((component, data) => component.OnRefreshUserMenu(data));

        public MovableFlags flag;
        public int originCell;
        
        public void OnRefreshUserMenu(object _)
        {
            if (gameObject.HasTag("OilWell")
                && gameObject.GetComponent<BuildingAttachPoint>()?.points[0].attachedBuilding != null) return;
            if (flag.HasFlag(MovableFlags.LonelyMinionHouse)) return;

            Game.Instance.userMenu.AddButton(
                gameObject,
                new KIconButtonMenu.ButtonInfo(
                    "action_control",
                    UI.USERMENUACTIONS.PICKUPABLEMOVE.NAME,
                    OnClickMove,
                    tooltipText: UI.USERMENUACTIONS.PICKUPABLEMOVE.TOOLTIP)
            );
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            originCell = Grid.PosToCell(transform.position);
            Subscribe((int)GameHashes.RefreshUserMenu, OnRefreshUserMenuDelegate);
        }

        protected virtual void OnClickMove()
        {
            MoveInterfaceTool.Instance.Activate(this);
        }

        public bool CanMoveTo(int targetCell)
        {
            if ((flag & MovableFlags.BannedCrossPlantMove) != 0 && Grid.WorldIdx[targetCell] != gameObject.GetMyWorldId()) return false;
            try
            {
                if (!Grid.IsValidCell(targetCell)) return false;
                return !Grid.Element[targetCell].IsSolid;
            }
#if DEBUG
            catch (Exception e)
            {
                CLog.Error(e.Message);
                return false;
            }
#else
            catch (Exception) {
                return false;
            }
#endif
        }

        public int SetOriginCell(int cell)
        {
            return originCell = cell;
        }

        public virtual void StableMove(int targetCell)
        {
            var hasBuilding = gameObject.TryGetComponent(out Building building);
            var hasAnimController = gameObject.TryGetComponent(out KBatchedAnimController animController);
            if (hasBuilding) UnmarkBuilding(gameObject, building);
            transform.SetPosition(GetBuildingPosCbc(targetCell));
            if (gameObject.TryGetComponent(out OccupyArea occupyArea)) occupyArea.UpdateOccupiedArea();
            if (hasBuilding) MarkBuilding(gameObject, building);
            if (hasAnimController)
            {
                animController.SetDirty();
                animController.Play(animController.GetCurrentAnim().name, animController.GetMode());
            }
            WarpConduitHandler();
            // ConduitConsumerHandler();
            
            // if (gameObject.TryGetComponent(out KSelectable selectable))
            // {
            //     CLog.Info("找到 selectable");
            //     var shouldRefreshItems = new List<StatusItem>();
            //     foreach (var entry in selectable.GetStatusItemGroup())
            //     {
            //         if (entry.item.ShouldShowIcon()) shouldRefreshItems.Add(entry.item);
            //     }
            //     
            //     foreach (var statusItem in shouldRefreshItems)
            //     {
            //         CLog.Info($"刷新 {statusItem} 状态");
            //         selectable.RemoveStatusItem(statusItem);
            //         selectable.AddStatusItem(statusItem);
            //     }
            // }
        }

        public virtual void Move(int targetCell)
        {
            MovePipeline.Move(new MovePipeline.MoveMomentContext
            {
                Movable = this,
                TargetCell = targetCell,
            });
            // StableMove(targetCell);
        }

        private void ConduitConsumerHandler()
        {
            // BuildingDef
            if (!gameObject.TryGetComponent(out ConduitConsumer consumer)) return;
            DestroyImmediate(consumer);
            gameObject.AddComponent<ConduitConsumer>();
            // var getInputCell = AccessTools.Method(typeof(ConduitConsumer), "GetInputCell");
            // if (getInputCell == null) return;
            // var conduitManager = consumer.conduitType;
            // var cell = getInputCell.Invoke(consumer, new object[] { conduitManager });
            // var utiltiyCell = AccessTools.Field(typeof(ConduitConsumer), "utilityCell");
            // var partitionerEntryField = AccessTools.Field(typeof(ConduitConsumer), "partitionerEntry");
            // var partitionerEntry =  (HandleVector<int>.Handle)partitionerEntryField.GetValue(consumer);
            // var onConduitConnectionChanged = AccessTools.Method(typeof(ConduitConsumer), "OnConduitConnectionChanged");
            // CLog.Info($"Method {onConduitConnectionChanged == null}");
            // if (onConduitConnectionChanged == null) return; 
            // GameScenePartitioner.Instance.Free(ref partitionerEntry);
            // CLog.Info($"old {utiltiyCell.GetValue(consumer)}; new {cell}");
            // utiltiyCell.SetValue(consumer, cell);
            // var layer = GameScenePartitioner.Instance.objectLayers[(consumer.conduitType == ConduitType.Gas) ? 12 : 16];
            // var newPartitionerEntry = GameScenePartitioner.Instance.Add(
            //     "ConduitConsumer.OnSpawn", 
            //     gameObject, 
            //     (int)cell, 
            //     layer, 
            //     (Action<object>)Delegate.CreateDelegate(
            //         typeof(Action<object>),
            //         consumer, 
            //         onConduitConnectionChanged
            //         )
            //     );
            // CLog.Info(newPartitionerEntry);
            // CLog.Info(newPartitionerEntry.IsValid());
            // partitionerEntryField.SetValue(consumer, newPartitionerEntry);
            //
            // CLog.Info($"now {utiltiyCell.GetValue(consumer)}");
            // GameScenePartitioner.Instance.TriggerEvent(
            //     (int) cell,
            //     layer,
            //     null
            //     );
            // // Trigger(-2094018600, consumer.IsConnected);
        }

        private void WarpConduitHandler()
        {
            // if (!isWarpConduit) return;
            if (gameObject.TryGetComponent(out WarpConduitReceiver receiver))
            {
                var senderGameObject = receiver.senderGasStorage.gameObject;
                WarpConduitStatus.UpdateWarpConduitsOperational(senderGameObject, receiver.gameObject);
                return;
            }

            if (!gameObject.TryGetComponent(out WarpConduitSender sender)) return;
            
            var receiverGameObject = sender.receiver.gameObject;   
            WarpConduitStatus.UpdateWarpConduitsOperational(receiverGameObject, sender.gameObject);
        }
        
        private static Vector3 GetBuildingPosCbc(int cell)
        {
            return Grid.CellToPosCBC(cell, Grid.SceneLayer.Building);
        }

        private static void UnmarkBuilding(GameObject go, Building building)
        {
            var cell = Grid.PosToCell(go);
            building.Def.UnmarkArea(
                cell,
                building.Orientation,
                building.Def.ObjectLayer,
                go
            );
            go.GetComponent<LogicPorts>()?.OnMove();
            go.GetComponent<BuildingComplete>()?.UpdatePosition();
        }

        private static void MarkBuilding(GameObject go, Building building)
        {
            var cell = Grid.PosToCell(go);

            building.Def.MarkArea(
                cell,
                building.Orientation,
                building.Def.ObjectLayer,
                go
            );
            go.GetComponent<LogicPorts>()?.OnMove();
            go.GetComponent<BuildingComplete>()?.UpdatePosition();
        }

    }
}