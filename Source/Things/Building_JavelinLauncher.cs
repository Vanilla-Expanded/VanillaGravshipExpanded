using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class TurretExtension_Barrels : DefModExtension
    {
        public List<Vector3> barrels;
    }
    
    [HotSwappable]

    public class Building_JavelinLauncher : Building_GravshipTurret
    {
        private int barrelIndex;
        private Vector3[] barrels;

        public Vector3 CastSource
        {
            get
            {
                var result = DrawPos + barrels[barrelIndex].RotatedBy(top.CurRotation);
                Log.Message("Cast source: " + result + " - def.GetModExtension<TurretExtension_Barrels>(): " + def.GetModExtension<TurretExtension_Barrels>().barrels[barrelIndex]);
                return result;
            }
        }

        public static Vector3 GetCastSource(Thing thing) => thing is Building_JavelinLauncher turret ? turret.CastSource : thing.DrawPos;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            var ext = def.GetModExtension<TurretExtension_Barrels>();
            if (ext != null)
            {
                barrels = ext.barrels.ToArray();
                barrelIndex = barrels.Length - 1;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref barrelIndex, "barrelIndex");
        }

        public static int Wrap(int value, int min, int max)
        {
            if (min == max) return min;
            while (value < min) value = max - (min - value);
            while (value > max) value = min + (value - max);
            return value;
        }

        public static void Cycle(ref int value, int min, int max) => value = Wrap(value + 1, min, max);
        public static void Cycle(ref int value, Array arr) => Cycle(ref value, 0, arr.Length - 1);

        public void SwitchBarrel()
        {
            Cycle(ref barrelIndex, barrels);
        }

        public override Material TurretTopMaterial
        {
            get
            {
                if (refuelableComp.IsFull)
                {
                    return def.building.turretGunDef.building.turretTopLoadedMat;
                }
                return def.building.turretTopMat;
            }
        }
    }
}
