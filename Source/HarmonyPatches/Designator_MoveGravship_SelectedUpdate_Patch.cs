using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Designator_MoveGravship), "SelectedUpdate")]
    public static class Designator_MoveGravship_SelectedUpdate_Patch
    {
        public static void Prefix()
        {
            Designator_MoveGravship_IsValidCell_Patch.ignore = true;
        }

        public static void Postfix()
        {
            Designator_MoveGravship_IsValidCell_Patch.ignore = false;
        }
    }
}
