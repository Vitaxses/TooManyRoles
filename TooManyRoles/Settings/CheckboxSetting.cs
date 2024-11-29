using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using static TooManyRoles.Settings.CustomSetting;

namespace TooManyRoles.Settings;

public class CheckboxSetting : CustomSetting {

    public CheckboxSetting(int Id, string Name, MenuType menu, string[] nothing, int defualtSelectable = 1)
     : base(Id, Name, menu, SettingType.Checkbox, nothing, defualtSelectable) {
        
        // 0 is on 1 is off
        if ((int)defaultSelectable > 1) defaultSelectable = 1;
        selection = (int)defaultSelectable;
    }

    public override object getValue() {
        if (selection > 0) return false;
        else return true;
    }

    public void Toggle() {
        if (selection > 0)
        Set(0);
        else Set(1);
    }

}


[HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Toggle))]
public class ToggleOptionPatch {
    public static bool Prefix(ToggleOption __instance) {
        var option = allSettings.FirstOrDefault(option => option.Setting == __instance); // Works but may need to change to gameObject.name check
        if (option is CheckboxSetting toggle)
        {
            toggle.Toggle();
            return false;
        }
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek || __instance.boolOptionName == BoolOptionNames.VisualTasks ||
            __instance.boolOptionName == BoolOptionNames.AnonymousVotes || __instance.boolOptionName == BoolOptionNames.ConfirmImpostor) return true;
        return false;
    }
}