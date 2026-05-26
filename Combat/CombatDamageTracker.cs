using System.Collections.Generic;
using ExtraBattleUpgrades.Upgrades;
using HarmonyLib;
using UnityEngine;

namespace ExtraBattleUpgrades.Combat;

internal static class CombatDamageTracker
{
    private static readonly Stack<PlayerAvatar[]> OwnerStack = new Stack<PlayerAvatar[]>();

    private static readonly AccessTools.FieldRef<PhysGrabObject, PlayerAvatar> LastPlayerGrabbing =
        AccessTools.FieldRefAccess<PhysGrabObject, PlayerAvatar>("lastPlayerGrabbing");

    private static readonly AccessTools.FieldRef<PhysGrabObject, float> GrabbedTimer =
        AccessTools.FieldRefAccess<PhysGrabObject, float>("grabbedTimer");

    internal static PlayerAvatar[] CurrentOwners => OwnerStack.Count == 0
        ? System.Array.Empty<PlayerAvatar>()
        : OwnerStack.Peek();

    internal static void Push(PlayerAvatar owner)
    {
        if (owner == null)
        {
            OwnerStack.Push(System.Array.Empty<PlayerAvatar>());
            return;
        }

        OwnerStack.Push(new[] { owner });
    }

    internal static void PushFromPhysGrabObject(PhysGrabObject physGrabObject)
    {
        if (physGrabObject == null)
        {
            OwnerStack.Push(System.Array.Empty<PlayerAvatar>());
            return;
        }

        List<PlayerAvatar> owners = new List<PlayerAvatar>();
        if (physGrabObject.playerGrabbing != null)
        {
            foreach (PhysGrabber grabber in physGrabObject.playerGrabbing)
            {
                AddOwner(owners, grabber?.playerAvatar);
            }
        }

        if (owners.Count == 0 && GrabbedTimer(physGrabObject) > 0f)
        {
            AddOwner(owners, LastPlayerGrabbing(physGrabObject));
        }

        OwnerStack.Push(owners.ToArray());
    }

    internal static void Pop()
    {
        if (OwnerStack.Count > 0)
        {
            OwnerStack.Pop();
        }
    }

    internal static void ApplyEnergyLeech(int actualDamage, PlayerAvatar[] owners)
    {
        if (actualDamage <= 0 || owners == null || owners.Length == 0)
        {
            return;
        }

        EnergyLeechShopUpgrade upgrade = ExtraBattleUpgradesPlugin.EnergyLeech;
        if (upgrade == null || !upgrade.Enabled.Value || upgrade.RegisteredUpgrade == null)
        {
            return;
        }

        List<PlayerAvatar> healedPlayers = new List<PlayerAvatar>();
        foreach (PlayerAvatar owner in owners)
        {
            if (owner == null || owner.playerHealth == null || healedPlayers.Contains(owner))
            {
                continue;
            }

            int healing = upgrade.HealingFromDamage(actualDamage, owner);
            if (healing <= 0)
            {
                continue;
            }

            owner.playerHealth.HealOther(healing, true);
            healedPlayers.Add(owner);
        }
    }

    private static void AddOwner(List<PlayerAvatar> owners, PlayerAvatar owner)
    {
        if (owner != null && !owners.Contains(owner))
        {
            owners.Add(owner);
        }
    }
}
