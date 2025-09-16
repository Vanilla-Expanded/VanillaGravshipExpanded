using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class Building_EnemyMechTurret : Building_GravshipTurret
    {
        public override bool CanFire => true;
        public override float GravshipTargeting => 1f;
        public override bool CanSetForcedTarget => true;
        public override bool HideForceTargetGizmo => true;
        private int GetTargetPriority(Thing t)
        {
            if (t is Building_GravshipTurret)
                return 1;
            if (t.def == VGEDefOf.VGE_GiantThruster)
                return 2;
            if (t.def == ThingDefOf.LargeThruster)
                return 3;
            if (t.def == ThingDefOf.SmallThruster)
                return 4;
            if (t.def == VGEDefOf.VGE_GiantAstrofuelTank)
                return 5;
            if (t.def == VGEDefOf.LargeChemfuelTank)
                return 6;
            if (t.def == ThingDefOf.ChemfuelTank)
                return 7;
            if (t is Building_Bed)
                return 8;
            if (t is Pawn pawn && pawn.IsColonist && pawn.Downed is false)
                return 9;
            return 10;
        }

        public override LocalTargetInfo TryFindNewTarget()
        {
            var comp = this.GetComp<CompWorldArtillery>();
            if (comp != null)
            {
                var maps = Find.Maps.Where(x => GravshipHelper.GetDistance(Map.Tile, x.Tile) <= comp.Props.worldMapAttackRange).OrderBy(x => GravshipHelper.GetDistance(Map.Tile, x.Tile)).ToList();
                foreach (var map in maps)
                {
                    var target = GetTargetForMap(map);
                    if (target.IsValid)
                    {
                        Log.Message("Found target: " + target.Thing);
                        if (target.Thing.Map != Map)
                        {
                            comp.StartAttack(new GlobalTargetInfo(target.Thing), target, this);
                            return LocalTargetInfo.Invalid;
                        }
                        return target;
                    }
                }
            }
            return GetTargetForMap(Map);
        }

        private LocalTargetInfo GetTargetForMap(Map map)
        {
            var searcher = this;
            var verb = AttackVerb;
            var searcherThing = searcher;
            TargetScanFlags flags = TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
            if (!AttackVerb.ProjectileFliesOverhead())
            {
                flags |= TargetScanFlags.NeedLOSToAll;
                flags |= TargetScanFlags.LOSBlockableByGas;
            }
            if (AttackVerb.IsIncendiary_Ranged())
            {
                flags |= TargetScanFlags.NeedNonBurning;
            }
            if (IsMortar)
            {
                flags |= TargetScanFlags.NeedNotUnderThickRoof;
            }
            
            Predicate<IAttackTarget> innerValidator = delegate (IAttackTarget t)
            {
                Thing thing = t.Thing;
                if (t == searcher)
                {
                    return false;
                }
                if (thing.Map == Map)
                {
                    float num3 = verb.verbProps.EffectiveMinRange(thing, searcherThing);
                    if (num3 > 0f && (float)(searcherThing.Position - thing.Position).LengthHorizontalSquared < num3 * num3)
                    {
                        return false;
                    }
                }
                if (!searcherThing.HostileTo(thing))
                {
                    return false;
                }
                if ((flags & TargetScanFlags.NeedNotUnderThickRoof) != 0)
                {
                    RoofDef roof = thing.Position.GetRoof(thing.Map);
                    if (roof != null && roof.isThickRoof)
                    {
                        return false;
                    }
                }
                if (((flags & TargetScanFlags.NeedThreat) != 0 || (flags & TargetScanFlags.NeedAutoTargetable) != 0) && t.ThreatDisabled(searcher))
                {
                    return false;
                }
                if ((flags & TargetScanFlags.NeedAutoTargetable) != 0 && !AttackTargetFinder.IsAutoTargetable(t))
                {
                    return false;
                }
                if ((flags & TargetScanFlags.NeedActiveThreat) != 0 && !GenHostility.IsActiveThreatTo(t, searcher.Faction))
                {
                    return false;
                }
                return true;
            };
            
            var potentialTargets = new List<Thing>();
            foreach (IAttackTarget target in map.attackTargetsCache.GetPotentialTargetsFor(this))
            {
                Log.Message("Target: " + target.Thing);
                if (innerValidator(target))
                {
                    potentialTargets.Add(target.Thing);
                }
            }
            potentialTargets.SortBy(t => GetTargetPriority(t));
            foreach (Thing target in potentialTargets)
            {
                return target;
            }
            return LocalTargetInfo.Invalid;
        }
    }
}
