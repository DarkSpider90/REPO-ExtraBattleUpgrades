using ExtraBattleUpgrades.Upgrades;
using HarmonyLib;

namespace ExtraBattleUpgrades.Patches;

[HarmonyPatch(typeof(PlayerController), "Update")]
internal static class PanicResponseSprintDrainPatch
{
    private static float _baseSprintDrain;
    private static bool _hasBaseSprintDrain;

    private static void Postfix(PlayerAvatar ___playerAvatarScript)
    {
        PlayerController controller = PlayerController.instance;
        if (controller == null || ___playerAvatarScript == null)
        {
            return;
        }

        PanicResponseShopUpgrade upgrade = ExtraBattleUpgradesPlugin.PanicResponse;
        if (upgrade == null
            || !upgrade.Enabled.Value
            || upgrade.RegisteredUpgrade == null)
        {
            Restore(controller);
            return;
        }

        if (!_hasBaseSprintDrain)
        {
            _baseSprintDrain = controller.EnergySprintDrain;
            _hasBaseSprintDrain = true;
        }

        float multiplier = upgrade.SprintDrainMultiplier(___playerAvatarScript);
        controller.EnergySprintDrain = _baseSprintDrain * multiplier;
    }

    private static void Restore(PlayerController controller)
    {
        if (!_hasBaseSprintDrain || controller == null)
        {
            return;
        }

        controller.EnergySprintDrain = _baseSprintDrain;
    }
}