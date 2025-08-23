using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class Building_JavelinLauncher : Building_GravshipTurret
    {
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
