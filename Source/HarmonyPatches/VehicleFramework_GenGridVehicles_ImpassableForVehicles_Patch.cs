using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch("GenGridVehicles", "ImpassableForVehicles")]
public static class VehicleFramework_GenGridVehicles_ImpassableForVehicles_Patch
{
    private static bool Prepare(MethodBase methodBase) => methodBase != null || ModsConfig.IsActive("SmashPhil.VehicleFramework") || ModsConfig.IsActive("SmashPhil.VehicleFramework_steam");

    private static void Postfix(ThingDef __0, ref bool __result)
    {
        if (__result && __0.thingClass.SameOrSubclassOf(typeof(Building_VacBarrier)))
            __result = false;
    }
}