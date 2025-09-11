using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.Graphic), MethodType.Getter)]
public static class Building_GravEngine_Graphic_Patch
{
    private static bool Prefix(Building_GravEngine __instance, ref Graphic __result)
    {
        var styleDef = __instance.StyleDef;
        if (styleDef?.Graphic != null)
            __result = __instance.styleGraphicInt ??= styleDef.graphicData != null ? styleDef.graphicData.GraphicColoredFor(__instance) : styleDef.Graphic;
        else
            __result = __instance.DefaultGraphic;

        // If graphic is not grav engine graphic, and we're on cooldown, use the default behavior of hardcoded graphic
        if (__result is not IGravEngineGraphic && Find.TickManager.TicksGame < __instance.cooldownCompleteTick)
            __result = Building_GravEngine.OnCooldownGraphic;

        return false;
    }
}