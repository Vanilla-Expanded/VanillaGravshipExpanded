using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using static RimWorld.SectionLayer_SubstructureProps;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(SectionLayer_SubstructureProps), "Regenerate")]
    public static class SectionLayer_SubstructureProps_Regenerate_Patch
    {
        private static readonly CachedMaterial CustomBottom = new CachedMaterial("Things/Terrain/Substructure/Subscaffolding/SubscaffoldingProps_Loops", ShaderDatabase.Transparent);
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var finalizeMeshMethod = AccessTools.Method(typeof(MapDrawLayer), nameof(MapDrawLayer.FinalizeMesh));
            var drawMethod = AccessTools.Method(typeof(SectionLayer_SubstructureProps_Regenerate_Patch), nameof(DrawCustomBottomForGravshipSubscaffold));

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(finalizeMeshMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, drawMethod);
                }
                yield return instruction;
            }
        }

        private static void DrawCustomBottomForGravshipSubscaffold(SectionLayer_SubstructureProps instance)
        {
            var section = instance.section;
            var map = instance.Map;
            var terrainGrid = map.terrainGrid;
            var cellRect = section.CellRect;
            float altitude = AltitudeLayer.TerrainScatter.AltitudeFor();
            LayerSubMesh subMesh = instance.GetSubMesh(CustomBottom.Material);

            foreach (IntVec3 item in cellRect)
            {
                TerrainDef foundationDef = terrainGrid.FoundationAt(item);
                if (foundationDef == VGEDefOf.VGE_GravshipSubscaffold)
                {
                    SectionLayer_SubstructureProps_ShouldDrawPropsOn_Patch.doVanilla = true;
                    if (instance.ShouldDrawPropsOn(item, terrainGrid, out var edgeEdgeDirections, out var cornerDirections))
                    {
                        instance.DrawEdges(item, edgeEdgeDirections, altitude);
                        instance.DrawCorners(item, cornerDirections, edgeEdgeDirections, altitude);
                        SectionLayer_GravshipHull.ShouldDrawCornerPiece(item + IntVec3.South, map, terrainGrid, out var cornerType, out var _);
                        bool flag = cornerType == SectionLayer_GravshipHull.CornerType.Corner_NW || cornerType == SectionLayer_GravshipHull.CornerType.Diagonal_NW || cornerType == SectionLayer_GravshipHull.CornerType.Corner_NE || cornerType == SectionLayer_GravshipHull.CornerType.Diagonal_NE;
                        if (edgeEdgeDirections.HasFlag(EdgeDirections.South) && !flag)
                        {
                            instance.AddQuad(subMesh, item + IntVec3.South, altitude, Rot4.North, SectionLayer_GravshipMask.IsValidSubstructure(item));
                        }
                    }
                    SectionLayer_SubstructureProps_ShouldDrawPropsOn_Patch.doVanilla = false;
                }
                else if (foundationDef == VGEDefOf.VGE_MechanoidSubstructure)
                {
                    SectionLayer_SubstructureProps_ShouldDrawPropsOn_Patch.doVanilla = true;
                    if (instance.ShouldDrawPropsOn(item, terrainGrid, out var edgeEdgeDirections, out var cornerDirections))
                    {
                        instance.DrawEdges(item, edgeEdgeDirections, altitude);
                        instance.DrawCorners(item, cornerDirections, edgeEdgeDirections, altitude);
                        SectionLayer_GravshipHull.ShouldDrawCornerPiece(item + IntVec3.South, map, terrainGrid, out var cornerType, out var _);
                        bool flag = cornerType == SectionLayer_GravshipHull.CornerType.Corner_NW || cornerType == SectionLayer_GravshipHull.CornerType.Diagonal_NW || cornerType == SectionLayer_GravshipHull.CornerType.Corner_NE || cornerType == SectionLayer_GravshipHull.CornerType.Diagonal_NE;
                        if (edgeEdgeDirections.HasFlag(EdgeDirections.South) && !flag)
                        {
                            instance.AddQuad(subMesh, item + IntVec3.South, altitude, Rot4.North, SectionLayer_GravshipMask.IsValidSubstructure(item));
                        }
                    }
                    SectionLayer_SubstructureProps_ShouldDrawPropsOn_Patch.doVanilla = false;
                }
            }
        }
    }
}
