using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    public class Projectile_SpaceDebris : Projectile_Space
    {
        protected override void TryDropLoot()
        {
            if (Rand.Chance(0.25f))
            {
                Thing slag = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);
                GenPlace.TryPlaceThing(slag, this.Position, this.Map, ThingPlaceMode.Near);
            }
        }
    }
}
