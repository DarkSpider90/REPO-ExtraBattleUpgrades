using HarmonyLib;

namespace ExtraBattleUpgrades.Patches;

[HarmonyPatch(typeof(StatsManager), "Start")]
internal static class StatsLabelPatch
{
    private static void Postfix(StatsManager __instance)
    {
        ExtraBattleUpgradesPlugin.RefreshStatsLabels(__instance);
    }
}
