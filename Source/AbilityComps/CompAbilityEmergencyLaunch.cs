using Verse;
using RimWorld;
using System.Linq;
using Verse.AI;
using RimWorld.Planet;

namespace VanillaGravshipExpanded
{
    public class CompAbilityEmergencyLaunch : CompAbilityEffect
    {
        public new CompProperties_AbilityEmergencyLaunch Props
        {
            get { return (CompProperties_AbilityEmergencyLaunch)this.props; }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            var comp = target.Thing.TryGetComp<CompPilotConsole>();
            // Just in case
            if (comp?.engine == null)
            {
                Log.Error("[VGE] Emergency launch failed, building is missing CompPilotConsole or isn't linked to an engine.");
                return;
            }

            // In case fuel was drained, or something along those lines
            if (!CanLaunch(comp))
                return;

            var tile = FindTile(comp.engine);
            if (!tile.Valid)
                return;

            comp.engine.launchInfo = new LaunchInfo
            {
                quality = 0f,
                doNegativeOutcome = true,
            };
            Find.GravshipController.InitiateTakeoff(comp.engine, tile);
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest) => base.CanApplyOn(target, dest) && Valid(target);

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!base.Valid(target, throwMessages))
            {
                return false;
            }

            Pawn pawn = parent.pawn;

            var thing = target.Thing;
            if (target.Thing is null)
            {
                return false;
            }

            var lookTargets = new LookTargets(pawn, thing);

            if (thing.Faction != Faction.OfPlayer)
            {
                if (throwMessages)
                    Messages.Message("VGE_EmergencyLaunch_MustBeOnwedByPlayer".Translate(parent.def.Named("ABILITY")), lookTargets, MessageTypeDefOf.RejectInput, false);

                return false;
            }

            if (!pawn.CanReserve(thing))
            {
                if (throwMessages)
                {
                    var reservedBy = pawn.Map.reservationManager.FirstRespectedReserver(thing, pawn);
                    Messages.Message(reservedBy == null ? "Reserved".Translate() : "ReservedBy".Translate(pawn.LabelShort, reservedBy), lookTargets, MessageTypeDefOf.RejectInput, false);
                }

                return false;
            }

            var comp = thing.TryGetComp<CompPilotConsole>();
            if (comp == null)
            {
                if (throwMessages)
                    Messages.Message("VGE_EmergencyLaunch_MustTargetPilotConsole".Translate(parent.def.Named("ABILITY")), lookTargets, MessageTypeDefOf.RejectInput, false);

                return false;
            }

            var engine = comp.engine;
            if (engine == null)
            {
                if (throwMessages)
                    Messages.Message("VGE_EmergencyLaunch_ConsoleMustBeLinkedToGravEngine".Translate(parent.def.Named("ABILITY")), lookTargets, MessageTypeDefOf.RejectInput, false);

                return false;
            }

            if (!comp.CanBeActive)
            {
                if (throwMessages)
                    Messages.Message("VGE_EmergencyLaunch_ConsoleMustBeActive".Translate(parent.def.Named("ABILITY")), lookTargets, MessageTypeDefOf.RejectInput, false);

                return false;
            }

            if (!CanLaunch(comp, lookTargets, throwMessages))
                return false;

            if (!FindTile(engine, true).Valid)
            {
                if (throwMessages)
                    Messages.Message("VGE_EmergencyLaunch_NoValidTargets".Translate(parent.def.Named("ABILITY")), lookTargets, MessageTypeDefOf.RejectInput, false);

                return false;
            }

            return true;
        }

        private static bool CanLaunch(CompPilotConsole comp, bool throwMessages = false) => CanLaunch(comp, LookTargets.Invalid, throwMessages);

        private static bool CanLaunch(CompPilotConsole comp, LookTargets lookTargets, bool throwMessages = false)
        {
            var tempCooldownTicks = comp.engine.cooldownCompleteTick;
            try
            {
                // Temporarily reset cooldown so we can check if the grav engine can launch, ignoring the cooldown requirement.
                comp.engine.cooldownCompleteTick = -1;
                var report = comp.engine.CanLaunch(comp);
                if (!report.Accepted)
                {
                    if (throwMessages && !report.Reason.NullOrEmpty())
                        Messages.Message(report.Reason, lookTargets, MessageTypeDefOf.RejectInput, false);

                    return false;
                }
            }
            finally
            {
                comp.engine.cooldownCompleteTick = tempCooldownTicks;
            }

            return true;
        }

        private static PlanetTile FindTile(Building_GravEngine engine, bool exitOnFirstTileFound = false)
        {
            var centerTile = Find.WorldGrid.Surface.GetClosestTile_NewTemp(engine.Tile);
            var range = GravshipUtility.MaxDistForFuel(engine.TotalFuel, engine.Tile.Layer, centerTile.Layer, fuelFactor: engine.FuelUseageFactor);

            if (!TileFinder.TryFindTileWithDistance(centerTile, 0, range, out var result, Validator, TileFinderMode.Near, exitOnFirstTileFound))
                return PlanetTile.Invalid;

            return result;

            bool Validator(PlanetTile tile)
            {
                if (!tile.Valid)
                    return false;
                if (tile == engine.Tile)
                    return false;
                if (!TileFinder.IsValidTileForNewSettlement(tile))
                    return false;
                return Find.WorldObjects.ObjectsAt(tile).All(x => x.GravShipCanLandOn && (!x.RequiresSignalJammerToReach || engine.HasSignalJammer));
            }
        }
    }
}