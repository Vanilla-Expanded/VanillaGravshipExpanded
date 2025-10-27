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
        [DebugAction("Vanilla Gravship Expanded", "Spawn Structure as Skyfaller", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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


        private static void SpawnGravship(LocalTargetInfo target, KCSG.StructureLayoutDef layout)
        {
            var landingStructure = (LandingStructure)ThingMaker.MakeThing(VGEDefOf.VGE_LandingStructure);
            landingStructure.layoutDef = layout;
            IntVec3 spawnCell = target.Cell;
            GenSpawn.Spawn(landingStructure, spawnCell, Find.CurrentMap, Rot4.North);
        }
    }
}
