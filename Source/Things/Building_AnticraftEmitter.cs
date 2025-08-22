using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class Building_AnticraftEmitter : Building_GravshipTurret
    {
        private bool isFiringBurst = false;
        public override void Tick()
        {
            base.Tick();

            bool shouldBeFiringBurst = MannedByPlayer && CurrentTarget.IsValid && Active && AttackVerb.state == VerbState.Bursting;

            if (shouldBeFiringBurst != isFiringBurst)
            {
                isFiringBurst = shouldBeFiringBurst;
                UpdatePowerOutput();
            }

            if (isFiringBurst && !(PowerComp as CompPowerTrader).PowerOn)
            {
                ResetCurrentTarget();
                isFiringBurst = false;
                UpdatePowerOutput();
            }
        }

        private void UpdatePowerOutput()
        {
            if (PowerComp is CompPowerTrader powerTrader)
            {
                var currentPowerConsumption = isFiringBurst ? PowerComp.Props.basePowerConsumption : PowerComp.Props.idlePowerDraw;
                powerTrader.PowerOutput = 0f - currentPowerConsumption;
            }
        }

        public bool IsFiringBurst => isFiringBurst;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isFiringBurst, "isFiringBurst");
        }
    }

}
