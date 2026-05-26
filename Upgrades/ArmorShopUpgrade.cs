using BepInEx.Configuration;
using System;
using UnityEngine;

namespace ExtraBattleUpgrades.Upgrades;

internal sealed class ArmorShopUpgrade : ShopUpgrade
{
    internal const string StatsDictionaryKey = "playerUpgradeArmor";

    protected override string UpgradeId => "Armor";

    internal ArmorShopUpgrade(ConfigFile config, AssetBundle bundle)
        : base(
            config,
            bundle,
            "Armor",
            "assets/extrabattleupgrades/items/item upgrade player armor.prefab",
            1.0f)
    {
    }

    internal int ReduceDamage(int damage, PlayerAvatar player)
    {
        int level = GetLevel(player);
        if (damage <= 0 || level <= 0)
        {
            return damage;
        }

        float damageMultiplier = Math.Max(0f, 1f - DamageReduction(level));
        return Math.Max(1, (int)Math.Ceiling(damage * damageMultiplier));
    }

    private static float DamageReduction(int level)
    {
        int firstTenLevels = Math.Min(level, 10);
        int extraLevels = Math.Max(0, level - 10);
        return firstTenLevels * 0.05f + extraLevels * 0.01f;
    }
}
