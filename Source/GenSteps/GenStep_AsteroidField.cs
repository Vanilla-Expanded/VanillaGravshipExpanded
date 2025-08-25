
using RimWorld;
using RimWorld.Planet;
using RimWorld.SketchGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Verse;
using Verse.Noise;
namespace VanillaGravshipExpanded
{
    public class GenStep_AsteroidField : GenStep_Asteroid
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
                    if (num < 0.35f)
                    {

                        map.terrainGrid.SetTerrain(allCell, ThingDefOf.Vacstone.building.naturalTerrain);
                    }

                    if (num < 0.3f)
                    {
                        GenSpawn.Spawn(ThingDefOf.Vacstone, allCell, map);
                        if (num2 != 0f)
                        {
                            map.roofGrid.SetRoof(allCell, RoofDefOf.RoofRockThin);
                        }
                    }


                }

            }
        }

      

        public override ModuleBase ConfigureNoise(Map map, GenStepParams parms)
        {
            ModuleBase input;

            input = new Voronoi2D(0.04f,Rand.Int,0.0f,1.0f,staggered: true);

            input = MapNoiseUtility.AddDisplacementNoise(input, 0.03f, 8f, 2);

            ModuleBase input2 = new DistFromPoint((float)map.Size.x * 0.95f);
            input2 = new Scale(1, 1.0, 1.0, input2);
            input2 = new Translate(input: new Rotate(0.0, 0.3, 0.0, input2), x: (float)(-map.Size.x) / 2f, y: 0.0, z: (float)(-map.Size.z) / 2f);
            input2 = MapNoiseUtility.AddDisplacementNoise(input2, 0.015f, 35f, 4, map.Tile.tileId);

            input = new Max(input2, input);


            NoiseDebugUI.StoreNoiseRender(input, "Asteroid");
            return input;
        }

       
    }
}