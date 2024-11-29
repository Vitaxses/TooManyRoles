using HarmonyLib;
using TooManyRoles.Buttons;

namespace TooManyRoles.Patches;

[HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
public class ExileControllerPatch {

    public static void Postfix(ExileController __instance) {

        CustomButton.MeetingEndedUpdate();
        CustomButton.ResetAllCooldowns();
    }

}