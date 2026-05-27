using System.Collections.Generic;
using ExtraBattleUpgrades.Upgrades;
using HarmonyLib;
using UnityEngine;

namespace ExtraBattleUpgrades.Patches;

[HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.Hurt))]
internal static class PanicResponsePatch
{
    private static readonly Dictionary<string, float> BuffEnds = new Dictionary<string, float>();
    private static readonly Dictionary<string, float> CooldownEnds = new Dictionary<string, float>();

    private readonly struct HurtState
    {
        internal readonly int HealthBefore;

        internal HurtState(int healthBefore)
        {
            HealthBefore = healthBefore;
        }
    }

    private static void Prefix(int ___health, out HurtState __state)
    {
        __state = new HurtState(___health);
    }

    private static void Postfix(PlayerAvatar ___playerAvatar, int ___health, HurtState __state)
    {
        if (!CanActivate(___playerAvatar, __state.HealthBefore, ___health))
        {
            return;
        }

        PanicResponseShopUpgrade upgrade = ExtraBattleUpgradesPlugin.PanicResponse;
        if (upgrade == null)
        {
            return;
        }

        string playerId = PlayerId(___playerAvatar);
        float now = Time.time;

        if (CooldownEnds.TryGetValue(playerId, out float cooldownEnd) && cooldownEnd > now)
        {
            return;
        }

        float duration = upgrade.DurationSeconds(___playerAvatar);
        float cooldown = upgrade.CooldownSeconds(___playerAvatar);
        float speedMultiplier = upgrade.SpeedMultiplier(___playerAvatar);

        if (duration <= 0f)
        {
            return;
        }

        BuffEnds[playerId] = now + duration;
        CooldownEnds[playerId] = now + cooldown;

        // PlayerController already has a vanilla temporary speed override system.
        PlayerController.instance.OverrideSpeed(speedMultiplier, duration);
    }

    internal static bool IsActive(PlayerAvatar player)
    {
        if (player == null)
        {
            return false;
        }

        return BuffEnds.TryGetValue(PlayerId(player), out float buffEnd) && buffEnd > Time.time;
    }

    private static bool CanActivate(PlayerAvatar player, int healthBefore, int healthAfter)
    {
        if (player == null || healthBefore <= 0)
        {
            return false;
        }

        if (healthAfter >= healthBefore)
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

        PanicResponseShopUpgrade upgrade = ExtraBattleUpgradesPlugin.PanicResponse;
        return upgrade != null
            && upgrade.Enabled.Value
            && upgrade.RegisteredUpgrade != null
            && upgrade.GetLevel(player) > 0;
    }

    private static string PlayerId(PlayerAvatar player)
    {
        string playerId = SemiFunc.PlayerGetSteamID(player);
        return string.IsNullOrWhiteSpace(playerId) ? player.GetInstanceID().ToString() : playerId;
    }
    
    internal static float RemainingCooldownSeconds(PlayerAvatar player)
    {
        if (player == null)
        {
            return 0f;
        }

        string playerId = PlayerId(player);
        return CooldownEnds.TryGetValue(playerId, out float cooldownEnd)
            ? Mathf.Max(0f, cooldownEnd - Time.time)
            : 0f;
    }

    internal static bool IsReady(PlayerAvatar player)
    {
        PanicResponseShopUpgrade upgrade = ExtraBattleUpgradesPlugin.PanicResponse;

        return player != null
               && upgrade != null
               && upgrade.Enabled.Value
               && upgrade.RegisteredUpgrade != null
               && upgrade.GetLevel(player) > 0
               && RemainingCooldownSeconds(player) <= 0f;
    }

    internal static bool HasUpgrade(PlayerAvatar player)
    {
        PanicResponseShopUpgrade upgrade = ExtraBattleUpgradesPlugin.PanicResponse;

        return player != null
               && upgrade != null
               && upgrade.Enabled.Value
               && upgrade.RegisteredUpgrade != null
               && upgrade.GetLevel(player) > 0;
    }
    
    internal static void ResetState()
    {
        BuffEnds.Clear();
        CooldownEnds.Clear();
    }
    
}

[HarmonyPatch(typeof(PlayerController), "FixedUpdate")]
internal static class PanicResponseEnergyPatch
{
    private static void Prefix(PlayerAvatar ___playerAvatarScript)
    {
        if (___playerAvatarScript == null || !PanicResponsePatch.IsActive(___playerAvatarScript))
        {
            return;
        }

        // Infinite stamina while Panic Response is active.
        PlayerController controller = PlayerController.instance;
        if (controller == null)
        {
            return;
        }

        controller.EnergyCurrent = controller.EnergyStart;
    }
}