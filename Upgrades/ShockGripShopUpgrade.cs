using BepInEx.Configuration;
using UnityEngine;

namespace ExtraBattleUpgrades.Upgrades;

internal sealed class ShockGripShopUpgrade : ShopUpgrade
{
    internal const string StatsDictionaryKey = "playerUpgradeShockGrip";

    protected override string UpgradeId => "ShockGrip";

    private readonly ConfigEntry<int> _damagePerLevelFirstTen;
    private readonly ConfigEntry<int> _damagePerLevelAfterTen;

    internal ShockGripShopUpgrade(ConfigFile config, AssetBundle bundle)
        : base(
            config,
            bundle,
            "Shock Grip",
            "assets/extrabattleupgrades/items/item upgrade player shock grip.prefab",
            1.0f)
    {
        _damagePerLevelFirstTen = config.Bind(
            "Shock Grip Upgrade",
            "Damage Per Level First Ten",
            2,
            "Damage per second gained per Shock Grip level from level 1 to 10.");

        _damagePerLevelAfterTen = config.Bind(
            "Shock Grip Upgrade",
            "Damage Per Level After Ten",
            1,
            "Damage per second gained per Shock Grip level after level 10.");
    }

    internal int DamagePerSecond(PlayerAvatar player)
    {
        int level = GetLevel(player);
        if (level <= 0)
        {
            return 0;
        }

        int firstTenLevels = System.Math.Min(level, 10);
        int extraLevels = System.Math.Max(0, level - 10);

        return firstTenLevels * System.Math.Max(0, _damagePerLevelFirstTen.Value)
               + extraLevels * System.Math.Max(0, _damagePerLevelAfterTen.Value);
    }
}