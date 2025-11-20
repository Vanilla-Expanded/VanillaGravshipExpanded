using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(AutoHomeAreaMaker), "MarkHomeAroundThing")]
    public static class AutoHomeAreaMaker_MarkHomeAroundThing_Patch
    {
        public static bool preventHomeArea;
        public static bool Prefix(Thing t)
        {
            if (preventHomeArea)
            {
                return false;
            }
            return true;
        }
    }
}
