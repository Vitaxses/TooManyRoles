using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using static TooManyRoles.Objects.PercentageTypes;
using Random = System.Random;
using static TooManyRoles.Roles.RoleHelper;
using static TooManyRoles.SettingsHandler;
using TooManyRoles.Settings;

namespace TooManyRoles.Patches;

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
public class RoleManagerPatch {

    private static GameMode gameMode;
    public static System.Random rndm = new System.Random();
    private static int crewValues;
    private static int impValues;

    public static bool allPlayersHaveRole = false;

    public static bool Prefix(RoleManager __instance) {
        gameMode = SettingsHandler.gameMode;
        if (gameMode == GameMode.Draft) return false;
        rndm = new System.Random((int)DateTime.Now.Ticks);
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPCs.ResetVariables, SendOption.Reliable, -1);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCMethods.Reset();
        MessageWriter writer1 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPCs.ShareGamemode, SendOption.Reliable, -1);
        writer1.Write((byte)gameMode);
        AmongUsClient.Instance.FinishRpcImmediately(writer1);
        CustomSetting.ShareSettingsRPC();
        RPCMethods.ShareGamemode((byte)gameMode);

        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;

        List<PlayerControl> players = new();
        foreach (NetworkedPlayerInfo In in GameData.Instance.AllPlayers) {
            players.Add(In._object);
        }
        if (players.Count == 0) TooManyRolesPlugin.logger.LogMessage("Players count is 0 ?. line 24 ish, AssignRolesPatch.cs");

        AssignRoles();
        return true;
    }

    private static void AssignRoles() {
        RoleSelectorData dataSelector = selectRolesInData(gameMode);
        var num = 0;
        while (num < 301) {
            num++;
        }
        while (num > 300) {
            
            //if (gameMode == GameMode.Mafia) { AssignMafiaRoles(crewmates, imps, thirdpartys); return; }
            /*if (gameMode == GameMode.PropHunt) { AssignProphunt(); return; }
            if (gameMode == GameMode.HideNSeek) {AssignHideNSeekRoles(); return; }*/

            if (gameMode == GameMode.Normal) { AssignNormalRoles(dataSelector); return; }
            break;
        }
    }

    private static RoleSelectorData selectRolesInData(GameMode gamemode) {
        List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
        crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
        List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
        impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

        var crewmateMin = SettingsHandler.minCrewmateRoles.getSelection();
        var crewmateMax = SettingsHandler.maxCrewmateRoles.getSelection();
        var neutralMin = SettingsHandler.minNeutralRoles.getSelection();
        var neutralMax = SettingsHandler.maxNeutralRoles.getSelection();
        var MaxNKCount = SettingsHandler.maxNeutralKillerRoles.getSelection();
        var impostorMin = SettingsHandler.minImpostorRoles.getSelection();
        var impostorMax = SettingsHandler.maxImpostorRoles.getSelection();
        var modifierMin = SettingsHandler.minModifierRoles.getSelection();
        var modifierMax = SettingsHandler.maxModifierRoles.getSelection();
 
        if (crewmateMin > crewmateMax) crewmateMin = crewmateMax;
        if (neutralMin > neutralMax) neutralMin = neutralMax;
        if (impostorMin > impostorMax) impostorMin = impostorMax;
        if (modifierMin > modifierMax) modifierMin = modifierMax;
 
        if (SettingsHandler.GiveEveryCrewmateRole.getBool()) {
            crewmateMax = crewmates.Count - neutralMin;
            crewmateMin = crewmates.Count - neutralMax;
        }
         
        int crewCountSettings = rndm.Next(crewmateMin, crewmateMax + 1);
        int neutralCountSettings = rndm.Next(neutralMin, neutralMax + 1);
        int impCountSettings = rndm.Next(impostorMin, impostorMax + 1);
 
        int maxCrewmateRoles = Mathf.Min(crewmates.Count, crewCountSettings);
        int maxNeutralRoles = Mathf.Min(crewmates.Count, neutralCountSettings);
        int maxImpostorRoles = Mathf.Min(impostors.Count, impCountSettings);

        List<Role> crewRoles = new();
        List<Role> impRoles = new();
        List<Role> neutralRoles = new();
        List<Role> modifiers = new();
        
        foreach (Role role in RPCMethods.Roles) {
            if (!role.Gamemodes.Contains(gamemode)) continue;
            if (role.IsModifier) modifiers.Add(role);
            if (role.isCrewmate() && !role.IsModifier) crewRoles.Add(role);
            if (role.isImpostor() && !role.IsModifier) impRoles.Add(role);
            if (role.isNeutral() && !role.IsModifier) {
                if (role.IsKiller() && MaxNKCount > neutralRoles.Count) {neutralRoles.Add(role); continue;}
                neutralRoles.Add(role);
            }
        }

        return new RoleSelectorData {
            crewmates = crewmates,
            impostors = impostors,
            crewRoles = crewRoles,
            neutralRoles = neutralRoles,
            impRoles = impRoles,
            modifiers = modifiers,
            maxCrewmateRoles = maxCrewmateRoles,
            maxNeutralRoles = maxNeutralRoles,
            maxImpostorRoles = maxImpostorRoles,
            maxModifierRoles = modifierMax
        };
    }

    private static void AssignNormalRoles(RoleSelectorData data) {
        data.AssignEnsured();
        data.AssignUnsure();
        data.AssignModifiers();
    }

    private static void setRoleToPlayer(int roleId, List<PlayerControl> playersThatCanGetThisRole) {
        playersThatCanGetThisRole.RemoveAll(x => x.Data == null || x == null || x.hasRole());
        
        if (playersThatCanGetThisRole.Count == 0) return;


        var playerToBeAssigned = getRandomPlayerFromList(playersThatCanGetThisRole);
    
        if (playerToBeAssigned == null) { TooManyRolesPlugin.logger.LogMessage("Player To Be Assigned is null while assigning Roles"); return; }
        
        MessageWriter setRoleWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPCs.SetRole, Hazel.SendOption.Reliable);
        setRoleWriter.Write(playerToBeAssigned.Data.PlayerId);
        setRoleWriter.Write(roleId);
        
        AmongUsClient.Instance.FinishRpcImmediately(setRoleWriter);
        
        RPCMethods.SetRole(playerToBeAssigned.Data.PlayerId, roleId);
    }

    public static PlayerControl getRandomPlayerFromList(List<PlayerControl> players) {
        int i = rndm.Next(0, players.Count);
        PlayerControl player = null;
        try {
            player = players[i];
        } catch {
            TooManyRolesPlugin.logger.LogMessage("Error: AssignRolesPatch.cs at getRandomListFromPlayerthingie");
        }
        return player;
    }

    public class RoleSelectorData {
        public List<PlayerControl> crewmates {get;set;}
        public List<PlayerControl> impostors {get;set;}
        public List<Role> impRoles = new();
        public List<Role> neutralRoles = new();
        public List<Role> crewRoles = new();
        public List<Role> modifiers = new();
        public int maxCrewmateRoles {get;set;}
        public int maxNeutralRoles {get;set;}
        public int maxImpostorRoles {get;set;}
        public int maxModifierRoles {get;set;}

        public void AssignEnsured() {
            List<Role> crew = new();
            foreach (Role role in crewRoles) { if (role.is100()) crew.Add(role); }
            List<Role> neutral = new();
            foreach (Role role in neutralRoles) { if (role.is100()) neutral.Add(role); }
            List<Role> imps = new();
            foreach (Role role in impRoles) { if (role.is100()) imps.Add(role); }

            while (impostors.Count > 0 && maxImpostorRoles > 0 && imps.Count > 0) {
                var index = rndm.Next(0, imps.Count);
                setRoleToPlayer(index, impostors);
                maxImpostorRoles--; impValues -= 10; break;
            }

            while (crewmates.Count > 0 && maxCrewmateRoles > 0 && crew.Count > 0) {
                var index = rndm.Next(0, crew.Count);
                setRoleToPlayer(index, crewmates);
                maxCrewmateRoles--; crewValues -= 10; break;
            }

            while (crewmates.Count > 0 && maxNeutralRoles > 0 && neutral.Count > 0) {     
                var index = rndm.Next(0, crew.Count);
                setRoleToPlayer(index, crewmates);      
                maxNeutralRoles--; break;
            }
        }

        public void AssignUnsure() {
            List<Role> crew = new();
            foreach (Role role in crewRoles) { if (role.is100()) crew.Add(role); }
            List<Role> neutral = new();
            foreach (Role role in neutralRoles) { if (role.is100()) neutral.Add(role); }
            List<Role> imps = new();
            foreach (Role role in impRoles) { if (role.is100()) imps.Add(role); }
            crew = crew.OrderByDescending(r => r.getSelection()).ToList();
            neutral = neutral.OrderByDescending(r => r.getSelection()).ToList();
            imps = imps.OrderByDescending(r => r.getSelection()).ToList();

            while (impostors.Count > 0 && maxImpostorRoles > 0 && imps.Count > 0) {
                var index = rndm.Next(0, imps.Count);
                setRoleToPlayer(index, impostors);
                maxImpostorRoles--; impValues -= 10;
            }

            while (crewmates.Count > 0 && maxCrewmateRoles > 0 && crew.Count > 0) {
                var index = rndm.Next(0, crew.Count);
                setRoleToPlayer(index, crewmates);
                maxCrewmateRoles--; crewValues -= 10;
            }

            while (crewmates.Count > 0 && maxNeutralRoles > 0 && neutral.Count > 0) {     
                var index = rndm.Next(0, crew.Count);
                setRoleToPlayer(index, crewmates);      
                maxNeutralRoles--;
            }
        }
    
        public void AssignModifiers() {
            List<PlayerControl> players = new();
            foreach (var player in impostors) {
                players.Add(player);
            }
            foreach (var player in crewmates) {
                players.Add(player);
            }
            List<Role> UnsureModifiers = new();
            List<Role> EnsuredModifiers = new();
            foreach (Role r in EnsuredModifiers) {
                if (r.getSelection() > 0) {
                    if (r.is100()) EnsuredModifiers.Add(r); 
                    else UnsureModifiers.Add(r);
                }
            }
            UnsureModifiers.OrderByDescending(r => r.getSelection()).ToList();

            foreach (Role role in EnsuredModifiers) {
                if(maxModifierRoles == 0) break;
                var team = role.Team;
                foreach (var player in players) {
                    
                    var pTeam = player.Data.Role.IsImpostor ? Team.Impostor : Team.Crewmate;
                    if ((team == Team.All || pTeam == team) && player.getModifier() == null) {
                        role.setPlayer(player);
                        players.Remove(player);
                        maxModifierRoles--;
                    }
                }
            }

        }

    }
}


[HarmonyPatch(typeof(RoleOptionsCollectionV07), nameof(RoleOptionsCollectionV07.GetNumPerGame))]
public class DeactivateVannilaRoles {

    public static void Postfix(ref int __result) {
        __result = 0;
    }

}