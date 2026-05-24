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
            gameObject.transform.SetPosition(GetBuildingPosCbc(targetCell));
        }

        public virtual void Move(int targetCell)
        {
            MovePipeline.Move(new MovePipeline.MoveMomentContext
            {
                Movable = this,
                TargetCell = targetCell,
            });
        }
        
        private static Vector3 GetBuildingPosCbc(int cell)
        {
            return Grid.CellToPosCBC(cell, Grid.SceneLayer.Building);
        }

    }
}