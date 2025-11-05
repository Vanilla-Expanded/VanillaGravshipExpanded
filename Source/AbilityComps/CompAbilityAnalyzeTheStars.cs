using System;
using Verse;
using RimWorld;
using System.Linq;
using Verse.Sound;
using Verse.AI;
using System.Collections.Generic;
using LudeonTK;
using RimWorld.Planet;

namespace VanillaGravshipExpanded
{
    public class CompAbilityAnalyzeTheStars : CompAbilityEffect
    {
       public List<WorldObjectDef> possibleObjects = new List<WorldObjectDef>() { VGEDefOf.OrbitalItemStash, VGEDefOf.AsteroidBasic, VGEDefOf.AsteroidMiningSite };
        public List<ThingDef> mineables = new List<ThingDef>() { ThingDefOf.MineableGold, VGEDefOf.MineablePlasteel, VGEDefOf.MineableUranium };


        public new CompProperties_AbilityAnalyzeTheStars Props
        {
            get
            {
                return (CompProperties_AbilityAnalyzeTheStars)this.props;
            }
        }

        private bool TryFindSiteTile(out PlanetTile tile)
        {
            tile = PlanetTile.Invalid;
            
            PlanetTile tile2;
            PlanetTile origin = (TileFinder.TryFindRandomPlayerTile(out tile2, allowCaravans: false, null, canBeSpace: true) ? tile2 : new PlanetTile(0, Find.WorldGrid.Surface));
            if (!Find.WorldGrid.TryGetFirstAdjacentLayerOfDef(origin, PlanetLayerDefOf.Orbit, out var layer))
            {
                return false;
            }
            FastTileFinder.TileQueryParams query = new FastTileFinder.TileQueryParams(origin, 1f, 15f);
            return layer.FastTileFinder.Query(query).TryRandomElement(out tile);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            TryFindSiteTile(out var tile);
          
            WorldObject worldObject = WorldObjectMaker.MakeWorldObject(possibleObjects.RandomElement());
            
            if (worldObject is SpaceMapParent) {
                SpaceMapParent spaceMapParent = (SpaceMapParent)worldObject;
                spaceMapParent.preciousResource = mineables.RandomElement();
            }
            worldObject.Tile = tile;
            Find.WorldObjects.Add(worldObject);
            Window_RenameAsteroid renameWindow = new Window_RenameAsteroid(worldObject,this.parent.pawn);
            Find.WindowStack.Add(renameWindow);

        }
        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn = parent.pawn;

            if (target.Thing is null)
            {
                return false;
            }

            if (target.Thing.def != VGEDefOf.Telescope)
            {

                return false;
            }

            if (!pawn.CanReserve(target.Thing))
            {
                return false;
            }

            return true;
        }


    }
}
