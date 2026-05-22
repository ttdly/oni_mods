using System.Reflection;
using HarmonyLib;
using UnityEngine;
using CLog = GlobalUtil.Logger;

namespace MovableFeatures
{
    public class ComponentAttrToggle
    {
        private static readonly FieldInfo LoreBareUsed =
            AccessTools.Field(typeof(LoreBearer), "BeenClicked");
        private static readonly FieldInfo SetLockerUsed =
            AccessTools.Field(typeof(SetLocker), "used");
        private static readonly FieldInfo Activated =
            AccessTools.Field(typeof(Activatable), "activated");
        private static readonly MethodInfo UpdateFlag =
            AccessTools.Method(typeof(Activatable), "UpdateFlag");


        public static void ToggleLoreBearer(GameObject origin, GameObject cloned) {
            if (origin == null || cloned == null) return;
            var originComponent = origin.GetComponent<LoreBearer>();
            var clonedComponent = cloned.GetComponent<LoreBearer>();
            ToggleLoreBearer(originComponent, clonedComponent);
        }

        public static void ToggleLoreBearer(LoreBearer origin, LoreBearer cloned) {
            if (origin == null || cloned == null) return;
            if (origin.SidescreenButtonInteractable()) return;
            LoreBareUsed?.SetValue(cloned, true);
        }

        public static void ToggleSetLocker(GameObject origin, GameObject cloned) {
            if (origin == null || cloned == null) return;
            var originComponent = origin.GetComponent<SetLocker>();
            var clonedComponent = cloned.GetComponent<SetLocker>();
            ToggleSetLocker(originComponent, clonedComponent);
        }

        public static void ToggleSetLocker(SetLocker origin, SetLocker cloned) {
            if (origin == null || cloned == null) return;
            if (origin.SidescreenButtonInteractable()) return;
            SetLockerUsed?.SetValue(cloned, true);
        }

        public static void ToggleActivated(Activatable origin, Activatable cloned)
        {
            var activated = origin.IsActivated;
            if (!activated) return;
            Activated?.SetValue(cloned, true);
            UpdateFlag?.Invoke(cloned, null);

        }
    }
}