using System.ComponentModel.DataAnnotations;
using TooManyRoles.Objects;
using TooManyRoles.Roles;
using TooManyRoles.Settings;
using UnityEngine;
using Menu = TooManyRoles.Settings.CustomSetting.MenuType;

namespace TooManyRoles;

public class SettingsHandler {

    public static SettingsHandler Instance;

    public enum GameMode {
        Normal,
        Draft,
        PropHunt,
        Mafia,
        HideNSeek
    }

    public static GameMode gameMode = GameMode.Normal;
    public static CheckboxSetting GiveEveryCrewmateRole;

    public static HeaderSetting sheriffHeader;
    public static StringSetting sheriffPercentage;
    public static CheckboxSetting sheriffCanKillNeutral;

    public static NumberSetting minCrewmateRoles;
    public static NumberSetting maxCrewmateRoles;
    public static NumberSetting minImpostorRoles;
    public static NumberSetting maxImpostorRoles;
    public static NumberSetting minNeutralRoles;
    public static NumberSetting maxNeutralRoles;
    public static NumberSetting maxNeutralKillerRoles;
    public static NumberSetting minModifierRoles;
    public static NumberSetting maxModifierRoles;

    public static string colorfy(Color c, string s) {
        return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", (byte)(Mathf.Clamp01(c.r) * 255), (byte)(Mathf.Clamp01(c.g) * 255), (byte)(Mathf.Clamp01(c.b) * 255), (byte)(Mathf.Clamp01(c.a) * 255), s);
    }

    public static void Load() {
        int num = 0;

        GiveEveryCrewmateRole = new CheckboxSetting(num++, "Everymate", Menu.Other, PercentageTypes.nothing, 0);
        minCrewmateRoles = new NumberSetting(num++, "Min Crewmate Roles", Menu.Other, new string[]{"0", "1", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13"}, 3);
        maxCrewmateRoles = new NumberSetting(num++, "Max Crewmate Roles", Menu.Other, new string[]{"0", "1", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13"}, 3);
        minImpostorRoles = new NumberSetting(num++, "Min Impostor Roles", Menu.Other, new string[]{"0", "1", "2", "3"}, 3);
        maxImpostorRoles = new NumberSetting(num++, "Max Impostor Roles", Menu.Other, new string[]{"0", "1", "2", "3"}, 3);
        minNeutralRoles = new NumberSetting(num++, "Min Neutral Roles", Menu.Other, new string[]{"0", "1", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13"}, 3);
        maxNeutralKillerRoles = new NumberSetting(num++, "Max Neutral Killer Roles", Menu.Other, new string[]{"0", "1",}, 3);
        maxNeutralRoles = new NumberSetting(num++, "Max Neutral Roles", Menu.Other, new string[]{"0", "1", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13"}, 3);
        minModifierRoles = new NumberSetting(num++, "Min Modifier Roles", Menu.Other, new string[]{"0", "1", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15"}, 1);
        minModifierRoles = new NumberSetting(num++, "Min Modifier Roles", Menu.Other, new string[]{"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15"}, 2);

        sheriffHeader = new HeaderSetting(num++, colorfy(Sheriff.color, "Sheriff"), Menu.Crewmate);
        sheriffPercentage = new StringSetting(num++, colorfy(Sheriff.color, "Sheriff"), Menu.Crewmate, PercentageTypes.percentages, 0);
        sheriffCanKillNeutral= new(num++, "Can Kill Neutrals", Menu.Crewmate, PercentageTypes.nothing, 0);

    }

}