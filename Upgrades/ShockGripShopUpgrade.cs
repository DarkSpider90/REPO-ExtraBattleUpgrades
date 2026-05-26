using BepInEx.Configuration;
using UnityEngine;

namespace ExtraBattleUpgrades.Upgrades;

internal sealed class ShockGripShopUpgrade : ShopUpgrade
{
    internal const string StatsDictionaryKey = "playerUpgradeShockGrip";

    protected override string UpgradeId => "ShockGrip";

    internal ShockGripShopUpgrade(ConfigFile config, AssetBundle bundle)
        : base(
            config,
            bundle,
            "Shock Grip",
            "assets/extrabattleupgrades/items/item upgrade player shock grip.prefab",
            1.0f)
    {
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
        return firstTenLevels * 2 + extraLevels;
    }
}
