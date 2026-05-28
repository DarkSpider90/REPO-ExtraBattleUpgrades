using BepInEx.Configuration;
using UnityEngine;

namespace ExtraBattleUpgrades.Upgrades;

internal sealed class OverchargeShopUpgrade : ShopUpgrade
{
    internal const string StatsDictionaryKey = "playerUpgradeOvercharge";

    protected override string UpgradeId => "Overcharge";

    private readonly ConfigEntry<float> _bonusPerLevelFirstTen;
    private readonly ConfigEntry<float> _bonusPerLevelAfterTen;
    private readonly ConfigEntry<float> _holdStabilityEffectMultiplier;
    private readonly ConfigEntry<float> _recoveryBasePerSecond;

    internal OverchargeShopUpgrade(ConfigFile config, AssetBundle bundle)
        : base(
            config,
            bundle,
            "Overcharge",
            "assets/extrabattleupgrades/items/item upgrade player overcharge.prefab",
            0.75f)
    {
        _bonusPerLevelFirstTen = config.Bind(
            "Overcharge Upgrade",
            "Bonus Per Level First Ten",
            0.10f,
            "Bonus per Overcharge level from level 1 to 10. 0.10 = 10%.");

        _bonusPerLevelAfterTen = config.Bind(
            "Overcharge Upgrade",
            "Bonus Per Level After Ten",
            0.05f,
            "Bonus per Overcharge level after level 10. 0.05 = 5%.");

        _holdStabilityEffectMultiplier = config.Bind(
            "Overcharge Upgrade",
            "Hold Stability Effect Multiplier",
            0.25f,
            "How strongly Overcharge affects enemy hold stability. 0.25 = 25% of the Overcharge bonus.");

        _recoveryBasePerSecond = config.Bind(
            "Overcharge Upgrade",
            "Recovery Base Per Second",
            0.1f,
            "Base vanilla overcharge recovery per second used by this mod. Default vanilla-like value is 0.1.");
    }

    internal float SlowOvercharge(float value, PlayerAvatar player)
    {
        int level = GetLevel(player);
        if (value <= 0f || level <= 0)
        {
            return value;
        }

        return value / (1f + BonusMultiplier(player));
    }

    internal float BonusMultiplier(PlayerAvatar player)
    {
        return BonusTime(GetLevel(player));
    }

    internal float HoldStabilityBonus(PlayerAvatar player)
    {
        return BonusMultiplier(player) * _holdStabilityEffectMultiplier.Value;
    }

    internal float RecoveryBasePerSecond()
    {
        return Mathf.Max(0f, _recoveryBasePerSecond.Value);
    }

    private float BonusTime(int level)
    {
        if (level <= 0)
        {
            return 0f;
        }

        int firstTenLevels = System.Math.Min(level, 10);
        int extraLevels = System.Math.Max(0, level - 10);

        return firstTenLevels * Mathf.Max(0f, _bonusPerLevelFirstTen.Value)
            + extraLevels * Mathf.Max(0f, _bonusPerLevelAfterTen.Value);
    }
}