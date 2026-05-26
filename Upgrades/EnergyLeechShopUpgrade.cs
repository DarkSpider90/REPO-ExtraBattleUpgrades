using BepInEx.Configuration;
using System;
using UnityEngine;

namespace ExtraBattleUpgrades.Upgrades;

internal sealed class EnergyLeechShopUpgrade : ShopUpgrade
{
    internal const string StatsDictionaryKey = "playerUpgradeEnergyLeech";

    protected override string UpgradeId => "EnergyLeech";

    internal EnergyLeechShopUpgrade(ConfigFile config, AssetBundle bundle)
        : base(
            config,
            bundle,
            "Energy Leech",
            "assets/extrabattleupgrades/items/item upgrade player energy leech.prefab",
            1.0f)
    {
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

    private static float HealingPercent(int level)
    {
        int firstTenLevels = Math.Min(level, 10);
        int extraLevels = Math.Max(0, level - 10);
        return firstTenLevels * 0.025f + extraLevels * 0.01f;
    }
}
