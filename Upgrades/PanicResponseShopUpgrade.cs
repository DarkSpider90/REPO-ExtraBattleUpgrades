using BepInEx.Configuration;
using System;
using UnityEngine;

namespace ExtraBattleUpgrades.Upgrades;

internal sealed class PanicResponseShopUpgrade : ShopUpgrade
{
    internal const string StatsDictionaryKey = "playerUpgradePanicResponse";

    protected override string UpgradeId => "PanicResponse";

    private readonly ConfigEntry<float> _durationLevel1;
    private readonly ConfigEntry<float> _durationLevel2To3;
    private readonly ConfigEntry<float> _durationLevel4Plus;

    private readonly ConfigEntry<float> _cooldownBase;
    private readonly ConfigEntry<float> _cooldownReductionStartLevel;
    private readonly ConfigEntry<float> _cooldownReductionPerLevel;
    private readonly ConfigEntry<float> _cooldownMin;

    private readonly ConfigEntry<float> _speedMultiplierLevel1To2;
    private readonly ConfigEntry<float> _speedMultiplierLevel3Plus;

    internal PanicResponseShopUpgrade(ConfigFile config, AssetBundle bundle)
        : base(
            config,
            bundle,
            "Panic Response",
            "assets/extrabattleupgrades/items/item upgrade player panic response.prefab",
            1.0f)
    {
        _durationLevel1 = config.Bind(
            "Panic Response Upgrade",
            "Duration Level 1",
            5f,
            "Panic Response duration at level 1, in seconds.");

        _durationLevel2To3 = config.Bind(
            "Panic Response Upgrade",
            "Duration Level 2 To 3",
            6f,
            "Panic Response duration from level 2 to level 3, in seconds.");

        _durationLevel4Plus = config.Bind(
            "Panic Response Upgrade",
            "Duration Level 4 Plus",
            7f,
            "Panic Response duration from level 4 and above, in seconds.");

        _cooldownBase = config.Bind(
            "Panic Response Upgrade",
            "Cooldown Base",
            60f,
            "Base Panic Response cooldown, in seconds.");

        _cooldownReductionStartLevel = config.Bind(
            "Panic Response Upgrade",
            "Cooldown Reduction Start Level",
            5f,
            "Level where cooldown reduction starts. Default 5 means level 5+ starts reducing cooldown.");

        _cooldownReductionPerLevel = config.Bind(
            "Panic Response Upgrade",
            "Cooldown Reduction Per Level",
            5f,
            "Cooldown reduction per level after the reduction start level, in seconds.");

        _cooldownMin = config.Bind(
            "Panic Response Upgrade",
            "Cooldown Minimum",
            30f,
            "Minimum Panic Response cooldown, in seconds.");

        _speedMultiplierLevel1To2 = config.Bind(
            "Panic Response Upgrade",
            "Speed Multiplier Level 1 To 2",
            1.2f,
            "Speed multiplier from level 1 to level 2. 1.2 = 20% faster.");

        _speedMultiplierLevel3Plus = config.Bind(
            "Panic Response Upgrade",
            "Speed Multiplier Level 3 Plus",
            1.35f,
            "Speed multiplier from level 3 and above. 1.35 = 35% faster.");
    }

    internal float DurationSeconds(PlayerAvatar player)
    {
        int level = Math.Min(GetLevel(player), 10);

        if (level <= 0)
        {
            return 0f;
        }

        if (level == 1)
        {
            return Math.Max(0f, _durationLevel1.Value);
        }

        if (level <= 3)
        {
            return Math.Max(0f, _durationLevel2To3.Value);
        }

        return Math.Max(0f, _durationLevel4Plus.Value);
    }

    internal float CooldownSeconds(PlayerAvatar player)
    {
        int level = Math.Min(GetLevel(player), 10);

        if (level <= 0)
        {
            return 9999f;
        }

        float baseCooldown = Math.Max(0f, _cooldownBase.Value);
        float minCooldown = Math.Max(0f, _cooldownMin.Value);
        int reductionStartLevel = Math.Max(1, Mathf.RoundToInt(_cooldownReductionStartLevel.Value));

        if (level < reductionStartLevel)
        {
            return baseCooldown;
        }

        int reductionLevels = level - reductionStartLevel + 1;
        float reduction = reductionLevels * Math.Max(0f, _cooldownReductionPerLevel.Value);

        return Math.Max(minCooldown, baseCooldown - reduction);
    }

    internal float SpeedMultiplier(PlayerAvatar player)
    {
        int level = Math.Min(GetLevel(player), 10);

        if (level <= 0)
        {
            return 1f;
        }

        if (level <= 2)
        {
            return Math.Max(1f, _speedMultiplierLevel1To2.Value);
        }

        return Math.Max(1f, _speedMultiplierLevel3Plus.Value);
    }
}