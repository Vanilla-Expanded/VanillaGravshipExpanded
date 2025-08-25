using RimWorld;
using RimWorld.Planet;
using RimWorld.SketchGen;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace VanillaGravshipExpanded
{
    public class GenStep_ShatteredAsteroid : GenStep_Asteroid
    {
       
        protected virtual float GetCurveFrequency => 0.007f;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (ModLister.CheckOdyssey("Asteroid"))
            {
                GenerateAsteroidElevation(map, parms);
                GenerateCaveElevation(map, parms);
                SpawnAsteroidInternal(map);
                SpawnOres(map, parms);
                if (Rand.Chance(ruinsChance))
                {
                    GenerateRuins(map, parms);
                }
                if (Rand.Chance(archeanTreeChance))
                {
                    GenerateArcheanTree(map, parms);
                }
                map.OrbitalDebris = OrbitalDebrisDefOf.Asteroid;
            }
        }

        private static void SpawnAsteroidInternal(Map map)
        {
            using (map.pathing.DisableIncrementalScope())
            {
                foreach (IntVec3 allCell in map.AllCells)
                {
                    float num = MapGenerator.Elevation[allCell];
                    float num2 = MapGenerator.Caves[allCell];
                    if (num > 0.5f)
                    {
                        map.terrainGrid.SetTerrain(allCell, ThingDefOf.Vacstone.building.naturalTerrain);
                    }
                    if (num > 0.7f && num2 == 0f)
                    {
                        GenSpawn.Spawn(ThingDefOf.Vacstone, allCell, map);
                    }
                    if (num > 0.7f)
                    {
                        map.roofGrid.SetRoof(allCell, RoofDefOf.RoofRockThin);
                    }
                }
               
            }
        }

      
        public override ModuleBase ConfigureNoise(Map map, GenStepParams parms)
        {
            ModuleBase input = new DistFromPoint((float)map.Size.x * Radius);
            input = new ScaleBias(-1.0, 1.0, input);
            input = new Scale(0.64999997615814209, 1.0, 1.0, input);
            input = new Rotate(0.0, Rand.Range(0f, 360f), 0.0, input);
            input = new Translate(-map.Center.x, 0.0, -map.Center.z, input);
            NoiseDebugUI.StoreNoiseRender(input, "Base asteroid shape");
            input = new Blend(new Perlin(0.0060000000521540642, 2.0, 2.0, 3, Rand.Int, QualityMode.Medium), input, new Const(0.800000011920929));
            input = new Blend(new Perlin(0.05000000074505806, 2.0, 0.5, 6, Rand.Int, QualityMode.Medium), input, new Const(0.85000002384185791));
            input = new Power(input, new Const(0.20000000298023224));

            ModuleBase chasmNoise = new DistFromPoint((float)map.Size.x * 0.4f);
            chasmNoise = new Scale(1.3, 1.0, 1.0, chasmNoise);
            chasmNoise = new Rotate(0.0, Rand.Range(0f, 360f), 0.0, chasmNoise);
            chasmNoise = new Translate(-map.Center.x, 0.0, -map.Center.z, chasmNoise);         
            chasmNoise = MapNoiseUtility.AddDisplacementNoise(chasmNoise, 0.015f, 40f);

            ModuleBase chasmNoise2 = new DistFromPoint((float)map.Size.x * 0.4f);
            chasmNoise2 = new Scale(1.3, 1.0, 1.0, chasmNoise);
            chasmNoise2 = new Rotate(0.0, Rand.Range(0f, 360f), 0.0, chasmNoise);
            chasmNoise2 = new Translate(-map.Center.x, 0.0, -map.Center.z, chasmNoise);
            chasmNoise2 = MapNoiseUtility.AddDisplacementNoise(chasmNoise, 0.015f, 40f);

            ModuleBase shatteredAsteroid = new Min(input, chasmNoise);
            shatteredAsteroid = new Min(shatteredAsteroid, chasmNoise2);

            NoiseDebugUI.StoreNoiseRender(shatteredAsteroid, "shattered asteroid");

            return shatteredAsteroid;
        }

      
    }
}
