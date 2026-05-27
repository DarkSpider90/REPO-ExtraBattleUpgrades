using BepInEx.Configuration;
using System;
using UnityEngine;

namespace ExtraBattleUpgrades.Upgrades;

internal sealed class PanicResponseShopUpgrade : ShopUpgrade
{
    internal const string StatsDictionaryKey = "playerUpgradePanicResponse";

    protected override string UpgradeId => "PanicResponse";

    internal PanicResponseShopUpgrade(ConfigFile config, AssetBundle bundle)
        : base(
            config,
            bundle,
            "Panic Response",
            "assets/extrabattleupgrades/items/item upgrade player panic response.prefab",
            1.0f)
    {
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
            return 5f;
        }

        if (level <= 3)
        {
            return 6f;
        }

        return 7f;
    }

    internal float CooldownSeconds(PlayerAvatar player)
    {
        int level = Math.Min(GetLevel(player), 10);

        if (level <= 0)
        {
            return 9999f;
        }

        if (level <= 4)
        {
            return 60f;
        }

        return Math.Max(30f, 60f - ((level - 4) * 5f));
    }

    internal float SpeedMultiplier(PlayerAvatar player)
    {
        int level = Math.Min(GetLevel(player), 10);

        if (level <= 0)
        {
            return 1f;
        }

        // Level 1-2 = about +1 vanilla speed upgrade.
        // Level 3+ = about +2 vanilla speed upgrades.
        return level >= 3 ? 1.35f : 1.2f;
    }
    
}