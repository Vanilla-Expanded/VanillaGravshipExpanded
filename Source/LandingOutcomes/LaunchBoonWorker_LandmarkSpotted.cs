using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LaunchBoonWorker_LandmarkSpotted : LaunchBoonWorker
    {
        public LaunchBoonWorker_LandmarkSpotted(LaunchBoonDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            return FindValidLandmarkTile(gravship.Engine.Tile, out _, out _);
        }

        public override void ApplyBoon(Gravship gravship)
        {
            if (FindValidLandmarkTile(gravship.Engine.Tile, out PlanetTile landmarkTile, out LandmarkDef landmarkDef))
            {
                Find.World.landmarks.AddLandmark(landmarkDef, landmarkTile, landmarkTile.Layer);
                var engine = gravship.Engine;
                var text = LetterText.Formatted(engine.RenamableLabel.Named("GRAVSHIP"), engine.launchInfo.pilot.Named("PILOT"), engine.launchInfo.copilot.Named("COPILOT"), landmarkDef.label.Named("LANDMARK"));
                SendStandardLetter(gravship.Engine, null, new LookTargets(landmarkTile), text);
            }
        }

        private bool FindValidLandmarkTile(PlanetTile originTile, out PlanetTile result, out LandmarkDef landmarkDef)
        {
            if (originTile.Layer is not SurfaceLayer)
            {
                var surfaceLayer = originTile.Layer.connections.Keys.FirstOrDefault(l => l is SurfaceLayer);
                if (surfaceLayer == null)
                {
                    result = PlanetTile.Invalid;
                    landmarkDef = null;
                    return false;
                }
                originTile = surfaceLayer.GetClosestTile_NewTemp(originTile);
                if (originTile == PlanetTile.Invalid)
                {
                    result = PlanetTile.Invalid;
                    landmarkDef = null;
                    return false;
                }
            }
            result = PlanetTile.Invalid;
            landmarkDef = null;
            
            bool foundTile = TileFinder.TryFindTileWithDistance(originTile, 1, 5, out result,
                (PlanetTile tile) => Find.World.landmarks[tile] == null &&
                                   tile.Layer[tile]?.PrimaryBiome != null &&
                                   !tile.Layer[tile].PrimaryBiome.impassable &&
                                   tile.Layer[tile].hilliness != Hilliness.Impassable &&
                                   GetValidLandmarks(tile).Any(),
                TileFinderMode.Random, exitOnFirstTileFound: false);
            
            if (foundTile)
            {
                var resultTile = result;
                var validLandmarks = GetValidLandmarks(resultTile).ToList();
                
                if (validLandmarks.Count > 0)
                {
                    landmarkDef = validLandmarks.RandomElement();
                    return landmarkDef != null;
                }
            }
            
            return false;
        }
        
        private IEnumerable<LandmarkDef> GetValidLandmarks(PlanetTile tile)
        {
            return DefDatabase<LandmarkDef>.AllDefsListForReading.Where(ld => ld.IsValidTile(tile, tile.Layer));
        }
    }
}
