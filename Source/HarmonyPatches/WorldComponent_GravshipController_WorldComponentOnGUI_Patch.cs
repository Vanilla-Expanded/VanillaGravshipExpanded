using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(WorldComponent_GravshipController), "WorldComponentOnGUI")]
    public static class WorldComponent_GravshipController_WorldComponentOnGUI_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            var beginLandingMethod = AccessTools.Method(typeof(GravshipLandingMarker), nameof(GravshipLandingMarker.BeginLanding));
            for (int i = 0; i < code.Count; i++)
            {
                var instruction = code[i];
                if (instruction.Calls(beginLandingMethod))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(WorldComponent_GravshipController_WorldComponentOnGUI_Patch), nameof(TryBeginLanding)));
                    i += 6;
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static void TryBeginLanding(GravshipLandingMarker marker, WorldComponent_GravshipController controller)
        {
            var map = marker.Map;
            var gravshipCells = marker.GravshipCells.Select(x => x + marker.Position).ToList();
            if (gravshipCells.Any(c => !c.InBounds(map)))
            {
                Messages.Message("GravshipOutOfBounds".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return;
            }
            if (gravshipCells.Any(c => Designator_MoveGravship_IsValidCell_Patch.HasIndestructibleBuilding(c, map)))
            {
                Messages.Message("VGE_CannotLandIndestructibleObstacles".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return;
            }
            var things = GravshipMapGenUtility.GetBlockingThings(gravshipCells, map);
            if (things.Any())
            {
                string text = "VGE_ConfirmCrashLanding".Translate();
                Dialog_MessageBox dialog = Dialog_MessageBox.CreateConfirmation(text, delegate
                {
                    marker.BeginLanding(controller);
                    controller.landingMarker = null;
                    SoundDefOf.Gravship_Land.PlayOneShotOnCamera();
                });
                Find.WindowStack.Add(dialog);
                return;
            }
            marker.BeginLanding(controller);
            controller.landingMarker = null;
            SoundDefOf.Gravship_Land.PlayOneShotOnCamera();
        }
    }
}
