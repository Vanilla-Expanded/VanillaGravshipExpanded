using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;


public class GravshipsMod : Mod
{
    public const string HarmonyLatePatchCategory = "LatePatch";

    public GravshipsMod(ModContentPack content) : base(content)
    {
        var harmony = new Harmony("vanillaexpanded.gravship");
        harmony.PatchAllUncategorized();
        // Some patches may need to be loaded later to avoid initializing resources on main thread.
        LongEventHandler.ExecuteWhenFinished(() => harmony.PatchCategory(HarmonyLatePatchCategory));
        settings = GetSettings<GravshipsMod_Settings>();
    }

    public static GravshipsMod_Settings settings;
  
    public override string SettingsCategory()
    {
        return "VE - Gravships";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        settings.DoWindowContents(inRect);
    }

    // [DebugOutput("Soky debug")]
    // private static void NoCoverThings()
    // {
    //     var enumerable = DefDatabase<ThingDef>.AllDefs.Where(def => def.building != null && def.category == ThingCategory.Building && !def.IsFrame && def.fillPercent <= Mathf.Epsilon);
    //     
    //     var array = new TableDataGetter<ThingDef>[]
    //     {
    //         new("defName", def => def.defName),
    //         new("label", def => def.label),
    //         new("modContentPack", def => def.modContentPack?.Name ?? "(unknown)")
    //     };
    //
    //     DebugTables.MakeTablesDialog(enumerable, array);
    // }
}
