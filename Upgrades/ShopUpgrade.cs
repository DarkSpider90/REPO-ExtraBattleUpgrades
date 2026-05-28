using BepInEx.Configuration;
using REPOLib.Modules;
using UnityEngine;
using RepoUpgrades = REPOLib.Modules.Upgrades;

namespace ExtraBattleUpgrades.Upgrades;

internal abstract class ShopUpgrade
{
    private readonly AssetBundle _bundle;
    private readonly string _prefabPath;
    private readonly string _label;
    private readonly float _defaultPriceMultiplier;

    internal ConfigEntry<bool> Enabled { get; }
    internal ConfigEntry<float> PriceMultiplier { get; }
    internal PlayerUpgrade RegisteredUpgrade { get; private set; }

    protected abstract string UpgradeId { get; }

    protected ShopUpgrade(
        ConfigFile config,
        AssetBundle bundle,
        string label,
        string prefabPath,
        float defaultPriceMultiplier)
    {
        _bundle = bundle;
        _prefabPath = prefabPath;
        _label = label;
        _defaultPriceMultiplier = defaultPriceMultiplier;

        string configGroup = $"{_label} Upgrade";
        Enabled = config.Bind(configGroup, "Enabled", true, $"Enable the {_label} upgrade.");
        PriceMultiplier = config.Bind(configGroup, "Price Multiplier", _defaultPriceMultiplier, "Multiplier applied to the base shop price.");
     }

    internal void Register()
    {
        if (!Enabled.Value)
        {
            ExtraBattleUpgradesPlugin.Log.LogInfo($"{_label} upgrade disabled by config.");
            return;
        }

        GameObject prefab = _bundle.LoadAsset<GameObject>(_prefabPath);
        ItemAttributes itemAttributes = prefab != null ? prefab.GetComponent<ItemAttributes>() : null;
        if (prefab == null || itemAttributes == null || itemAttributes.item == null)
        {
            ExtraBattleUpgradesPlugin.Log.LogError($"Could not load {_label} upgrade prefab '{_prefabPath}'.");
            return;
        }

        Item shopItem = itemAttributes.item;
        shopItem.value = MakePriceRange(shopItem.value, PriceMultiplier.Value);

        Items.RegisterItem(itemAttributes);
        RegisteredUpgrade = RepoUpgrades.RegisterUpgrade(UpgradeId, shopItem, OnRunStart, OnUpgradeBought);

        if (RegisteredUpgrade == null)
        {
            ExtraBattleUpgradesPlugin.Log.LogError($"Failed to register {_label} upgrade with REPOLib.");
            return;
        }

        ExtraBattleUpgradesPlugin.Log.LogInfo($"Registered {_label} upgrade.");
    }

    internal int GetLevel(PlayerAvatar player)
    {
        return player == null ? 0 : RegisteredUpgrade?.GetLevel(player) ?? 0;
    }

    protected virtual void OnRunStart(PlayerAvatar player, int level)
    {
        if (player != null && ReferenceEquals(player, SemiFunc.PlayerAvatarLocal()))
        {
            ExtraBattleUpgradesPlugin.Log.LogDebug($"Initialized {_label} level {level} for {SemiFunc.PlayerGetName(player)}.");
        }
    }

    protected virtual void OnUpgradeBought(PlayerAvatar player, int level)
    {
        if (player != null && ReferenceEquals(player, SemiFunc.PlayerAvatarLocal()))
        {
            ExtraBattleUpgradesPlugin.Log.LogDebug($"Applied {_label} level {level} for {SemiFunc.PlayerGetName(player)}.");
        }
    }

    private static Value MakePriceRange(Value source, float multiplier)
    {
        Value range = ScriptableObject.CreateInstance<Value>();
        if (source == null)
        {
            range.valueMin = 100f * multiplier;
            range.valueMax = 200f * multiplier;
            return range;
        }

        range.valueMin = source.valueMin * multiplier;
        range.valueMax = source.valueMax * multiplier;
        return range;
    }
}
