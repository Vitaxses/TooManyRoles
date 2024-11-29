using InnerNet;
using static TooManyRoles.Roles.RoleHelper;

namespace TooManyRoles;

public static class PlayerHelper {

    public static PlayerControl GetPlayer(byte id) {
        PlayerControl p = PlayerControl.LocalPlayer;
        foreach (PlayerControl pp in PlayerControl.AllPlayerControls) {
            if (pp.Data.PlayerId == id) p = pp;
        }
        return p;
    }

    public static bool canMurderPlayer(PlayerControl target) {
        if (target.inMovingPlat) return false;
        return true;
    }

    public static bool hasRole(this PlayerControl p, bool countModifiers = false) {
        if (p == null) return false;
        foreach (Role role in RPCMethods.Roles) {
            if (role.Player.Data == p.Data) return true;
        }
        return false;
    }

    public static Role getRole(this PlayerControl p) {
        foreach (Role role in RPCMethods.Roles) {
            if (role.Player.Data == p.Data && !role.IsModifier) return role;
        }
        return null;
    }

    public static Role getModifier(this PlayerControl p) {
        foreach (Role role in RPCMethods.Roles) {
            if (role.Player.Data == p.Data && role.IsModifier) return role;
        }
        return null;
    }

    public static void setRole(this PlayerControl p, Role role) {
        role.setPlayer(p);
    }

    public static bool InLobby() {
        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started) {
            return false;
        }
        return true;
    }

}