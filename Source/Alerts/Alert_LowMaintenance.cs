using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;


namespace VanillaGravshipExpanded
{
    public class Alert_LowMaintenance : Alert
    {
        public List<string> maintainablesInternal = new List<string>();

        public Alert_LowMaintenance()
        {
            defaultPriority = AlertPriority.High;
            defaultLabel = "VGE_Alert_LowMaintenance".Translate();
           
        }

        public override Color BGColor
        {
            get
            {
                float num = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
                return new Color(num, num, num) * Color.yellow;
            }
        }

        public override TaggedString GetExplanation()
        {
          
            return "VGE_Alert_LowMaintenance_Desc".Translate(maintainablesInternal.ToLineList(" - "));
        }

        public override AlertReport GetReport()
        {

            var map = Find.CurrentMap;
            if (map == null)
            {
                return AlertReport.Inactive;
            }

            return AlertReport.CulpritsAre(GetLowMaintenance(map).ToList());
        }

        public IEnumerable<Thing> GetLowMaintenance(Map map)
        {
            maintainablesInternal.Clear();
            HashSet<Thing> maintainables = map.GetComponent<GravMaintainables_MapComponent>()?.maintainables_InMap;

            if (!maintainables.NullOrEmpty()) {
                foreach (Thing maintainable in maintainables)
                {
                    CompGravMaintainable comp = maintainable.TryGetComp<CompGravMaintainable>();
                    if (comp != null) {
                        if (comp.maintenance > 0.3f)
                        {
                            continue;
                        }
                        else {
                            maintainablesInternal.Add(maintainable.LabelCap);
                            yield return maintainable;
                        }
                            
                    }
                    


                }
            }
            
        }
    }
}
