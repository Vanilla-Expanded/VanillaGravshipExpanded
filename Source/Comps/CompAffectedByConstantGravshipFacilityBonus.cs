using System.Text;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class CompAffectedByConstantGravshipFacilityBonus : ThingComp
{
    // Additional bonuses from gravship facilities that only care if the facility is linked to the gravship, nothing more.
    // This one is added to the grav engine itself, and it requires CompAffectedByFacilities to be present on its parent.

    private CompAffectedByFacilities facilities;

    public override void PostPostMake()
    {
        base.PostPostMake();
        InitializeComps();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        if (Scribe.mode == LoadSaveMode.LoadingVars)
            InitializeComps();
    }

    private void InitializeComps() => facilities = parent.GetComp<CompAffectedByFacilities>();

    public override float GetStatOffset(StatDef stat)
    {
        if (facilities == null)
            return 0f;

        var val = 0f;

        for (var i = 0; i < facilities.linkedFacilities.Count; i++)
        {
            var comp = facilities.linkedFacilities[i].TryGetComp<CompConstantGravshipFacilityBonus>();
            if (comp is { StatOffsets: not null, ConnectedToGravEngine: true })
            {
                var offset = comp.StatOffsets.GetStatOffsetFromList(stat);
                if (offset != 0f)
                    val += offset;
            }
        }

        return val;
    }

    public override void GetStatsExplanation(StatDef stat, StringBuilder sb, string whitespace = "")
    {
        facilities.alreadyUsed.Clear();
        var initialText = false;

        for (var i = 0; i < facilities.linkedFacilities.Count; i++)
        {
            var facility = facilities.linkedFacilities[i];
            var alreadyUsed = false;

            for (var j = 0; j < facilities.alreadyUsed.Count; j++)
            {
                if (facilities.alreadyUsed[j] == facility.def)
                {
                    alreadyUsed = true;
                    break;
                }
            }

            if (alreadyUsed)
                continue;

            var comp = facilities.linkedFacilities[i].TryGetComp<CompConstantGravshipFacilityBonus>();
            if (comp is { StatOffsets: not null, ConnectedToGravEngine: true })
            {
                var offset = comp.StatOffsets.GetStatOffsetFromList(stat);
                if (offset != 0f)
                {
                    if (!initialText)
                    {
                        initialText = true;
                        sb.AppendLine();
                        sb.AppendLine(whitespace + "StatsReport_Facilities".Translate() + ":"); // TODO: Change this to something else
                    }

                    var linkedCount = 0;
                    for (var k = 0; k < facilities.linkedFacilities.Count; k++)
                    {
                        var otherFacility = facilities.linkedFacilities[k];
                        if (facility.def == otherFacility.def && otherFacility.TryGetComp<CompConstantGravshipFacilityBonus>().ConnectedToGravEngine)
                            linkedCount++;
                    }

                    offset *= linkedCount;
                    sb.Append(whitespace + "    ");
                    if (linkedCount != 1)
                        sb.Append($"{linkedCount}x ");
                    sb.AppendLine($"{facility.LabelCap}: {offset.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset)}");
                    facilities.alreadyUsed.Add(facility.def);
                }
            }
        }
    }
}