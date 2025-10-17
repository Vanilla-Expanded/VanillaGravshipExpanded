
﻿using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class CompVariableEfficiencyBattery : CompPowerBattery
    {

        CompAffectedByFacilities affectedBy;

        public float Efficiency
        {
            get
            {
                List<Thing> linkedFacilitiesListForReading = affectedBy.LinkedFacilitiesListForReading;
                for (int i = 0; i < linkedFacilitiesListForReading.Count; i++)
                {
                    if (linkedFacilitiesListForReading[i].def == VGEDefOf.VGE_CapacitorHarmonizer && linkedFacilitiesListForReading[i].TryGetComp<CompBreakdownable>()?.BrokenDown !=true)
                    {
                        return 1;
                    }
                }
                return Props.efficiency;
            }

        }

        public float SelfDischarge
        {
            get
            {

                if (Efficiency == 1)
                {
                    return 0;
                }

                return 10;
            }

        }

        public new float AmountCanAccept
        {
            get
            {
                if (parent.IsBrokenDown() || StunnedByEMP)
                {
                    return 0f;
                }
                CompProperties_Battery compProperties_Battery = Props;
                return (compProperties_Battery.storedEnergyMax - storedEnergy) / Efficiency;
            }
        }

        public new CompProperties_VariableEfficiencyBattery Props => (CompProperties_VariableEfficiencyBattery)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            stunnableComp = parent.GetComp<CompStunnable>();
            affectedBy = parent.GetComp<CompAffectedByFacilities>();
        }

        public override void CompTick()
        {
            if (Efficiency != 1)
            {
                DrawPower(Mathf.Min(SelfDischarge * CompPower.WattsToWattDaysPerTick, storedEnergy));
            }

        }


        public new void AddEnergy(float amount)
        {
            if (amount < 0f)
            {
                Log.Error("Cannot add negative energy " + amount);
            }
            else if (!StunnedByEMP)
            {
                if (amount > AmountCanAccept)
                {
                    amount = AmountCanAccept;
                }
                amount *= Efficiency;
                storedEnergy += amount;
            }
        }

        public override string CompInspectStringExtra()
        {

            string text = "PowerBatteryStored".Translate() + ": " + storedEnergy.ToString("F0") + " / " + Props.storedEnergyMax.ToString("F0") + " Wd";
            text += "\n" + "PowerBatteryEfficiency".Translate() + ": " + (Efficiency * 100f).ToString("F0") + "%";
            if (storedEnergy > 0f)
            {
                text += "\n" + "SelfDischarging".Translate() + ": " + SelfDischarge.ToString("F0") + " W";
            }
            return text;
        }


    }
}