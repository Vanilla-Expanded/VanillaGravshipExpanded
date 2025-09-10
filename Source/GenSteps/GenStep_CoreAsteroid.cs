
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
    public class GenStep_CoreAsteroid : GenStep_Asteroid
    {


        public override void Generate(Map map, GenStepParams parms)
        {
            if (ModLister.CheckOdyssey("Asteroid"))
            {
                GenerateAsteroidElevation(map, parms);
                SpawnAsteroidInternal(map);
                SpawnOresInternal(map, parms);
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
                        map.terrainGrid.SetTerrain(allCell, VGEDefOf.VGE_Compressed_Vacstone_Floor);
                    }
                    if (num > 0.7f && num2 == 0f)
                    {
                        GenSpawn.Spawn(VGEDefOf.VGE_Compressed_Vacstone, allCell, map);
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

        private void SpawnOresInternal(Map map, GenStepParams parms)
        {
            ThingDef thingDef = ((SpaceMapParent)map.ParentHolder).preciousResource;
           
            int randomInRange = numChunks.RandomInRange;
           
            GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
            genStep_ScatterLumpsMineable.count = 1;
            genStep_ScatterLumpsMineable.nearMapCenter=true;
            genStep_ScatterLumpsMineable.forcedDefToScatter = thingDef;
            genStep_ScatterLumpsMineable.forcedLumpSize = randomInRange;
            genStep_ScatterLumpsMineable.Generate(map, parms);
        }


    }
}