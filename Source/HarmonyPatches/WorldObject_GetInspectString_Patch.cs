using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(WorldObject), nameof(WorldObject.GetInspectString))]
public static class WorldObject_GetInspectString_Patch
{
    public static void Postfix(WorldObject __instance, ref string __result)
    {
        if (__instance.def.requiresSignalJammerToReach is false || __instance.Faction != Faction.OfPlayer || __instance.RequiresSignalJammerToReach)
            return;

        var line = "RequiresSignalJammer".Translate().RawText;
        __result = __result.Replace(line + "\r\n", "").Replace(line + "\n", "").Replace(line, "");
    }
}
