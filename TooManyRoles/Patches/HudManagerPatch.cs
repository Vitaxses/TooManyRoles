using HarmonyLib;
using TooManyRoles.Buttons;
using TooManyRoles.Roles;

namespace TooManyRoles.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public class HudUpdatePatch {

    public static void Postfix(HudManager __instance) {
        CustomButton.HudUpdate();
    }

}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public class HudManagerStart {

    public static void Postfix(HudManager __instance) {
        Sheriff.getSheriffKillButton();
    }

}