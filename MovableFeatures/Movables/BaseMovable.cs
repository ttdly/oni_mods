using System;
using STRINGS;
using UnityEngine;
using CLog = GlobalUtil.Logger;

namespace MovableFeatures.Movables
{
    public class BaseMovable : KMonoBehaviour
    {
        private static readonly EventSystem.IntraObjectHandler<BaseMovable> OnRefreshUserMenuDelegate =
            new EventSystem.IntraObjectHandler<BaseMovable>((component, data) => component.OnRefreshUserMenu(data));

        public bool crossPlantMove = true;
        public int originCell;
        public bool isGeyser = false;

        public void OnRefreshUserMenu(object _)
        {
            if (gameObject.HasTag("OilWell") &&
                gameObject.GetComponent<BuildingAttachPoint>()?.points[0].attachedBuilding != null) return;

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
            if (!crossPlantMove && Grid.WorldIdx[targetCell] != gameObject.GetMyWorldId()) return false;
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
            var originLayer = Grid.SceneLayer.SceneMAX;
            if (hasBuilding) UnmarkBuilding(gameObject, building);
            if (hasAnimController)
            {
                originLayer = hasBuilding ? building.Def.SceneLayer : animController.sceneLayer;
#if DEBUG
                CLog.Info(originLayer);
                if (hasBuilding) CLog.Info($"BuildingDef 中的 layer {building.Def.SceneLayer}");        
#endif
                animController.SetSceneLayer(Grid.SceneLayer.SceneMAX);
            }

            transform.SetPosition(GetBuildingPosCbc(targetCell));
            if (gameObject.TryGetComponent(out OccupyArea occupyArea)) occupyArea.UpdateOccupiedArea();
            if (hasBuilding) MarkBuilding(gameObject, building);
            if (hasAnimController) animController.SetSceneLayer(originLayer);
            if (gameObject.TryGetComponent(out BuildingEnabledButton enabledButton))
            {
                // 强制刷新建筑状态
                enabledButton.HandleToggle();
                enabledButton.HandleToggle();
            }
        }

        public virtual void Move(int targetCell)
        {
            StableMove(targetCell);
        }

        private static Vector3 GetBuildingPosCbc(int cell)
        {
            return Grid.CellToPosCBC(cell, Grid.SceneLayer.Building);
        }

        public static void MoveOver(GameObject origin, GameObject cloned)
        {
            cloned.SetActive(false);
            cloned.SetActive(true);
            origin.SetActive(false);
            origin.DeleteObject();
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