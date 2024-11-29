using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using UnityEngine;

namespace TooManyRoles.Settings;

public class NumberSetting : CustomSetting {
    public NumberSetting(int Id, string Name, MenuType menu, string[] selectables, int defualtSelectable = 0)
     : base(Id, Name, menu, SettingType.Number, selectables, defualtSelectable) {
        
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
        return float.Parse(selectables[selection]);
    }
    
    public void Decrease() {
        Set(selection -1);
        if (Setting is NumberOption setting) {
            setting.Value = selection;
        }
    }

    public void Increase() {
        Set(selection +1);
        if (Setting is NumberOption setting) {
            setting.Value = selection;
        }
    }

}


[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
public class NumberOptionDecreasePatch {

    public static bool Prefix(NumberOption __instance) {
        var setting = CustomSetting.allSettings.FirstOrDefault(setting => setting.Setting = __instance);
        if (setting is NumberSetting setting1) {
            setting1.Decrease();
            return false;
        }
        return true;
    }

}

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
public class NumberOptionIncreasePatch {

    public static bool Prefix(NumberOption __instance) {
        var setting = CustomSetting.allSettings.FirstOrDefault(setting => setting.Setting = __instance);
        if (setting is NumberSetting setting1) {
            setting1.Increase();
            return false;
        }
        return true;
    }

}