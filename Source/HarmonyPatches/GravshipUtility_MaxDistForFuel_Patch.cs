using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(GravshipUtility), nameof(GravshipUtility.MaxDistForFuel))]
public static class GravshipUtility_MaxDistForFuel_Patch
{
    private static void Prefix(ref float fuelPerTile)
    {
        // We're changing the cost from 10 chemfuel to 5 astrofuel (which, by default, costs 10 chemfuel).
        // The method takes 10 as the default value, and never specifies any other cost. So the simplest
        // way to handle this is just cut all the costs in half. We don't bother considering the fuel
        // savings factor in this, since that is a separate argument for this method (as opposed to
        // Building_GravEngine:FuelPerTile getter, which includes the savings already).
        fuelPerTile /= 2f;
    }
}