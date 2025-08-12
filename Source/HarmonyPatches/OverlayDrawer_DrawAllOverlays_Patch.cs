using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(OverlayDrawer), "DrawAllOverlays")]
    public static class OverlayDrawer_DrawAllOverlays_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var overlaysToDrawField = AccessTools.Field(typeof(OverlayDrawer), "overlaysToDraw");
            var clearMethod = AccessTools.Method(typeof(Dictionary<Thing, OverlayTypes>), "Clear");
            var enumeratorMoveNext = AccessTools.Method(typeof(Dictionary<Thing, RimWorld.OverlayTypes>.Enumerator), "MoveNext");
            var curOffsetField = AccessTools.Field(typeof(OverlayDrawer), "curOffset");
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(clearMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OverlayDrawer_DrawAllOverlays_Patch), nameof(RenderNoLinkOverlay)));
                }
                yield return codes[i];
                if (codes[i].StoresField(curOffsetField))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, curOffsetField);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OverlayDrawer_DrawAllOverlays_Patch), nameof(RenderNoLinkOverlayInForLoop)));
                }
            }
        }

        public static void RenderNoLinkOverlayInForLoop(OverlayDrawer overlayDrawer, KeyValuePair<Thing, OverlayTypes> pair, ref Vector3 curOffset)
        {
            if (pair.Key is Building_GravshipTurret turret && turret.linkedTerminal == null)
            {
                turret.RenderPulsingOverlay(Building_GravshipTurret.NoLinkOverlay, MeshPool.plane08);
            }
        }

        public static void RenderNoLinkOverlay(OverlayDrawer overlayDrawer)
        {
            foreach (var map in Find.Maps)
            {
                if (map == Find.CurrentMap)
                {
                    foreach (var turret in map.listerThings.GetThingsOfType<Building_GravshipTurret>())
                    {
                        if (turret.linkedTerminal == null)
                        {
                            if (overlayDrawer.overlaysToDraw.TryGetValue(turret, out var existingOverlays))
                            {
                                OverlayTypes overlayTypes = OverlayTypes.NeedsPower | OverlayTypes.PowerOff;
                                int bitCountOf = Gen.GetBitCountOf((long)(existingOverlays & overlayTypes));
                                float num = overlayDrawer.StackOffsetFor(turret);
                                switch (bitCountOf)
                                {
                                    case 1:
                                        overlayDrawer.curOffset = new UnityEngine.Vector3(num, 0, 0);
                                        break;
                                    case 2:
                                        overlayDrawer.curOffset = new UnityEngine.Vector3(0.5f * num, 0f, 0f);
                                        break;
                                    case 3:
                                        overlayDrawer.curOffset = new UnityEngine.Vector3(1.5f * num, 0f, 0f);
                                        break;
                                }
                            }
                            turret.RenderPulsingOverlay(Building_GravshipTurret.NoLinkOverlay, MeshPool.plane08);
                        }
                    }
                }
            }
        }
    }
}
