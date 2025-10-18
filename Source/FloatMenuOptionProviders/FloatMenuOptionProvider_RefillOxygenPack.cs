using System.Collections.Generic;
using RimWorld;
using RimWorld.Utility;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

public class FloatMenuOptionProvider_RefillOxygenPack : FloatMenuOptionProvider
{
    private static readonly IEnumerable<FloatMenuOption> EmptyOptions = [];

    public override bool Drafted => true;
    public override bool Undrafted => true;
    public override bool Multiselect => false;

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
        => GetOptionsFor(context, context.FirstSelectedPawn, clickedThing);

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
        => clickedPawn != context.FirstSelectedPawn ? GetOptionsFor(context, clickedPawn, null) : EmptyOptions;

    private static IEnumerable<FloatMenuOption> GetOptionsFor(FloatMenuContext context, Pawn targetPawn, Thing clickedThing)
    {
        foreach (var oxygenProvider in JobGiver_RefuelOxygenPack.FindOxygenProviders(targetPawn, true))
        {
            if (clickedThing != null && oxygenProvider.AmmoDef != clickedThing.def)
                continue;

            var text = $"{"VGE_Refill".Translate(oxygenProvider.parent.Named("GEAR"), oxygenProvider.AmmoDef.Named("AMMO"))} ({oxygenProvider.LabelRemaining})";
            List<Thing> chosenAmmo;

            if (!context.FirstSelectedPawn.CanReach(clickedThing ?? targetPawn, PathEndMode.ClosestTouch, Danger.Deadly))
                yield return new FloatMenuOption($"{text}: {"NoPath".Translate().CapitalizeFirst()}", null);
            else if (!oxygenProvider.NeedsReload(true))
                yield return new FloatMenuOption($"{text}: {"ReloadFull".Translate()}", null);
            else if ((chosenAmmo = ReloadableUtility.FindEnoughAmmo(context.FirstSelectedPawn, clickedThing?.Position ?? context.FirstSelectedPawn.Position, oxygenProvider, true)) == null)
                yield return new FloatMenuOption($"{text}: {"ReloadNotEnough".Translate()}", null);
            else if (context.FirstSelectedPawn.carryTracker.AvailableStackSpace(oxygenProvider.AmmoDef) < oxygenProvider.MinAmmoNeeded(true))
                yield return new FloatMenuOption($"{text}: {"VGE_RefillCannotCarryEnough".Translate(oxygenProvider.AmmoDef.Named("AMMO"))}", null);
            else
            {
                void Reload() => context.FirstSelectedPawn.jobs.TryTakeOrderedJob(JobGiver_RefuelOxygenPack.MakeReloadJob(oxygenProvider, chosenAmmo));
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, Reload), context.FirstSelectedPawn, clickedThing ?? targetPawn);
            }
        }
    }
}