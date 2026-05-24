

using GlobalUtil.UI;
using UnityEngine.UI;

namespace MovableFeatures.Screen
{
    public class SettingScreen : FScreen
    {
        private Toggle _generateUnobtanium;
        private Toggle _toggleGeyserAttribute;
        private Toggle _stableMode;

        public override void SetObjects()
        {
            base.SetObjects();
            transform.Find("Container/Head/Title").GetComponent<LocText>().text =
                Strings.Get("STRINGS.UI.SCHEDULESCREEN.SETTINGS");
            transform.Find("Container/Head/Close").GetComponent<Button>().onClick.AddListener(OnClickCancel);
            var setting1 = transform.Find("Container/Content/Setting1");
            _generateUnobtanium = setting1.GetComponent<Toggle>();
            setting1.Find("Label").GetComponent<LocText>().text = Text.GenerateUnobtanium;
            var setting2 = transform.Find("Container/Content/Setting2");
            _toggleGeyserAttribute = setting2.GetComponent<Toggle>();
            setting2.Find("Label").GetComponent<LocText>().text = Text.ToggleGeyserNum;
            var setting3 = transform.Find("Container/Content/Setting3");
            _stableMode = setting3.GetComponent<Toggle>();
            setting3.Find("Label").GetComponent<LocText>().text = Text.StableMode;
            var buttonCancel = transform.Find("Container/Content/ButtonGroup/Cancel");
            buttonCancel.GetComponent<Button>().onClick.AddListener(OnClickCancel);
            buttonCancel.Find("Label").gameObject.GetComponent<LocText>().text =
                Strings.Get("STRINGS.UI.FRONTEND.SAVESCREEN.CANCELNAME");
            var buttonSave = transform.Find("Container/Content/ButtonGroup/Confirm");
            buttonSave.GetComponent<Button>().onClick.AddListener(SaveSettings);
            buttonSave.Find("Label").gameObject.GetComponent<LocText>().text =
                Strings.Get("STRINGS.UI.FRONTEND.PAUSE_SCREEN.SAVE");

            _generateUnobtanium.isOn = Settings.GenerateUnobtanium;
            _toggleGeyserAttribute.isOn = Settings.ToggleGeyserAttribute;
            _stableMode.isOn = Settings.StableMode;

            _generateUnobtanium.onValueChanged.AddListener( isOn =>
            {
                if (isOn == Settings.GenerateUnobtanium) return;
                Settings.GenerateUnobtanium = isOn;
                Settings.Dirty =  true;
            });
            _toggleGeyserAttribute.onValueChanged.AddListener( isOn =>
            {
                if (isOn == Settings.ToggleGeyserAttribute) return;
                Settings.ToggleGeyserAttribute = isOn;
                Settings.Dirty =  true;
            });
            _stableMode.onValueChanged.AddListener(isOn =>
            {
                if (isOn == Settings.StableMode) return;
                Settings.StableMode = isOn;
                Settings.Dirty =  true;
            });

            setting1.gameObject.AddComponent<ToolTip>().toolTip = Text.GenerateUnobtaniumDesc;
            setting2.gameObject.AddComponent<ToolTip>().toolTip = Text.ToggleGeyserNumDesc;
            setting3.gameObject.AddComponent<ToolTip>().toolTip = Text.StableModeDesc;
        }

        public void SaveSettings()
        {
            Settings.Save();
            OnClickCancel();
        }
    }
}