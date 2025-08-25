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
    public class GenStep_PorousAsteroid : GenStep_Asteroid
    {
       

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
                HashSet<IntVec3> mainIsland = new HashSet<IntVec3>();
                map.floodFiller.FloodFill(map.Center, (IntVec3 x) => x.GetTerrain(map) != TerrainDefOf.Space, delegate (IntVec3 x)
                {
                    mainIsland.Add(x);
                });
                foreach (IntVec3 allCell2 in map.AllCells)
                {
                    if (!mainIsland.Contains(allCell2))
                    {
                        map.terrainGrid.SetTerrain(allCell2, TerrainDefOf.Space);
                        map.roofGrid.SetRoof(allCell2, null);
                        foreach (Thing item in allCell2.GetThingList(map).ToList())
                        {
                            item.Destroy();
                        }
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

            ModuleBase rawHoles = new Perlin(frequency: 0.1,lacunarity: 2.0,persistence: 0.5,octaves: 2,seed: Rand.Int,quality: QualityMode.Medium);
            rawHoles = new ScaleBias(-1, 1, rawHoles);
            ModuleBase porousAsteroid = new Min(input, rawHoles);
            NoiseDebugUI.StoreNoiseRender(porousAsteroid, "Porous asteroid");

            return porousAsteroid;
        }

       
    }
}
