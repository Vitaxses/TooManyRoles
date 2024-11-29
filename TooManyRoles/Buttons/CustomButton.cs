using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace TooManyRoles.Buttons;

public class CustomButton {

    public static List<CustomButton> allButtons = new List<CustomButton>();

    public ActionButton actionButton;
    public GameObject actionButtonGameObject;
    public SpriteRenderer actionButtonRenderer;
    public Material actionButtonMat;
    public TextMeshPro actionButtonLabelText;
    public Vector3 PositionOffset;
    public float MaxTimer = float.MaxValue;
    public float Timer = 0f;
    private Action Click;
    private Action OnMeetingEnds;
    private Action OnMeetingStarts;
    public Func<bool> IsVisible;
    public Func<bool> CanUse;
    private Action OnEffectEnds;
    public bool HasEffect;
    public Color? Effectcolor;
    public bool EffectActive = false;
    public bool ButtonTextVisible = false;
    public float EffectDuration;
    public Sprite Sprite;
    public HudManager hudManager;
    public KeyCode keyCode;
    public string buttonText;

    public static class buttonPos {
        public static readonly Vector3 lowerRowRight = new Vector3(-2f, -0.06f, 0);
        public static readonly Vector3 lowerRowCenter = new Vector3(-3f, -0.06f, 0);
        public static readonly Vector3 lowerRowLeft = new Vector3(-4f, -0.06f, 0);
        public static readonly Vector3 upperRowRight = new Vector3(0f, 1f, 0f);  
        public static readonly Vector3 upperRowCenter = new Vector3(-1f, 1f, 0f);  
        public static readonly Vector3 upperRowLeft = new Vector3(-2f, 1f, 0f);
        public static readonly Vector3 upperRowFarLeft = new Vector3(-3f, 1f, 0f);
    }

    public static class EffectColor {
        public static readonly Color32 red = new Color32(255, 0, 0, 255);
        public static readonly Color LightRed = new Color(0.8f, 0f, 0f);
        public static readonly Color32 green = new Color32(0, 255, 0, 255);
        public static readonly Color LightGreen = new Color(0f, 0.8f, 0f);
        public static readonly Color32 blue = new Color32(0, 0, 255, 255);
        public static readonly Color LightBlue = new Color(0f, 0f, 0.8f);
    }

    public CustomButton(Action Click, Func<bool> IsVisible, Func<bool> CanUse, Action OnMeetingStarts, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager, KeyCode keyCode, bool HasEffect = false, Color? Effectcolor = null, float EffectDuration = 0f, Action OnEffectEnds = null, string buttonText = "", float MaxTimer = 16f) {
        this.Click = Click;
        this.IsVisible = IsVisible;
        this.CanUse = CanUse;
        this.OnMeetingStarts = OnMeetingStarts;
        this.OnMeetingEnds = OnMeetingEnds;
        this.Sprite = Sprite;
        this.PositionOffset = PositionOffset;
        this.hudManager = hudManager;
        this.keyCode = keyCode;
        this.HasEffect = HasEffect;
        this.EffectDuration = EffectDuration;
        OnEffectEnds ??= () => {};
        this.OnEffectEnds = OnEffectEnds;
        this.Effectcolor = Effectcolor;
        this.buttonText = buttonText;
        this.MaxTimer = MaxTimer;

        actionButton = UnityEngine.Object.Instantiate(hudManager.KillButton, hudManager.KillButton.transform.parent);
        actionButtonGameObject = actionButton.gameObject;
        actionButtonRenderer = actionButton.graphic;
        actionButtonMat = actionButtonRenderer.material;
        actionButtonLabelText = actionButton.buttonLabelText;
        PassiveButton button = actionButton.GetComponent<PassiveButton>();
        ButtonTextVisible = actionButtonRenderer.sprite == Sprite || buttonText != "";
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener((UnityEngine.Events.UnityAction)onClick);

        allButtons.Add(this);
    }
    

    public CustomButton(Action Click, Func<bool> IsVisible, Func<bool> CanUse, Action OnMeetingStarts, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager, KeyCode keyCode, bool HasEffect, Color Effectcolor, float EffectDuration, Action OnEffectEnds, string buttonText = "", float MaxTimer = 16f) {
        this.Click = Click;
        this.IsVisible = IsVisible;
        this.CanUse = CanUse;
        this.OnMeetingStarts = OnMeetingStarts;
        this.OnMeetingEnds = OnMeetingEnds;
        this.Sprite = Sprite;
        this.PositionOffset = PositionOffset;
        this.hudManager = hudManager;
        this.keyCode = keyCode;
        this.HasEffect = HasEffect;
        this.EffectDuration = EffectDuration;
        this.OnEffectEnds = OnEffectEnds;
        this.Effectcolor = Effectcolor;
        this.buttonText = buttonText;
        this.MaxTimer = MaxTimer;

        actionButton = UnityEngine.Object.Instantiate(hudManager.KillButton, hudManager.KillButton.transform.parent);
        actionButtonGameObject = actionButton.gameObject;
        actionButtonRenderer = actionButton.graphic;
        actionButtonMat = actionButtonRenderer.material;
        actionButtonLabelText = actionButton.buttonLabelText;
        PassiveButton button = actionButton.GetComponent<PassiveButton>();
        ButtonTextVisible = actionButtonRenderer.sprite == Sprite || buttonText != "";
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener((UnityEngine.Events.UnityAction)onClick);

        allButtons.Add(this);
    }

    public void onClick() {
        if (Timer < 0f && IsVisible() && CanUse()) {
            actionButtonRenderer.color = new Color(1f, 1f, 1f, 0.3f);
            Click();

            if (HasEffect && !EffectActive) {
                Timer = EffectDuration;
                if (Effectcolor.HasValue) actionButton.cooldownTimerText.color = Effectcolor.Value;
                else actionButton.cooldownTimerText.color = EffectColor.LightBlue;
                EffectActive = true;
            } else Timer = MaxTimer;
        }
    }

    public void Update() {
        if (hudManager || MeetingHud.Instance || ExileController.Instance || !IsVisible()) {
            setActive(false);
            return;
        }

        if (hudManager.UseButton != null) {
            actionButton.transform.localPosition = hudManager.UseButton.transform.localPosition + PositionOffset;
        }

        if (Timer >= 0) {
            if (HasEffect && EffectActive) Timer -= Time.deltaTime; 
            else if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.CanMove) Timer -= Time.deltaTime;
        }

        setActive(hudManager.UseButton.isActiveAndEnabled || hudManager.PetButton.isActiveAndEnabled);
        actionButtonRenderer.sprite = Sprite;

        actionButton.SetCoolDown(Timer, (HasEffect && EffectActive) ? EffectDuration : MaxTimer);

        if (Input.GetKeyDown(keyCode)) onClick();

        if (HasEffect && EffectActive && Timer <= 0) {
            EffectActive = false;
            OnEffectEnds();
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
        }

        actionButtonLabelText.enabled = ButtonTextVisible;
        if (buttonText != "" && ButtonTextVisible) {
            actionButton.OverrideText(buttonText);
        }
        
        if (CanUse()) {
            actionButtonRenderer.color = actionButtonLabelText.color = Palette.EnabledColor;
            actionButtonMat.SetFloat(Shader.PropertyToID("_Desat"), 0f);
        } else {
            actionButtonRenderer.color = actionButtonLabelText.color = Palette.DisabledClear;
            actionButtonMat.SetFloat(Shader.PropertyToID("_Desat"), 1f);
        }
    }


    public static void HudUpdate() {
        allButtons.RemoveAll(item => item.actionButton == null);
        
        for (int i = 0; i < allButtons.Count; i++)
        {
            try
            {
                allButtons[i].Update();
            }
            catch (NullReferenceException)
            {
                System.Console.WriteLine("[WARNING] NullReferenceException from HudUpdate().HasButton(), if theres only one warning its fine");
            }
        }
    }

        public static void MeetingEndedUpdate() {
            allButtons.RemoveAll(item => item.actionButton == null);
            for (int i = 0; i < allButtons.Count; i++)
            {
                try
                {
                    allButtons[i].OnMeetingEnds();
                    allButtons[i].Update();
                }
                catch (NullReferenceException)
                {
                    System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
                }
            }
        }

        public static void MeetingStartedUpdate() {
            allButtons.RemoveAll(item => item.actionButton == null);
            for (int i = 0; i < allButtons.Count; i++)
            {
                try
                {
                    allButtons[i].OnMeetingStarts();
                    allButtons[i].Update();
                }
                catch (NullReferenceException)
                {
                    System.Console.WriteLine("[WARNING] NullReferenceException from MeetingStartedUpdate().HasButton(), if theres only one warning its fine");
                }
            }
        }

        public static void ResetAllCooldowns() {
            for (int i = 0; i < allButtons.Count; i++)
            {
                try
                {
                    allButtons[i].Timer = allButtons[i].MaxTimer;
                    allButtons[i].Update();
                }
                catch (NullReferenceException)
                {
                    System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
                }
            }
        }

        public void setActive(bool isActive) {
            if (isActive) {
                actionButtonGameObject.SetActive(true);
                actionButtonRenderer.enabled = true;
            } else {
                actionButtonGameObject.SetActive(false);
                actionButtonRenderer.enabled = false;
            }
        }

}