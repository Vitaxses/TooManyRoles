using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sentry.Unity.NativeUtils;
using TooManyRoles.Objects;
using UnityEngine;
using static TooManyRoles.SettingsHandler;

namespace TooManyRoles.Roles;

public static class RoleHelper {

    public enum Team {
        Impostor,
        Crewmate,
        Neutral,
        All
    }

    public abstract class Role {
        public PlayerControl Player;
        public string Name = "role name";
        public string TaskObjective = "TaskObjective";
        public string IntroObjective = "IntroObjective";
        public Team Team = Team.Crewmate;
        public bool IsModifier = false;
        public List<GameMode> Gamemodes = new();
        public Color32 Color = new Color32(255, 0, 0, 255);
        public int Id = 0;
        public bool isKiller = false;

        public virtual void addMe() {

        }

        public virtual int getSelection() {
            return 0;
        }

        public void setPlayer(PlayerControl player) {
            if (player == null ||player.Data == null) return;
            Player = player;
        }

        public void resetPlayer() {
            Player = null;
        }

        public bool IsKiller() {
            return isKiller;
        }

    }

    public static bool is100(this Role role) {
        if (role.getSelection() == PercentageTypes.percentages.Length) return true;
        return false;
    }

    public static bool isImpostor(this Role role) {
        return role.Team == Team.Impostor;
    }

    public static bool isCrewmate(this Role role) {
        return role.Team == Team.Crewmate;
    }
    public static bool isNeutral(this Role role) {
        return role.Team == Team.Neutral;
    }

    public static Role getRoleFromName(string name) {
        Role role = null;
        switch (name) {
            case "Sheriff":
            role = Sheriff.sheriffRole;
            break;
        }
        return role;
    }

    public static Role getRoleFromId(int id) {
        Role role = null;
        switch (id) {
            case (int)RoleId.Sheriff:
            role = Sheriff.sheriffRole;
            break;
        }
        return role;
    }

}