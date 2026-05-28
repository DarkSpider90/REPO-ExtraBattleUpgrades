using BepInEx.Configuration;
using System;
using UnityEngine;

namespace ExtraBattleUpgrades.Upgrades;

internal sealed class ArmorShopUpgrade : ShopUpgrade
{
    internal const string StatsDictionaryKey = "playerUpgradeArmor";
    
    protected override string UpgradeId => "Armor";
    
    private readonly ConfigEntry<float> _bonusPerLevelFirstTen;
    private readonly ConfigEntry<float> _bonusPerLevelAfterTen;
    
    internal ArmorShopUpgrade(ConfigFile config, AssetBundle bundle)
        : base(
            config,
            bundle,
            "Armor",
            "assets/extrabattleupgrades/items/item upgrade player armor.prefab",
            1.0f)
    {
        _bonusPerLevelFirstTen = config.Bind(
            "Armor Upgrade",
            "Bonus Per Level First Ten",
            0.05f,
            "Damage Reduction per Armor level 1 to 10. 0.05 = 5%.");

        _bonusPerLevelAfterTen = config.Bind(
            "Armor Upgrade",
            "Damage Reduction Per Level After Ten",
            0.05f,
            "Damage reduction per Armor level 10. 0.01 = 1%.");
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

    private float DamageReduction(int level)
    {
        int firstTenLevels = Math.Min(level, 10);
        int extraLevels = Math.Max(0, level - 10);
        return firstTenLevels * Math.Max(0f, _bonusPerLevelFirstTen.Value)
               + extraLevels * Math.Max(0f, _bonusPerLevelAfterTen.Value);
    }
}
