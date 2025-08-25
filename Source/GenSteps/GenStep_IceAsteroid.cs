
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
    public class GenStep_IceAsteroid : GenStep_Asteroid
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
                map.OrbitalDebris = VGEDefOf.VGE_IceAsteroid;
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
                        map.terrainGrid.SetTerrain(allCell, VGEDefOf.VGE_AsteroidIce);
                    }
                    if (num > 0.7f && num2 == 0f)
                    {
                        GenSpawn.Spawn(ThingDefOf.SolidIce, allCell, map);
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

    }
}