
using System.Collections.Generic;
using RimWorld;
using Verse;
namespace VanillaGravshipExpanded
{
    public class Alert_AstrofireInHomeArea : Alert_Critical
    {
        private Astrofire AstroFireInHomeArea
        {
            get
            {
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++)
                {
                    List<Thing> list = maps[i].listerThings.ThingsOfDef(VGEDefOf.VGE_Astrofire);
                    for (int j = 0; j < list.Count; j++)
                    {
                        Thing thing = list[j];
                        if (maps[i].areaManager.Home[thing.Position] && !thing.Position.Fogged(thing.Map))
                        {
                            return (Astrofire)thing;
                        }
                    }
                }
                return null;
            }
        }

        public Alert_AstrofireInHomeArea()
        {
            defaultLabel = "VGE_AstrofireInHomeArea".Translate();
            defaultExplanation = "VGE_AstrofireInHomeAreaDesc".Translate();
        }

        public override AlertReport GetReport()
        {
            return AstroFireInHomeArea;
        }
    }
}