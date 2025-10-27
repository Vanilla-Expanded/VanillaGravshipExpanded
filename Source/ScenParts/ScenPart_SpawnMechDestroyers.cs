using KCSG;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class ScenPart_SpawnMechDestroyers : ScenPart
    {
        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            if (map.IsStartingMap is false) return;
            List<KCSG.StructureLayoutDef> destroyerLayouts = new List<KCSG.StructureLayoutDef>
            {
                VGEDefOf.VGE_MechOrbitalDestroyer_Alpha,
                VGEDefOf.VGE_MechOrbitalDestroyer_Beta,
                VGEDefOf.VGE_MechOrbitalDestroyer_Gamma
            };

            IntVec3 playerStartSpot = MapGenerator.PlayerStartSpot;
            int mapSizeX = map.Size.x;
            int mapSizeZ = map.Size.z;
            List<CellRect> placedRects = new List<CellRect>();

            for (int i = 0; i < 4; i++)
            {
                KCSG.StructureLayoutDef layout = destroyerLayouts.RandomElement();
                int layoutWidth = layout.Sizes.x;
                int layoutHeight = layout.Sizes.z;

                IntVec3 spawnSpot = IntVec3.Invalid;
                int attempts = 0;
                while (!spawnSpot.IsValid && attempts < 200)
                {
                    int x = Rand.Range(20, mapSizeX - layoutWidth - 20);
                    int z = Rand.Range(20, mapSizeZ - layoutHeight - 20);
                    CellRect rect = new CellRect(x, z, layoutWidth, layoutHeight);
                    if (rect.Contains(playerStartSpot) || rect.Overlaps(CellRect.CenteredOn(playerStartSpot, 50)))
                    {
                        attempts++;
                        continue;
                    }
                    bool overlapsWithExisting = false;
                    foreach (CellRect placedRect in placedRects)
                    {
                        CellRect expandedRect = new CellRect(placedRect.minX - 5, placedRect.minZ - 5, placedRect.Width + 10, placedRect.Height + 10);
                        if (rect.Overlaps(expandedRect))
                        {
                            overlapsWithExisting = true;
                            break;
                        }
                    }

                    if (overlapsWithExisting)
                    {
                        attempts++;
                        continue;
                    }

                    bool canSpawn = true;
                    foreach (IntVec3 cell in rect)
                    {
                        if (!cell.InBounds(map) || cell.Fogged(map) || cell.Roofed(map))
                        {
                            canSpawn = false;
                            break;
                        }
                    }
                    if (canSpawn)
                    {
                        spawnSpot = rect.Min;
                        placedRects.Add(rect);
                    }
                    attempts++;
                }

                if (spawnSpot.IsValid)
                {
                    CellRect spawnRect = new CellRect(spawnSpot.x, spawnSpot.z, layoutWidth, layoutHeight);
                    LayoutUtils.CleanRect(layout, map, spawnRect, true);
                    layout.Generate(spawnRect, map, Faction.OfMechanoids);
                    foreach (IntVec3 cell in spawnRect)
                    {
                        foreach (Thing thing in map.thingGrid.ThingsListAt(cell))
                        {
                            if (thing is Building_TurretGun turret && turret.Faction == Faction.OfMechanoids)
                            {
                                var dormantComp = turret.GetComp<CompCanBeDormant>();
                                if (dormantComp != null)
                                {
                                    dormantComp.wakeUpOnTick = Find.TickManager.TicksGame + 30000;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
