using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(CompFirefoamPack), nameof(CompFirefoamPack.ChanceToUse))]
public static class CompFirefoamPack_ChanceToUse_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instr, generator);

        matcher.MatchEndForward(
            // Loads the pawn argument (0th argument is "this")
            CodeMatch.IsLdarg(1),
            // Loads the fire field
            CodeMatch.LoadsField(typeof(ThingDefOf).DeclaredField(nameof(ThingDefOf.Fire))),
            // Calls the "GetAttachment" method
            CodeMatch.Calls(() => AttachmentUtility.GetAttachment)
        );

        matcher.InsertAfter(
            // Load pawn argument
            CodeInstruction.LoadArgument(1),
            // Load our method
            CodeInstruction.Call(() => GetAstrofireWrapper)
        );

        InsertAstrofireToCondition(matcher);

        return matcher.Instructions();
    }

    private static Thing GetAstrofireWrapper(Thing thing, Pawn wearer) => thing ?? wearer.GetAttachment(VGEDefOf.VGE_Astrofire);

    public static void InsertAstrofireToCondition(CodeMatcher matcher)
    {
        // Looks for:
        // if (thingList[i] is Fire || thingList[i].HasAttachment(ThingDefOf.Fire)) {}
        // And wraps HasAttachement with our extra astrofire conditions.

        matcher.Reset();

        matcher.MatchEndForward(
            // Load list
            CodeMatch.IsLdloc(),
            // Load index
            CodeMatch.IsLdloc(),
            // Grabs the value from the list
            CodeMatch.Calls(typeof(List<Thing>).IndexerGetter([typeof(int)])),
            // Grabs the fire field
            CodeMatch.LoadsField(typeof(ThingDefOf).DeclaredField(nameof(ThingDefOf.Fire))),
            // Calls HasAttachment
            CodeMatch.Calls(() => AttachmentUtility.HasAttachment),
            // Branches over remaining arguments
            CodeMatch.Branches()
        );

        matcher.Insert(
            // Copy instructions we matched, since they point to fields we want:
            // Load the thing list
            new CodeInstruction(matcher.InstructionAt(-5)),
            // Load the index in the list
            new CodeInstruction(matcher.InstructionAt(-4)),
            // Call the indexer getter from the list
            new CodeInstruction(matcher.InstructionAt(-3)),
            // Call our wrapper method
            CodeInstruction.Call(() => AstrofireCheckWrapper)
        );
    }

    private static bool AstrofireCheckWrapper(bool result, Thing thing) => result || thing is Astrofire || thing.HasAttachment(VGEDefOf.VGE_Astrofire);
}