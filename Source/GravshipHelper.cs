using System.Linq;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    public static class GravshipHelper
    {
        public static Building_GravEngine GetGravEngine(this Thing thing)
        {
            if (thing.Map == null)
                return null;

            return thing.Map.listerBuildings.AllBuildingsColonistOfClass<Building_GravEngine>().FirstOrDefault(x => x.AllConnectedSubstructure.Contains(thing.Position));
        }
    }
}
