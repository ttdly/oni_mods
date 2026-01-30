using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MovableFeatures.Movables;
using UnityEngine;
using CLog = GlobalUtil.Logger;

namespace MovableFeatures
{
    public class PatchRegistry
    {
        public enum GameObjectType
        {
            Building = 0,
            Entity = 1
        }

        public static List<HarmonyMethod> PatchMethods = new List<HarmonyMethod>
        {
            new HarmonyMethod(AccessTools.Method(typeof(PatchMethod), nameof(PatchMethod.BuildingPatch))),
            new HarmonyMethod(AccessTools.Method(typeof(PatchMethod), nameof(PatchMethod.EntityPatch)))
        };

        public static Dictionary<MethodBase, PatchContext> PatchContexts = new Dictionary<MethodBase, PatchContext>();

        public static void Register<TPatchType>(
            GameObjectType gameObjectType = GameObjectType.Entity,
            bool crossMove = true,
            bool geyser = false,
            bool warpConduit = false)
        {
            var originMethod = GetPatchMethod<TPatchType>(gameObjectType);
            if (originMethod == null)
            {
                CLog.Error("Can't find patch method for " + typeof(TPatchType));
                return;
            }

            var patchContext = new PatchContext
            {
                GameObjectType = gameObjectType,
                CrossMove = crossMove,
                Neutronium = geyser,
                WarpConduit = warpConduit
            };

            PatchContexts.Add(originMethod, patchContext);
#if DEBUG
            CLog.Info($"已注册 {typeof(TPatchType)}");
#endif
        }

        public static void RegisterPatches()
        {
            // 打印仓
            Register<HeadquartersConfig>(GameObjectType.Building, false);
            // 便携式打印舱
            Register<ExobaseHeadquartersConfig>(GameObjectType.Building, false);
            // 时空裂口开放器
            Register<TemporalTearOpenerConfig>(GameObjectType.Building);
            // 供给传送器输出端
            Register<WarpConduitSenderConfig>(GameObjectType.Building, warpConduit: true);
            // 供给传送器输入端
            Register<WarpConduitReceiverConfig>(GameObjectType.Building, warpConduit: true);
            // 神经振荡仪
            Register<GeneShufflerConfig>();
            // 反熵热量中和器
            Register<MassiveHeatSinkConfig>(GameObjectType.Building);
            // 睡衣柜 
            Register<GravitasContainerConfig>(GameObjectType.Building);
            // 柴堆
            Register<WoodStorageConfig>(GameObjectType.Building);
            // 储油石
            Register<OilWellConfig>();
            // 试验体52B
            Register<SapTreeConfig>();
            // 辐射蜂巢
            Register<BaseBeeHiveConfig>();
            // 流明石英
            Register<PinkRockConfig>();
            // 低温箱3000
            Register<CryoTankConfig>();
            // 传送器
            Register<WarpPortalConfig>();
            // 接收器
            Register<WarpReceiverConfig>();
            // TODO 故事特质 + 间歇泉 
        }

        public static MethodInfo GetPatchMethod<TPatchType>(GameObjectType gameObjectType)
        {
            switch (gameObjectType)
            {
                case GameObjectType.Building:
                    return AccessTools.Method(typeof(TPatchType), nameof(IBuildingConfig.DoPostConfigureComplete));
                case GameObjectType.Entity:
                    return AccessTools.Method(typeof(TPatchType), nameof(IEntityConfig.CreatePrefab));
                default:
                    return null;
            }
        }

        public class PatchContext
        {
            public GameObjectType GameObjectType { get; set; }
            public bool CrossMove { get; set; }
            public bool Neutronium { get; set; }
            public bool WarpConduit { get; set; }
        }

        public class PatchMethod
        {
            public static void BuildingPatch(MethodBase __originalMethod, GameObject go)
            {
                DoPatch(__originalMethod, go);
            }

            public static void EntityPatch(MethodBase __originalMethod, GameObject __result)
            {
                DoPatch(__originalMethod, __result);
            }

            public static void DoPatch(MethodBase originalMethod, GameObject go)
            {
#if DEBUG
                CLog.Info($"Patching {originalMethod.DeclaringType}_{originalMethod.Name}");
#endif
                if (!PatchContexts.TryGetValue(originalMethod, out var patchContext)) return;
#if DEBUG
                CLog.Info($"{originalMethod.DeclaringType} context is found. Patching may have success!");
#endif
                var movable = go.AddOrGet<BaseMovable>();
                movable.crossPlantMove = patchContext.CrossMove;
                movable.haveNeutronium = patchContext.Neutronium;
                movable.isWarpConduit = patchContext.WarpConduit;
            }
        }
    }
}