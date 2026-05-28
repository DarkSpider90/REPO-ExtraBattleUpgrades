using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

using ExtraBattleUpgrades.Upgrades;
using ExtraBattleUpgrades.Hud;

namespace ExtraBattleUpgrades;

[BepInDependency("REPOLib")]
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class ExtraBattleUpgradesPlugin : BaseUnityPlugin
{
    public const string PluginGuid = "DarkSpider90.ExtraBattleUpgrades";
    public const string PluginName = "Extra Battle Upgrades";
    public const string PluginVersion = "1.0.0";

    internal static ExtraBattleUpgradesPlugin Instance { get; private set; }
    internal static ManualLogSource Log { get; private set; }
    internal static ArmorShopUpgrade Armor { get; private set; }
    internal static OverchargeShopUpgrade Overcharge { get; private set; }
    internal static EnergyLeechShopUpgrade EnergyLeech { get; private set; }
    internal static ShockGripShopUpgrade ShockGrip { get; private set; }
    internal static SecondChanceShopUpgrade SecondChance { get; private set; }
    internal static PanicResponseShopUpgrade PanicResponse { get; private set; }
    internal static BattleUpgradeHudConfig HudConfig { get; private set; }
    
    private Harmony _harmony;
    private bool _statsLabelsReady;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        AssetBundle bundle = ReadBundledAssets();
        bundle.name = "extra_battle_upgrades_assets";

        Armor = new ArmorShopUpgrade(Config, bundle);
        Overcharge = new OverchargeShopUpgrade(Config, bundle);
        EnergyLeech = new EnergyLeechShopUpgrade(Config, bundle);
        ShockGrip = new ShockGripShopUpgrade(Config, bundle);
        SecondChance = new SecondChanceShopUpgrade(Config, bundle);
        PanicResponse = new PanicResponseShopUpgrade(Config, bundle);
        HudConfig = new BattleUpgradeHudConfig(Config);
        
        Armor.Register();
        Overcharge.Register();
        EnergyLeech.Register();
        ShockGrip.Register();
        SecondChance.Register();
        PanicResponse.Register();

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll();

        Log.LogInfo($"{PluginName} v{PluginVersion} loaded for R.E.P.O. v0.4.0.");
    }

    private void Update()
    {
        if (!_statsLabelsReady && StatsManager.instance != null)
        {
            RefreshStatsLabels(StatsManager.instance);
            _statsLabelsReady = true;
        }

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            return;
        }

        if (LevelGenerator.Instance == null || !LevelGenerator.Instance.Generated)
        {
            return;
        }
        
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }

    internal static void RefreshStatsLabels(StatsManager statsManager)
    {
        SetStatsLabel(statsManager, ArmorShopUpgrade.StatsDictionaryKey, "Armor");
        SetStatsLabel(statsManager, OverchargeShopUpgrade.StatsDictionaryKey, "Overcharge");
        SetStatsLabel(statsManager, EnergyLeechShopUpgrade.StatsDictionaryKey, "Energy Leech");
        SetStatsLabel(statsManager, ShockGripShopUpgrade.StatsDictionaryKey, "Shock Grip");
        SetStatsLabel(statsManager, SecondChanceShopUpgrade.StatsDictionaryKey, "Second Chance");
        SetStatsLabel(statsManager, PanicResponseShopUpgrade.StatsDictionaryKey, "Panic Response");
        
        if (StatsUI.instance != null)
        {
            StatsUI.instance.ResetPlayerUpgradeNames();
        }
    }

    private static void SetStatsLabel(StatsManager statsManager, string statsKey, string displayName)
    {
        if (statsManager.upgradesInfo.TryGetValue(statsKey, out StatsManager.UpgradeInfo info))
        {
            info.displayName = displayName;
            info.displayNameLocalized = null;
            return;
        }

        statsManager.upgradesInfo.Add(statsKey, new StatsManager.UpgradeInfo
        {
            displayName = displayName,
            displayNameLocalized = null
        });
    }

    private static AssetBundle ReadBundledAssets()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream stream = assembly.GetManifestResourceStream("ExtraBattleUpgrades.Assets.extra_battle_upgrades_assets");
        if (stream == null)
        {
            throw new FileNotFoundException("Embedded asset bundle ExtraBattleUpgrades.Assets.extra_battle_upgrades_assets was not found.");
        }

        byte[] bytes = new byte[stream.Length];
        int offset = 0;
        while (offset < bytes.Length)
        {
            int read = stream.Read(bytes, offset, bytes.Length - offset);
            if (read == 0)
            {
                break;
            }

            offset += read;
        }

        AssetBundle assetBundle = AssetBundle.LoadFromMemory(bytes);
        if (assetBundle == null)
        {
            throw new InvalidDataException("Failed to load embedded extra_battle_upgrades_assets asset bundle.");
        }

        return assetBundle;
    }
}
