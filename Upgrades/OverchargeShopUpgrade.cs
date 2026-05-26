using BepInEx.Configuration;
using UnityEngine;

namespace ExtraBattleUpgrades.Upgrades;

internal sealed class OverchargeShopUpgrade : ShopUpgrade
{
    internal const string StatsDictionaryKey = "playerUpgradeOvercharge";

    protected override string UpgradeId => "Overcharge";

    internal OverchargeShopUpgrade(ConfigFile config, AssetBundle bundle)
        : base(
            config,
            bundle,
            "Overcharge",
            "assets/extrabattleupgrades/items/item upgrade player overcharge.prefab",
            0.75f)
    {
    }

    internal float SlowOvercharge(float value, PlayerAvatar player)
    {
        int level = GetLevel(player);
        if (value <= 0f || level <= 0)
        {
            return value;
        }

        return value / (1f + BonusTime(level));
    }

    private static float BonusTime(int level)
    {
        int firstTenLevels = System.Math.Min(level, 10);
        int extraLevels = System.Math.Max(0, level - 10);
        return firstTenLevels * 0.1f + extraLevels * 0.01f;
    }
}
