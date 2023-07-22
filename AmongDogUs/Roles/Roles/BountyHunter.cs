namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class BountyHunter : RoleBase<BountyHunter>
{
    public BountyHunter()
    {
        RoleType = roleType = RoleType.BountyHunter;
    }

    internal static CustomArrow Arrow;
    internal static PlayerControl Bounty;
    internal static TMP_Text CooldownTimer;
    internal static Vector3 BountyPos;

    internal static float SuccessCooldown { get { return CustomOptionsHolder.BountyHunterSuccessKillCooldown.GetFloat(); } }
    internal static float AdditionalCooldown { get { return CustomOptionsHolder.BountyHunterAdditionalKillCooldown.GetFloat(); } }
    internal static float Duration { get { return CustomOptionsHolder.BountyHunterDuration.GetFloat(); } }
    internal static bool ShowArrow { get { return CustomOptionsHolder.BountyHunterShowArrow.GetBool(); } }
    internal static float ArrowUpdate { get { return CustomOptionsHolder.BountyHunterArrowUpdateCooldown.GetFloat(); } }

    internal static float KillCooldowns = 30f;
    internal static float ArrowUpdateTimer = 0f;
    internal static float BountyUpdateTimer = 0f;

    internal override void OnMeetingStart()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleType.BountyHunter)) KillCooldowns = player.killTimer;
    }
    internal override void OnMeetingEnd()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleType.BountyHunter)) player.SetKillTimerUnchecked(KillCooldowns);
        if (Exists) BountyUpdateTimer = 0f;
    }
    internal override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleType.BountyHunter))
        {
            if (player.Data.IsDead)
            {
                if (Arrow != null || Arrow.arrow != null) Object.Destroy(Arrow.arrow);
                Arrow = null;
                if (CooldownTimer != null && CooldownTimer.gameObject != null) Object.Destroy(CooldownTimer.gameObject);
                CooldownTimer = null;
                Bounty = null;
                foreach (PoolablePlayer p in ModMapOptions.PlayerIcons.Values) if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
                return;
            }

            ArrowUpdateTimer -= Time.fixedDeltaTime;
            BountyUpdateTimer -= Time.fixedDeltaTime;

            if (Bounty == null || BountyUpdateTimer <= 0f)
            {
                // Set new bounty
                ArrowUpdateTimer = 0f; // Force arrow to update
                BountyUpdateTimer = Duration;
                var BountyList = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) if (!p.Data.IsDead && !p.Data.Disconnected && p != p.Data.Role.IsImpostor) BountyList.Add(p);

                Bounty = BountyList[Main.Random.Next(0, BountyList.Count)];
                if (Bounty == null) return;

                // Show poolable player
                if (FastDestroyableSingleton<HudManager>.Instance != null && FastDestroyableSingleton<HudManager>.Instance.UseButton != null)
                    foreach (PoolablePlayer pp in ModMapOptions.PlayerIcons.Values) pp.gameObject.SetActive(false);
                {
                    if (ModMapOptions.PlayerIcons.ContainsKey(Bounty.PlayerId) && ModMapOptions.PlayerIcons[Bounty.PlayerId].gameObject != null)
                        ModMapOptions.PlayerIcons[Bounty.PlayerId].gameObject.SetActive(true);
                }
            }

            // Update Cooldown Text
            if (CooldownTimer != null) CooldownTimer.text = Mathf.CeilToInt(Mathf.Clamp(BountyUpdateTimer, 0, Duration)).ToString();

            // Update Arrow
            if (ShowArrow && Bounty != null)
            {
                Arrow ??= new CustomArrow(ImpostorRed);
                if (ArrowUpdateTimer <= 0f)
                {
                    BountyPos = Bounty.transform.position;
                    Arrow.Update(BountyPos);
                    ArrowUpdateTimer = ArrowUpdate;
                }
                Arrow.Update(BountyPos);
            }
        }
    }
    internal override void OnKill(PlayerControl target)
    {
        if (target == Bounty)
        {
            Bounty = null;
            player.SetKillTimer(SuccessCooldown);
            BountyUpdateTimer = 0f; // Force bounty update
        }
        else player.SetKillTimer(GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown) + AdditionalCooldown);
    }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm) { }
    internal override void SetButtonCooldowns() { }

    internal override void Clear()
    {
        players = new List<BountyHunter>();

        BountyPos = new();
        ArrowUpdateTimer = 0f;
        BountyUpdateTimer = 0f;
        Arrow = new(ImpostorRed);
        Bounty = null;
        if (Arrow != null && Arrow.arrow != null) Object.Destroy(Arrow.arrow);
        Arrow = null;
        if (CooldownTimer != null && CooldownTimer.gameObject != null) Object.Destroy(CooldownTimer.gameObject);
        CooldownTimer = null;
        foreach (PoolablePlayer p in ModMapOptions.PlayerIcons.Values) if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
        KillCooldowns = player.killTimer / 2;
    }
}