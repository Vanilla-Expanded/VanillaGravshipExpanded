using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded.Compat;

[StaticConstructorOnStartup]
public static class ReplaceStuff
{
    static ReplaceStuff()
    {
        if (VGEDefOf.VGE_VacCheckpoint == null)
            return;
        if (!ModsConfig.IsActive("Memegoddess.ReplaceStuff") && !ModsConfig.IsActive("Uuugggg.ReplaceStuff"))
            return;

        const string newThingReplacementClassName = "Replace_Stuff.NewThing.NewThingReplacement";
        var type = AccessTools.TypeByName(newThingReplacementClassName);
        if (type == null)
        {
            Log.Error($"[VGE] Initializing Replace Stuff compat failed, couldn't find the type: {newThingReplacementClassName}.");
            return;
        }

        const string replacementClassName = "Replacement";
        var replacementType = type.Inner(replacementClassName);
        if (replacementType == null)
        {
            Log.Error($"[VGE] Initializing Replace Stuff compat failed, couldn't find the type: {newThingReplacementClassName}+{replacementClassName}.");
            return;
        }

        const string replacementsFieldName = "replacements";
        var replacementsField = type.DeclaredField(replacementsFieldName);
        if (replacementsField == null || !replacementsField.IsStatic || replacementsField.FieldType != typeof(List<>).MakeGenericType(replacementType))
        {
            Log.Error($"[VGE] Initializing Replace Stuff compat failed, field is incorrect: {newThingReplacementClassName}:{replacementsFieldName}. Field: {replacementsField}.");
            return;
        }

        const string newCheckFieldName = "newCheck";
        var newCheckField = replacementType.DeclaredField(newCheckFieldName);
        if (newCheckField == null || newCheckField.IsStatic || newCheckField.FieldType != typeof(Predicate<ThingDef>))
        {
            Log.Error($"[VGE] Initializing Replace Stuff compat failed, field is incorrect: {newThingReplacementClassName}+{replacementsFieldName}:{newCheckFieldName}. Field: {newCheckField}.");
            return;
        }

        const string oldCheckFieldName = "oldCheck";
        var oldCheckField = replacementType.DeclaredField(oldCheckFieldName);
        if (oldCheckField == null || oldCheckField.IsStatic || oldCheckField.FieldType != typeof(Predicate<ThingDef>))
        {
            Log.Error($"[VGE] Initializing Replace Stuff compat failed, field is incorrect: {newThingReplacementClassName}+{replacementsFieldName}:{oldCheckFieldName}. Field: {oldCheckField}.");
            return;
        }

        if (replacementsField.GetValue(null) is not IList replacementsList)
        {
            Log.Error($"[VGE] Initializing Replace Stuff compat failed, value is null: {newThingReplacementClassName}:{replacementsFieldName}.");
            return;
        }

        foreach (var replacement in replacementsList)
        {
            if (newCheckField.GetValue(replacement) is not Predicate<ThingDef> newCheck || oldCheckField.GetValue(replacement) is not Predicate<ThingDef> oldCheck)
                continue;

            if (newCheck(VGEDefOf.VGE_VacCheckpoint) && oldCheck(ThingDefOf.Door))
            {
                bool ReplacedNewCheck(ThingDef def)
                {
                    // If the type is VacCheckpoint, and the work to build is 0 (a spot), don't allow replacements
                    if (typeof(Building_VacCheckpoint).IsAssignableFrom(def.thingClass) && def.GetStatValueAbstract(StatDefOf.WorkToBuild) <= 0)
                        return false;
                    // Otherwise, use original check
                    return newCheck(def);
                }

                newCheckField.SetValue(replacement, (Predicate<ThingDef>)ReplacedNewCheck);
            }
        }
    }
}