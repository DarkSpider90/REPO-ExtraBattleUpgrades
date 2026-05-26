using HarmonyLib;

namespace ExtraBattleUpgrades.Patches;

[HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.Hurt))]
internal static class ArmorDamagePatch
{
    private static void Prefix(ref int damage, PlayerAvatar ___playerAvatar)
    {
        ApplyProtection(ref damage, ___playerAvatar);
    }

    private static void ApplyProtection(ref int damage, PlayerAvatar player)
    {
        if (damage <= 0 || player == null || ExtraBattleUpgradesPlugin.Armor == null)
        {
            return;
        }

        if (!ExtraBattleUpgradesPlugin.Armor.Enabled.Value || ExtraBattleUpgradesPlugin.Armor.RegisteredUpgrade == null)
        {
            return;
        }

        damage = ExtraBattleUpgradesPlugin.Armor.ReduceDamage(damage, player);
    }
}
