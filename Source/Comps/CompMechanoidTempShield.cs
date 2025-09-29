using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace VanillaGravshipExpanded
{
    public class CompProperties_MechanoidTempShield : CompProperties_ProjectileInterceptor
    {
        public CompProperties_MechanoidTempShield()
        {
            compClass = typeof(CompMechanoidTempShield);
        }
    }

    public class CompMechanoidTempShield : CompProjectileInterceptor
    {
        private enum ShieldState
        {
            Cooldown,
            Active
        }

        private ShieldState state = ShieldState.Cooldown;
        private int ticksToNextStateChange;
        public override void PostPostMake()
        {
            base.PostPostMake();
            state = ShieldState.Cooldown;
            ticksToNextStateChange = 0;
            currentHitPoints = 0;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksToNextStateChange, "ticksToNextStateChange", 0);
            Scribe_Values.Look(ref state, "state", ShieldState.Cooldown);
        }
        public override void CompTick()
        {
            switch (state)
            {
                case ShieldState.Active:
                    if (ticksToNextStateChange <= 0 || currentHitPoints <= 0)
                    {
                        StartCooldown();
                    }
                    else
                    {
                        ticksToNextStateChange--;
                    }
                    break;

                case ShieldState.Cooldown:
                    if (ticksToNextStateChange <= 0)
                    {
                        ActivateShield();
                    }
                    else
                    {
                        ticksToNextStateChange--;
                    }
                    break;
            }
        }

        private void ActivateShield()
        {
            state = ShieldState.Active;
            ticksToNextStateChange = Props.activeDuration;
            currentHitPoints = HitPointsMax;
            if (Props.reactivateEffect != null)
            {
                Props.reactivateEffect.Spawn(parent, parent.MapHeld).Cleanup();
            }
            this.lastInterceptTicks = -9999;
        }

        private void StartCooldown()
        {
            state = ShieldState.Cooldown;
            ticksToNextStateChange = Props.chargeDurationTicks;
            currentHitPoints = 0;
            EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, Props.radius);
        }
        public override string CompInspectStringExtra()
        {
            switch (state)
            {
                case ShieldState.Active:
                    return "VGE_RemainingShieldTime".Translate(ticksToNextStateChange.ToStringTicksToPeriod());
                case ShieldState.Cooldown:
                    return "VGE_CooldownRemaining".Translate(ticksToNextStateChange.ToStringTicksToPeriod());
                default:
                    return null;
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Dev: Force activation",
                    action = ActivateShield
                };
                yield return new Command_Action
                {
                    defaultLabel = "Dev: Force cooldown",
                    action = StartCooldown
                };
            }
        }
    }
}
