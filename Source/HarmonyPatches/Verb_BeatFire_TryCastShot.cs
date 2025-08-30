using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Verb_BeatFire), nameof(Verb_BeatFire.TryCastShot))]
    public static class VanillaGravshipExpanded_Verb_BeatFire_TryCastShot_Patch
    {
        public static bool Prefix(Verb_BeatFire __instance, ref bool __result)
        {
            if(__instance.currentTarget.Thing is Astrofire)
            {
                Astrofire fire = (Astrofire)__instance.currentTarget.Thing;
                Pawn casterPawn = __instance.CasterPawn;
                if (casterPawn.stances.FullBodyBusy || fire.TicksSinceSpawn == 0)
                {
                    __result = false;
                    return false;
                }
                fire.TakeDamage(new DamageInfo(VGEDefOf.VGE_ExtinguishAstrofire, 16f, 0f, -1f, __instance.caster));
                casterPawn.Drawer.Notify_MeleeAttackOn(fire);
                __result = true;
                return false;
            } 
            return true;
            
          
        }
    }
}
