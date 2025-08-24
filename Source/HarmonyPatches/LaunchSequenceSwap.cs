using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    public class GravshipLaunchState
    {
        public Precept_Ritual instance;
        public TargetInfo targetInfo;
        public RitualObligation forObligation;
        public Pawn selectedPawn;
        public Dictionary<string, Pawn> forcedForRole;
        public GravshipLaunchState(Precept_Ritual instance, TargetInfo targetInfo, RitualObligation forObligation, Pawn selectedPawn, Dictionary<string, Pawn> forcedForRole, Action launchAction)
        {
            this.instance = instance;
            this.targetInfo = targetInfo;
            this.forObligation = forObligation;
            this.selectedPawn = selectedPawn;
            this.forcedForRole = forcedForRole;
        }
    }

    [HarmonyPatch(typeof(LordJob_Ritual), "ExposeData")]
    public static class LordJob_Ritual_ExposeData_Patch
    {
        public static Dictionary<LordJob_Ritual, PlanetTile> targetTile = new ();
        public static void Postfix(LordJob_Ritual __instance)
        {
            PlanetTile targetTile = PlanetTile.Invalid;
            if (!LordJob_Ritual_ExposeData_Patch.targetTile.TryGetValue(__instance, out targetTile))
            {
                targetTile = PlanetTile.Invalid;
            }
            Scribe_Values.Look(ref targetTile, "targetTile", PlanetTile.Invalid);
            if (targetTile != null)
            {
                LordJob_Ritual_ExposeData_Patch.targetTile[__instance] = targetTile;
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_BeginLordJob), "Cancel")]
    public static class Dialog_BeginLordJob_Cancel_Patch
    {
        public static void Postfix()
        {
            Dialog_BeginRitual_ShowRitualBeginWindow_Patch.state = null;
        }
    }

    [HarmonyPatch(typeof(TilePicker), "StopTargeting")]
    public static class TilePicker_StopTargeting_Patch
    {
        public static void Prefix(TilePicker __instance)
        {
            if (__instance.active && __instance.noTileChosen != null)
            {
                Dialog_BeginRitual_ShowRitualBeginWindow_Patch.state = null;
            }
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(GravshipUtility), "PreLaunchConfirmation")]
    public static class GravshipUtility_PreLaunchConfirmation_Patch
    {
        public static void Prefix(Building_GravEngine engine, ref Action launchAction)
        {
            var lordJob = engine.Map.lordManager.lords.Select(x => x.LordJob).OfType<LordJob_Ritual>().FirstOrDefault(lordJob => lordJob.ritual.def == PreceptDefOf.GravshipLaunch);
            if (lordJob is not null && LordJob_Ritual_ExposeData_Patch.targetTile.TryGetValue(lordJob, out var tile))
            {
                launchAction = delegate
                {
                    WorldComponent_GravshipController.DestroyTreesAroundSubstructure(engine.Map, engine.ValidSubstructure);
                    Find.World.renderer.wantedMode = WorldRenderMode.None;
                    engine.ConsumeFuel(tile);
                    Find.GravshipController.InitiateTakeoff(engine, tile);
                    SoundDefOf.Gravship_Launch.PlayOneShotOnCamera();
                };
            }
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(SettlementProximityGoodwillUtility), "CheckConfirmSettle")]
    public static class SettlementProximityGoodwillUtility_CheckConfirmSettle_Patch
    {
        public static PlanetTile targetTile;
        public static void Prefix(PlanetTile tile, ref Action settleAction, Action cancelAction = null, Building_GravEngine gravEngine = null)
        {
            var state = Dialog_BeginRitual_ShowRitualBeginWindow_Patch.state;
            if (gravEngine != null && state is not null)
            {
                settleAction = delegate
                {
                    CameraJumper.TryHideWorld();
                    Current.Game.CurrentMap = gravEngine.Map;
                    Find.CameraDriver.JumpToCurrentMapLoc(gravEngine.Position);
                    state.instance.ShowRitualBeginWindow(state.targetInfo, state.forObligation, state.selectedPawn, state.forcedForRole);
                    targetTile = tile;
                };
            }
        }
    }
    
    [HotSwappable]
    [HarmonyPatch(typeof(RitualBehaviorWorker_GravshipLaunch), "TryExecuteOn")]
    public static class RitualBehaviorWorker_GravshipLaunch_TryExecuteOn_Patch
    {
        public static void Postfix(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments, bool playerForced = false)
        {
            var lordJob = target.Map.lordManager.lords.Select(x => x.LordJob).OfType<LordJob_Ritual>().FirstOrDefault(lordJob => lordJob.ritual.def == PreceptDefOf.GravshipLaunch);
            if (lordJob is not null)
            {
                LordJob_Ritual_ExposeData_Patch.targetTile[lordJob] = SettlementProximityGoodwillUtility_CheckConfirmSettle_Patch.targetTile;
            }
        }
    }
    
    [HotSwappable]
    [HarmonyPatch(typeof(Precept_Ritual), "ShowRitualBeginWindow")]
    public static class Dialog_BeginRitual_ShowRitualBeginWindow_Patch
    {
        public static GravshipLaunchState state;
        public static bool Prefix(Precept_Ritual __instance, TargetInfo targetInfo, RitualObligation forObligation = null, Pawn selectedPawn = null, Dictionary<string, Pawn> forcedForRole = null)
        {
            if (__instance.def == PreceptDefOf.GravshipLaunch)
            {
                if (state is null)
                {
                    var comp = targetInfo.Thing.TryGetComp<CompPilotConsole>();
                    state = new(__instance, targetInfo, forObligation, selectedPawn, forcedForRole, null);
                    comp.StartChoosingDestination_NewTemp();
                    return false;
                }
            }
            return true;
        }
    }
}
