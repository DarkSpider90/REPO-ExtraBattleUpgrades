using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtraBattleUpgrades.Upgrades;

internal sealed class SecondChanceShopUpgrade : ShopUpgrade
{
    private readonly Dictionary<string, float> _cooldownEnds = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _protectionEnds = new Dictionary<string, float>();

    internal const string StatsDictionaryKey = "playerUpgradeSecondChance";

    protected override string UpgradeId => "SecondChance";

    private readonly ConfigEntry<float> _invulnerabilitySecondsPerLevel;
    private readonly ConfigEntry<int> _invulnerabilityMaxScalingLevel;

    private readonly ConfigEntry<float> _cooldownBase;
    private readonly ConfigEntry<int> _cooldownReductionStartLevel;
    private readonly ConfigEntry<int> _cooldownFastReductionMaxLevel;
    private readonly ConfigEntry<float> _cooldownReductionPerLevel;
    private readonly ConfigEntry<float> _cooldownReductionPerExtraLevel;
    private readonly ConfigEntry<float> _cooldownMin;

    internal SecondChanceShopUpgrade(ConfigFile config, AssetBundle bundle)
        : base(
            config,
            bundle,
            "Second Chance",
            "assets/extrabattleupgrades/items/item upgrade player second chance.prefab",
            1.0f)
    {
        _invulnerabilitySecondsPerLevel = config.Bind(
            "Second Chance Upgrade",
            "Invulnerability Seconds Per Level",
            1f,
            "Invulnerability duration gained per level before the cap. 1 = 1 second.");

        _invulnerabilityMaxScalingLevel = config.Bind(
            "Second Chance Upgrade",
            "Invulnerability Max Scaling Level",
            5,
            "Level where invulnerability duration stops increasing.");

        _cooldownBase = config.Bind(
            "Second Chance Upgrade",
            "Cooldown Base",
            120f,
            "Base Second Chance cooldown, in seconds.");

        _cooldownReductionStartLevel = config.Bind(
            "Second Chance Upgrade",
            "Cooldown Reduction Start Level",
            6,
            "Level where cooldown reduction starts.");

        _cooldownFastReductionMaxLevel = config.Bind(
            "Second Chance Upgrade",
            "Cooldown Fast Reduction Max Level",
            10,
            "Last level that uses the main cooldown reduction value.");

        _cooldownReductionPerLevel = config.Bind(
            "Second Chance Upgrade",
            "Cooldown Reduction Per Level",
            5f,
            "Cooldown reduction per level during the main reduction range, in seconds.");

        _cooldownReductionPerExtraLevel = config.Bind(
            "Second Chance Upgrade",
            "Cooldown Reduction Per Extra Level",
            0.5f,
            "Cooldown reduction per level after the fast reduction range, in seconds.");

        _cooldownMin = config.Bind(
            "Second Chance Upgrade",
            "Cooldown Minimum",
            30f,
            "Minimum Second Chance cooldown, in seconds.");
    }

    internal bool TryActivate(PlayerAvatar player, out float invulnerabilitySeconds)
    {
        invulnerabilitySeconds = 0f;
        int level = GetLevel(player);
        if (level <= 0)
        {
            return false;
        }

        string playerId = PlayerId(player);
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return false;
        }

        float now = Time.time;
        if (_cooldownEnds.TryGetValue(playerId, out float cooldownEnd) && cooldownEnd > now)
        {
            return false;
        }

        invulnerabilitySeconds = InvulnerabilitySeconds(level);
        _protectionEnds[playerId] = now + invulnerabilitySeconds;
        _cooldownEnds[playerId] = now + CooldownSeconds(level);
        return true;
    }

    internal bool TryRescueFromPit(PlayerAvatar player, out float invulnerabilitySeconds, out bool activatedNow)
    {
        activatedNow = false;
        invulnerabilitySeconds = RemainingProtectionSeconds(player);
        if (invulnerabilitySeconds > 0f)
        {
            return true;
        }

        activatedNow = TryActivate(player, out invulnerabilitySeconds);
        return activatedNow;
    }

    internal float RemainingProtectionSeconds(PlayerAvatar player)
    {
        string playerId = PlayerId(player);
        if (string.IsNullOrWhiteSpace(playerId) || !_protectionEnds.TryGetValue(playerId, out float protectionEnd))
        {
            return 0f;
        }

        return Math.Max(0f, protectionEnd - Time.time);
    }

    internal float RemainingCooldownSeconds(PlayerAvatar player)
    {
        string playerId = PlayerId(player);
        if (string.IsNullOrWhiteSpace(playerId) || !_cooldownEnds.TryGetValue(playerId, out float cooldownEnd))
        {
            return 0f;
        }

        return Math.Max(0f, cooldownEnd - Time.time);
    }

    internal bool IsReady(PlayerAvatar player)
    {
        return player != null
               && Enabled.Value
               && RegisteredUpgrade != null
               && GetLevel(player) > 0
               && RemainingCooldownSeconds(player) <= 0f;
    }

    internal bool HasUpgrade(PlayerAvatar player)
    {
        return player != null
               && Enabled.Value
               && RegisteredUpgrade != null
               && GetLevel(player) > 0;
    }

    internal void ResetState()
    {
        _cooldownEnds.Clear();
        _protectionEnds.Clear();
    }

    private float InvulnerabilitySeconds(int level)
    {
        int maxScalingLevel = Math.Max(1, _invulnerabilityMaxScalingLevel.Value);
        int effectiveLevel = Math.Min(level, maxScalingLevel);

        return Math.Max(0f, effectiveLevel * _invulnerabilitySecondsPerLevel.Value);
    }

    private float CooldownSeconds(int level)
    {
        float baseCooldown = Math.Max(0f, _cooldownBase.Value);
        float minCooldown = Math.Max(0f, _cooldownMin.Value);

        int startLevel = Math.Max(1, _cooldownReductionStartLevel.Value);
        int fastMaxLevel = Math.Max(startLevel, _cooldownFastReductionMaxLevel.Value);

        float reduction = 0f;

        if (level >= startLevel)
        {
            int fastLevels = Math.Min(level, fastMaxLevel) - startLevel + 1;
            reduction += Math.Max(0, fastLevels) * Math.Max(0f, _cooldownReductionPerLevel.Value);
        }

        if (level > fastMaxLevel)
        {
            int extraLevels = level - fastMaxLevel;
            reduction += extraLevels * Math.Max(0f, _cooldownReductionPerExtraLevel.Value);
        }

        return Math.Max(minCooldown, baseCooldown - reduction);
    }

    private static string PlayerId(PlayerAvatar player)
    {
        if (player == null)
        {
            return string.Empty;
        }

        string playerId = SemiFunc.PlayerGetSteamID(player);
        return string.IsNullOrWhiteSpace(playerId) ? player.GetInstanceID().ToString() : playerId;
    }
}