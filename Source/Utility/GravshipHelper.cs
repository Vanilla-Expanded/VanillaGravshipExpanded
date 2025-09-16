using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;
using System.Collections.Generic;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public static class GravshipHelper
    {
        private static readonly SimpleCurve LaunchBoonChanceFromQualityCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(0.5f, 0.03f),
            new CurvePoint(1f, 0.3f)
        };
        public static readonly Material TileMaterial = MaterialPool.MatFrom("Things/Terrain/Substructure/SubscaffoldingTile", ShaderDatabase.Cutout);
        public static readonly Material MaskOverlayMaterial = SolidColorMaterials.NewSolidColorMaterial(new Color(1f, 1f, 0f), ShaderDatabase.TransparentPostLight);
        public static void AddScaffoldQuad(LayerSubMesh subMesh, IntVec3 cell, float y)
        {
            int count = subMesh.verts.Count;
            subMesh.verts.Add(new Vector3(cell.x, y, cell.z));
            subMesh.verts.Add(new Vector3(cell.x, y, cell.z + 1));
            subMesh.verts.Add(new Vector3(cell.x + 1, y, cell.z + 1));
            subMesh.verts.Add(new Vector3(cell.x + 1, y, cell.z));
            subMesh.uvs.Add(new Vector2(0f, 0f));
            subMesh.uvs.Add(new Vector2(0f, 1f));
            subMesh.uvs.Add(new Vector2(1f, 1f));
            subMesh.uvs.Add(new Vector2(1f, 0f));
            subMesh.tris.Add(count);
            subMesh.tris.Add(count + 1);
            subMesh.tris.Add(count + 2);
            subMesh.tris.Add(count);
            subMesh.tris.Add(count + 2);
            subMesh.tris.Add(count + 3);
        }

        public static bool IsSustructureOrScaffold(this TerrainDef terrainDef)
        {
            return terrainDef.HasTag("Substructure") || terrainDef == VGEDefOf.VGE_DamagedSubstructure
            || terrainDef == VGEDefOf.VGE_GravshipSubscaffold;
        }

        public static bool IsScaffold(this TerrainDef terrainDef)
        {
            return terrainDef == VGEDefOf.VGE_GravshipSubscaffold;
        }

        public static void RegenerateScaffoldLayer(SectionLayer sectionLayer, Material material, AltitudeLayer altitudeLayer, int? renderLayer = null)
        {
            sectionLayer.ClearSubMeshes(MeshParts.All);
            TerrainGrid terrainGrid = sectionLayer.Map.terrainGrid;
            foreach (IntVec3 cell in sectionLayer.section.CellRect)
            {
                if (terrainGrid.FoundationAt(cell).IsScaffold())
                {
                    LayerSubMesh subMesh = sectionLayer.GetSubMesh(material);
                    if (subMesh != null)
                    {
                        if (renderLayer.HasValue)
                            subMesh.renderLayer = renderLayer.Value;
                        float y = altitudeLayer.AltitudeFor();
                        AddScaffoldQuad(subMesh, cell, y);
                    }
                }
            }
            sectionLayer.FinalizeMesh(MeshParts.All);
        }

        private static PlanetTile cachedOrigin;
        private static PlanetTile cachedDest;
        private static int cachedDistance;
        private static PlanetLayer cachedOriginLayer;
        private static PlanetLayer cachedDestLayer;
        private static readonly List<PlanetLayerConnection> connections = new List<PlanetLayerConnection>();

        public static int GetDistance(PlanetTile from, PlanetTile to)
        {
            if (cachedOrigin == from && cachedDest == to)
            {
                return cachedDistance;
            }
            cachedOrigin = from;
            cachedDest = to;
            cachedDistance = 0;
            if (from.Layer != to.Layer)
            {
                if (cachedOriginLayer == from.Layer && cachedDestLayer == to.Layer)
                {
                }
                else
                {
                    if (!from.Layer.TryGetPath(to.Layer, connections, out var cost))
                    {
                        connections.Clear();
                        return 0;
                    }
                    //Log.Message($"[VGE] Path from {from.Layer} to {to.Layer} with cost {cost} connections: {connections.Select(c => c.origin + " -> " + c.target + " (" + c.fuelCost + ")").ToStringSafeEnumerable()}");
                    cachedOriginLayer = to.Layer;
                    cachedDestLayer = from.Layer;
                    connections.Clear();
                }
                from = to.Layer.GetClosestTile_NewTemp(from);
            }
            cachedDistance = (int)(Find.WorldGrid.TraversalDistanceBetween(from, to) * to.LayerDef.rangeDistanceFactor);
            //Log.Message($"[VGE] Distance from {from} to {to} is {cachedDistance} with range distance factor {to.LayerDef.rangeDistanceFactor}");
            return cachedDistance;
        }

        public static bool IsGravshipLaunch(this PreceptDef ritual)
        {
            return ritual == PreceptDefOf.GravshipLaunch ||
                   ritual == VGEDefOf.VGE_GravjumperLaunch ||
                   ritual == VGEDefOf.VGE_GravhulkLaunch;
        }

        public static float LaunchBoonChanceFromQuality(float quality)
        {
            return LaunchBoonChanceFromQualityCurve.Evaluate(quality);
        }
    }
}
