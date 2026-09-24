using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using LudeonTK;
using KCSG;
using RimWorld.Planet;
using HarmonyLib;

namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    [HotSwappable]
    public static class DebugActions
    {
        private const string CategoryName = "Vanilla Gravship Expanded";

        public static bool EnableFuelUsageOrder = false;

        [DebugAction(CategoryName, "Spawn Structure as Skyfaller", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> SpawnStructure()
        {
            List<DebugActionNode> list = new List<DebugActionNode>();

            if (DefDatabase<KCSG.StructureLayoutDef>.AllDefsListForReading is List<KCSG.StructureLayoutDef> slDefs && !slDefs.NullOrEmpty())
            {
                foreach (var layoutDef in slDefs)
                {
                    list.Add(new DebugActionNode(layoutDef.defName, DebugActionType.ToolMap, () =>
                    {
                        var map = Find.CurrentMap;
                        if (UI.MouseCell().InBounds(map))
                        {
                            SpawnGravship(UI.MouseCell(), layoutDef);
                        }
                    }));
                }
            }
            return list;
        }


        public static void SpawnGravship(LocalTargetInfo target, KCSG.StructureLayoutDef layout)
        {
            var landingStructure = (LandingStructure)ThingMaker.MakeThing(VGEDefOf.VGE_LandingStructure);
            landingStructure.layoutDef = layout;
            IntVec3 spawnCell = target.Cell;
            GenSpawn.Spawn(landingStructure, spawnCell, Find.CurrentMap, Rot4.North);
        }

        [DebugAction(CategoryName, allowedGameStates = AllowedGameStates.Playing)]
        private static void ToggleDisplayFuelUseOrder() => EnableFuelUsageOrder = !EnableFuelUsageOrder;

        [DebugAction(CategoryName, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AttachAstrofire()
        {
            var thing = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).FirstOrDefault();
            if (thing == null)
                return;

            if (thing.CanEverAttachFire())
                thing.TryAttachAstrofire(1f, null);
            else
                AstrofireUtility.TryStartAstrofireIn(UI.MouseCell(), Find.CurrentMap, 1.75f, null);
        }
    }
}
