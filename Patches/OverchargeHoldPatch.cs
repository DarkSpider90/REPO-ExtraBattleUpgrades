using HarmonyLib;
using UnityEngine;

namespace ExtraBattleUpgrades.Patches;

[HarmonyPatch(typeof(EnemyRigidbody), "FixedUpdate")]
internal static class OverchargeHoldPatch
{
    private static readonly AccessTools.FieldRef<EnemyRigidbody, PhysGrabObject> EnemyGrabObjectRef =
        AccessTools.FieldRefAccess<EnemyRigidbody, PhysGrabObject>("physGrabObject");

    private static readonly AccessTools.FieldRef<EnemyRigidbody, float> GrabStrengthTimerRef =
        AccessTools.FieldRefAccess<EnemyRigidbody, float>("grabStrengthTimer");
    
    private static readonly AccessTools.FieldRef<EnemyRigidbody, float> GrabTimeCurrentRef =
        AccessTools.FieldRefAccess<EnemyRigidbody, float>("grabTimeCurrent");

    private static void Prefix(
        EnemyRigidbody __instance,
        ref float ___grabStrengthTimer)
    {
        PhysGrabObject grabObject = EnemyGrabObjectRef(__instance);

        if (grabObject == null
            || grabObject.playerGrabbing == null
            || grabObject.playerGrabbing.Count <= 0)
        {
            return;
        }

        float bestBonus = 0f;

        foreach (PhysGrabber grabber in grabObject.playerGrabbing)
        {
            PlayerAvatar player = grabber?.playerAvatar;

            if (player == null)
            {
                continue;
            }

            float bonus = ExtraBattleUpgradesPlugin.Overcharge.HoldStabilityBonus(player);

            if (bonus > bestBonus)
            {
                bestBonus = bonus;
            }
        }

        if (bestBonus <= 0f)
        {
            return;
        }

        float currentGrabTime = GrabTimeCurrentRef(__instance);

        GrabTimeCurrentRef(__instance) =
            Mathf.Max(0f, currentGrabTime - Time.fixedDeltaTime * bestBonus);

        float timer = GrabStrengthTimerRef(__instance);

        if (timer > 0f)
        {
            ___grabStrengthTimer += Time.fixedDeltaTime * bestBonus;
        }
    }
}