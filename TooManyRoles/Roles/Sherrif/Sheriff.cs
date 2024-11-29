using AmongUs.Data.Settings;
using Il2CppSystem.CodeDom;
using TooManyRoles.Buttons;
using UnityEngine;
using static TooManyRoles.Buttons.CustomButton;
using static TooManyRoles.Roles.RoleHelper;

namespace TooManyRoles.Roles;

public static class Sheriff {

    public class SheriffRole : Role {
        // all of these are used as tmplate loaders.

        public SheriffRole() {
            this.Color = color;
            this.Gamemodes.Clear();
            this.Gamemodes.Add(SettingsHandler.GameMode.Normal);
            this.Gamemodes.Add(SettingsHandler.GameMode.Draft);
            this.Id = id;
            this.Name = name;
            this.TaskObjective = taskObjective;
            this.IntroObjective = introObjective;
            this.Team = team;
            this.IsModifier = isModifier;
            addMe();
        }

        public static string name = "Sheriff";
        public static int id = (int)RoleId.Sheriff;
        public static string taskObjective = "Shoot The Impostors";
        public static string introObjective = "Shoot The Impostors";
        public static Team team = Team.Crewmate;
        public static bool isModifier = false;
        public static Color32 color = new Color32(255, 183, 0, 255);

        public override void addMe() {
            RPCMethods.Roles.Add(this);
        }
        public override int getSelection() {
            return SettingsHandler.sheriffPercentage.getSelection();
        }
    }

    public static PlayerControl target = null;
    public static Role sheriffRole;
    public static Color32 color = new Color32(255, 183, 0, 255);
    public static float KillCooldown = 16f;
    public static bool canKillNeutral = true;
    private static CustomButton killButton;
    public static CustomButton getSheriffKillButton() {
        if (killButton == null && HudManager.Instance != null) {
            killButton = new CustomButton(
                () => {
                    if (PlayerHelper.canMurderPlayer(target)) {
                        PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
                    } else {
                        if (PlayerHelper.canMurderPlayer(PlayerControl.LocalPlayer)) PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer, true);
                    }
                }, () => {return sheriffRole.Player.Data == PlayerControl.LocalPlayer.Data && !sheriffRole.Player.Data.IsDead;},
                 () => {return !sheriffRole.Player.inVent && target != null && !sheriffRole.Player.inMovingPlat;},
                () => {}, () => {killButton.Timer = killButton.MaxTimer;}, HudManager.Instance.KillButton.graphic.sprite, buttonPos.upperRowLeft, 
                HudManager.Instance, KeyCode.Q, false, sheriffRole.Color, 0f, ()=>{}, "kill", KillCooldown);
        }
        return killButton;
    }

    public static void Reset() {
        sheriffRole.resetPlayer();
        sheriffRole = new SheriffRole();
        sheriffRole.isKiller = true;
        killButton = null;
        KillCooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
        canKillNeutral = SettingsHandler.sheriffCanKillNeutral.getBool();
    }
    

}