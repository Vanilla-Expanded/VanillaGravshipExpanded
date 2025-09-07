
using RimWorld;
using Verse;
namespace VanillaGravshipExpanded
{
    public class StatPart_EngineeringConsole : StatPart
    {
        private float offset = -0.5f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing.Map!=null && EngineeringConsolePresent(req.Thing.Map))
            {
                val += offset;
            }
        }

        public bool EngineeringConsolePresent(Map map)
        {
            return map.listerThings.ThingsOfDef(VGEDefOf.VGE_EngineeringConsole).Count > 0;
            

        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing.Map != null && EngineeringConsolePresent(req.Thing.Map))
            {
                  return "VGE_StatsReport_EngineeringConsole".Translate() + (": -0.5");
              
            }
            return null;
        }
    }
}