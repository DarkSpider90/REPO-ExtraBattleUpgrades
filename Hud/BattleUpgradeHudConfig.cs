using BepInEx.Configuration;
using UnityEngine;

namespace ExtraBattleUpgrades.Hud;

internal sealed class BattleUpgradeHudConfig
{
    internal ConfigEntry<bool> PanicIconEnabled { get; }
    internal ConfigEntry<float> PanicIconX { get; }
    internal ConfigEntry<float> PanicIconY { get; }
    internal ConfigEntry<float> PanicIconSize { get; }

    internal ConfigEntry<bool> SecondChanceIconEnabled { get; }
    internal ConfigEntry<float> SecondChanceIconX { get; }
    internal ConfigEntry<float> SecondChanceIconY { get; }
    internal ConfigEntry<float> SecondChanceIconSize { get; }

    internal BattleUpgradeHudConfig(ConfigFile config)
    {
        PanicIconEnabled = config.Bind("HUD - Panic Response", "Icon Enabled", true, "Show Panic Response HUD icon.");
        PanicIconX = config.Bind("HUD - Panic Response", "Icon X", 68f, "Panic Response icon local X position.");
        PanicIconY = config.Bind("HUD - Panic Response", "Icon Y", -23f, "Panic Response icon local Y position.");
        PanicIconSize = config.Bind("HUD - Panic Response", "Icon Size", 11f, "Panic Response icon size.");

        SecondChanceIconEnabled = config.Bind("HUD - Second Chance", "Icon Enabled", true, "Show Second Chance HUD icon.");
        SecondChanceIconX = config.Bind("HUD - Second Chance", "Icon X", 68f, "Second Chance icon local X position.");
        SecondChanceIconY = config.Bind("HUD - Second Chance", "Icon Y", -23f, "Second Chance icon local Y position.");
        SecondChanceIconSize = config.Bind("HUD - Second Chance", "Icon Size", 15f, "Second Chance icon size.");
    }
}