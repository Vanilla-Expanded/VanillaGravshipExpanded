using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VanillaGravshipExpanded;

namespace VanillaGravshipExpanded
{
    public class CompProperties_GravheatAbsorber : CompProperties
    {
        public int cooldownTicks = 900000;
        public float heatPushedPerSecond = 21f;
        public CompProperties_GravheatAbsorber()
        {
            compClass = typeof(CompGravheatAbsorber);
        }
    }
    [StaticConstructorOnStartup]
    [HotSwappable]
    public class CompGravheatAbsorber : CompFacilityConnected
    {
        public CompProperties_GravheatAbsorber Props => props as CompProperties_GravheatAbsorber;
        private int cooldownEndTick = -1;
        private bool isAbsorbing = false;
        private static readonly Texture2D GizmoIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/GravheatAbsorber");
        
        private Graphic cooldownGraphic;
        public Graphic CooldownGraphic => cooldownGraphic ??= GraphicDatabase.Get<Graphic_Multi>(parent.Graphic.path + "_Cooldown", parent.Graphic.Shader, parent.Graphic.drawSize, parent.Graphic.color);
        public bool IsOnCooldown => Find.TickManager.TicksGame < cooldownEndTick;
        public bool IsAbsorbing => isAbsorbing;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref cooldownEndTick, "cooldownEndTick", -1);
            Scribe_Values.Look(ref isAbsorbing, "isAbsorbing");
        }

        public override void CompTick()
        {
            base.CompTick();
            if (isAbsorbing && parent.Spawned)
            {
                var room = parent.Position.GetRoom(parent.Map);
                if (room != null)
                {
                    room.PushHeat(Props.heatPushedPerSecond / 60f);
                }
                if (Find.TickManager.TicksGame >= cooldownEndTick)
                {
                    isAbsorbing = false;
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (!IsOnCooldown)
            {
                var absorbGizmo = new Command_Action
                {
                    defaultLabel = "VGE_AbsorbGravheat".Translate(),
                    defaultDesc = "VGE_AbsorbGravheatDesc".Translate(),
                    icon = GizmoIcon,
                    action = AbsorbGravheat
                };

                var heatManager = FindHeatManager();
                if (heatManager == null)
                {
                    absorbGizmo.Disable("VGE_NoGravEngine".Translate());
                }
                else
                {
                    var gravEngine = heatManager.Engine;
                    if (gravEngine.cooldownCompleteTick <= Find.TickManager.TicksGame)
                    {
                        absorbGizmo.Disable("VGE_NoCooldownToAbsorb".Translate());
                    }
                }

                yield return absorbGizmo;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Reset cooldown",
                    defaultDesc = "Reset the cooldown timer to 0",
                    action = () => cooldownEndTick = Find.TickManager.TicksGame
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEV: Stop absorption",
                    defaultDesc = "Stop current heat absorption",
                    action = () => isAbsorbing = false
                };
            }
        }

        private void AbsorbGravheat()
        {
            var heatManager = FindHeatManager();
            if (heatManager == null)
                return;
                
            heatManager.ClearGravEngineHeat();
            ResetGravshipCooldown();
            cooldownEndTick = Find.TickManager.TicksGame + Props.cooldownTicks;
            isAbsorbing = true;
        }

        private CompHeatManager FindHeatManager()
        {
            if (!CanBeOn(out Building_GravEngine engine))
                return null;

            return engine?.GetComp<CompHeatManager>();
        }

        private void ResetGravshipCooldown()
        {
            if (!CanBeOn(out Building_GravEngine engine))
                return;

            var heatManager = engine?.GetComp<CompHeatManager>();
            if (heatManager == null)
                return;

            var gravEngine = heatManager.Engine;
            gravEngine.cooldownCompleteTick = Find.TickManager.TicksGame;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (IsOnCooldown)
            {
                CooldownGraphic.Draw(parent.DrawPos + new Vector3(0f, 0.1f, 0f), parent.Rotation, parent);
            }
        }

        public override string CompInspectStringExtra()
        {
            if (IsOnCooldown)
            {
                int ticksRemaining = cooldownEndTick - Find.TickManager.TicksGame;
                return "VGE_GravheatAbsorberCoolingDown".Translate(ticksRemaining.ToStringTicksToDays());
            }
            return null;
        }
    }
}
