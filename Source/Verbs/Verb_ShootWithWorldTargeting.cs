using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class Verb_ShootWithWorldTargeting : Verb_LaunchProjectile
    {
        public override int ShotsPerBurst => base.BurstShotCount;
        public Building_GravshipTurret Turret => (Building_GravshipTurret)caster;
        public override void WarmupComplete()
        {
            base.WarmupComplete();
            var casterPawn = (caster as Building_GravshipTurret).ManningPawn;
            if (casterPawn == null || casterPawn.skills == null) return;
            if (currentTarget.Thing is Pawn { Downed: false, IsColonyMech: false } pawn)
            {
                float num = (pawn.HostileTo(caster) ? 170f : 20f);
                float num2 = verbProps.AdjustedFullCycleTime(this, casterPawn);
                casterPawn.skills.Learn(SkillDefOf.Shooting, num * num2);
            }
        }

        public override bool TryCastShot()
        {
            var originalWarmupTime = verbProps.warmupTime;
            if (caster is Building_GravshipTurret building_GravshipTurret && building_GravshipTurret.CanFire)
            {
                var gravshipTargeting = building_GravshipTurret.GravshipTargeting;
                float alpha = 1.2f;
                float multiplier = Mathf.Clamp(Mathf.Pow(gravshipTargeting, -alpha), 0.1f, 2.0f);
                verbProps.warmupTime *= multiplier;
            }
            try
            {
                var target = CurrentTarget;
                if (target.Cell.InBounds(caster.Map) is false)
                {
                    ThingDef projectile = Projectile;
                    ShootLine resultingLine;
                    TryFindShootLineFromTo(caster.Position, currentTarget, out resultingLine);
                    Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, resultingLine.Source, caster.Map);
                    ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
                    Vector3 drawPos = caster.DrawPos;
                    Thing equipmentSource = base.EquipmentSource;
                    var turret = caster as Building_GravshipTurret;
                    projectile2.Launch(turret, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags4, preventFriendlyFire, equipmentSource, null);
                    return true;
                }
                else
                {
                    bool num = base.TryCastShot();
                    if (num && CasterIsPawn)
                    {
                        CasterPawn.records.Increment(RecordDefOf.ShotsFired);
                    }
                    return num;
                }
            }
            finally
            {
                verbProps.warmupTime = originalWarmupTime;
            }
        }
    }
}
