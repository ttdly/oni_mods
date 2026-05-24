using System.IO;
using Newtonsoft.Json;

namespace MovableFeatures
{
    public class Settings
    {
        public static bool GenerateUnobtanium = true;
        public static bool ToggleGeyserAttribute = true;
        public static bool StableMode = false;

        public static bool Dirty = false;

        public static void Save()
        {
            if (!Dirty)  return;
            var data = new SettingsData()
            {
                GenerateUnobtanium = GenerateUnobtanium,
                ToggleGeyserAttribute = ToggleGeyserAttribute,
                StableMode = StableMode
            };

            var json = JsonConvert.SerializeObject(
                data,
                Formatting.Indented
            );

            File.WriteAllText(Mod.ConfigPath, json);
        }

        public static void Load()
        {
            if (!File.Exists(Mod.ConfigPath))
            {
                Save();
                GlobalUtil.Logger.Info("config file not found, while create one");
                return;
            }

            try
            {
                var json = File.ReadAllText(Mod.ConfigPath);

                var data =
                    JsonConvert.DeserializeObject<SettingsData>(json);

                if (data == null)
                    return;

                GenerateUnobtanium = data.GenerateUnobtanium;
                ToggleGeyserAttribute = data.ToggleGeyserAttribute;
                StableMode = data.StableMode;
            }
            catch (System.Exception e)
            {
                GlobalUtil.Logger.Error(e);
            }
        }
    }

    public class SettingsData
    {
        public bool GenerateUnobtanium;
        public bool ToggleGeyserAttribute;
        public bool StableMode;
    }
}