using static TooManyRoles.PlayerHelper;
using HarmonyLib;
using Hazel;
using static TooManyRoles.Roles.RoleHelper;
using TooManyRoles.Roles;
using static TooManyRoles.SettingsHandler;
using System.Collections.Generic;
using TooManyRoles.Patches;
using TooManyRoles.Settings;
using System;
using static TooManyRoles.Settings.CustomSetting;
using System.Linq;
using UnityEngine;

namespace TooManyRoles;
    
    public enum CustomRPCs {
        ResetVariables = 64,
        SetRole,
        ShareGamemode,
        ShareSettings
    }

    public enum RoleId {

        Sheriff = 1
    }

    public static class RPCMethods {

        public static List<Role> Roles = new();
        
        public static void Reset() {
            Sheriff.Reset();
            NamePatch.resetNames();
        }

        public static void ShareSettings(MessageReader reader) {
            if (AmongUsClient.Instance.AmHost) return; // if it somehow got it!
            TooManyRolesPlugin.logger.LogMessage("Recieved Settings... Syncing them");
            while (reader.BytesRemaining > 0) {
                var id = reader.ReadInt32();
                var customSetting = allSettings.FirstOrDefault(option => option.Id == id); // Works but may need to change to gameObject.name check
                var type = customSetting?.settingType;

                customSetting?.Set(reader.ReadInt32());

                var panels = GameObject.FindObjectsOfType<ViewSettingsInfoPanel>();
                foreach (var panel in panels) {
                    if (panel.titleText.text == customSetting.Name) {
                        panel.SetInfo(StringNames.ImpostorsCategory, customSetting.ToString(), 61);
                        panel.titleText.text = customSetting.Name;
                    }
                }
            }
        }

        public static void ShareGamemode(byte id) {
            if (id > 4) {
                SettingsHandler.gameMode = GameMode.Normal;
                TooManyRolesPlugin.logger.LogWarning("id was over 4. SharegameMode RPC");
                return;
            }
            switch (id) {
                case (byte)GameMode.Draft:
                SettingsHandler.gameMode = GameMode.Draft;
                break;
                case (byte)GameMode.PropHunt:
                SettingsHandler.gameMode = GameMode.PropHunt;
                break;
                case (byte)GameMode.Normal:
                SettingsHandler.gameMode = GameMode.Normal;
                break;
                case (byte)GameMode.Mafia:
                SettingsHandler.gameMode = GameMode.Mafia;
                break;
                case (byte)GameMode.HideNSeek:
                SettingsHandler.gameMode = GameMode.HideNSeek;
                break;
            }
        }

        public static void SetRole(byte playerId, int roleId) {
            PlayerControl p = GetPlayer(playerId);
            if (p == null) return;
            Role newrole = null;
            switch (roleId) {
                case (int)RoleId.Sheriff:
                Sheriff.sheriffRole.setPlayer(p);
                break;
            }
            if (PlayerControl.LocalPlayer != null && p.Data.PlayerId == PlayerControl.LocalPlayer.Data.PlayerId) NamePatch.myLocalRole = newrole;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    public static class RPCPatches {

        public static void Postfix([HarmonyArgument(0)]byte rpcId, [HarmonyArgument(1)]MessageReader reader) {
            switch (rpcId) {
                case (byte)CustomRPCs.SetRole:
                    byte pId = reader.ReadByte();
                    int RoleId = reader.ReadInt32();
                    RPCMethods.SetRole(pId, RoleId);
                    break;
                case (byte)CustomRPCs.ShareGamemode:
                    byte gamemodeId = reader.ReadByte();
                    RPCMethods.ShareGamemode(gamemodeId);
                    break;
                case (byte)CustomRPCs.ResetVariables:
                    RPCMethods.Reset();
                    break;
                case (byte)CustomRPCs.ShareSettings:
                    RPCMethods.ShareSettings(reader);
                    break;
            }
        }

    }