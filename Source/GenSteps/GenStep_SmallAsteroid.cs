
using RimWorld;
using Verse;
namespace VanillaGravshipExpanded
{
    public class GenStep_SmallAsteroid : GenStep_Asteroid
    {
        public override float Radius => 0.1f;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (ModLister.CheckOdyssey("Asteroid"))
            {
                GenerateAsteroidElevation(map, parms);
                GenerateCaveElevation(map, parms);
                SpawnAsteroid(map);
                SpawnOres(map, parms);
                if (Rand.Chance(ruinsChance))
                {
                    GenerateRuins(map, parms);
                }
                if (Rand.Chance(archeanTreeChance))
                {
                    GenerateArcheanTree(map, parms);
                }
                map.OrbitalDebris = null;
            }
        }
    }
}
