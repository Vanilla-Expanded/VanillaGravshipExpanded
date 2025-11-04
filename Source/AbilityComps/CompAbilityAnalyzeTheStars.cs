using System;
using Verse;
using RimWorld;
using System.Linq;
using Verse.Sound;
using Verse.AI;
using System.Collections.Generic;

namespace VanillaGravshipExpanded
{
    public class CompAbilityAnalyzeTheStars : CompAbilityEffect
    {


        public new CompProperties_AbilityAnalyzeTheStars Props
        {
            get
            {
                return (CompProperties_AbilityAnalyzeTheStars)this.props;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {

            base.Apply(target, dest);

           
            //Do stuff

        }


    }
}
