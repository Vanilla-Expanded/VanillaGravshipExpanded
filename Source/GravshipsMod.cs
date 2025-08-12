using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded;

public class GravshipsMod : Mod
{
    public GravshipsMod(ModContentPack content) : base(content)
    {
        new Harmony("vanillaexpanded.gravship").PatchAll();
    }
}
