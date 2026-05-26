using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtraBattleUpgrades.Upgrades;

internal sealed class SecondChanceShopUpgrade : ShopUpgrade
{
    private readonly Dictionary<string, float> _cooldownEnds = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _protectionEnds = new Dictionary<string, float>();

    internal const string StatsDictionaryKey = "playerUpgradeSecondChance";

    protected override string UpgradeId => "SecondChance";

    internal SecondChanceShopUpgrade(ConfigFile config, AssetBundle bundle)
        : base(
            config,
            bundle,
            "Second Chance",
            "assets/extrabattleupgrades/items/item upgrade player second chance.prefab",
            1.0f)
    {
    }

    internal bool TryActivate(PlayerAvatar player, out float invulnerabilitySeconds)
    {
        invulnerabilitySeconds = 0f;
        int level = GetLevel(player);
        if (level <= 0)
        {
            return false;
        }

        string playerId = SemiFunc.PlayerGetSteamID(player);
        if (string.IsNullOrWhiteSpace(playerId))
        {
            playerId = player.GetInstanceID().ToString();
        }

        float now = Time.time;
        if (_cooldownEnds.TryGetValue(playerId, out float cooldownEnd) && cooldownEnd > now)
        {
            return false;
        }

        invulnerabilitySeconds = InvulnerabilitySeconds(level);
        _protectionEnds[playerId] = now + invulnerabilitySeconds;
        _cooldownEnds[playerId] = now + CooldownSeconds(level);
        return true;
    }

    internal bool TryRescueFromPit(PlayerAvatar player, out float invulnerabilitySeconds, out bool activatedNow)
    {
        activatedNow = false;
        invulnerabilitySeconds = RemainingProtectionSeconds(player);
        if (invulnerabilitySeconds > 0f)
        {
            return true;
        }

        activatedNow = TryActivate(player, out invulnerabilitySeconds);
        return activatedNow;
    }

    internal float RemainingProtectionSeconds(PlayerAvatar player)
    {
        string playerId = PlayerId(player);
        if (string.IsNullOrWhiteSpace(playerId) || !_protectionEnds.TryGetValue(playerId, out float protectionEnd))
        {
            return 0f;
        }

        return Math.Max(0f, protectionEnd - Time.time);
    }

    private static string PlayerId(PlayerAvatar player)
    {
        if (player == null)
        {
            return string.Empty;
        }

        string playerId = SemiFunc.PlayerGetSteamID(player);
        return string.IsNullOrWhiteSpace(playerId) ? player.GetInstanceID().ToString() : playerId;
    }

    private static float InvulnerabilitySeconds(int level)
    {
        return Math.Min(level, 5);
    }

    private static float CooldownSeconds(int level)
    {
        float reduction = 0f;
        if (level > 5)
        {
            int firstCooldownLevels = Math.Min(level - 5, 5);
            int extraCooldownLevels = Math.Max(0, level - 10);
            reduction = firstCooldownLevels * 5f + extraCooldownLevels * 0.5f;
        }

        return Math.Max(30f, 120f - reduction);
    }
}
