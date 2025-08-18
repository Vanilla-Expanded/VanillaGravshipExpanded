using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class GravshipsMod : Mod
{
    public GravshipsMod(ModContentPack content) : base(content)
    {
        new Harmony("vanillaexpanded.gravship").PatchAll();
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

}
