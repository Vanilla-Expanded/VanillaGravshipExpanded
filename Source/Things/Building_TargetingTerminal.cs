using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using VEF.Graphics;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    public class Building_TargetingTerminal : Building
    {
        public Building_GravshipTurret linkedTurret;
        
        public virtual bool MannedByPlayer => MannableComp?.MannedNow ?? false;
        
        public virtual float GravshipTargeting => MannableComp?.ManningPawn?.GetStatValue(VGEDefOf.VGE_GravshipTargeting) ?? 0f;

        private CompMannable mannableComp;
        private CustomOverlayDrawer overlayDrawer;

        private static readonly Texture2D LinkIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/LinkWithTurret");
        private static readonly Texture2D UnlinkIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/UnlinkWithTurret");
        private static readonly Texture2D SelectIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/SelectLinkedTurret");

        public CompMannable MannableComp
        {
            get
            {
                if (mannableComp == null)
                {
                    mannableComp = this.GetComp<CompMannable>();
                }
                return mannableComp;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref linkedTurret, "linkedTurret");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            overlayDrawer = map.GetComponent<CustomOverlayDrawer>();
            overlayDrawer.Enable(this, VGEDefOf.VGE_NoLinkedTurretOverlay);
        }

        public override void Tick()
        {
            base.Tick();

            if (linkedTurret != null && (linkedTurret.Destroyed || !linkedTurret.Spawned))
            {
                Unlink();
            }
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            if (linkedTurret != null)
            {
                GenDraw.DrawLineBetween(this.TrueCenter(), linkedTurret.TrueCenter(), SimpleColor.White);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (linkedTurret == null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "VGE_LinkWithTurret".Translate(),
                    defaultDesc = "VGE_LinkWithTurretDesc".Translate(),
                    icon = LinkIcon,
                    action = delegate { StartLinking(); }
                };
            }
            else
            {
                yield return new Command_Action
                {
                    defaultLabel = "VGE_UnlinkWithTurret".Translate(),
                    defaultDesc = "VGE_UnlinkWithTurretDesc".Translate(),
                    icon = UnlinkIcon,
                    action = delegate { Unlink(); }
                };
                yield return new Command_Action
                {
                    defaultLabel = "VGE_SelectLinkedTurret".Translate(),
                    defaultDesc = "VGE_SelectLinkedTurretDesc".Translate(),
                    icon = SelectIcon,
                    action = delegate { SelectLinkedTurret(); }
                };
            }
        }

        private void StartLinking()
        {
            var targetingParameters = new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo t) => t.Thing is Building_GravshipTurret && t.Thing.Position.InHorDistOf(this.Position, 36)
            };
            Find.Targeter.BeginTargeting(targetingParameters, delegate (LocalTargetInfo t)
            {
                var turret = t.Thing as Building_GravshipTurret;
                LinkTo(turret);
            }, onGuiAction: delegate { GenDraw.DrawRadiusRing(this.Position, 36f); });
        }

        public void LinkTo(Building_GravshipTurret turret)
        {
            if (turret.linkedTerminal != null)
            {
                turret.linkedTerminal?.Unlink();
            }
            linkedTurret = turret;
            turret.LinkTo(this);
            SoundDefOf.Tick_High.PlayOneShotOnCamera();
            overlayDrawer?.Disable(this, VGEDefOf.VGE_NoLinkedTurretOverlay);
        }

        public void Unlink()
        {
            linkedTurret?.Unlink();
            linkedTurret = null;
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            overlayDrawer?.Enable(this, VGEDefOf.VGE_NoLinkedTurretOverlay);
        }

        private void SelectLinkedTurret()
        {
            if (linkedTurret != null)
            {
                Find.Selector.ClearSelection();
                Find.Selector.Select(linkedTurret);
            }
        }
    }
}
