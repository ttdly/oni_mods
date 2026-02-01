using System;
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
#if DEBUG
            CLog.Info("创建翻译模板");
            ModUtil.RegisterForTranslation(typeof(Text));
#endif
        }
    }

    // [HarmonyPatch(typeof(GeyserGenericConfig))]
    // [HarmonyPatch(nameof(GeyserGenericConfig.CreateGeyser))]
    // [HarmonyPatch(new[]
    // {
    //     typeof(string), typeof(string), typeof(int), typeof(int), typeof(string), typeof(string),
    //     typeof(HashedString), typeof(float), typeof(string[]), typeof(string[])
    // })]
    // public class GeyserGenericConfigPatch
    // {
    //     public static void Postfix(GameObject __result)
    //     {
    //         __result.AddOrGet<Movables.BaseMovable>().haveNeutronium = true;
    //     }
    // }

    [HarmonyPatch(typeof(Assets), nameof(Assets.AddBuildingDef))]
    public class AssetsAddBuildingDefPatch
    {
        public static void Postfix(BuildingDef def)
        {
            if (!PatchRegistry.BuildingMovableContexts.ContainsKey(def.BuildingComplete.gameObject.PrefabID())) return;
            var movable = def.BuildingComplete.gameObject.AddOrGet<BaseMovable>();
            var context = PatchRegistry.BuildingMovableContexts[def.BuildingComplete.gameObject.PrefabID()];
            movable.haveNeutronium = context.Neutronium;
            movable.crossPlantMove = context.CrossMove;
            movable.isWarpConduit = context.WarpConduit;
            movable.isLonelyMinion = context.LonelyMinion;
            CLog.Info($"组件添加至建筑 {def.BuildingComplete.gameObject.PrefabID()}");
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
                movable.haveNeutronium = context.Neutronium;
                movable.crossPlantMove = context.CrossMove;
                movable.isWarpConduit = context.WarpConduit;
                movable.isLonelyMinion = context.LonelyMinion;
                CLog.Info($"组件已添加至 {__result.PrefabID()}");
                return;
            }

            if (__result.HasTag(GameTags.GeyserFeature))
            {
                __result.AddOrGet<BaseMovable>().haveNeutronium = true;
                return;
            }
            if (__result.HasTag(GameTags.Gravitas)) __result.AddOrGet<BaseMovable>();
        }
    }
}