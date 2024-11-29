using System;
using System.Collections.Generic;
using Hazel;

namespace TooManyRoles.Settings;

public class CustomSetting {

    public static List<CustomSetting> allSettings = new();

    public enum MenuType {
        Crewmate,
        Neutral,
        Impostor,
        Modifier,
        Other
    }

    public enum SettingType {
        Checkbox,
        Number,
        String,
        Header
    }

    public OptionBehaviour Setting;
    public int Id;
    public string Name;
    public MenuType Menu;
    public SettingType settingType;
    public string[] selectables;
    public int selection;
    public Object defaultSelectable;

    public CustomSetting(int Id, string Name, MenuType Menu, SettingType settingType, string[] selectables, int defaultSelectable) {
        this.Id = Id;
        this.Name = Name;
        this.Menu = Menu;
        this.settingType = settingType;
        this.selectables = selectables;
        this.defaultSelectable = defaultSelectable;

        allSettings.Add(this);
    }

    public virtual object getValue() {
        // gets overriden
        return null;
    }

    public bool getBool() {
        return (bool)getValue();
    }

    public float getFloat() {
        return (float)getValue();
    }

    public int getSelection() {
        return selection;
    }

    public virtual void Set(int index, bool sendRPC = true) {
        System.Console.WriteLine($"{Name} set to {index}");

            selection = index;

            if (Setting != null && AmongUsClient.Instance.AmHost)

            try {
                if (Setting is ToggleOption toggle) {
                    toggle.oldValue = (bool)getValue();
                    if (toggle.CheckMark != null) toggle.CheckMark.enabled = (bool)getValue();
                }
                else if (Setting is NumberOption number) {
                    var newValue = getFloat();

                    number.Value = number.oldValue = newValue;
                    number.ValueText.text = ToString();
                }
                else if (Setting is StringOption str) {
                    var newValue = (int)getValue();

                    str.Value = str.oldValue = newValue;
                    str.ValueText.text = ToString();
                }
            }
            catch
            {
            }
        
        if (sendRPC) ShareSettingsRPC();
    }

    public virtual void OnOptionCreated() {
        if (settingType == SettingType.Number) {
            Setting = new NumberOption();
            if (Setting.gameObject != null) Setting.gameObject.name = Name;
            if (Setting is NumberOption option) {
                option.TitleText.text = Name;
                option.ValueText.text = (string)getValue();
                option.ValidRange = new FloatRange(0, selectables.Length -1);
                option.oldValue = option.Value = selection;
            }
        } else if (settingType == SettingType.String) {
            Setting = new StringOption();
            if (Setting.gameObject != null) Setting.gameObject.name = Name;
            if (Setting is StringOption option) {
                option.TitleText.text = Name;
                option.ValueText.text = (string)getValue();
                option.oldValue = option.Value = selection;
            }
        } else if (settingType == SettingType.Checkbox) {
            Setting = new ToggleOption();
            if (Setting.gameObject != null) Setting.gameObject.name = Name;
            if (Setting is ToggleOption option) {
                option.TitleText.text = Name;
                option.oldValue = option.CheckMark.enabled = (bool)getValue();
            }
        } else if (settingType == SettingType.Header) {
            Setting.Cast<ToggleOption>().TitleText.text = Name;
        }
    }

    public static void ShareSettingsRPC() {
        if (!AmongUsClient.Instance.AmHost) return;
             List<CustomSetting> options = allSettings;

            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte) CustomRPCs.ShareSettings, SendOption.Reliable);
            foreach (var option in options) {
                if (writer.Position > 1000) {
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte) CustomRPCs.ShareSettings, SendOption.Reliable);
                }
                writer.Write(option.Id);
                writer.Write(option.selection);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

}