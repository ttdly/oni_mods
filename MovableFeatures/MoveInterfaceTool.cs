using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace MovableFeatures
{
    public class MoveInterfaceTool : InterfaceTool
    {
        public static MoveInterfaceTool Instance;
        private static readonly Color Red = Color.red;
        private BaseMovable _targetMovable;

        protected override void OnPrefabInit()
        {
            Instance = this;
        }

        public override void OnLeftClickDown(Vector3 cursor_pos)
        {
            base.OnLeftClickDown(cursor_pos);
            if (!(_targetMovable != null))
                return;
            var mouseCell = DebugHandler.GetMouseCell();
            if (_targetMovable.CanMoveTo(mouseCell))
            {
                PlaySound(GlobalAssets.GetSound("HUD_Click"));
                if (Settings.StableMode)
                    _targetMovable.StableMove(mouseCell);
                else
                    _targetMovable.Move(mouseCell);

                _targetMovable.SetOriginCell(mouseCell);
                SelectTool.Instance.Activate();
            }
            else
            {
                PlaySound(GlobalAssets.GetSound("Negative"));
            }
        }

        public override void OnMouseMove(Vector3 cursor_pos)
        {
            base.OnMouseMove(cursor_pos);
            RefreshColor();
        }

        protected override void OnActivateTool()
        {
            base.OnActivateTool();
            if (_targetMovable == null) return;
            var (visualBuffer, needOffset) = CreateVisualizer();
            visualizer = GameUtil.KInstantiate(visualBuffer, Grid.SceneLayer.Building,
                gameLayer: LayerMask.NameToLayer("Place"));
            var animController = visualizer.GetComponent<KBatchedAnimController>();
            if (needOffset && animController != null)
            {
                var offset = animController.Offset;
                offset.x += 0.5f;
                animController.Offset = offset;
                animController.PlayMode = KAnim.PlayMode.Paused;
            }

            visualizer.SetActive(true);
            if (animController != null) animController.TriggerStop();
            // 显示鼠标周围的网格效果
            GridCompositor.Instance.ToggleMajor(true);
        }

        protected override void OnDeactivateTool(InterfaceTool new_tool)
        {
            Destroy(visualizer);
            GridCompositor.Instance.ToggleMajor(false);
            if (new_tool == SelectTool.Instance)
                Game.Instance.Trigger(-1190690038);
            base.OnDeactivateTool(new_tool);
        }

        public void Activate(BaseMovable movable)
        {
            _targetMovable = movable;
            PlayerController.Instance.ActivateTool(this);
            OnActivateTool();
        }

        private void RefreshColor()
        {
            if (_targetMovable == null) return;
            var c = Red;
            if (_targetMovable.CanMoveTo(DebugHandler.GetMouseCell()))
                c = Color.white;
            if (visualizer.TryGetComponent(out KBatchedAnimController controller)) controller.TintColour = c;
        }

        // 创建一个工具视图对象
        private (GameObject visualizer, bool needOffset) CreateVisualizer()
        {
            var visualBuffer = new GameObject(_targetMovable.gameObject.name + "Proxy");
            visualBuffer.SetActive(false);
            visualBuffer.AddOrGet<KPrefabID>();
            visualBuffer.AddOrGet<KSelectable>();
            visualBuffer.AddOrGet<StateMachineController>();
            var primaryElement = visualBuffer.AddOrGet<PrimaryElement>();
            primaryElement.Mass = 1f;
            primaryElement.Temperature = 293f;
            DontDestroyOnLoad(visualBuffer);
            var visualAnimController = visualBuffer.AddOrGet<KBatchedAnimController>();
            var gameObjectController = _targetMovable.gameObject.GetComponent<KBatchedAnimController>();
            visualAnimController.AnimFiles = gameObjectController.AnimFiles;
            visualAnimController.initialAnim = gameObjectController.GetCurrentAnim().name;
            var needOffset = _targetMovable.gameObject.TryGetComponent(out KBoxCollider2D kBoxCollider2D) &&
                             kBoxCollider2D.size.x % 2 == 0;
            return (visualBuffer, needOffset);
        }

        [HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
        public static class PlayerControllerOnPrefabInitPatch
        {
            private static T CreateToolInstance<T>(PlayerController playerController)
                where T : InterfaceTool
            {
                var proxyGameObject = new GameObject(typeof(T).Name);
                var tool = proxyGameObject.AddComponent<T>();
                proxyGameObject.transform.SetParent(playerController.gameObject.transform);
                proxyGameObject.SetActive(true);
                proxyGameObject.SetActive(false);
                return tool;
            }

            internal static void Postfix(PlayerController __instance)
            {
                var interfaceTools = new List<InterfaceTool>(__instance.tools)
                {
                    CreateToolInstance<MoveInterfaceTool>(__instance)
                };
                __instance.tools = interfaceTools.ToArray();
            }
        }
    }
}