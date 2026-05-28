using BepInEx.Configuration;
using System;
using UnityEngine;

namespace ExtraBattleUpgrades.Upgrades;

internal sealed class EnergyLeechShopUpgrade : ShopUpgrade
{
    internal const string StatsDictionaryKey = "playerUpgradeEnergyLeech";

    protected override string UpgradeId => "EnergyLeech";
    
    private readonly ConfigEntry<float> _bonusPerLevelFirstTen;
    private readonly ConfigEntry<float> _bonusPerLevelAfterTen;

    internal EnergyLeechShopUpgrade(ConfigFile config, AssetBundle bundle)
        : base(
            config,
            bundle,
            "Energy Leech",
            "assets/extrabattleupgrades/items/item upgrade player energy leech.prefab",
            1.0f)
    {
        _bonusPerLevelFirstTen = config.Bind(
            "Energy Leech Upgrade",
            "Healing Percent Per Level First Ten",
            0.025f,
            "Healing Percent Per Energy Leech level from level 1 to 10. 0.025 = 2.5%.");

        _bonusPerLevelAfterTen = config.Bind(
            "Energy Leech Upgrade",
            "Healing Percent Per Level After Ten",
            0.01f,
            "Percent Of Steel HP per Energy Leech level after level 10. 0.01 = 1%.");
    }

    internal int HealingFromDamage(int damage, PlayerAvatar player)
    {
        int level = GetLevel(player);
        if (damage <= 0 || level <= 0)
        {
            return 0;
        }

        return Math.Max(1, (int)Math.Ceiling(damage * HealingPercent(level)));
    }

    private float HealingPercent(int level)
    {
        int firstTenLevels = Math.Min(level, 10);
        int extraLevels = Math.Max(0, level - 10);
        return firstTenLevels * Math.Max(0f, _bonusPerLevelFirstTen.Value)
               + extraLevels * Math.Max(0f, _bonusPerLevelAfterTen.Value);
    }
}
