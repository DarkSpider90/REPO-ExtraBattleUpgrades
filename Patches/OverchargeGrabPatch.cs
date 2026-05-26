using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ExtraBattleUpgrades.Patches;

[HarmonyPatch(typeof(PhysGrabber))]
internal static class OverchargeGrabPatch
{
    private static readonly AccessTools.FieldRef<PhysGrabber, PlayerAvatar> OwnerRef =
        AccessTools.FieldRefAccess<PhysGrabber, PlayerAvatar>("playerAvatar");

    public static bool Prepare()
    {
        return HasPhysGrabberMethod("PhysGrabOverCharge") && HasPhysGrabberMethod("PhysGrabOverChargeLogic");
    }

    [HarmonyPatch("PhysGrabOverCharge")]
    [HarmonyPrefix]
    private static void ReduceChargeGain(PhysGrabber __instance, ref float _amount, ref float _multiplier)
    {
        if (!TryGetChargeUpgrade(out PlayerAvatar player, __instance))
        {
            return;
        }

        _amount = ExtraBattleUpgradesPlugin.Overcharge.SlowOvercharge(_amount, player);
        _multiplier = ExtraBattleUpgradesPlugin.Overcharge.SlowOvercharge(_multiplier, player);
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

        float vanillaRecovery = 0.1f * Time.deltaTime;
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

    private static bool HasPhysGrabberMethod(string methodName)
    {
        return typeof(PhysGrabber).GetMethods().Any(method => method.Name == methodName);
    }
}
