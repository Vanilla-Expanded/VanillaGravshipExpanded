using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded
{
    public class CompHackableEscapePod : CompHackable
    {
        public override void OnHacked(Pawn hacker = null, bool suppressMessages = false)
        {
            base.OnHacked(hacker, suppressMessages);
            var position = this.parent.Position;
            var map = this.parent.Map;
            SpawnRandomContents();
            Thing slag = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);
            GenPlace.TryPlaceThing(slag, position, map, ThingPlaceMode.Near);
            this.parent.Destroy(DestroyMode.Vanish);
        }

        private void SpawnRandomContents()
        {
            var outcomes = new List<(float weight, System.Action action)>
            {
                (35f, SpawnResourceCache),
                (20f, SpawnFriendlySurvivor),
                (20f, SpawnHostileStowaway),
                (20f, SpawnInsectoidBreach),
                (5f, SpawnEmptyPod)
            };

            outcomes.RandomElementByWeight(x => x.weight).action();
        }

        private void SpawnResourceCache()
        {
            List<ThingDef> resources = new List<ThingDef> { ThingDefOf.Steel, ThingDefOf.Plasteel, ThingDefOf.Silver, ThingDefOf.Chemfuel };
            ThingDef resourceType = resources.RandomElement();
            int stackCount = (int)(250f / resourceType.BaseMarketValue);
            if (stackCount < 1)
            {
                stackCount = 1;
            }

            Thing resource = ThingMaker.MakeThing(resourceType);
            resource.stackCount = stackCount;
            GenPlace.TryPlaceThing(resource, this.parent.Position, this.parent.Map, ThingPlaceMode.Near);
            if (Rand.Chance(0.25f))
            {
                Thing component = ThingMaker.MakeThing(ThingDefOf.ComponentSpacer);
                GenPlace.TryPlaceThing(component, this.parent.Position, this.parent.Map, ThingPlaceMode.Near);
            }
        }

        private void SpawnFriendlySurvivor()
        {
            PawnGenerationRequest request = new PawnGenerationRequest(
                PawnKindDefOf.Villager,
                null,
                PawnGenerationContext.NonPlayer,
                -1,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: true,
                canGeneratePawnRelations: false,
                mustBeCapableOfViolence: false,
                colonistRelationChanceFactor: 0f
            );

            Pawn pawn = PawnGenerator.GeneratePawn(request);
            GenPlace.TryPlaceThing(pawn, this.parent.Position, this.parent.Map, ThingPlaceMode.Near);
            ApplyRandomInjuries(pawn);
            HealthUtility.DamageUntilDowned(pawn, false);
        }

        private void SpawnHostileStowaway()
        {
            PawnGenerationRequest request = new PawnGenerationRequest(
                PawnKindDefOf.Pirate,
                Faction.OfPirates,
                PawnGenerationContext.NonPlayer,
                -1,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: false,
                canGeneratePawnRelations: false,
                mustBeCapableOfViolence: true
            );

            Pawn pawn = PawnGenerator.GeneratePawn(request);
            GenPlace.TryPlaceThing(pawn, this.parent.Position, this.parent.Map, ThingPlaceMode.Near);
        }

        private void SpawnInsectoidBreach()
        {
            SpawnInsect(PawnKindDefOf.Megaspider);
            SpawnInsect(PawnKindDefOf.Spelopede);
            for (int i = 0; i < 2; i++)
            {
                SpawnInsect(PawnKindDefOf.Megascarab);
            }
        }

        private void SpawnInsect(PawnKindDef pawnKind)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(pawnKind);
            GenPlace.TryPlaceThing(pawn, this.parent.Position, this.parent.Map, ThingPlaceMode.Near);
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
        }

        private void SpawnEmptyPod()
        {
        }

        private void ApplyRandomInjuries(Pawn pawn)
        {
            if (pawn.health == null || pawn.health.hediffSet == null)
                return;
            int injuryCount = Rand.RangeInclusive(1, 4);
            for (int i = 0; i < injuryCount; i++)
            {
                BodyPartRecord part = pawn.health.hediffSet.GetRandomNotMissingPart(DamageDefOf.Cut);
                if (part != null)
                {
                    int damage = Rand.RangeInclusive(5, 15);
                    DamageInfo damageInfo = new DamageInfo(DamageDefOf.Cut, damage, 999f, -1f, null, part);
                    pawn.TakeDamage(damageInfo);
                }
            }
        }
    }
}
