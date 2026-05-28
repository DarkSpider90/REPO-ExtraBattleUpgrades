using System.Collections.Generic;
using ExtraBattleUpgrades.Combat;
using HarmonyLib;
using UnityEngine;
using ExtraBattleUpgrades.Visuals;

namespace ExtraBattleUpgrades.Patches;

[HarmonyPatch(typeof(EnemyRigidbody), "FixedUpdate")]
internal static class ShockGripPatch
{
    private const float DamageTickSeconds = 0.5f;

    private static readonly Dictionary<ShockTarget, float> DamageTimers = new Dictionary<ShockTarget, float>();

    private static readonly AccessTools.FieldRef<EnemyRigidbody, PhysGrabObject> EnemyGrabObjectRef =
        AccessTools.FieldRefAccess<EnemyRigidbody, PhysGrabObject>("physGrabObject");

    private static readonly AccessTools.FieldRef<EnemyRigidbody, bool> EnemyGrabbedRef =
        AccessTools.FieldRefAccess<EnemyRigidbody, bool>("grabbed");

    private static readonly AccessTools.FieldRef<EnemyRigidbody, float> GrabStrengthTimerRef =
        AccessTools.FieldRefAccess<EnemyRigidbody, float>("grabStrengthTimer");

    private static readonly AccessTools.FieldRef<PhysGrabObject, PlayerAvatar> LastPlayerGrabbingRef =
        AccessTools.FieldRefAccess<PhysGrabObject, PlayerAvatar>("lastPlayerGrabbing");

    private static readonly AccessTools.FieldRef<PhysGrabObject, float> GrabbedTimerRef =
        AccessTools.FieldRefAccess<PhysGrabObject, float>("grabbedTimer");

    private static readonly AccessTools.FieldRef<EnemyGrounded, bool> GroundedRef =
        AccessTools.FieldRefAccess<EnemyGrounded, bool>("grounded");

    private static void Postfix(EnemyRigidbody __instance)
    {
        if (!CanRunShockGrip())
        {
            return;
        }

        Enemy enemy = __instance.enemy;
        PhysGrabObject grabObject = EnemyGrabObjectRef(__instance);
        EnemyHealth health = enemy != null ? enemy.GetComponent<EnemyHealth>() : null;
        List<PlayerAvatar> holders = GetHolders(grabObject);
        if (!CanDamageEnemy(__instance, enemy, grabObject, health, holders))
        {
            ClearEnemyTimers(__instance.GetInstanceID());
            return;
        }

        foreach (PlayerAvatar holder in holders)
        {
            int damagePerSecond = ExtraBattleUpgradesPlugin.ShockGrip.DamagePerSecond(holder);
            if (damagePerSecond <= 0)
            {
                continue;
            }

            TickDamage(__instance.GetInstanceID(), holder, health, damagePerSecond);
        }
    }

    private static List<PlayerAvatar> GetHolders(PhysGrabObject grabObject)
    {
        List<PlayerAvatar> holders = new List<PlayerAvatar>();
        if (grabObject == null)
        {
            return holders;
        }

        if (grabObject.playerGrabbing != null)
        {
            foreach (PhysGrabber grabber in grabObject.playerGrabbing)
            {
                AddHolder(holders, grabber?.playerAvatar);
            }
        }

        if (holders.Count == 0 && GrabbedTimerRef(grabObject) > 0f)
        {
            AddHolder(holders, LastPlayerGrabbingRef(grabObject));
        }

        return holders;
    }

    private static void TickDamage(int enemyId, PlayerAvatar holder, EnemyHealth health, int damagePerSecond)
    {
        ShockTarget target = new ShockTarget(enemyId, holder.GetInstanceID());
        DamageTimers.TryGetValue(target, out float timer);
        timer += Time.fixedDeltaTime;

        if (timer < DamageTickSeconds)
        {
            DamageTimers[target] = timer;
            return;
        }

        DamageTimers[target] = timer - DamageTickSeconds;
        int damage = Mathf.CeilToInt(damagePerSecond * DamageTickSeconds);
        CombatDamageTracker.Push(holder);
        try
        {
            health.Hurt(damage, Vector3.up);

            ShockGripVisuals.PlayLocal(holder);

            if (GameManager.Multiplayer())
            {
                ShockGripVisualRelay.Broadcast(holder);
            }
        }
        finally
        {
            CombatDamageTracker.Pop();
        }
    }

    private static bool CanRunShockGrip()
    {
        return SemiFunc.IsMasterClientOrSingleplayer()
            && ExtraBattleUpgradesPlugin.ShockGrip != null
            && ExtraBattleUpgradesPlugin.ShockGrip.Enabled.Value
            && ExtraBattleUpgradesPlugin.ShockGrip.RegisteredUpgrade != null;
    }

    private static bool CanDamageEnemy(
        EnemyRigidbody enemyBody,
        Enemy enemy,
        PhysGrabObject grabObject,
        EnemyHealth health,
        List<PlayerAvatar> holders)
    {
        if (enemyBody == null || enemy == null || grabObject == null || health == null || holders.Count == 0)
        {
            return false;
        }

        if (!enemy.IsStunned() && !EnemyGrabbedRef(enemyBody) && GrabStrengthTimerRef(enemyBody) <= 0f)
        {
            return false;
        }

        return IsAirborne(enemyBody, enemy, grabObject);
    }

    private static bool IsAirborne(EnemyRigidbody enemyBody, Enemy enemy, PhysGrabObject grabObject)
    {
        EnemyGrounded grounded = enemy.GetComponentInChildren<EnemyGrounded>();
        if (grounded != null && !GroundedRef(grounded))
        {
            return true;
        }

        Vector3 checkPosition = grabObject.centerPoint != Vector3.zero
            ? grabObject.centerPoint
            : enemyBody.transform.position;

        return !SemiFunc.OnGroundCheck(checkPosition, 0.75f, grabObject);
    }

    private static void AddHolder(List<PlayerAvatar> holders, PlayerAvatar holder)
    {
        if (holder != null && !holders.Contains(holder))
        {
            holders.Add(holder);
        }
    }

    private static void ClearEnemyTimers(int enemyId)
    {
        foreach (ShockTarget target in new List<ShockTarget>(DamageTimers.Keys))
        {
            if (target.EnemyId == enemyId)
            {
                DamageTimers.Remove(target);
            }
        }
    }

    private readonly struct ShockTarget
    {
        internal readonly int EnemyId;
        private readonly int _playerId;

        internal ShockTarget(int enemyId, int playerId)
        {
            EnemyId = enemyId;
            _playerId = playerId;
        }
    }
}
