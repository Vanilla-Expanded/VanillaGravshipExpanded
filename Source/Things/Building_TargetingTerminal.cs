using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    public class Building_TargetingTerminal : Building
    {
        public Building_GravshipTurret linkedTurret;

        private CompMannable mannableComp;

        private static readonly Texture2D LinkIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/LinkWithTurret");
        private static readonly Texture2D UnlinkIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/UnlinkWithTurret");
        private static readonly Texture2D SelectIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/SelectLinkedTurret");
        private static readonly Material NoLinkOverlay = MaterialPool.MatFrom("UI/Overlays/NoLinkedTurret_Overlay");

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
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (linkedTurret is null)
            {
                this.Map.overlayDrawer.RenderPulsingOverlay(this, NoLinkOverlay, 3);
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
        }

        public void Unlink()
        {
            linkedTurret?.Unlink();
            linkedTurret = null;
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
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
