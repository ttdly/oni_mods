using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
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

        public static List<HarmonyMethod> PatchMethods = new List<HarmonyMethod>()
        {
            new HarmonyMethod(AccessTools.Method(typeof(PatchMethod), nameof(PatchMethod.BuildingPatch))),
            new HarmonyMethod(AccessTools.Method(typeof(PatchMethod), nameof(PatchMethod.EntityPatch))),
        };

        public static Dictionary<MethodBase, PatchContext> PatchContexts = new Dictionary<MethodBase, PatchContext>();

        public static void Registry<TPatchType>(
            GameObjectType gameObjectType = GameObjectType.Entity,
            bool crossMove = true,
            bool geyser = false)
        {
            var originMethod = GetPatchMethod<TPatchType>(gameObjectType);
            if (originMethod == null)
            {
                CLog.Error("Can't find patch method for " + gameObjectType);
                return;
            }

            var patchContext = new PatchContext
            {
                GameObjectType = gameObjectType,
                CrossMove = crossMove,
                Geyser = geyser,
            };

            PatchContexts.Add(originMethod, patchContext);
#if DEBUG
            CLog.Info($"已注册 {typeof(TPatchType)}");
#endif
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
            public bool Geyser { get; set; }
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
                var movable = go.AddOrGet<Movables.BaseMovable>();
                movable.crossPlantMove = patchContext.CrossMove;
                movable.isGeyser = patchContext.Geyser;
            }
        }
    }
}