using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse.Noise;
using Verse.AI;

namespace VanillaGravshipExpanded
{
    public class CompEngineeringConsole : ThingComp
    {
        public CompProperties_EngineeringConsole Props => props as CompProperties_EngineeringConsole;

     

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            Command_Action command_Action = new Command_Action();

            command_Action.defaultDesc = "VGE_SetDesiredMaintenanceDesc".Translate();
            command_Action.defaultLabel = "VGE_SetDesiredMaintenance".Translate();
            command_Action.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/SetDesiredMaintenance", true);
            command_Action.hotKey = KeyBindingDefOf.Misc1;
            command_Action.action = delegate
            {
                Window_SetDesiredMaintenance tuningWindow = new Window_SetDesiredMaintenance(this.parent);
                Find.WindowStack.Add(tuningWindow);
            };

            yield return command_Action;


        }


    }


}

