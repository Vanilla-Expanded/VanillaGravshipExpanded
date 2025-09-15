using RimWorld;
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
            if (t is Pawn)
                return 9;
            return 10;
        }

        public override LocalTargetInfo TryFindNewTarget()
        {
            var potentialTargets = new List<Thing>();
            foreach (IAttackTarget target in this.Map.attackTargetsCache.GetPotentialTargetsFor(this))
            {
                if (IsValidTarget(target.Thing))
                {
                    potentialTargets.Add(target.Thing);
                }
            }
            potentialTargets.SortBy(t => GetTargetPriority(t));
            foreach (Thing target in potentialTargets)
            {
                if (this.CurrentTarget.IsValid && this.CurrentTarget.Thing == target || this.AttackVerb.CanHitTarget(target))
                {
                    Log.Message($"Found target for {this}: {target}");
                    return target;
                }
            }
            
            Log.Error($"Failed to find target for {this}");
            return LocalTargetInfo.Invalid;
        }
    }
}
