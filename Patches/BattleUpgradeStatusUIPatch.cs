using ExtraBattleUpgrades.Upgrades;
using HarmonyLib;
using TMPro;
using UnityEngine;
using ExtraBattleUpgrades.Hud;
using UnityEngine.UI;

namespace ExtraBattleUpgrades.Patches;

[HarmonyPatch]
internal static class BattleUpgradeStatusUIPatch
{
    private static readonly Color PanicActiveColor = new Color(0.1f, 0.85f, 1f, 1f);
    private static readonly Color PanicReadyColor = new Color(1f, 0.25f, 0.75f, 1f);
    private static readonly Color SecondChanceActiveColor = new Color(0.1f, 0.85f, 1f, 1f);
    private static readonly Color SecondChanceReadyColor = new Color(1f, 0.86f, 0.05f, 1f);
    private static readonly Color CooldownColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    private static readonly AccessTools.FieldRef<EnergyUI, TextMeshProUGUI> EnergyTextRef =
        AccessTools.FieldRefAccess<EnergyUI, TextMeshProUGUI>("Text");

    private static readonly AccessTools.FieldRef<HealthUI, TextMeshProUGUI> HealthTextRef =
        AccessTools.FieldRefAccess<HealthUI, TextMeshProUGUI>("Text");

    private static Image _panicIcon;
    private static Image _secondChanceIcon;
    private static Color? _energyDefaultColor;
    private static Color? _healthDefaultColor;
    
    [HarmonyPatch(typeof(EnergyUI), "Update")]
    [HarmonyPostfix]
    private static void EnergyUIPostfix(EnergyUI __instance)
    {
        PlayerAvatar player = PlayerController.instance?.playerAvatarScript;
        if (player == null)
        {
            return;
        }

        TextMeshProUGUI text = EnergyTextRef(__instance);
        if (text == null)
        {
            return;
        }
        
        _energyDefaultColor ??= text.color;

        EnsurePanicIcon(__instance.transform, text);

        bool hasPanic = PanicResponsePatch.HasUpgrade(player);
        
        bool showIcon = ExtraBattleUpgradesPlugin.HudConfig.PanicIconEnabled.Value && hasPanic;
        _panicIcon.gameObject.SetActive(showIcon);

        if (!hasPanic)
        {
            return;
        }

        if (PanicResponsePatch.IsActive(player))
        {
            SetTextColor(text, PanicActiveColor);
        }
        else
        {
            SetTextColor(text, _energyDefaultColor.Value);
        }

        _panicIcon.color = PanicResponsePatch.IsReady(player)
            ? PanicReadyColor
            : CooldownColor;
    }

    [HarmonyPatch(typeof(HealthUI), "Update")]
    [HarmonyPostfix]
    private static void HealthUIPostfix(HealthUI __instance)
    {
        PlayerAvatar player = PlayerController.instance?.playerAvatarScript;
        if (player == null)
        {
            return;
        }

        TextMeshProUGUI text = HealthTextRef(__instance);
        if (text == null)
        {
            return;
        }
        
        _healthDefaultColor ??= text.color;

        EnsureSecondChanceIcon(__instance.transform, text);

        SecondChanceShopUpgrade upgrade = ExtraBattleUpgradesPlugin.SecondChance;
        bool hasSecondChance = upgrade != null && upgrade.HasUpgrade(player);
       
        bool showIcon = ExtraBattleUpgradesPlugin.HudConfig.SecondChanceIconEnabled.Value && hasSecondChance;
        _secondChanceIcon.gameObject.SetActive(showIcon);

        if (!hasSecondChance)
        {
            return;
        }

        if (upgrade.RemainingProtectionSeconds(player) > 0f)
        {
            SetTextColor(text, SecondChanceActiveColor);
        }
        else
        {
            SetTextColor(text, _healthDefaultColor.Value);
        }

        _secondChanceIcon.color = upgrade.IsReady(player)
            ? SecondChanceReadyColor
            : CooldownColor;
    }

    private static void EnsurePanicIcon(Transform parent, TextMeshProUGUI sourceText)
    {
        if (_panicIcon != null)
        {
            return;
        }

        BattleUpgradeHudConfig config = ExtraBattleUpgradesPlugin.HudConfig;

        _panicIcon = CreateIcon(
            sourceText.transform,
            BattleUpgradeIconPainter.LightningSprite(),
            "Panic Response Icon",
            new Vector3(config.PanicIconX.Value, config.PanicIconY.Value, 0f),
            new Vector2(config.PanicIconSize.Value, config.PanicIconSize.Value));
        
    }

    private static void EnsureSecondChanceIcon(Transform parent, TextMeshProUGUI sourceText)
    {
        if (_secondChanceIcon != null)
        {
            return;
        }

        BattleUpgradeHudConfig config = ExtraBattleUpgradesPlugin.HudConfig;

        _secondChanceIcon = CreateIcon(
            sourceText.transform,
            BattleUpgradeIconPainter.HeartSprite(),
            "Second Chance Icon",
            new Vector3(config.SecondChanceIconX.Value, config.SecondChanceIconY.Value, 0f),
            new Vector2(config.SecondChanceIconSize.Value, config.SecondChanceIconSize.Value));
        
    }

    private static Image CreateIcon(
        Transform parent,
        Sprite sprite,
        string objectName,
        Vector3 localOffset,
        Vector2 size)
    {
        GameObject iconObject = new GameObject(objectName, typeof(RectTransform));

        System.Type canvasRendererType =
            System.Type.GetType("UnityEngine.CanvasRenderer, UnityEngine.CoreModule");

        if (canvasRendererType != null)
        {
            iconObject.AddComponent(canvasRendererType);
        }

        iconObject.transform.SetParent(parent, false);
        iconObject.transform.localPosition = localOffset;
        iconObject.transform.localScale = Vector3.one;

        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        Image icon = iconObject.AddComponent<Image>();
        icon.sprite = sprite;
        icon.color = CooldownColor;
        icon.raycastTarget = false;
        icon.preserveAspect = true;

        return icon;
    }

    private static void SetTextColor(TextMeshProUGUI text, Color color)
    {
        text.color = color;
        text.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, color);
        text.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, color);
    }
}