using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(GravshipLandingMarker), "BeginLanding")]
    public static class GravshipLandingMarker_BeginLanding_Patch
    {
        public static bool Prefix(GravshipLandingMarker __instance, WorldComponent_GravshipController gravshipController)
        {
            if (__instance.GravshipCells.Select(x => x + __instance.Position).Any(c => Designator_MoveGravship_IsValidCell_Patch.HasIndestructibleBuilding(c, __instance.Map)))
            {
                Messages.Message("VGE_CannotLandIndestructibleObstacles".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }

            var things = GravshipMapGenUtility.GetBlockingThings(__instance.GravshipCells.Select(x => x + __instance.Position), __instance.Map);
            if (things.Any())
            {
                string text = "VGE_ConfirmCrashLanding".Translate();
                Dialog_MessageBox dialog = Dialog_MessageBox.CreateConfirmation(text, delegate
                {
                    Map map = __instance.Map;
                    CameraJumper.TryJump(__instance);
                    __instance.Destroy();
                    gravshipController.landingMarker = __instance;
                    gravshipController.InitiateLanding(__instance.gravship, map, __instance.Position, __instance.GravshipRotation);
                    gravshipController.landingMarker = null;
                }, delegate
                {
                    gravshipController.landingMarker = __instance;
                });
                Find.WindowStack.Add(dialog);
                return false;
            }
            return true;
        }
    }
}
