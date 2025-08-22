using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaGravshipExpanded
{
    public class Verb_ShootWithSmoke : Verb_ShootWithWorldTargeting
    {
        public override bool TryCastShot()
        {
            bool result = base.TryCastShot();
            if (result)
            {
                for (var i = 0; i < 3; i++)
                {
                    ThrowSmoke(caster.Position.ToVector3Shifted(), caster.Map, 1.5f);
                }
            }
            return result;
        }

        public static void ThrowSmoke(Vector3 loc, Map map, float size)
        {
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(VGEDefOf.VGE_GaussSmoke, null);
            moteThrown.Scale = Rand.Range(1.5f, 2.5f) * size;
            moteThrown.rotationRate = Rand.Range(-30f, 30f);
            moteThrown.exactPosition = loc;
            moteThrown.SetVelocity((float)Rand.Range(30, 40), Rand.Range(0.5f, 0.7f));
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map, WipeMode.Vanish);
        }
    }
}
