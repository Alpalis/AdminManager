using HarmonyLib;
using SDG.Unturned;
using System.Diagnostics.CodeAnalysis;

namespace Alpalis.AdminManager.Patches;

[HarmonyPatch(typeof(PlayerLife))]
public static class StatUpdatingPatch
{
    public delegate bool StatUpdating(PlayerLife player);
    public static event StatUpdating? OnStatUpdating;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerLife.askStarve))]
    [SuppressMessage("Style", "IDE1006:Style", Justification = "Original name in the game.")]
    private static bool askStarve(PlayerLife __instance)
    {
        return !OnStatUpdating?.Invoke(__instance) ?? true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerLife.askDehydrate))]
    [SuppressMessage("Style", "IDE1006:Style", Justification = "Original name in the game.")]
    private static bool askDehydrate(PlayerLife __instance)
    {
        return !OnStatUpdating?.Invoke(__instance) ?? true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerLife.askInfect))]
    [SuppressMessage("Style", "IDE1006:Style", Justification = "Original name in the game.")]
    private static bool askInfect(PlayerLife __instance)
    {
        return !OnStatUpdating?.Invoke(__instance) ?? true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerLife.serverSetBleeding))]
    [SuppressMessage("Style", "IDE1006:Style", Justification = "Original name in the game.")]
    private static bool serverSetBleeding(PlayerLife __instance, bool newBleeding)
    {
        return !(newBleeding && (OnStatUpdating?.Invoke(__instance) ?? false));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerLife.serverSetLegsBroken))]
    [SuppressMessage("Style", "IDE1006:Style", Justification = "Original name in the game.")]
    private static bool serverSetLegsBroken(PlayerLife __instance, bool newLegsBroken)
    {
        return !(newLegsBroken && (OnStatUpdating?.Invoke(__instance) ?? false));
    }
}
