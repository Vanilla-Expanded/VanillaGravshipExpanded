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
        private Mote aimChargeMote;

        public override void Tick()
        {
            base.Tick();

            bool shouldBeFiringBurst = CanFire && CurrentTarget.IsValid && Active && AttackVerb.state == VerbState.Bursting;

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
            if (angleDiff <= 0.1f && CanFire && CurrentTarget.IsValid && Active && burstWarmupTicksLeft > 0)
            {
                if (aimChargeMote == null || aimChargeMote.Destroyed)
                {
                    var verbProps = AttackVerb.verbProps;
                    if (verbProps.aimingChargeMote != null)
                    {
                        aimChargeMote = MoteMaker.MakeStaticMote(Position.ToVector3Shifted(), Map, verbProps.aimingChargeMote, 1f, makeOffscreen: true);
                    }
                }
                if (aimChargeMote != null && !aimChargeMote.Destroyed)
                {
                    var verbProps = AttackVerb.verbProps;
                    Vector3 vector = (CurrentTarget.CenterVector3 - Position.ToVector3Shifted());
                    vector.y = 0f;
                    vector.Normalize();
                    float exactRotation = vector.AngleFlat();
                    bool stunned = IsStunned;
                    aimChargeMote.paused = stunned;
                    aimChargeMote.exactRotation = exactRotation;
                    aimChargeMote.exactPosition = Position.ToVector3Shifted() + vector * verbProps.aimingChargeMoteOffset;
                    aimChargeMote.Maintain();
                }
            }
            else if (aimChargeMote != null && !aimChargeMote.Destroyed)
            {
                aimChargeMote.Destroy();
                aimChargeMote = null;
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
