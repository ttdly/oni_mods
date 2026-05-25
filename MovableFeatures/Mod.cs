using System.Collections;
using System.IO;
using System.Reflection;
using GlobalUtil.UI;
using HarmonyLib;
using KMod;
using MovableFeatures.Screen;
using STRINGS;
using UnityEngine;
using CLog = GlobalUtil.Logger;

namespace MovableFeatures
{
    public class Mod : UserMod2
    {
        public static GameObject SettingScreenPrefab;
        public static string ConfigPath;

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            CLog.Init("【位移信标|MovableFeatures】");
            CreateTranslationTemplate();
            PatchRegistry.RegisterPatches();
            LoadAssets();
            ConfigPath = Path.Combine(Manager.GetDirectory(), "movable_features_config.json");
            Settings.Load();
        }

        private static void CreateTranslationTemplate()
        {
#if DEBUG && false
            CLog.Info("创建翻译模板");
            ModUtil.RegisterForTranslation(typeof(Text));
#endif
        }

        public static void PrintName(GameObject go)
        {
            CLog.Info(UI.StripLinkFormatting(go.GetProperName()));
        }

        private static void LoadAssets()
        {
            var bundle = GlobalUtil.UI.Util.LoadAssetBundle("movable_features", platformSpecific: true);
            var prefab = bundle.LoadAsset<GameObject>("Assets/UIs/MovableFeaturesSettings.prefab");
            SettingScreenPrefab = prefab;

            var tmPConverter = new TMPConverter();
            tmPConverter.ReplaceAllText(prefab);
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
            // CLog.Info($"组件添加至建筑 {def.BuildingComplete.gameObject.PrefabID()}");
            Mod.PrintName(def.BuildingComplete.gameObject);
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
                // CLog.Info($"组件已添加至 {__result.PrefabID()}");
                Mod.PrintName(__result.gameObject);
#endif
            }
            else if (__result.HasTag(GameTags.GeyserFeature))
            {
                __result.AddOrGet<BaseMovable>().flag = MovableFlags.IsGeyser;
#if DEBUG
                // CLog.Info($"组件已添加至 {__result.PrefabID()}");
                Mod.PrintName(__result.gameObject);
#endif
            }
            else if (__result.HasTag(GameTags.Gravitas))
            {
                __result.AddOrGet<BaseMovable>();
#if DEBUG
                // CLog.Info($"组件已添加至 {__result.PrefabID()}");
                Mod.PrintName(__result.gameObject);
#endif
            }
        }
    }

    [HarmonyPatch(typeof(ModsScreen), "BuildDisplay")]
    public class ModsScreenBuildDisplayPatch
    {
        private static void Postfix(ModsScreen __instance, object ___displayedMods)
        {
            var mods = Global.Instance.modManager.mods;
            foreach (var entry in (IEnumerable)___displayedMods)
            {
                var index = Traverse.Create(entry).Field<int>("mod_index").Value;
                var mod = mods[index];
                if (mod.staticID != "CalYu.MovableFeatures") continue;
                var transform = Traverse.Create(entry).Field<RectTransform>("rect_transform").Value;
                if (transform.TryGetComponent(out HierarchyReferences references))
                {
                    var button = references.GetReference<KButton>("ManageButton").transform;
                    var settingButton = Util.KInstantiateUI<KButton>(button.gameObject, button.parent.gameObject, true);
                    settingButton.transform.SetSiblingIndex(button.transform.GetSiblingIndex() - 1);
                    settingButton.GetComponentInChildren<LocText>().text =
                        Strings.Get("STRINGS.UI.SCHEDULESCREEN.SETTINGS");
                    settingButton.onClick += ShowDialog;
                }

                break;
            }
        }

        private static void ShowDialog()
        {
            Settings.Load();
            DialogCreator.CreateFDialog<SettingScreen>(Mod.SettingScreenPrefab, "MFSetting");
        }
    }

    [HarmonyPatch(typeof(Localization), "Initialize")]
    public class LocalizationInitializePatch
    {
        public static void Postfix()
        {
            var textType = typeof(Text);
            Localization.RegisterForTranslation(textType);
            var code = Localization.GetLocale()?.Code;
            if (code.IsNullOrWhiteSpace()) return;
            var modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (modPath == null) return;
            var path = Path.Combine(modPath, "translations", code + ".po");
            if (!File.Exists(path)) return;
            Localization.OverloadStrings(Localization.LoadStringsFile(path, false));
            for (var index = 0; index != Global.Instance.modManager.mods.Count; ++index)
            {
                var mod = Global.Instance.modManager.mods[index];
                if (mod.staticID != "CalYu.MovableFeatures") continue;
                mod.title = Text.Name;
                mod.description = Text.Desc;
                break;
            }
        }
    }
}