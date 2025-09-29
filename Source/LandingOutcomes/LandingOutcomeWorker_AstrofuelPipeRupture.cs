using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace VanillaGravshipExpanded
{
    public class LandingOutcomeWorker_AstrofuelPipeRupture : LandingOutcomeWorker_GravshipBase
    {
        public LandingOutcomeWorker_AstrofuelPipeRupture(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            return gravship.Things.Any(t => t.def == VGEDefOf.VGE_AstrofuelPipe);
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            var astrofuelPipes = gravship.Things
                .Where(t => t.def == VGEDefOf.VGE_AstrofuelPipe)
                .ToList();
            var selectedPipe = astrofuelPipes.RandomElement();
            var map = gravship.Engine.Map;
            int puddleCount = Rand.RangeInclusive(3, 5);
            for (int i = 0; i < puddleCount; i++)
            {
                var spawnCell = selectedPipe.Position + GenRadial.RadialPattern[Rand.RangeInclusive(1, 8)];
                if (spawnCell.InBounds(map) && spawnCell.GetTerrain(map) != TerrainDefOf.Space)
                {
                    Thing puddle = ThingMaker.MakeThing(VGEDefOf.VGE_Filth_Astrofuel);
                    GenPlace.TryPlaceThing(puddle, spawnCell, map, ThingPlaceMode.Near);
                }
            }
            int damageAmount = (int)(selectedPipe.MaxHitPoints * 0.5f);
            selectedPipe.TakeDamage(new DamageInfo(VGEDefOf.VGE_AstrofireDamage, damageAmount));
            int fireCount = Rand.RangeInclusive(2, 3);
            for (int i = 0; i < fireCount; i++)
            {
                var spawnCell = selectedPipe.Position + GenRadial.RadialPattern[Rand.RangeInclusive(1, 8)];
                if (spawnCell.InBounds(map) && spawnCell.GetTerrain(map) != TerrainDefOf.Space)
                {
                    AstrofireUtility.TryStartAstrofireIn(spawnCell, map, Rand.Range(0.5f, 1.5f), null);
                }
            }
            SendStandardLetter(gravship.Engine, null, new LookTargets(selectedPipe));
        }
    }
}
