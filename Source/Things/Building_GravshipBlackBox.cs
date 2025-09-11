using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class Building_GravshipBlackBox : Building
    {
        private int storedGravdata = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref storedGravdata, "storedGravdata", 0);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString());
            stringBuilder.AppendLine("VGE_GravdataStored".Translate(storedGravdata));
            stringBuilder.AppendLine("VGE_GravdataStoredInfo".Translate());
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            var currentProject = Find.ResearchManager.currentProj;
            bool canConvert = currentProject != null;

            yield return new Command_Action
            {
                defaultLabel = "VGE_ConvertGravdata".Translate(),
                defaultDesc = "VGE_ConvertGravdataDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/Gizmo_ConvertGravdata"),
                action = delegate
                {
                    if (storedGravdata > 0 && currentProject != null)
                    {
                        Find.ResearchManager.AddProgress(currentProject, storedGravdata);
                        int convertedAmount = storedGravdata;
                        storedGravdata = 0;
                        Messages.Message("VGE_ConvertedGravdataToResearch".Translate(convertedAmount, currentProject.LabelCap), MessageTypeDefOf.TaskCompletion);
                    }
                },
                disabled = !canConvert,
                disabledReason = !canConvert ? "VGE_NoNonGravtechProjectSelected".Translate() : null
            };
        }

        public void AddGravdata(int amount)
        {
            storedGravdata += amount;
        }

        public int TakeGravdata(int amount)
        {
            int taken = Math.Min(amount, storedGravdata);
            storedGravdata -= taken;
            return taken;
        }

        public int StoredGravdata => storedGravdata;
    }
}
