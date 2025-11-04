using System;
using Verse;
using RimWorld;
using System.Linq;
using Verse.Sound;
using Verse.AI;
using System.Collections.Generic;
using VEF.Abilities;

namespace VanillaGravshipExpanded
{
    public class CompAbilityEmergencyLaunch : CompAbilityEffect
    {

        public List<ThingDef> listOfConsoles = new List<ThingDef>() { VGEDefOf.VGE_PilotBridge, VGEDefOf.VGE_PilotCockpit, VGEDefOf.PilotConsole };

        public new CompProperties_AbilityEmergencyLaunch Props
        {
            get
            {
                return (CompProperties_AbilityEmergencyLaunch)this.props;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {


          

            base.Apply(target, dest);

            Pawn pawn = parent.pawn;

          

            //Do stuff

        }
    

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn = parent.pawn;

            if(target.Thing is null)
            {
                return false;
            }
            
            if (!listOfConsoles.Contains(target.Thing.def))
            {
               
                return false;
            }

            if (!pawn.CanReserve(target.Thing))
            {               
                return false;
            }

            return true;
        }
    }
}
