using ExtraBattleUpgrades.Combat;
using HarmonyLib;

namespace ExtraBattleUpgrades.Patches;

[HarmonyPatch(typeof(EnemyHealth), nameof(EnemyHealth.Hurt))]
internal static class EnergyLeechDamagePatch
{
    private readonly struct DamageSnapshot
    {
        internal readonly int HealthBefore;
        internal readonly PlayerAvatar[] Owners;

        internal DamageSnapshot(int healthBefore, PlayerAvatar[] owners)
        {
            HealthBefore = healthBefore;
            Owners = owners;
        }
    }

    private static void Prefix(int ___healthCurrent, out DamageSnapshot __state)
    {
        __state = new DamageSnapshot(___healthCurrent, CombatDamageTracker.CurrentOwners);
    }

    private static void Postfix(int ___healthCurrent, DamageSnapshot __state)
    {
        int actualDamage = __state.HealthBefore - ___healthCurrent;
        CombatDamageTracker.ApplyEnergyLeech(actualDamage, __state.Owners);
    }
}

[HarmonyPatch(typeof(HurtCollider), "EnemyHurt")]
internal static class EnergyLeechHurtColliderPatch
{
    private static void Prefix(
        HurtCollider __instance,
        PlayerAvatar ___playerCausingHurt,
        PlayerAvatar ___playerCausingHurtOverride,
        PhysGrabObject ___parentPhysGrabObject)
    {
        if (__instance.deathPit)
        {
            CombatDamageTracker.Push(null);
            return;
        }

        if (___playerCausingHurtOverride != null)
        {
            CombatDamageTracker.Push(___playerCausingHurtOverride);
            return;
        }

        if (___parentPhysGrabObject != null)
        {
            CombatDamageTracker.PushFromPhysGrabObject(___parentPhysGrabObject);
            return;
        }

        CombatDamageTracker.Push(___playerCausingHurt);
    }

    private static void Finalizer()
    {
        CombatDamageTracker.Pop();
    }
}

[HarmonyPatch(typeof(PhysGrabObjectImpactDetector), "OnCollisionStay")]
internal static class EnergyLeechHeldObjectImpactPatch
{
    private static void Prefix(PhysGrabObject ___physGrabObject)
    {
        CombatDamageTracker.PushFromPhysGrabObject(___physGrabObject);
    }

    private static void Finalizer()
    {
        CombatDamageTracker.Pop();
    }
}
