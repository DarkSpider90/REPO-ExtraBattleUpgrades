using ExtraBattleUpgrades.Upgrades;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace ExtraBattleUpgrades.Patches;

[HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.Hurt))]
internal static class SecondChancePatch
{
    private static readonly AccessTools.FieldRef<PlayerAvatar, PlayerTumble> PlayerTumbleRef =
        AccessTools.FieldRefAccess<PlayerAvatar, PlayerTumble>("tumble");

    private static readonly AccessTools.FieldRef<PlayerTumble, PhysGrabObject> TumbleBodyRef =
        AccessTools.FieldRefAccess<PlayerTumble, PhysGrabObject>("physGrabObject");

    private static readonly AccessTools.FieldRef<PlayerHealth, int> HealthRef =
        AccessTools.FieldRefAccess<PlayerHealth, int>("health");

    private static readonly AccessTools.FieldRef<PlayerHealth, int> MaxHealthRef =
        AccessTools.FieldRefAccess<PlayerHealth, int>("maxHealth");

    private static readonly System.Collections.Generic.Dictionary<int, float> PitRescueTimes =
        new System.Collections.Generic.Dictionary<int, float>();

    private readonly struct RescueState
    {
        internal readonly bool Activated;
        internal readonly float InvulnerabilitySeconds;

        internal RescueState(bool activated, float invulnerabilitySeconds)
        {
            Activated = activated;
            InvulnerabilitySeconds = invulnerabilitySeconds;
        }
    }

    [HarmonyPriority(Priority.Last)]
    private static void Prefix(
        ref int damage,
        bool savingGrace,
        float ___invincibleTimer,
        PlayerAvatar ___playerAvatar,
        int ___health,
        out RescueState __state)
    {
        __state = new RescueState(false, 0f);

        if (!CanTryRescue(damage, savingGrace, ___invincibleTimer, ___playerAvatar, ___health))
        {
            return;
        }

        SecondChanceShopUpgrade upgrade = ExtraBattleUpgradesPlugin.SecondChance;
        if (upgrade == null || !upgrade.TryActivate(___playerAvatar, out float invulnerabilitySeconds))
        {
            return;
        }

        damage = Mathf.Max(0, ___health - 1);
        __state = new RescueState(true, invulnerabilitySeconds);
    }

    private static void Postfix(PlayerHealth __instance, PlayerAvatar ___playerAvatar, ref int ___health, RescueState __state)
    {
        if (!__state.Activated || ___playerAvatar == null)
        {
            return;
        }

        if (___health <= 0)
        {
            ___health = 1;
        }

        ApplyProtectionFeel(___playerAvatar, __state.InvulnerabilitySeconds, launchUp: true);
    }

    private static bool CanTryRescue(int damage, bool savingGrace, float invincibleTimer, PlayerAvatar player, int health)
    {
        if (damage <= 0 || player == null || health <= 0 || invincibleTimer > 0f)
        {
            return false;
        }

        if (GameManager.Multiplayer() && (player.photonView == null || !player.photonView.IsMine))
        {
            return false;
        }

        if (GameDirector.instance == null || GameDirector.instance.currentState != GameDirector.gameState.Main)
        {
            return false;
        }

        if (savingGrace && damage <= 25 && health > 5 && health <= 20)
        {
            return false;
        }

        if (health - damage > 0)
        {
            return false;
        }

        SecondChanceShopUpgrade upgrade = ExtraBattleUpgradesPlugin.SecondChance;
        return upgrade != null
            && upgrade.Enabled.Value
            && upgrade.RegisteredUpgrade != null;
    }

    internal static bool TryRescueFromPit(PlayerAvatar player)
    {
        if (!CanTryPitRescue(player))
        {
            return false;
        }

        SecondChanceShopUpgrade upgrade = ExtraBattleUpgradesPlugin.SecondChance;
        if (upgrade == null || !upgrade.TryRescueFromPit(player, out float invulnerabilitySeconds, out bool activatedNow))
        {
            return false;
        }

        ForceHealthToOne(player);
        ApplyProtectionFeel(player, invulnerabilitySeconds, launchUp: true);
        LaunchFromPit(player, activatedNow);
        return true;
    }

    private static bool CanTryPitRescue(PlayerAvatar player)
    {
        if (player == null || player.playerHealth == null || GameDirector.instance == null)
        {
            return false;
        }

        if (GameDirector.instance.currentState != GameDirector.gameState.Main)
        {
            return false;
        }

        if (GameManager.Multiplayer() && (player.photonView == null || !player.photonView.IsMine))
        {
            return false;
        }

        SecondChanceShopUpgrade upgrade = ExtraBattleUpgradesPlugin.SecondChance;
        return upgrade != null
            && upgrade.Enabled.Value
            && upgrade.RegisteredUpgrade != null;
    }

    private static void ForceHealthToOne(PlayerAvatar player)
    {
        PlayerHealth health = player.playerHealth;
        if (HealthRef(health) != 1)
        {
            HealthRef(health) = 1;
            StatsManager.instance?.SetPlayerHealth(SemiFunc.PlayerGetSteamID(player), 1, setInShop: false);
        }

        if (GameManager.Multiplayer() && player.photonView != null)
        {
            player.photonView.RPC("UpdateHealthRPC", RpcTarget.Others, 1, MaxHealthRef(health), true, false);
        }
    }

    private static void ApplyProtectionFeel(PlayerAvatar player, float invulnerabilitySeconds, bool launchUp)
    {
        PlayerHealth health = player.playerHealth;
        health.InvincibleSet(invulnerabilitySeconds);
        health.SetMaterialSpecial(new Color(1f, 0.86f, 0.05f, 1f));
        health.EyeMaterialOverride(PlayerHealth.EyeOverrideState.CeilingEye, invulnerabilitySeconds, 50);

        PlayerTumble tumble = PlayerTumbleRef(player);
        if (tumble == null)
        {
            return;
        }

        tumble.TumbleRequest(true, false);
        tumble.TumbleOverrideTime(Mathf.Max(0.75f, invulnerabilitySeconds * 0.35f));
        if (launchUp)
        {
            tumble.TumbleForce(Vector3.up * 8f);
        }
    }

    private static void LaunchFromPit(PlayerAvatar player, bool activatedNow)
    {
        int playerId = player.GetInstanceID();
        float now = Time.time;
        if (PitRescueTimes.TryGetValue(playerId, out float nextRescueTime) && nextRescueTime > now)
        {
            return;
        }

        PitRescueTimes[playerId] = now + 0.25f;

        PlayerTumble tumble = PlayerTumbleRef(player);
        PhysGrabObject body = tumble != null ? TumbleBodyRef(tumble) : null;
        if (body == null || body.rb == null)
        {
            return;
        }

        body.DeathPitEffectCreate();
        Vector3 velocity = body.rb.velocity;
        if (velocity.y < 0f)
        {
            velocity.y = 0f;
            body.rb.velocity = velocity;
        }

        float verticalForce = activatedNow ? 18f : 14f;
        Vector3 sidewaysInstability = new Vector3(
            Random.Range(-2f, 2f),
            0f,
            Random.Range(-2f, 2f));

        body.rb.AddForce((Vector3.up * verticalForce + sidewaysInstability) * body.rb.mass, ForceMode.Impulse);
        body.rb.AddTorque(Random.insideUnitSphere * body.rb.mass, ForceMode.Impulse);
    }
}

[HarmonyPatch(typeof(HurtCollider), "PlayerHurt")]
internal static class SecondChanceDeathPitPatch
{
    private static bool Prefix(HurtCollider __instance, PlayerAvatar _player)
    {
        return !__instance.deathPit || !SecondChancePatch.TryRescueFromPit(_player);
    }
}

[HarmonyPatch(typeof(PlayerHealth), "Update")]
internal static class SecondChanceHealthColorPatch
{
    private static readonly int AlbedoColor = Shader.PropertyToID("_AlbedoColor");
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static readonly Color ProtectedHealthColor = new Color(1f, 0.86f, 0.05f, 1f);

    private static void Postfix(PlayerAvatar ___playerAvatar, Material ___healthMaterial)
    {
        SecondChanceShopUpgrade upgrade = ExtraBattleUpgradesPlugin.SecondChance;
        if (upgrade == null || ___playerAvatar == null || ___healthMaterial == null)
        {
            return;
        }

        if (upgrade.RemainingProtectionSeconds(___playerAvatar) <= 0f)
        {
            return;
        }

        ___healthMaterial.SetColor(AlbedoColor, ProtectedHealthColor);

        Color emission = ProtectedHealthColor;
        if (___healthMaterial.HasProperty(EmissionColor))
        {
            emission.a = ___healthMaterial.GetColor(EmissionColor).a;
        }

        ___healthMaterial.SetColor(EmissionColor, emission);
    }
}
