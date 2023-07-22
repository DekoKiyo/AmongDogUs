namespace AmongDogUs.Modules;

internal enum ButtonPositions
{
    ZoomIn, ZoomOut,
    LeftTop, CenterTop, RightTop,
    LeftBottom, CenterBottom, RightBottom
}

internal class CustomButton
{
    internal static List<CustomButton> buttons = new();
    internal ActionButton actionButton;
    internal ButtonPositions ButtonPosition;
    internal Vector3 LocalScale = Vector3.one;
    internal float MaxTimer = float.MaxValue;
    internal float Timer = 0f;
    internal bool EffectCancellable = false;
    private readonly Action OnClick;
    private readonly Action OnMeetingEnds;
    private readonly Func<bool> HasButton;
    private readonly Func<bool> CouldUse;
    internal Action OnEffectEnds;
    internal bool HasEffect;
    internal bool IsEffectActive = false;
    internal bool ShowButtonText = true;
    internal string ButtonText = null;
    internal float EffectDuration;
    internal Sprite Sprite;
    private readonly HudManager hudManager;
    private readonly bool mirror;
    private readonly KeyCode? hotkey;

    internal CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, ButtonPositions ButtonPosition, HudManager hudManager, ActionButton textTemplate, KeyCode? hotkey, bool HasEffect, float EffectDuration, Action OnEffectEnds, bool mirror = false, string ButtonText = null)
    {
        this.hudManager = hudManager;
        this.OnClick = OnClick;
        this.HasButton = HasButton;
        this.CouldUse = CouldUse;
        this.ButtonPosition = ButtonPosition;
        this.OnMeetingEnds = OnMeetingEnds;
        this.HasEffect = HasEffect;
        this.EffectDuration = EffectDuration;
        this.OnEffectEnds = OnEffectEnds;
        this.Sprite = Sprite;
        this.mirror = mirror;
        this.hotkey = hotkey;
        this.ButtonText = ButtonText;
        Timer = 16.2f;
        buttons.Add(this);
        actionButton = Object.Instantiate(hudManager.KillButton, hudManager.KillButton.transform.parent);
        PassiveButton button = actionButton.GetComponent<PassiveButton>();
        button.OnClick = new ButtonClickedEvent();
        button.OnClick.AddListener((UnityAction)OnClickEvent);

        LocalScale = actionButton.transform.localScale;
        if (textTemplate)
        {
            Object.Destroy(actionButton.buttonLabelText);
            actionButton.buttonLabelText = Object.Instantiate(textTemplate.buttonLabelText, actionButton.transform);
        }

        SetActive(false);
    }

    internal CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, ButtonPositions ButtonPosition, HudManager hudManager, ActionButton textTemplate, KeyCode? hotkey, bool mirror = false, string ButtonText = null)
    : this(OnClick, HasButton, CouldUse, OnMeetingEnds, Sprite, ButtonPosition, hudManager, textTemplate, hotkey, false, 0f, () => { }, mirror, ButtonText) { }

    internal void OnClickEvent()
    {
        if ((Timer < 0f && HasButton() && CouldUse()) || (HasEffect && IsEffectActive && EffectCancellable))
        {
            actionButton.graphic.color = new Color(1f, 1f, 1f, 0.3f);
            OnClick();

            if (HasEffect && !IsEffectActive)
            {
                Timer = EffectDuration;
                actionButton.cooldownTimerText.color = new(0F, 0.8F, 0F);
                IsEffectActive = true;
            }
        }
    }

    internal static void HudUpdate()
    {
        buttons.RemoveAll(item => item.actionButton == null);

        for (int i = 0; i < buttons.Count; i++)
        {
            try
            {
                buttons[i].Update();
            }
            catch (NullReferenceException)
            {
                System.Console.WriteLine("[WARNING] NullReferenceException from HudUpdate().HasButton(), if theres only one warning its fine");
            }
        }
    }

    internal static void MeetingEndedUpdate()
    {
        buttons.RemoveAll(item => item.actionButton == null);
        for (int i = 0; i < buttons.Count; i++)
        {
            try
            {
                buttons[i].OnMeetingEnds();
                buttons[i].Update();
            }
            catch (NullReferenceException)
            {
                System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
            }
        }
    }

    internal static void ResetAllCooldowns()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            try
            {
                buttons[i].Timer = buttons[i].MaxTimer;
                buttons[i].Update();
            }
            catch (NullReferenceException)
            {
                System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
            }
        }
    }

    internal void SetActive(bool isActive)
    {
        if (isActive)
        {
            actionButton.gameObject.SetActive(true);
            actionButton.graphic.enabled = true;
        }
        else
        {
            actionButton.gameObject.SetActive(false);
            actionButton.graphic.enabled = false;
        }
    }

    private void Update()
    {
        if (PlayerControl.LocalPlayer.Data == null || MeetingHud.Instance || ExileController.Instance || !HasButton())
        {
            SetActive(false);
            return;
        }
        SetActive(hudManager.UseButton.isActiveAndEnabled || hudManager.PetButton.isActiveAndEnabled);

        actionButton.graphic.sprite = Sprite;
        if (ShowButtonText && ButtonText != null)
        {
            actionButton.OverrideText(ButtonText);
        }
        actionButton.buttonLabelText.enabled = ShowButtonText; // Only show the text if it's a kill button

        if (hudManager.UseButton != null)
        {
            Vector3 PositionOffset = new(0f, 0f, 0f);
            Vector3 pos = hudManager.UseButton.transform.localPosition;
            if (mirror) pos = new(-pos.x, pos.y, pos.z);

            switch (ButtonPosition)
            {
                case ButtonPositions.ZoomIn:
                    PositionOffset = Vector3.zero + Vector3.up * 3.75f + Vector3.right * 0.2f;
                    break;
                case ButtonPositions.ZoomOut:
                    PositionOffset = Vector3.zero + Vector3.up * 3.75f + Vector3.right * 0.55f;
                    break;
                case ButtonPositions.LeftTop: // Kill Button
                    PositionOffset = new(-2f, 1f, 0f);
                    break;
                case ButtonPositions.CenterTop: // Sabotage Button
                    PositionOffset = new(-1f, 1f, 0f);
                    break;
                case ButtonPositions.RightTop: // Ability Button
                    PositionOffset = new(0f, 1f, 0f);
                    break;
                case ButtonPositions.LeftBottom: // Vent Button
                    PositionOffset = new(-2f, -0.06f, 0f);
                    break;
                case ButtonPositions.CenterBottom: // Report Button
                    PositionOffset = new(-1f, -0.06f, 0f);
                    break;
                case ButtonPositions.RightBottom: // Use/Pet Button
                    PositionOffset = new(0f, -0.06f, 0f);
                    break;
            }

            actionButton.transform.localPosition = pos + PositionOffset;
            actionButton.transform.localScale = LocalScale;
        }
        if (CouldUse())
        {
            actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.EnabledColor;
            actionButton.graphic.material.SetFloat("_Desat", 0f);
        }
        else
        {
            actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.DisabledClear;
            actionButton.graphic.material.SetFloat("_Desat", 1f);
        }

        if (Timer >= 0)
        {
            if (HasEffect && IsEffectActive) Timer -= Time.deltaTime;
            else if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable) Timer -= Time.deltaTime;
        }

        if (Timer <= 0 && HasEffect && IsEffectActive)
        {
            IsEffectActive = false;
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
            OnEffectEnds();
        }

        actionButton.SetCoolDown(Timer, (HasEffect && IsEffectActive) ? EffectDuration : MaxTimer);

        // Trigger OnClickEvent if the hotkey is being pressed down
        if (hotkey.HasValue && Input.GetKeyDown(hotkey.Value)) OnClickEvent();
    }

    internal void Destroy()
    {
        SetActive(false);
        Object.Destroy(actionButton);
        actionButton = null;
    }
}