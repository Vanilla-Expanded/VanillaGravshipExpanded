using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class Projectile_JavelinRocket : Projectile_Artillery
    {
        private int smokeTicksLeft = 0;
        private Material _flameMaterial;
        private MaterialPropertyBlock _flameBlock;
        private ThrusterProjectileExtension _extension;
        private FleckSystemThrown _exhaustFleckSystem;
        private int exhaustTicksLeft;
        private int exhaustEmissionInterval;
        private Material FlameMaterial => _flameMaterial ??= MaterialPool.MatFrom(new MaterialRequest(Extension.flameShaderType.Shader)
        {
            renderQueue = 3201,
            shaderParameters = Extension.flameShaderParameters
        });

        private MaterialPropertyBlock FlameBlock => _flameBlock ??= new MaterialPropertyBlock();

        private List<ShaderParameter> FlameShaderParameters => Extension.flameShaderParameters;

        private ThrusterProjectileExtension Extension => _extension ??= def.GetModExtension<ThrusterProjectileExtension>();

        private FleckSystemThrown ExhaustFleckSystem
        {
            get
            {
                if (_exhaustFleckSystem == null)
                {
                    _exhaustFleckSystem = new FleckSystemThrown(this.Map.flecks);
                    _exhaustFleckSystem.handledDefs.AddUnique(Extension.exhaustFleckDef);
                }
                return _exhaustFleckSystem;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref smokeTicksLeft, "smokeTicksLeft");
            Scribe_Values.Look(ref exhaustTicksLeft, "exhaustTicksLeft");
            Scribe_Values.Look(ref exhaustEmissionInterval, "exhaustEmissionInterval");
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            smokeTicksLeft = 180;
            exhaustTicksLeft = Mathf.RoundToInt(Extension.emissionsPerSecond * (ticksToImpact / 60f));
            exhaustEmissionInterval = Mathf.RoundToInt(60f / Extension.emissionsPerSecond);
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
        }

        public override void Tick()
        {
            base.Tick();
            if (smokeTicksLeft > 0 && this.Map != null)
            {
                smokeTicksLeft--;
                if (Find.TickManager.TicksGame % 4 == 0)
                {
                    FleckMaker.ThrowSmoke(this.ExactPosition, this.Map, 0.6f);
                }
            }
            if (Find.TickManager.TicksGame + 60 >= this.spawnedTick)
            {
                if (exhaustTicksLeft > 0)
                {
                    exhaustTicksLeft--;
                    if (this.IsHashIntervalTick(this.exhaustEmissionInterval))
                    {
                        EmitExhaust();
                    }
                }
                ExhaustFleckSystem.parent = this.Map.flecks;
                ExhaustFleckSystem.Update(1);
            }
        }

        private void EmitExhaust()
        {
            Vector3 velocityDirection = this.ExactRotation * Vector3.forward;
            Vector3 exhaustPosition = this.ExactPosition - velocityDirection * 0.5f;

            Vector3 exhaustVelocity = Extension.velocity;
            exhaustVelocity = Quaternion.Euler(0f, 0f, Rand.Range(Extension.velocityRotationRange.min, Extension.velocityRotationRange.max)) * exhaustVelocity;
            exhaustVelocity *= Rand.Range(Extension.velocityMultiplierRange.min, Extension.velocityMultiplierRange.max);
            exhaustVelocity = velocityDirection * exhaustVelocity.z + Vector3.up * exhaustVelocity.y + Vector3.right * exhaustVelocity.x;

            ExhaustFleckSystem.CreateFleck(new FleckCreationData
            {
                def = Extension.exhaustFleckDef,
                spawnPosition = exhaustPosition + Rand.InsideUnitCircleVec3 * Rand.Range(Extension.spawnRadiusRange.min, Extension.spawnRadiusRange.max),
                scale = Rand.Range(Extension.scaleRange.min, Extension.scaleRange.max),
                velocity = exhaustVelocity,
                rotationRate = Rand.Range(Extension.rotationOverTimeRange.min, Extension.rotationOverTimeRange.max),
                ageTicksOverride = -1
            });
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            Vector3 velocityDirection = this.ExactRotation * Vector3.forward;
            Vector3 flameOffset = velocityDirection * (def.graphicData.drawSize.x * 0.25f + Extension.flameSize * 0.25f);
            Vector3 flamePosition = this.ExactPosition - flameOffset;

            Color value = new Color(1f, 1f, 1f);
            value *= Mathf.Lerp(0.75f, 1f, Mathf.PerlinNoise1D(Find.TickManager.TicksGame * 0.1f));

            FlameBlock.Clear();
            FlameBlock.SetColor("_Color2", value);
            foreach (ShaderParameter param in FlameShaderParameters)
            {
                param.Apply(FlameBlock);
            }
            GenDraw.DrawQuad(FlameMaterial, flamePosition, this.ExactRotation, Extension.flameSize, FlameBlock);

            if(!Find.TickManager.Paused)
            {
                FleckMaker.ThrowSmoke(drawLoc, this.Map, 0.25f);

                if (this.IsHashIntervalTick(this.exhaustEmissionInterval))
                {
                    FleckCreationData data = FleckMaker.GetDataStatic(this.ExactPosition + velocityDirection * 2, this.Map, VGEDefOf.VGE_JavelinGlow, Rand.Range(1f, 2f) * 2);
                    data.rotationRate      = 0;
                    data.velocityAngle     = this.ExactPosition.AngleToFlat(flamePosition) + 90 + Rand.Range(-15, 15);
                    data.velocitySpeed     = this.def.projectile.speed/10 * this.DistanceCoveredFraction;
                    this.Map.flecks.CreateFleck(data);

                    FleckCreationData data2 = FleckMaker.GetDataStatic(this.ExactPosition + velocityDirection * 2, this.Map, FleckDefOf.LightningGlow, Rand.Range(0.25f, 0.5f) * 2);
                    data2.rotationRate  = 0;
                    data2.velocityAngle = this.ExactPosition.AngleToFlat(flamePosition) + 90 + Rand.Range(-15, 15);
                    data2.velocitySpeed = this.def.projectile.speed / 20 * this.DistanceCoveredFraction;
                    this.Map.flecks.CreateFleck(data2);
                }
            }
        }

    }
}
