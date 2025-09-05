using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Material = UnityEngine.Material;
using Rot4 = Verse.Rot4;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public abstract class SectionLayer_ScaffoldBase : SectionLayer
    {
        private static readonly int RenderLayer = LayerMask.NameToLayer("GravshipMask");
        
        public override bool Visible => DebugViewSettings.drawTerrain;
        
        protected SectionLayer_ScaffoldBase(Section section) : base(section)
        {
            relevantChangeTypes = MapMeshFlagDefOf.Terrain;
        }

        public override void Regenerate()
        {
            if (WorldComponent_GravshipController.CutsceneInProgress ||
                WorldComponent_GravshipController.GravshipRenderInProgess || true)
            {
                ClearSubMeshes(MeshParts.All);
                Map map = base.Map;
                TerrainGrid terrainGrid = map.terrainGrid;
                CellRect cellRect = section.CellRect;
                float baseY = GetAltitudeLayer().AltitudeFor();
                float borderY = baseY + GetBorderAltitudeOffset();
                foreach (IntVec3 item in cellRect)
                {
                    if (terrainGrid.FoundationAt(item).IsScaffold())
                    {
                        DrawScaffoldingTile(baseY, item);
                    }
                    if (ShouldDrawPropsOn(item, terrainGrid, out var edgeEdgeDirections, out var cornerDirections))
                    {
                        DrawEdges(item, edgeEdgeDirections, borderY);
                        DrawCorners(item, cornerDirections, edgeEdgeDirections, borderY);
                        Log.Message("Drawing props on " + item);
                    }
                }
                FinalizeMesh(MeshParts.All);
            }
        }

        protected abstract Material GetScaffoldingMaterial();
        
        protected virtual AltitudeLayer GetAltitudeLayer()
        {
            return AltitudeLayer.Floor;
        }
        
        protected virtual float GetBorderAltitudeOffset()
        {
            return 0.001f;
        }

        private void DrawScaffoldingTile(float baseY, IntVec3 cell)
        {
            LayerSubMesh subMesh = GetSubMesh(GetScaffoldingMaterial());
            subMesh.renderLayer = RenderLayer;
            GravshipHelper.AddScaffoldQuad(subMesh, cell, baseY);
        }

        private void DrawEdges(IntVec3 c, SectionLayer_SubstructureProps.EdgeDirections edgeDirs, float altitude)
        {
            if (SectionLayer_SubstructureProps.EdgeMats.TryGetValue(edgeDirs, out var value))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    (CachedMaterial cachedMaterial, Rot4 rotation) = value[i];
                    Material material = GetEdgeMaterial(cachedMaterial, rotation);
                    LayerSubMesh subMesh = GetSubMesh(material);
                    subMesh.renderLayer = RenderLayer;
                    AddQuad(subMesh, c, altitude, rotation, addGravshipMask: false);
                }
            }
        }

        private void DrawCorners(IntVec3 c, SectionLayer_SubstructureProps.CornerDirections cornerDirections, SectionLayer_SubstructureProps.EdgeDirections edgeDirs, float altitude)
        {
            if (cornerDirections.HasFlag(SectionLayer_SubstructureProps.CornerDirections.NorthWest) && !edgeDirs.HasFlag(SectionLayer_SubstructureProps.EdgeDirections.North) && !edgeDirs.HasFlag(SectionLayer_SubstructureProps.EdgeDirections.West))
            {
                Material material = GetCornerMaterial(SectionLayer_SubstructureProps.CornerInner);
                LayerSubMesh subMesh = GetSubMesh(material);
                subMesh.renderLayer = RenderLayer;
                AddQuad(subMesh, c, altitude, Rot4.South, addGravshipMask: false);
            }
            if (cornerDirections.HasFlag(SectionLayer_SubstructureProps.CornerDirections.NorthEast) && !edgeDirs.HasFlag(SectionLayer_SubstructureProps.EdgeDirections.North) && !edgeDirs.HasFlag(SectionLayer_SubstructureProps.EdgeDirections.East))
            {
                Material material = GetCornerMaterial(SectionLayer_SubstructureProps.CornerInner);
                LayerSubMesh subMesh = GetSubMesh(material);
                subMesh.renderLayer = RenderLayer;
                AddQuad(subMesh, c, altitude, Rot4.West, addGravshipMask: false);
            }
            if (cornerDirections.HasFlag(SectionLayer_SubstructureProps.CornerDirections.SouthEast) && !edgeDirs.HasFlag(SectionLayer_SubstructureProps.EdgeDirections.South) && !edgeDirs.HasFlag(SectionLayer_SubstructureProps.EdgeDirections.East))
            {
                Material material = GetCornerMaterial(SectionLayer_SubstructureProps.CornerInner);
                LayerSubMesh subMesh = GetSubMesh(material);
                subMesh.renderLayer = RenderLayer;
                AddQuad(subMesh, c, altitude, Rot4.North, addGravshipMask: false);
            }
            if (cornerDirections.HasFlag(SectionLayer_SubstructureProps.CornerDirections.SouthWest) && !edgeDirs.HasFlag(SectionLayer_SubstructureProps.EdgeDirections.South) && !edgeDirs.HasFlag(SectionLayer_SubstructureProps.EdgeDirections.West))
            {
                Material material = GetCornerMaterial(SectionLayer_SubstructureProps.CornerInner);
                LayerSubMesh subMesh = GetSubMesh(material);
                subMesh.renderLayer = RenderLayer;
                AddQuad(subMesh, c, altitude, Rot4.East, addGravshipMask: false);
            }
        }

        private void AddQuad(LayerSubMesh sm, IntVec3 c, float altitude, Rot4 rotation, bool addGravshipMask)
        {
            int count = sm.verts.Count;
            int num = Mathf.Abs(4 - rotation.AsInt);
            for (int i = 0; i < 4; i++)
            {
                sm.verts.Add(new Vector3((float)c.x + SectionLayer_SubstructureProps.UVs[i].x, altitude, (float)c.z + SectionLayer_SubstructureProps.UVs[i].y));
                sm.uvs.Add(SectionLayer_SubstructureProps.UVs[(num + i) % 4]);
            }
            sm.tris.Add(count);
            sm.tris.Add(count + 1);
            sm.tris.Add(count + 2);
            sm.tris.Add(count);
            sm.tris.Add(count + 2);
            sm.tris.Add(count + 3);
            if (addGravshipMask)
            {
                Material material = GetGravshipMaskMaterial(sm.material);
                AddQuad(GetSubMesh(material), c, altitude, rotation, addGravshipMask: false);
            }
        }

        protected virtual Material GetEdgeMaterial(CachedMaterial cachedMaterial, Rot4 rotation)
        {
            return cachedMaterial.Material;
        }
        
        protected virtual Material GetCornerMaterial(CachedMaterial cachedMaterial)
        {
            return cachedMaterial.Material;
        }
        
        protected virtual Material GetGravshipMaskMaterial(Material originalMaterial)
        {
            return MaterialPool.MatFrom(originalMaterial.mainTexture as Texture2D, color: originalMaterial.color, shader: ShaderDatabase.GravshipMaskMasked);
        }

        private bool ShouldDrawPropsOn(IntVec3 c, TerrainGrid terrGrid, out SectionLayer_SubstructureProps.EdgeDirections edgeDirections, out SectionLayer_SubstructureProps.CornerDirections cornerDirections)
        {
            edgeDirections = SectionLayer_SubstructureProps.EdgeDirections.None;
            cornerDirections = SectionLayer_SubstructureProps.CornerDirections.None;
            
            TerrainDef terrainDef = terrGrid.FoundationAt(c);
            if (terrainDef == null || !terrainDef.IsScaffold())
            {
                return false;
            }
            for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
            {
                IntVec3 c2 = c + GenAdj.CardinalDirections[i];
                if (!c2.InBounds(base.Map))
                {
                    edgeDirections |= (SectionLayer_SubstructureProps.EdgeDirections)(1 << i);
                    continue;
                }
                TerrainDef terrainDef2 = terrGrid.FoundationAt(c2);
                if (terrainDef2 == null || !terrainDef2.IsScaffold())
                {
                    edgeDirections |= (SectionLayer_SubstructureProps.EdgeDirections)(1 << i);
                }
            }
            for (int j = 0; j < GenAdj.DiagonalDirections.Length; j++)
            {
                IntVec3 c3 = c + GenAdj.DiagonalDirections[j];
                if (!c3.InBounds(base.Map))
                {
                    cornerDirections |= (SectionLayer_SubstructureProps.CornerDirections)(1 << j);
                    continue;
                }
                TerrainDef terrainDef3 = terrGrid.FoundationAt(c3);
                if (terrainDef3 == null || !terrainDef3.IsScaffold())
                {
                    cornerDirections |= (SectionLayer_SubstructureProps.CornerDirections)(1 << j);
                }
            }
            if (edgeDirections == SectionLayer_SubstructureProps.EdgeDirections.None)
            {
                return cornerDirections != SectionLayer_SubstructureProps.CornerDirections.None;
            }
            return true;
        }
    }
}
