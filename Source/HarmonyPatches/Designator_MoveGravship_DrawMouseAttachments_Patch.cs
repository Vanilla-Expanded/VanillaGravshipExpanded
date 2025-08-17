using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Designator_MoveGravship), "DrawMouseAttachments")]
    public static class Designator_MoveGravship_DrawMouseAttachments_Patch
    {
        public static bool Prefix(Designator_MoveGravship __instance)
        {
            Designator_MoveGravship_IsValidCell_Patch.ignore = false;
            var acceptanceReport = __instance.ValidGravshipLocation(UI.MouseCell(), __instance.tmpValidGravshipCells, __instance.tmpInvalidGravshipCells);

            if (!acceptanceReport.Accepted)
            {
                string reason = acceptanceReport.Reason;
                Color? textColor = ColorLibrary.RedReadable;
                Color textBgColor = new Color(0f, 0f, 0f, 0.5f);
                GenUI.DrawMouseAttachment(null, reason, 0f, default(Vector2), null, textColor, drawTextBackground: true, textBgColor);
            }
            return false;
        }
    }
}
