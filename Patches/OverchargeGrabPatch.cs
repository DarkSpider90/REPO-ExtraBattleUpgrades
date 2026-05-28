using System;
using HarmonyLib;
using UnityEngine;

namespace ExtraBattleUpgrades.Patches;

[HarmonyPatch(typeof(PhysGrabber))]
internal static class OverchargeGrabPatch
{
    private static readonly AccessTools.FieldRef<PhysGrabber, PlayerAvatar> OwnerRef =
        AccessTools.FieldRefAccess<PhysGrabber, PlayerAvatar>("playerAvatar");
    
    private static readonly AccessTools.FieldRef<PhysGrabber, float> OverchargeFloatRef =
        AccessTools.FieldRefAccess<PhysGrabber, float>("physGrabBeamOverChargeFloat");

    private static readonly AccessTools.FieldRef<PhysGrabber, byte> OverchargeByteRef =
        AccessTools.FieldRefAccess<PhysGrabber, byte>("physGrabBeamOverCharge");
    
    private static readonly AccessTools.FieldRef<PhysGrabber, float> OverchargeAmountRef =
        AccessTools.FieldRefAccess<PhysGrabber, float>("physGrabBeamOverChargeAmount");

    [HarmonyPatch("PhysGrabOverCharge")]
    [HarmonyPostfix]
    private static void ReduceFinalOverchargeGain(
        PhysGrabber __instance,
        float __state)
    {
        if (!TryGetChargeUpgrade(out PlayerAvatar player, __instance))
        {
            return;
        }
        
        float amount = OverchargeAmountRef(__instance);
        float slowedAmount = ExtraBattleUpgradesPlugin.Overcharge.SlowOvercharge(amount, player);
        OverchargeAmountRef(__instance) = slowedAmount;
        
        float current = OverchargeFloatRef(__instance);
        if (current <= __state)
        {
            return;
        }

        float vanillaGain = current - __state;
        float upgradedGain = ExtraBattleUpgradesPlugin.Overcharge.SlowOvercharge(vanillaGain, player);
        
        OverchargeFloatRef(__instance) = Mathf.Clamp01(__state + upgradedGain);
        OverchargeByteRef(__instance) = (byte)(OverchargeFloatRef(__instance) * 200f);
        
    }
    
    [HarmonyPatch("PhysGrabOverCharge")]
    [HarmonyPrefix]
    private static void RememberOverchargeBeforeGain(
        PhysGrabber __instance,
        out float __state)
    {
        __state = OverchargeFloatRef(__instance);
    }

    [HarmonyPatch("PhysGrabOverChargeLogic")]
    [HarmonyPrefix]
    private static void RememberChargeBeforeTick(float ___physGrabBeamOverChargeFloat, out float __state)
    {
        __state = ___physGrabBeamOverChargeFloat;
    }

    [HarmonyPatch("PhysGrabOverChargeLogic")]
    [HarmonyPostfix]
    private static void ImproveChargeRecovery(
        PhysGrabber __instance,
        float __state,
        ref float ___physGrabBeamOverChargeFloat,
        ref byte ___physGrabBeamOverCharge)
    {
        if (__state <= 0f || __state <= ___physGrabBeamOverChargeFloat)
        {
            return;
        }

        if (!TryGetChargeUpgrade(out PlayerAvatar player, __instance))
        {
            return;
        }

        float vanillaRecovery = ExtraBattleUpgradesPlugin.Overcharge.RecoveryBasePerSecond() * Time.deltaTime;
        float upgradedRecovery = ExtraBattleUpgradesPlugin.Overcharge.SlowOvercharge(vanillaRecovery, player);
        float recoveryBonus = Math.Max(0f, vanillaRecovery - upgradedRecovery);

        if (recoveryBonus <= 0f)
        {
            return;
        }

        ___physGrabBeamOverChargeFloat = Math.Max(0f, ___physGrabBeamOverChargeFloat - recoveryBonus);
        ___physGrabBeamOverCharge = (byte)(___physGrabBeamOverChargeFloat * 200f);
    }

    private static bool TryGetChargeUpgrade(out PlayerAvatar player, PhysGrabber grabber)
    {
        player = OwnerRef(grabber);
        return player != null
            && ExtraBattleUpgradesPlugin.Overcharge != null
            && ExtraBattleUpgradesPlugin.Overcharge.Enabled.Value
            && ExtraBattleUpgradesPlugin.Overcharge.RegisteredUpgrade != null;
    }

}
