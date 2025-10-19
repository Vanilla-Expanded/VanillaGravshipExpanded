using System.Collections.Generic;
using RimWorld;
using RimWorld.Utility;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

public class JobDriver_RefillOxygenPack : JobDriver
{
    private const TargetIndex GearInd = TargetIndex.A;
    private const TargetIndex FuelInd = TargetIndex.B;
    private const TargetIndex PawnInd = TargetIndex.C;

    private Thing Gear => job.GetTarget(GearInd).Thing;
    private List<LocalTargetInfo> Fuel => job.GetTargetQueue(FuelInd);
    private Pawn TargetPawn => job.GetTarget(PawnInd).Pawn;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        pawn.ReserveAsManyAsPossible(Fuel, job);
        return true;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
			var gear = Gear;
			var oxygenProvider = gear?.TryGetComp<CompApparelOxygenProvider>();
			var target = TargetPawn;

			this.FailOn(() => oxygenProvider == null);
			this.FailOn(() => ReloadableUtility.OwnerOf(oxygenProvider) != target);
			this.FailOn(() => !oxygenProvider!.NeedsReload(true));

			this.FailOnDestroyedOrNull(GearInd);
			this.FailOnIncapable(PawnCapacityDefOf.Manipulation);

			if (target != pawn)
			{
				this.FailOnDespawnedNullOrForbidden(PawnInd);
				this.FailOnAggroMentalState(PawnInd);
				this.FailOnCannotReach(PawnInd, PathEndMode.Touch);
				AddEndCondition(() =>
				{
					if (!pawn.jobs.curJob.GetTarget(PawnInd).Pawn.CanCasuallyInteractNow(canInteractWhileSleeping: true, canInteractWhileDrafted: true))
						return JobCondition.Incompletable;
					return JobCondition.Ongoing;
				});
			}

			var getNextIngredient = Toils_General.Label();
			yield return getNextIngredient;
			foreach (var refill in RefillAsMuchAsPossible(oxygenProvider, target))
				yield return refill;

			yield return Toils_JobTransforms.ExtractNextTargetFromQueue(FuelInd);
			yield return Toils_Goto.GotoThing(FuelInd, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(FuelInd).FailOnSomeonePhysicallyInteracting(FuelInd);
			yield return Toils_Haul.StartCarryThing(FuelInd, false, true).FailOnDestroyedNullOrForbidden(FuelInd);
			yield return Toils_Jump.JumpIf(getNextIngredient, () => !Fuel.NullOrEmpty());
			foreach (var refillFinal in RefillAsMuchAsPossible(oxygenProvider, target))
				yield return refillFinal;

			var dropFuel = ToilMaker.MakeToil();
			dropFuel.initAction = () =>
			{
				var carriedThing = pawn.carryTracker.CarriedThing;
				if (carriedThing is { Destroyed: false })
					pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
			};
			dropFuel.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return dropFuel;
    }

    private IEnumerable<Toil> RefillAsMuchAsPossible(CompApparelOxygenProvider oxygenProvider, Pawn target)
    {
        var done = Toils_General.Label();

        yield return Toils_Jump.JumpIf(done, () => pawn.carryTracker.CarriedThing == null || pawn.carryTracker.CarriedThing.stackCount < oxygenProvider.MinAmmoNeeded(true));

        if (target == pawn)
        {
			yield return Toils_General.Wait(oxygenProvider.BaseReloadTicks).WithProgressBarToilDelay(TargetIndex.A);
        }
        else
        {
	        yield return Toils_Goto.GotoThing(PawnInd, PathEndMode.Touch);
	        yield return Toils_General.WaitWith(PawnInd, oxygenProvider.BaseReloadTicks, true, true, true, PawnInd);
        }

        var refill = ToilMaker.MakeToil();
        refill.initAction = () => oxygenProvider.ReloadFrom(pawn.carryTracker.CarriedThing);
        refill.defaultCompleteMode = ToilCompleteMode.Instant;

        yield return refill;
        yield return done;
    }

   //  public override string GetReport()
   //  {
	  //   var target = TargetPawn;
	  //   if (pawn == target)
			// return base.GetReport();
	  //   return "VGE_RefillOxygenOther".Translate(Gear.Named("GEAR"), target.Named("TARGET"));
   //  }
}