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
            RegisterPatches();
        }

        private static void CreateTranslationTemplate()
        {
#if DEBUG
            CLog.Info("创建翻译模板");
            ModUtil.RegisterForTranslation(typeof(Text));
#endif
        }

        private static void RegisterPatches()
        {
            PatchRegistry.Registry<HeadquartersConfig>(PatchRegistry.GameObjectType.Building, false);
            PatchRegistry.Registry<GravitasContainerConfig>(PatchRegistry.GameObjectType.Building);
            CLog.Info(PatchRegistry.PatchContexts.Count);
        }
    }
    
    [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
    public class GeneratedBuildingsLoadGeneratedBuildingsPatch
    {
        private static void Prefix()
        {
            foreach (var kv in PatchRegistry.PatchContexts)
            {
                Mod.HarmonyInstance.Patch(kv.Key, postfix: PatchRegistry.PatchMethods[(int)kv.Value.GameObjectType]);
            }
        }
    }

    // [HarmonyPatch(typeof(SetLockerConfig), "CreatePrefab")]
    // public static class SetLockerConfigPatch
    // {
    //     private static void Postfix(GameObject __result)
    //     {
    //         __result.AddOrGet<BaseMovable>();
    //     }
    // }

    // [HarmonyPatch(typeof(HeadquartersConfig), "DoPostConfigureComplete")]
    // public static class HeadquartersConfigDoPostConfigureCompletePatch
    // {
    //     private static void Postfix(GameObject go)
    //     {
    //         go.AddOrGet<BaseMovable>();
    //     }
    // }
    
    [HarmonyPatch(typeof(EntityTemplates), "ConfigPlacedEntity")]
    public class EntityTemplatesConfigPlacedEntityPatch
    {
        private static void Postfix(GameObject __result)
        {
            if (__result.HasTag(GameTags.Gravitas)) __result.AddOrGet<BaseMovable>();
        }
    }
}