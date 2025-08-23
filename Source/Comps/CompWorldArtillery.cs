using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class CompProperties_WorldArtillery : CompProperties
    {
        public int worldMapAttackRange;
        public CompProperties_WorldArtillery()
        {
            compClass = typeof(CompWorldArtillery);
        }
    }
    
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class CompWorldArtillery : ThingComp
    {
        public GlobalTargetInfo worldTarget;
        public IntVec3 targetCell;
        private static readonly Texture2D WorldTargetIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/GravshipArtilleryForceTargetWorld");

        private Building_GravshipTurret Turret => parent as Building_GravshipTurret;
        
        public CompProperties_WorldArtillery Props => props as CompProperties_WorldArtillery;

        private float GetTargetingMultiplier(float targetingStat)
        {
            return 0.5f + (targetingStat * 0.5f);
        }


        private float HitFactorFromShooter(Thing caster, float distance)
        {
            float f = (caster is Pawn) ? caster.GetStatValue(StatDefOf.ShootingAccuracyPawn) : (caster?.GetStatValue(StatDefOf.ShootingAccuracyTurret) ?? 1f);
            float num = Mathf.Pow(f, distance);
            if (caster is Pawn)
            {
                var globalAccuracy = caster.GetStatValue(VGEDefOf.VGE_AccuracyGlobal);
                num = num * globalAccuracy;
            }
            var finalResult = Mathf.Max(num, 0.0201f);
            return finalResult;
        }

        public float GetHitChance(GlobalTargetInfo target, Pawn shooter)
        {
            var launcher = parent as Building_TurretGun;
            var distance = Find.WorldGrid.ApproxDistanceInTiles(launcher.Map.Tile, target.Tile);
            var hitFactor = HitFactorFromShooter(launcher, distance);
            var targetingStat = shooter?.GetStatValue(VGEDefOf.VGE_GravshipTargeting) ?? 1f;
            var targetingMultiplier = GetTargetingMultiplier(targetingStat);
            float hitChance = hitFactor * targetingMultiplier;
            return hitChance;
        }

        public virtual float FinalForcedMissRadius(GlobalTargetInfo target, Pawn shooter)
        {
            var launcher = parent as Building_TurretGun;
            var verb = launcher.AttackVerb;

            var baseMissRadius = verb.verbProps.ForcedMissRadius;
            var distance = Find.WorldGrid.ApproxDistanceInTiles(launcher.Map.Tile, target.Tile);
            var worldMultiplier = 1.0f;
            if (distance > 49)
            {
                worldMultiplier = 2.0f;
            }
            else if (distance > 25)
            {
                worldMultiplier = 1.6f;
            }
            else if (distance > 9)
            {
                worldMultiplier = 1.2f;
            }

            var targetingStat = shooter?.GetStatValue(VGEDefOf.VGE_GravshipTargeting) ?? 1f;
            var forcedMiss = (baseMissRadius * worldMultiplier) / GetTargetingMultiplier(targetingStat);
            return forcedMiss;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_TargetInfo.Look(ref worldTarget, "worldTarget");
            Scribe_Values.Look(ref targetCell, "targetCell");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            var worldTargetGizmo = new Command_Action
            {
                defaultLabel = "VGE_SetWorldTarget".Translate(),
                defaultDesc = "VGE_SetWorldTargetDesc".Translate(),
                icon = WorldTargetIcon,
                action = delegate { StartWorldTargeting(); }
            };

            if (!Turret.MannedByPlayer)
            {
                worldTargetGizmo.Disable("VGE_NeedsMannedTargetingTerminal".Translate());
            }
            yield return worldTargetGizmo;
        }

        private void StartWorldTargeting()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(
                (GlobalTargetInfo globalTarget) =>
                {
                    if (Find.WorldObjects.MapParentAt(globalTarget.Tile) is MapParent mapParent && mapParent.Map != null)
                    {
                        var map = mapParent.Map;
                        Current.Game.CurrentMap = map;
                        var targetingParameters = new TargetingParameters
                        {
                            canTargetPawns = true,
                            canTargetBuildings = true,
                            canTargetLocations = true
                        };
                        var turret = parent as Building_TurretGun;
                        Find.Targeter.BeginTargeting(targetingParameters, delegate (LocalTargetInfo target)
                        {
                            var cell = FindEdgeCell(turret.Map, globalTarget);
                            turret.OrderAttack(cell);
                            this.worldTarget = globalTarget;
                            this.targetCell = target.Cell;
                            Current.Game.CurrentMap = turret.Map;
                            Find.CameraDriver.JumpToCurrentMapLoc(turret.Position);
                        }, highlightAction: delegate (LocalTargetInfo x)
                        {
                            if (x.IsValid)
                            {
                                GenDraw.DrawTargetHighlight(x);
                            }
                        }, null, onGuiAction: delegate (LocalTargetInfo x)
                        {
                            Texture2D icon = (Texture2D)Turret.def.building.turretTopMat.mainTexture;
                            GenUI.DrawMouseAttachment(icon);
                        }, onUpdateAction: delegate (LocalTargetInfo x)
                        {
                            DrawTargetHighlightField(x, globalTarget);
                        });
                        return true;
                    }
                    Messages.Message("VGE_GravshipArtilleryNeedsVisibleMap".Translate(), MessageTypeDefOf.RejectInput, false);
                    return false;
                },
                true,
                WorldTargetIcon,
                true, onUpdate: delegate
                {
                    GenDraw.DrawWorldRadiusRing(parent.Map.Tile, Props.worldMapAttackRange);
                },
                null,
                delegate (GlobalTargetInfo t)
                {
                    if (Find.WorldGrid.ApproxDistanceInTiles(parent.Map.Tile, t.Tile) > Props.worldMapAttackRange)
                    {
                        return false;
                    }
                    return true;
                },
                null
            );
        }

        private void DrawTargetHighlightField(LocalTargetInfo target, GlobalTargetInfo worldTarget)
        {
            var turret = parent as Building_GravshipTurret;
            ThingDef projectile = turret.AttackVerb.verbProps.defaultProjectile;
            float num = projectile.projectile.explosionRadius + projectile.projectile.explosionRadiusDisplayPadding;
            float forcedMissRadius = FinalForcedMissRadius(worldTarget, turret.ManningPawn);
            if (forcedMissRadius > 0f && turret.AttackVerb.BurstShotCount > 1)
            {
                num += forcedMissRadius;
            }
            if (!(num > 0.2f))
            {
                return;
            }
            GenExplosion.RenderPredictedAreaOfEffect(target.Cell, num, turret.AttackVerb.verbProps.explosionRadiusRingColor);
        }

        public LocalTargetInfo FindEdgeCell(Map map, GlobalTargetInfo worldTarget)
        {
            float angle = Find.WorldGrid.GetHeadingFromTo(map.Tile, worldTarget.Tile);
            var edgeCells = new CellRect(0, 0, map.Size.x, map.Size.z).EdgeCells;
            var parentPos = parent.TrueCenter();
            IntVec3 targetCell = edgeCells.MinBy(c => Mathf.Abs(angle - (c.ToVector3() - parentPos).AngleFlat()));
            Vector3 normalized = (targetCell.ToVector3() - parentPos).normalized;
            IntVec3 outCell = new IntVec3(targetCell.x + (int)Math.Round(normalized.x), targetCell.y, targetCell.z + (int)Math.Round(normalized.z));
            return new LocalTargetInfo(outCell);
        }

        public void Reset()
        {
            worldTarget = GlobalTargetInfo.Invalid;
            targetCell = IntVec3.Invalid;
        }
    }
}

