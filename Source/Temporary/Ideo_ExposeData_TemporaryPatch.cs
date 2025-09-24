using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

// At the moment (1.6.4566), vanilla is adding missing non-Ideo special rituals.
// However, the game only does that for rituals that use Precept_Ritual - but not its subclasses.
// This causes issue with gravship launch (and related rituals), since those use a subclass
// of Precept_Ritual - they all use Precept_GravshipLaunch. Because of that, loading a save file
// where one of those rituals is missing (including Odyssey's gravship launch) the game will fail
// to add those to the game. The issue was reported to Ludeon (on RimWorld official development Discord),
// so hopefully it is fixed relatively soon. For now, we need to use a workaround to do exactly that.
// If this gets fixed in vanilla RW then we'll be able to remove this safely.
[HarmonyPatch(typeof(Ideo), nameof(Ideo.ExposeData))]
public class Ideo_ExposeData_TemporaryPatch
{
    private static void Postfix(Ideo __instance)
    {
        AddRitualIfNeeded(PreceptDefOf.GravshipLaunch);
        AddRitualIfNeeded(VGEDefOf.VGE_GravjumperLaunch);
        AddRitualIfNeeded(VGEDefOf.VGE_GravhulkLaunch);

        void AddRitualIfNeeded(PreceptDef p)
        {
            if ((p.takeNameFrom == null || __instance.precepts.Any(x => x.def == p.takeNameFrom)) && !__instance.precepts.Any(x => x.def.ritualPatternBase == p.ritualPatternBase))
            {
                var precept = PreceptMaker.MakePrecept(p);
                __instance.AddPrecept(precept, true, null, p.ritualPatternBase);
                Log.Warning("A hidden ritual precept was missing, adding: " + precept.def.LabelCap);
            }
        }
    }
}