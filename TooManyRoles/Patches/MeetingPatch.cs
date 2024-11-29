using HarmonyLib;
using TooManyRoles.Buttons;

namespace TooManyRoles.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
public class MeetingStartedPatch {

    public static void Prefix(PlayerControl __instance) {
        CustomButton.MeetingStartedUpdate();
    }

}