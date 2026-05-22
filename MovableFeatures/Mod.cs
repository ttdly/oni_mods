using HarmonyLib;
using KMod;
using MovableFeatures.Movables;
using UnityEngine;
using CLog = GlobalUtil.Logger;

namespace MovableFeatures
{
    public class Mod : UserMod2
    {
        public static Harmony HarmonyInstance;

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            HarmonyInstance = harmony;
            CLog.Init("【位移信标|MovableFeatures】");
            CreateTranslationTemplate();
            PatchRegistry.RegisterPatches();
        }

        private static void CreateTranslationTemplate()
        {
#if DEBUG && false
            CLog.Info("创建翻译模板");
            ModUtil.RegisterForTranslation(typeof(Text));
#endif
        }
    }


    [HarmonyPatch(typeof(Assets), nameof(Assets.AddBuildingDef))]
    public class AssetsAddBuildingDefPatch
    {
        public static void Postfix(BuildingDef def)
        {
            if (!PatchRegistry.BuildingMovableContexts.ContainsKey(def.BuildingComplete.gameObject.PrefabID())) return;
            var movable = def.BuildingComplete.gameObject.AddOrGet<BaseMovable>();
            var context = PatchRegistry.BuildingMovableContexts[def.BuildingComplete.gameObject.PrefabID()];
            movable.flag = context.Flags;
#if DEBUG
            CLog.Info($"组件添加至建筑 {def.BuildingComplete.gameObject.PrefabID()}");
#endif

        }
    }

    [HarmonyPatch(typeof(EntityTemplates), "ConfigPlacedEntity")]
    public class EntityTemplatesConfigPlacedEntityPatch
    {
        private static void Postfix(GameObject __result)
        {
            if (PatchRegistry.EntityMovableContexts.ContainsKey(__result.PrefabID()))
            {
                var movable = __result.AddOrGet<BaseMovable>();
                var context = PatchRegistry.EntityMovableContexts[__result.PrefabID()];
                movable.flag = context.Flags;
#if DEBUG
                CLog.Info($"组件已添加至 {__result.PrefabID()}");
#endif
                return;
            }

            if (__result.HasTag(GameTags.GeyserFeature))
            {
                __result.AddOrGet<BaseMovable>().flag = MovableFlags.HaveNeutronium;
                return;
            }
            if (__result.HasTag(GameTags.Gravitas)) __result.AddOrGet<BaseMovable>();
        }
    }
}