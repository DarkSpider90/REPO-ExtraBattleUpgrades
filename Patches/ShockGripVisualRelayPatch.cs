using ExtraBattleUpgrades.Visuals;
using HarmonyLib;

namespace ExtraBattleUpgrades.Patches;

[HarmonyPatch(typeof(PlayerAvatar), "Start")]
internal static class ShockGripVisualRelayPatch
{
    private static void Postfix(PlayerAvatar __instance)
    {
        ShockGripVisualRelay.Ensure(__instance);
    }
}