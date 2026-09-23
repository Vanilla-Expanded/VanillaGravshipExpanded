using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(WorldObject), nameof(WorldObject.GetDescription))]
public static class WorldObject_GetDescription_Patch
{
    public static void Postfix(WorldObject __instance, ref string __result)
    {
        if (__instance.def.requiresSignalJammerToReach is false || __instance.Faction != Faction.OfPlayer)
            return;

        __result = "VGE_PlayerOwnedOrbitalColonyDesc".Translate();
    }
}
