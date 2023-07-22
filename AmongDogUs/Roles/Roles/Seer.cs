namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Seer : RoleBase<Seer>
{
    public Seer()
    {
        RoleType = roleType = RoleType.Seer;
    }

    private static Sprite SoulSprite;
    internal static List<Vector3> DeadBodyPositions = new();
    internal static float SoulDuration { get { return CustomOptionsHolder.SeerSoulDuration.GetFloat(); } }
    internal static bool LimitSoulDuration { get { return CustomOptionsHolder.SeerLimitSoulDuration.GetBool(); } }
    internal static int Mode { get { return CustomOptionsHolder.SeerMode.GetSelection(); } }

    internal static Sprite GetSoulSprite()
    {
        if (SoulSprite) return SoulSprite;
        SoulSprite = Helpers.LoadSpriteFromTexture2D(ModAssets.Soul, 500f);
        return SoulSprite;
    }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd()
    {
        if (DeadBodyPositions != null && PlayerControl.LocalPlayer.IsRole(RoleType.Seer) && (Mode == 0 || Mode == 2))
        {
            foreach (Vector3 pos in DeadBodyPositions)
            {
                GameObject soul = new();
                // soul.transform.position = pos;
                soul.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000 - 1f);
                soul.layer = 5;
                var rend = soul.AddComponent<SpriteRenderer>();
                rend.sprite = GetSoulSprite();

                if (LimitSoulDuration)
                {
                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(SoulDuration, new Action<float>((p) =>
                    {
                        if (rend != null)
                        {
                            var tmp = rend.color;
                            tmp.a = Mathf.Clamp01(1 - p);
                            rend.color = tmp;
                        }
                        if (p == 1f && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                    })));
                }
            }
            DeadBodyPositions = new List<Vector3>();
        }
    }
    internal override void FixedUpdate() { }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm) { }
    internal override void SetButtonCooldowns() { }

    internal override void Clear()
    {
        players = new List<Seer>();
    }
}