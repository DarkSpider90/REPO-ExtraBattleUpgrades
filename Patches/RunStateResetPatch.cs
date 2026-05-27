using HarmonyLib;

namespace ExtraBattleUpgrades.Patches;

[HarmonyPatch(typeof(LevelGenerator), "GenerateDone")]
internal static class RunStateResetPatch
{
    private static void Postfix()
    {
        PanicResponsePatch.ResetState();
        SecondChancePatch.ResetState();
        ExtraBattleUpgradesPlugin.SecondChance?.ResetState();
    }
}