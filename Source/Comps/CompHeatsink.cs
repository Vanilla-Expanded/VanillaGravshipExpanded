using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    public class CompProperties_Heatsink : CompProperties
    {
        public float maxHeat = 10f;
        public float heatConsumptionPerHour = 1f;
        public float powerGenerated = 100f;
        public float cooldownReductionPercent;
        public float heatPushedPerSecond = 21f;
        public CompProperties_Heatsink()
        {
            compClass = typeof(CompHeatsink);
        }
    }

    [HotSwappable]
    [StaticConstructorOnStartup]
    public class CompHeatsink : CompFacilityConnected
    {
        public CompProperties_Heatsink Props => props as CompProperties_Heatsink;

        private float storedHeat;
        private CompPowerTrader powerComp;

        public float StoredHeat => storedHeat;
        public bool IsActive => storedHeat > 0 && (powerComp?.PowerOn ?? false) && CanBeOn(out _);
        private Graphic overlayGraphic;
        public Graphic OverlayGraphic => overlayGraphic ??= GraphicDatabase.Get<Graphic_Multi>(parent.Graphic.path + "_Overlay", parent.Graphic.Shader, parent.Graphic.drawSize, parent.Graphic.color);

        public CompGlower glower;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = parent.GetComp<CompPowerTrader>();
            glower = parent.GetComp<CompGlower>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref storedHeat, "storedHeat");
        }


        public void AddHeat(float amount)
        {
            storedHeat = Mathf.Min(storedHeat + amount, Props.maxHeat);
            UpdateLit();
        }

        public void ClearHeat()
        {
            storedHeat = 0;
            UpdateLit();
        }

        private void UpdateLit()
        {
            if (parent.Map != null)
            {
                glower.UpdateLit(parent.Map);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.Map is null) return;

            if (storedHeat <= 0 || !CanBeOn(out _))
            {
                powerComp.PowerOutput = powerComp.Props.basePowerConsumption;
                return;
            }

            if (powerComp.PowerOn)
            {
                float heatToConsume = Props.heatConsumptionPerHour / 2500f;
                if (storedHeat >= heatToConsume)
                {
                    storedHeat -= heatToConsume;
                    powerComp.PowerOutput = Props.powerGenerated;
                    var room = parent.Position.GetRoom(parent.Map);
                    if (room != null)
                    {
                        room.PushHeat(Props.heatPushedPerSecond / 60f);
                    }
                }
                else
                {
                    storedHeat = 0;
                    powerComp.PowerOutput = powerComp.Props.basePowerConsumption;
                    UpdateLit();
                }
            }
            else
            {
                powerComp.PowerOutput = powerComp.Props.basePowerConsumption;
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (storedHeat <= 0)
                return;

            var drawPos = parent.TrueCenter();
            drawPos.y += 0.1f;
            var transparency = 1f - (storedHeat / Props.maxHeat);
            var overlayColor = new Color(1f, 1f, 1f, transparency);
            OverlayGraphic.color = overlayColor;
            OverlayGraphic.Draw(parent.DrawPos + new Vector3(0f, 0.1f, 0f), parent.Rotation, parent);
        }

        public override string CompInspectStringExtra()
        {
            return "VGE_HeatsinkHeatStored".Translate(storedHeat.ToString("F1"), Props.maxHeat.ToString("F1"));
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Add heat",
                    defaultDesc = "Add 1 heat to heatsink",
                    action = () => AddHeat(1f)
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEV: Add max heat",
                    defaultDesc = "Set heat to maximum",
                    action = () => AddHeat(Props.maxHeat - storedHeat)
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEV: Clear heat",
                    defaultDesc = "Remove all stored heat",
                    action = ClearHeat
                };
            }
        }
    }
}
