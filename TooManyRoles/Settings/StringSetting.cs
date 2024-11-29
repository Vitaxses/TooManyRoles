using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace TooManyRoles.Settings;

public class StringSetting : CustomSetting {

    public StringSetting(int Id, string Name, MenuType menu, string[] selectables, int defualtSelectable = 0)
     : base(Id, Name, menu, SettingType.String, selectables, defualtSelectable) {
        
        defaultSelectable = Mathf.Clamp((int)defaultSelectable, 0, selectables.Length -1);
        selection = (int)defaultSelectable;
    }

    public override void Set(int index, bool sendRPC = true) {
        bool setSelection = true;
        if (selection < 1) { selection = selectables.Length -1; setSelection = false; }
        if (selection > selectables.Length -2) { selection = 0; setSelection = false; }

        if (setSelection) selection = index;

        base.Set(index, sendRPC);
    }

    public override object getValue() {
        return selectables[selection];
    }

    public void Decrease() {
        Set(selection -1);
        if (Setting is StringOption setting) {
            setting.Value = selection;
        }
    }

    public void Increase() {
        Set(selection +1);
        if (Setting is StringOption setting) {
            setting.Value = selection;
        }
    }

}


[HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
public class StringOptionDecreasePatch {

    public static bool Prefix(StringOption __instance) {
        var setting = CustomSetting.allSettings.FirstOrDefault(setting => setting.Setting = __instance);
        if (setting is StringSetting setting1) {
            setting1.Decrease();
            return false;
        }
        return true;
    }

}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
public class StringOptionIncreasePatch {

    public static bool Prefix(StringOption __instance) {
        var setting = CustomSetting.allSettings.FirstOrDefault(setting => setting.Setting = __instance);
        if (setting is StringSetting setting1) {
            setting1.Increase();
            return false;
        }
        return true;
    }

}