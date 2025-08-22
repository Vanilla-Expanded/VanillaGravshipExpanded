using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class Building_GravshipTurret : Building_TurretGun
    {
        private float curAngle;
        private float rotationSpeed;
        private float rotationVelocity;
        private int barrelIndex = -1;
        private List<Vector3> barrels;
        public Building_TargetingTerminal linkedTerminal;
        private static readonly Texture2D ForceTargetIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/GravshipArtilleryForceTarget");
        private static readonly Texture2D HoldFireIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/GravshipArtilleryHoldFire");
        private static readonly Texture2D LinkIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/LinkWithTerminal");
        private static readonly Texture2D UnlinkIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/UnlinkWithTerminal");
        private static readonly Texture2D SelectIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/SelectLinkedTerminal");
        public static readonly Material NoLinkOverlay = MaterialPool.MatFrom("UI/Overlays/NoLinkedTargetingTerminal");
        public bool MannedByPlayer => linkedTerminal?.MannableComp?.MannedNow ?? false;

        public Pawn ManningPawn => linkedTerminal?.MannableComp?.ManningPawn;

        public Vector3 CastSource
        {
            get
            {
                if (barrels != null)
                {
                    if (barrelIndex < 0)
                    {
                        barrelIndex = 0;
                    }
                    var result = DrawPos + barrels[barrelIndex].RotatedBy(top.CurRotation);
                    return result;
                }
                return DrawPos;
            }
        }

        public static Vector3 GetCastSource(Thing thing) => thing is Building_GravshipTurret turret ? turret.CastSource : thing.DrawPos;

        public void TrySwitchBarrel()
        {
            if (barrels != null)
            {
                barrelIndex = (barrelIndex + 1) % barrels.Count;
            }
        }

        public override bool CanSetForcedTarget
        {
            get
            {
                if (linkedTerminal != null && linkedTerminal.MannableComp.MannedNow)
                {
                    return true;
                }
                return false;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            var ext = def.GetModExtension<TurretExtension_RotationSpeed>();
            if (ext != null)
            {
                rotationSpeed = ext.rotationSpeed;
            }

            var barrelExt = def.GetModExtension<TurretExtension_Barrels>();
            if (barrelExt != null)
            {
                barrels = barrelExt.barrels;
            }
        }
        public override void Tick()
        {
            base.Tick();

            if (linkedTerminal != null && (linkedTerminal.Destroyed || !linkedTerminal.Spawned))
            {
                Unlink();
            }

            if (rotationSpeed > 0)
            {
                if (MannedByPlayer && CurrentTarget.IsValid && Active && AttackVerb.Available())
                {
                    var targetAngle = (CurrentTarget.Cell.ToVector3Shifted() - DrawPos).AngleFlat();
                    if (targetAngle < 0)
                    {
                        targetAngle += 360;
                    }
                    curAngle = top.CurRotation = Mathf.SmoothDampAngle(curAngle, targetAngle, ref rotationVelocity, 0.01f, rotationSpeed, 1f / GenTicks.TicksPerRealSecond);
                    if (curAngle < 0)
                    {
                        curAngle += 360;
                    }
                    else if (curAngle > 360)
                    {
                        curAngle -= 360;
                    }
                    var angleDiff = Mathf.Min(Mathf.Abs(curAngle - targetAngle), 360 - Mathf.Abs(curAngle - targetAngle));
                    if (angleDiff > 0.1f)
                    {
                        burstWarmupTicksLeft++;
                    }
                }
                else
                {
                    curAngle = top.CurRotation;
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref rotationVelocity, "rotationVelocity");
            Scribe_Values.Look(ref barrelIndex, "barrelIndex", -1);
            Scribe_References.Look(ref linkedTerminal, "linkedTerminal");
            Scribe_Values.Look(ref curAngle, "curAngle");
        }


        public void RenderPulsingOverlay(Material mat, Mesh mesh, bool incrementOffset = true)
        {
            Vector3 drawPos = this.TrueCenter();
            drawPos.y = this.DrawPos.y + 0.1f;
            drawPos += this.Map.overlayDrawer.curOffset;
            if (def.building != null && def.building.isAttachment)
            {
                drawPos += (Rotation.AsVector2 * 0.5f).ToVector3();
            }
            drawPos.y = Mathf.Min(drawPos.y, Find.Camera.transform.position.y - 0.1f);
            if (incrementOffset)
            {
                this.Map.overlayDrawer.curOffset.x += this.Map.overlayDrawer.StackOffsetFor(this);
            }
            float num = ((float)Math.Sin((Time.realtimeSinceStartup + 397f * (float)(thingIDNumber % 571)) * 4f) + 1f) * 0.5f;
            num = 0.3f + num * 0.7f;
            Material material = FadedMaterialPool.FadedVersionOf(mat, num);
            Graphics.DrawMesh(mesh, Matrix4x4.TRS(drawPos, Quaternion.identity, Vector3.one), material, 0);
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (linkedTerminal == null)
            {
                if (!text.NullOrEmpty())
                {
                    text += "\n";
                }
                text += "VGE_NeedsLinkedTargetingTerminal".Translate();
            }
            return text;
        }

        public void LinkTo(Building_TargetingTerminal terminal)
        {
            terminal.linkedTurret?.Unlink();
            linkedTerminal = terminal;
            terminal.linkedTurret = this;
            SoundDefOf.Tick_High.PlayOneShotOnCamera();
        }

        public void Unlink()
        {
            if (linkedTerminal != null)
            {
                linkedTerminal.linkedTurret = null;
            }
            linkedTerminal = null;
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
        }

        private void SelectLinkedTerminal()
        {
            if (linkedTerminal != null)
            {
                Find.Selector.ClearSelection();
                Find.Selector.Select(linkedTerminal);
            }
        }
        private void StartLinking()
        {
            var targetingParameters = new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo t) => t.Thing is Building_TargetingTerminal && t.Thing.Position.InHorDistOf(this.Position, 36)
            };
            Find.Targeter.BeginTargeting(targetingParameters, delegate (LocalTargetInfo t)
            {
                var terminal = t.Thing as Building_TargetingTerminal;
                LinkTo(terminal);
            }, onGuiAction: delegate { GenDraw.DrawRadiusRing(this.Position, 36f); });
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                if (gizmo is Command_VerbTarget command && command.defaultLabel == "CommandSetForceAttackTarget".Translate())
                {
                    command.icon = ForceTargetIcon;
                    if (!MannedByPlayer)
                    {
                        command.Disable("VGE_NeedsMannedTargetingTerminal".Translate());
                    }
                }
                else if (gizmo is Command_Toggle command2 && command2.defaultLabel == "CommandHoldFire".Translate())
                {
                    command2.icon = HoldFireIcon;
                }
                yield return gizmo;
            }

            if (linkedTerminal == null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "VGE_LinkWithTerminal".Translate(),
                    defaultDesc = "VGE_LinkWithTerminalDesc".Translate(),
                    icon = LinkIcon,
                    action = delegate { StartLinking(); }
                };
            }
            else
            {
                yield return new Command_Action
                {
                    defaultLabel = "VGE_UnlinkWithTerminal".Translate(),
                    defaultDesc = "VGE_UnlinkWithTerminalDesc".Translate(),
                    icon = UnlinkIcon,
                    action = delegate { Unlink(); }
                };
                yield return new Command_Action
                {
                    defaultLabel = "VGE_SelectLinkedTerminal".Translate(),
                    defaultDesc = "VGE_SelectLinkedTerminalDesc".Translate(),
                    icon = SelectIcon,
                    action = delegate { SelectLinkedTerminal(); }
                };
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
}
