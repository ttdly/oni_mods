using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace MovableFeatures
{
    internal static class WarpConduitPortAccess
    {
        private static readonly FieldInfo LiquidPortField =
            AccessTools.Field(typeof(WarpConduitSender), "liquidPort");
        private static readonly FieldInfo GasPortField =
            AccessTools.Field(typeof(WarpConduitSender), "gasPort");

        private static Type _portType;
        private static ConstructorInfo _portCtor;
        private static FieldInfo _portInputCellField;
        private static FieldInfo _portNetworkItemField;

        public static object GetLiquidPort(WarpConduitSender sender)
        {
            return LiquidPortField?.GetValue(sender);
        }

        public static object GetGasPort(WarpConduitSender sender)
        {
            return GasPortField?.GetValue(sender);
        }

        public static void SetLiquidPort(WarpConduitSender sender, object port)
        {
            LiquidPortField?.SetValue(sender, port);
        }

        public static void SetGasPort(WarpConduitSender sender, object port)
        {
            GasPortField?.SetValue(sender, port);
        }

        public static bool TryCreatePort(GameObject parent, ConduitPortInfo info, int number, Storage targetStorage,
            out object port)
        {
            port = null;
            if (parent == null || info == null || targetStorage == null) return false;
            if (!EnsurePortCtor()) return false;
            port = _portCtor.Invoke(new object[] { parent, info, number, targetStorage });
            if (port == null) return false;
            EnsurePortFields(port);
            return true;
        }

        public static bool TryGetPortValues(object port, out int inputCell, out object networkItem)
        {
            inputCell = 0;
            networkItem = null;
            if (port == null) return false;
            EnsurePortFields(port);
            if (_portInputCellField == null || _portNetworkItemField == null) return false;
            var inputCellObj = _portInputCellField.GetValue(port);
            var networkItemObj = _portNetworkItemField.GetValue(port);
            if (!(inputCellObj is int cell) || networkItemObj == null) return false;
            inputCell = cell;
            networkItem = networkItemObj;
            GlobalUtil.Logger.Info($"WarpConduitPort inputCell: {inputCell}");
            return true;
        }

        private static bool EnsurePortCtor()
        {
            if (_portCtor != null) return true;
            if (_portType == null)
                _portType = LiquidPortField?.FieldType ?? GasPortField?.FieldType;
            if (_portType == null) return false;
            _portCtor = AccessTools.Constructor(_portType,
                new[] { typeof(GameObject), typeof(ConduitPortInfo), typeof(int), typeof(Storage) });
            return _portCtor != null;
        }

        private static void EnsurePortFields(object port)
        {
            if (_portInputCellField != null && _portNetworkItemField != null) return;
            var type = port.GetType();
            if (_portType != null && _portType != type) return;
            _portType = type;
            _portInputCellField = AccessTools.Field(type, "inputCell");
            _portNetworkItemField = AccessTools.Field(type, "networkItem");
        }
    }
}
