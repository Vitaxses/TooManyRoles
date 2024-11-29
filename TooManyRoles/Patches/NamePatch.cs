using HarmonyLib;
using Reactor.Utilities.Extensions;
using UnityEngine;
using static TooManyRoles.Roles.RoleHelper;

namespace TooManyRoles.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public class NamePatch {

    public static Role myLocalRole = null;
    public static TMPro.TextMeshPro myRoleText;
    private static GameObject nameTextObject;
    public static void Prefix(HudManager __instance) {
        if (PlayerControl.LocalPlayer == null || PlayerHelper.InLobby()) return;

        // ROLE TEXT UPDATER
        if ((nameTextObject == null || myRoleText == null) && myLocalRole != null) nameTextObject = GameObject.Instantiate(PlayerControl.LocalPlayer.cosmetics.nameText.gameObject, PlayerControl.LocalPlayer.cosmetics.nameText.gameObject.transform.parent);
        myRoleText = nameTextObject.GetComponent<TMPro.TextMeshPro>();
        nameTextObject.name = "MyLocalRoleName";
        nameTextObject.transform.localPosition.Set(nameTextObject.transform.localPosition.x, nameTextObject.transform.localPosition.y + 1.5f, nameTextObject.transform.localPosition.z);
        myRoleText.fontSize = 0.83f;
        myRoleText.color = Palette.White;
        PlayerControl.LocalPlayer.cosmetics.nameText.color = myRoleText.color;
    }

    public static void Postfix(HudManager __instance) {
        if (PlayerControl.LocalPlayer == null) return;
        if (myRoleText != null) {
        myRoleText.text = myLocalRole.Name;
        myRoleText.color = myLocalRole.Color;
        PlayerControl.LocalPlayer.cosmetics.nameText.color = myRoleText.color;
        }
    }

    public static void resetNames(bool unstantiate = false) {
        myRoleText = nameTextObject.GetComponent<TMPro.TextMeshPro>();
        nameTextObject.name = "MyLocalRoleName";
        nameTextObject.transform.localPosition.Set(nameTextObject.transform.localPosition.x, nameTextObject.transform.localPosition.y + 1.5f, nameTextObject.transform.localPosition.z);
        myRoleText.fontSize = 0.83f;
        myRoleText.color = Palette.White;
        PlayerControl.LocalPlayer.cosmetics.nameText.color = myRoleText.color;

        if (unstantiate) {
            nameTextObject.Destroy();
            myRoleText.Destroy();
        }
    }

}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public class RoleUpdater {

    public static void Postfix(PlayerControl __instance) {
        if (__instance.Data != PlayerControl.LocalPlayer) return;
        NamePatch.myLocalRole = PlayerControl.LocalPlayer.getRole();
    }
}