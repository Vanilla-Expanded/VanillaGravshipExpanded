using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Utility;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

public class JobGiver_RefillOxygenPack : ThinkNode_JobGiver
{
    public override float GetPriority(Pawn pawn)
    {
        // Same as JobGiver_Reload
        if (!pawn.Map.Biome.inVacuum)
            return 5.9f;
        // Same priority as getting hemogen, lower priority than taking drugs for dependency/need or getting food.
        return 9.1f;
    }

    public override Job TryGiveJob(Pawn pawn)
    {
        if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            return null;

        var oxygenProvider = FindOxygenProvider(pawn, false);
        if (oxygenProvider == null)
            return null;
        if (pawn.carryTracker.AvailableStackSpace(oxygenProvider.AmmoDef) < oxygenProvider.MinAmmoNeeded(true))
            return null;

        var list = ReloadableUtility.FindEnoughAmmo(pawn, pawn.Position, oxygenProvider, false);
        if (list.NullOrEmpty())
            return null;

        return MakeRefillJob(oxygenProvider, list);
    }

    // Sadly, we can't use JobGiver_Reload as it specifically requires CompApparelReloadable or CompEquippableAbilityReloadable, which we can't use.
    public static Job MakeRefillJob(CompApparelOxygenProvider reloadable, List<Thing> chosenAmmo)
    {
        var job = JobMaker.MakeJob(VGEDefOf.VGE_ReplenishOxygenPack, reloadable.ReloadableThing, null, reloadable.Wearer);
        job.targetQueueB = chosenAmmo.Select(x => new LocalTargetInfo(x)).ToList();
        job.count = chosenAmmo.Sum(t => t.stackCount);
        job.count = Math.Min(job.count, reloadable.MaxAmmoNeeded(true));
        return job;
    }

    public static CompApparelOxygenProvider FindOxygenProvider(Pawn pawn, bool allowForceReload) => FindOxygenProviders(pawn, allowForceReload).FirstOrDefault();

    public static IEnumerable<CompApparelOxygenProvider> FindOxygenProviders(Pawn pawn, bool allowForceReload)
    {
        if (pawn.apparel != null)
        {
            foreach (var apparel in pawn.apparel.WornApparel)
            {
                var comp = apparel.GetComp<CompApparelOxygenProvider>();
                if (comp != null && comp.NeedsReload(allowForceReload))
                    yield return comp;
            }
        }
    }
}