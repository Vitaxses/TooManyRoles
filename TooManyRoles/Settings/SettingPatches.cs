using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using static TooManyRoles.Settings.CustomSetting;

namespace TooManyRoles.Settings.Patches; 

public static class SettingsPatch {

    // Code Taken from Town-Of-Us-R Under GPL3 license
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    public class GameSettingMenuStartPatch {
        public static List<GameObject> Tabs = new();
        public static List<PassiveButton> Buttons = new();

        public static void Postfix(GameSettingMenu __instance) {
            LobbyInfoPane.Instance.EditButton.gameObject.SetActive(false);
            Tabs.ForEach(i => i?.Destroy());
            Buttons.ForEach(i => i?.Destroy());
            Buttons = new List<PassiveButton>();
            Tabs = new List<GameObject>();
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

            GameObject.Find("What Is This?")?.Destroy();
            GameObject.Find("RoleSettingsButton")?.Destroy();
            GameObject.Find("GamePresetButton")?.Destroy();
            __instance.ChangeTab(1, false);

            var settingsButton = GameObject.Find("GameSettingsButton");
            settingsButton.transform.localPosition += new Vector3(0f, 2f, 0f);
            settingsButton.transform.localScale *= 0.9f;

            CreateSettings(__instance, 3, "ModSettings", "TooManyRoles Settings", settingsButton, MenuType.Other);
            CreateSettings(__instance, 4, "CrewSettings", "Crewmate Settings", settingsButton, MenuType.Crewmate);
            CreateSettings(__instance, 5, "NeutralSettings", "Neutral Settings", settingsButton, MenuType.Neutral);
            CreateSettings(__instance, 6, "ImpSettings", "Impostor Settings", settingsButton, MenuType.Impostor);
            CreateSettings(__instance, 7, "ModifierSettings", "Modifier Settings", settingsButton, MenuType.Modifier);
        }

        public static void CreateSettings(GameSettingMenu __instance, int target, string name, string text, GameObject settingsInstance, MenuType menu) {
            var panel = GameObject.Find("LeftPanel");
            var button = GameObject.Find(name);
            if (button == null) {
                button = GameObject.Instantiate(settingsInstance, panel.transform);
                button.transform.localPosition += new Vector3(0f, -0.55f * target + 1.1f, 0f);
                button.name = name;
                __instance.StartCoroutine(Effects.Lerp(1f, new Action<float>(p => { button.transform.FindChild("FontPlacer").GetComponentInChildren<TextMeshPro>().text = text; })));
                var passiveButton = button.GetComponent<PassiveButton>();
                passiveButton.OnClick.RemoveAllListeners();
                passiveButton.OnClick.AddListener((System.Action)(() =>
                {
                    __instance.ChangeTab(target, false);
                }));
                passiveButton.SelectButton(false); 
                Buttons.Add(passiveButton);
            }

            var settingsTab = GameObject.Find("GAME SETTINGS TAB");
            Tabs.RemoveAll(x => x == null);
            var tab = GameObject.Instantiate(settingsTab, settingsTab.transform.parent);
            tab.name = name;
            var tabOptions = tab.GetComponent<GameOptionsMenu>();
            foreach (var child in tabOptions.Children) child.Destroy();
            tabOptions.scrollBar.transform.FindChild("SliderInner").DestroyChildren();
            tabOptions.Children.Clear();
            var settings = allSettings.Where(x => x.Menu == menu).ToList();

            float num = 1.5f;

            foreach (CustomSetting setting in settings) {
               if (setting.settingType == SettingType.Header) {
                    CategoryHeaderMasked header = UnityEngine.Object.Instantiate<CategoryHeaderMasked>(tabOptions.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, tabOptions.settingsContainer);
                    header.SetHeader(StringNames.ImpostorsCategory, 20);
                    header.Title.text = setting.Name;
                    header.transform.localScale = Vector3.one * 0.65f;
                    header.transform.localPosition = new Vector3(-0.9f, num, -2f);
                    num -= 0.625f;
                    continue;
                }

                if (setting.settingType == SettingType.Number) {
                    OptionBehaviour optionBehaviour = UnityEngine.Object.Instantiate<NumberOption>(tabOptions.numberOptionOrigin, Vector3.zero, Quaternion.identity, tabOptions.settingsContainer);
                    optionBehaviour.transform.localPosition = new Vector3(0.95f, num, -2f);
                    optionBehaviour.SetClickMask(tabOptions.ButtonClickMask);
                    SpriteRenderer[] components = optionBehaviour.GetComponentsInChildren<SpriteRenderer>(true);
                    for (int i = 0; i < components.Length; i++) components[i].material.SetInt(PlayerMaterial.MaskLayer, 20);

                    var numberOption = optionBehaviour as NumberOption;
                    setting.Setting = numberOption;

                    tabOptions.Children.Add(optionBehaviour);
                }

                else if (setting.settingType == SettingType.Checkbox) {
                    OptionBehaviour optionBehaviour = UnityEngine.Object.Instantiate<ToggleOption>(tabOptions.checkboxOrigin, Vector3.zero, Quaternion.identity, tabOptions.settingsContainer);
                    optionBehaviour.transform.localPosition = new Vector3(0.95f, num, -2f);
                    optionBehaviour.SetClickMask(tabOptions.ButtonClickMask);
                    SpriteRenderer[] components = optionBehaviour.GetComponentsInChildren<SpriteRenderer>(true);
                    for (int i = 0; i < components.Length; i++) components[i].material.SetInt(PlayerMaterial.MaskLayer, 20);

                    var toggleOption = optionBehaviour as ToggleOption;
                    setting.Setting = toggleOption;

                    tabOptions.Children.Add(optionBehaviour);
                }

                else if (setting.settingType == SettingType.String) {
                    OptionBehaviour optionBehaviour = UnityEngine.Object.Instantiate<StringOption>(tabOptions.stringOptionOrigin, Vector3.zero, Quaternion.identity, tabOptions.settingsContainer);
                    optionBehaviour.transform.localPosition = new Vector3(0.95f, num, -2f);
                    optionBehaviour.SetClickMask(tabOptions.ButtonClickMask);
                    SpriteRenderer[] components = optionBehaviour.GetComponentsInChildren<SpriteRenderer>(true);
                    for (int i = 0; i < components.Length; i++) components[i].material.SetInt(PlayerMaterial.MaskLayer, 20);

                    var stringOption = optionBehaviour as StringOption;
                    setting.Setting = stringOption;
                    tabOptions.Children.Add(optionBehaviour);
                }
                
                num -= 0.45f;
                tabOptions.scrollBar.SetYBoundsMax(-num - 1.65f);
                setting.OnOptionCreated();
            }

            for (int i = 0; i < tabOptions.Children.Count; i++)
            {
                OptionBehaviour optionBehaviour = tabOptions.Children[i];
                if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost) optionBehaviour.SetAsPlayer();
            }

            Tabs.Add(tab);
            tab.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab))]
    class ChangeTab {

        public static void Postfix(GameSettingMenu __instance, int tabNum, bool previewOnly) {
            if (previewOnly) return;
            foreach (var tab in GameSettingMenuStartPatch.Tabs) if (tab != null) tab.SetActive(false);
            foreach (var button in GameSettingMenuStartPatch.Buttons) button.SelectButton(false);
            if (tabNum > 2)
            {
                tabNum -= 3;
                GameSettingMenuStartPatch.Tabs[tabNum].SetActive(true);
                if (tabNum > 4) return;
                GameSettingMenuStartPatch.Buttons[tabNum].SelectButton(true);

                __instance.StartCoroutine(Effects.Lerp(1f, new Action<float>(p => {
                    foreach (CustomSetting option in allSettings) {
                        if (option.settingType == SettingType.Number) {
                            var number = option.Setting.Cast<NumberOption>();
                            number.TitleText.text = option.Name;
                            if (number.TitleText.text.StartsWith("<color="))
                                number.TitleText.fontSize = 3f;
                            else if (number.TitleText.text.Length > 20)
                                number.TitleText.fontSize = 2.25f;
                            else if (number.TitleText.text.Length > 40)
                                number.TitleText.fontSize = 2f;
                            else number.TitleText.fontSize = 2.75f;
                        }

                        else if (option.settingType == SettingType.Checkbox) {
                            var tgl = option.Setting.Cast<ToggleOption>();
                            tgl.TitleText.text = option.Name;
                            if (tgl.TitleText.text.Length > 20)
                                tgl.TitleText.fontSize = 2.25f;
                            else if (tgl.TitleText.text.Length > 40)
                                tgl.TitleText.fontSize = 2f;
                            else tgl.TitleText.fontSize = 2.75f;
                        }

                        else if (option.settingType == SettingType.String) {
                            var str = option.Setting.Cast<StringOption>();
                            str.TitleText.text = option.Name;
                            if (str.TitleText.text.Length > 20)
                                str.TitleText.fontSize = 2.25f;
                            else if (str.TitleText.text.Length > 40)
                                str.TitleText.fontSize = 2f;
                            else str.TitleText.fontSize = 2.75f;
                        }
                    }
                })));
            }
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Close))]
    public class GameSettingMenuClosePatch {

        public static void Prefix(GameSettingMenu __instance) {
            LobbyInfoPane.Instance.EditButton.gameObject.SetActive(true);
        }

    }

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.SetTab))]
    class SetTabPane {
        public static bool Prefix(LobbyViewSettingsPane __instance) {
            if ((int)__instance.currentTab < 6) {
                LobbyViewSettingsPanePatch.Postfix(__instance, __instance.currentTab);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.ChangeTab))]
    class LobbyViewSettingsPanePatch {

        public static void Postfix(LobbyViewSettingsPane __instance, StringNames category) {
            int tab = (int)category;

            foreach (var button in SettingsAwake.ButtonsList) button.SelectButton(false);
            if (tab > 5) return;
            __instance.taskTabButton.SelectButton(false);

            if (tab > 0)
            {
                tab -= 1;
                SettingsAwake.ButtonsList[tab].SelectButton(true);
                SettingsAwake.AddSettings(__instance, SettingsAwake.ButtonTypes[tab]);
            }
        }
    }

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Update))]
    class UpdatePane {
        public static void Postfix(LobbyViewSettingsPane __instance) {
            if (SettingsAwake.ButtonsList.Count == 0) SettingsAwake.Postfix(__instance);
        }
    }

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Awake))]
    class SettingsAwake {
        public static List<PassiveButton> ButtonsList = new List<PassiveButton>();
        public static List<MenuType> ButtonTypes = new List<MenuType>();

        public static void Postfix(LobbyViewSettingsPane __instance)
        {
            ButtonsList.ForEach(x => x?.Destroy());
            ButtonsList.Clear();
            ButtonTypes.Clear();

            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

            GameObject.Find("RolesTabs")?.Destroy();
            var overview = GameObject.Find("OverviewTab");
            overview.transform.localScale += new Vector3(-0.4f, 0f, 0f);
            overview.transform.localPosition += new Vector3(-1f, 0f, 0f);

            CreateButton(__instance, 1, "ModTab", "Mod Settings", MenuType.Other, overview);
            CreateButton(__instance, 2, "CrewmateTab", "Crewmate Settings", MenuType.Crewmate, overview);
            CreateButton(__instance, 3, "NeutralTab", "Neutral Settings", MenuType.Neutral, overview);
            CreateButton(__instance, 4, "ImpostorTab", "Impostor Settings", MenuType.Impostor, overview);
            CreateButton(__instance, 5, "ModifierTab", "Modifier Settings", MenuType.Modifier, overview);
        }

        public static void AddSettings(LobbyViewSettingsPane __instance, MenuType menu) {
            var options = allSettings.Where(x => x.Menu == menu).ToList();

            float num = 1.5f;
            int headingCount = 0;
            int settingsThisHeader = 0;
            int settingRowCount = 0;

            foreach (var option in options)  {
                if (option.settingType == SettingType.Header) {
                    if (settingsThisHeader % 2 != 0) num -= 0.85f;
                    CategoryHeaderMasked header = UnityEngine.Object.Instantiate<CategoryHeaderMasked>(__instance.categoryHeaderOrigin);
                    header.SetHeader(StringNames.ImpostorsCategory, 61);
                    header.Title.text = option.Name;
                    header.transform.SetParent(__instance.settingsContainer);
                    header.transform.localScale = Vector3.one;
                    header.transform.localPosition = new Vector3(-9.8f, num, -2f);
                    __instance.settingsInfo.Add(header.gameObject);
                    num -= 1f;
                    headingCount += 1;
                    settingsThisHeader = 0;
                    continue;
                } else { 
                    ViewSettingsInfoPanel panel = UnityEngine.Object.Instantiate<ViewSettingsInfoPanel>(__instance.infoPanelOrigin);
                    panel.transform.SetParent(__instance.settingsContainer);
                    panel.transform.localScale = Vector3.one;
                    if (settingsThisHeader % 2 != 0) {
                        panel.transform.localPosition = new Vector3(-3f, num, -2f);
                        num -= 0.85f;
                    } else {
                        settingRowCount += 1;
                        panel.transform.localPosition = new Vector3(-9f, num, -2f);
                    }
                    settingsThisHeader += 1;
                    panel.SetInfo(StringNames.ImpostorsCategory, option.ToString(), 61);
                    panel.titleText.text = option.Name;
                    __instance.settingsInfo.Add(panel.gameObject);
                }
            }

            float spacing = (headingCount * 1f + settingRowCount * 0.85f + 2f) / (headingCount + settingRowCount);
            __instance.scrollBar.CalculateAndSetYBounds(__instance.settingsInfo.Count + headingCount + settingRowCount, 4f, 6f, spacing);
        }

        public static void CreateButton(LobbyViewSettingsPane __instance, int target, string name, string text, MenuType menu, GameObject overview) {
            var tab = GameObject.Find(name);
            if (tab == null) {
                tab = GameObject.Instantiate(overview, overview.transform.parent);
                tab.transform.localPosition += new Vector3(2.05f, 0f, 0f) * target;
                tab.name = name;
                __instance.StartCoroutine(Effects.Lerp(1f, new Action<float>(p => { tab.transform.FindChild("FontPlacer").GetComponentInChildren<TextMeshPro>().text = text; })));
                var pTab = tab.GetComponent<PassiveButton>();
                pTab.OnClick.RemoveAllListeners();
                pTab.OnClick.AddListener((System.Action)(() => {
                    __instance.ChangeTab((StringNames)target);
                }));
                pTab.SelectButton(false);
                ButtonsList.Add(pTab);
                ButtonTypes.Add(menu);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    private class PlayerControlPatch {
        public static void Postfix() {
            if (PlayerControl.AllPlayerControls.Count < 2 || !AmongUsClient.Instance || !PlayerControl.LocalPlayer || !AmongUsClient.Instance.AmHost) return;
            CustomSetting.ShareSettingsRPC();
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
    private class PlayerJoinPatch {
        public static void Postfix() {
            if (PlayerControl.AllPlayerControls.Count < 2 || !AmongUsClient.Instance || !PlayerControl.LocalPlayer || !AmongUsClient.Instance.AmHost) return;
            CustomSetting.ShareSettingsRPC();
        }
    }
}