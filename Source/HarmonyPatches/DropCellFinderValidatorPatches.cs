using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(DropCellFinder), nameof(DropCellFinder.IsGoodDropSpot))]
    public static class DropCellFinder_IsGoodDropSpot_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static bool Prefix(IntVec3 c, Map map, ref bool __result)
        {
            return c.CheckSpaceTerrain(map, ref __result);
        }
    }

    [HarmonyPatch(typeof(DropCellFinder), nameof(DropCellFinder.CanPhysicallyDropInto))]
    public static class DropCellFinder_CanPhysicallyDropInto_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static bool Prefix(IntVec3 c, Map map, ref bool __result)
        {
            return c.CheckSpaceTerrain(map, ref __result);
        }
    }

    [HarmonyPatch(typeof(DropCellFinder), nameof(DropCellFinder.SkyfallerCanLandAt))]
    public static class DropCellFinder_SkyfallerCanLandAt_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static bool Prefix(IntVec3 c, Map map, ref bool __result)
        {
            return c.CheckSpaceTerrain(map, ref __result);
        }
    }

    [HarmonyPatch(typeof(DropCellFinder), nameof(DropCellFinder.AnyAdjacentGoodDropSpot))]
    public static class DropCellFinder_AnyAdjacentGoodDropSpot_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static bool Prefix(IntVec3 c, Map map, ref bool __result)
        {
            return c.CheckSpaceTerrain(map, ref __result);
        }
    }

    [HarmonyPatch(typeof(DropCellFinder), nameof(DropCellFinder.IsSafeDropSpot))]
    public static class DropCellFinder_IsSafeDropSpot_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static bool Prefix(IntVec3 cell, Map map, ref bool __result)
        {
            return cell.CheckSpaceTerrain(map, ref __result);
        }
    }
    [HarmonyPatch]
    public static class DropCellFinder_LambdaPatches
    {
        public static MethodBase[] TargetMethods()
        {
            var type = typeof(DropCellFinder);
            var methods = new System.Collections.Generic.List<MethodBase>();
            var allMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in allMethods)
            {
                if (method.Name.Contains("<") && method.Name.Contains(">") && method.ReturnType == typeof(bool))
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length > 0 && parameters[0].ParameterType == typeof(IntVec3))
                    {
                        methods.Add(method);
                    }
                }
            }
            var allTypes = type.GetNestedTypes(BindingFlags.NonPublic);
            foreach (var nestedType in allTypes)
            {
                if (nestedType.Name.Contains("<>c"))
                {
                    var typeMethods = nestedType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var typeMethod in typeMethods)
                    {
                        if (typeMethod.ReturnType == typeof(bool))
                        {
                            var parameters = typeMethod.GetParameters();
                            if (parameters.Length > 0 && parameters[0].ParameterType == typeof(IntVec3))
                            {
                                methods.Add(typeMethod);
                            }
                        }
                    }
                }
            }

            return methods.ToArray();
        }

        [HarmonyPriority(int.MinValue)]
        public static bool Prefix(IntVec3 __0, Map ___map, ref bool __result)
        {
            return __0.CheckSpaceTerrain(___map, ref __result);
        }
    }
    public static class DropCellValidatorExtensions
    {
        public static bool CheckSpaceTerrain(this IntVec3 c, Map map, ref bool __result)
        {
            if (c.GetTerrain(map) == TerrainDefOf.Space)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
