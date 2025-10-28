using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.TickRare))]
public static class Pawn_TickRare_Patch
{
    private const float ChangePerInterval = (float)GenTicks.TickRareInterval / GenTicks.TicksPerRealSecond * 0.01f;

    private static void Postfix(Pawn __instance)
    {
        var map = __instance.Map;
        if (map is { Biome.inVacuum: true } && __instance.def.race.Humanlike && !__instance.RaceProps.IsMechanoid && (!__instance.IsMutant || __instance.mutant.Def.breathesAir))
        {
            var resistance = __instance.GetStatValue(StatDefOf.VacuumResistance, cacheStaleAfterTicks: 60);
            if (resistance >= 1f)
                return;

            var room = __instance.GetRoom();
            if (room is not { Vacuum: < 1f })
                return;

            var change = 100f / room.CellCount * (1f - resistance) * __instance.BodySize * (ChangePerInterval * 0.2f);
            room.Vacuum += change;
        }
    }
}