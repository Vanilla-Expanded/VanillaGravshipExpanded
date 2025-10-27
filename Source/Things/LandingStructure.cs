using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCSG;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class LandingStructure : Thing
    {
        private int ticksToImpact;

        public int ticksToImpactMax;

        public SavedTexture2D capturedTexture;
        public Vector3 drawSize;
        public Vector3 captureCenter;
        public CellRect captureBounds;
        public KCSG.StructureLayoutDef layoutDef;
        public HashSet<Thing> thrusters = new HashSet<Thing>();
        public List<IntVec3> gravFieldExtenderPositions = new List<IntVec3>();
        public IntVec3 enginePos;
        public Rot4 landingRotation;
        public IntVec3 launchDirection;
        private static readonly int ShaderPropertyGravshipHeight = Shader.PropertyToID("_GravshipHeight");
        private static readonly int ShaderPropertyIsTakeoff = Shader.PropertyToID("_IsTakeoff");
        private static readonly int GravshipCaptureLayerMaskExclude = LayerMask.GetMask("UI", "GravshipExclude");
        private static readonly int GravshipCaptureLayerMaskInclude = LayerMask.GetMask("GravshipMask");
        private static readonly Material MatGravshipBlit = MatLoader.LoadMat("Map/Gravship/GravshipBlit");
        private static readonly Material MatGravshipChromaKey = MatLoader.LoadMatDirect("Map/Gravship/GravshipChromaKey");
        private static readonly Material MatGravshipDownwash = MatLoader.LoadMat("Map/Gravship/GravshipDownwash");
        private static readonly Material MatGravshipLensFlare = MatLoader.LoadMat("Map/Gravship/GravshipLensFlare");
        private static readonly Material MatGravFieldExtenderGlow = MatLoader.LoadMat("Map/Gravship/GravFieldExtenderGlow");
        private static readonly Material MatGravEngineGlow = MatLoader.LoadMat("Map/Gravship/GravEngineGlow");

        private MaterialPropertyBlock flareBlock;
        private MaterialPropertyBlock thrusterFlameBlock;
        static LandingStructure()
        {
            (VGEDefOf.VGE_FakeTerrain.graphic as Graphic_Single).mat = MatGravshipChromaKey;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            distortionBlock = new MaterialPropertyBlock();
            flareBlock = new MaterialPropertyBlock();
            thrusterFlameBlock = new MaterialPropertyBlock();
            exhaustFleckSystem = new FleckSystemThrown(map.flecks);
            {
                MatGravshipShadowFallback = new Material(MatGravship.shader);
                MatGravshipShadowFallback.mainTexture = null;
                MatGravshipShadowFallback.color = new Color(0f, 0f, 0f, 0.7f);
            }

            if (respawningAfterLoad is false)
            {
                ticksToImpact = ticksToImpactMax = 600;
                Find.CameraDriver.shaker.DoShake(0.2f, 120);
                Find.CameraDriver.StartCoroutine(CaptureGravshipCoroutine());
            }
        }

        public override void Tick()
        {
            base.Tick();
            ticksToImpact--;
            if (ticksToImpact <= 0)
            {
                Impact();
            }
            if (exhaustFleckSystem != null)
            {
                exhaustFleckSystem.Update(1);
            }

        }

        private IEnumerator CaptureGravshipCoroutine()
        {
            var maxSize = Mathf.Max(layoutDef.Sizes.x, layoutDef.Sizes.z) + 3;
            CreateTempMap(new IntVec3(maxSize, 1, maxSize), Map, out var mapParent, out var tempMap);
            var originalMap = Current.Game.CurrentMap;
            var mainCamera = Find.Camera;
            var cameraDriver = mainCamera.GetComponent<CameraDriver>();

            var wasCamDriverEnabled = cameraDriver.enabled;
            var wasCamEnabled = mainCamera.enabled;
            cameraDriver.enabled = false;
            mainCamera.enabled = false;
            Current.Game.CurrentMap = tempMap;
            yield return new WaitForEndOfFrame();
            CellRect cellRect = SpawnLayout(tempMap, tempMap.Center);

            Building_GravEngine engine = null;
            Building pilotConsole = null;
            foreach (var pos in cellRect)
            {
                foreach (var thing in pos.GetThingList(tempMap))
                {
                    if (thing.TryGetComp<CompPilotConsole>() != null)
                    {
                        pilotConsole = (Building)thing;
                    }
                    else if (thing.TryGetComp<CompGravshipThruster>() != null)
                    {
                        thrusters.Add(thing);
                    }
                    else if (thing.def == ThingDefOf.GravFieldExtender)
                    {
                        gravFieldExtenderPositions.Add(thing.Position);
                    }
                    else if (thing is Building_GravEngine gravEngine)
                    {
                        engine = gravEngine;
                        enginePos = thing.Position;
                    }
                }
            }

            this.launchDirection = IntVec3.Zero;
            foreach (var thruster in this.thrusters)
            {
                var comp = thruster.TryGetComp<CompGravshipThruster>();
                if (comp != null && comp.CanBeActive)
                {
                    this.launchDirection += thruster.Rotation.AsIntVec3 * comp.Props.directionInfluence;
                }
            }

            if (this.launchDirection == IntVec3.Zero && pilotConsole != null)
            {
                this.launchDirection = pilotConsole.Rotation.AsIntVec3;
            }

            if (engine != null)
            {
                this.landingRotation = engine.Rotation;
            }
            else
            {
                this.landingRotation = Rot4.Random;
            }

            captureBounds = CellRect.FromCellList(cellRect.Cells).ExpandedBy(1);
            var captureCam = GravshipCacheCameraManager.GravshipCacheCamera;
            captureCam.cullingMask = (mainCamera.cullingMask & ~GravshipCaptureLayerMaskExclude) | GravshipCaptureLayerMaskInclude;
            captureCam.Fit(captureBounds, 15f);
            drawSize = captureBounds.Size.ToVector3().WithY(1f);
            captureCenter = captureBounds.CenterVector3;

            int screenshotWidth = Mathf.RoundToInt((float)Screen.height * captureCam.aspect);
            int screenshotHeight = Screen.height;
            RenderTexture screenshot = RenderTexture.GetTemporary(screenshotWidth, screenshotHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1);
            captureCam.targetTexture = screenshot;
            captureCam.clearFlags = CameraClearFlags.Color;
            captureCam.backgroundColor = MatGravshipChromaKey.color;

            if (engine != null)
            {
                SectionLayer_GravshipMask.Engine = engine;
                SectionLayer_GravshipMask.OverrideMode = SectionLayer_GravshipMask.MaskOverrideMode.Gravship;
            }

            tempMap.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_GravshipMask));
            tempMap.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_GravshipHull));
            tempMap.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_SubstructureProps));

            if (engine != null)
            {
                SectionLayer_GravshipMask.OverrideMode = SectionLayer_GravshipMask.MaskOverrideMode.None;
            }

            MapUpdate(tempMap);
            captureCam.Render();

            RenderTexture temporary = RenderTexture.GetTemporary(screenshotWidth, screenshotHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
            Graphics.Blit(screenshot, temporary, MatGravshipBlit);
            capturedTexture = (SavedTexture2D)temporary.CreateTexture2D(TextureFormat.ARGB32, mipChain: true);
            capturedTexture.Texture.filterMode = FilterMode.Bilinear;

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(temporary);
            RenderTexture.ReleaseTemporary(screenshot);
            captureCam.targetTexture = null;
            captureCam.clearFlags = CameraClearFlags.Color;
            captureCam.backgroundColor = new Color(0, 0, 0, 0);

            Current.Game.CurrentMap = originalMap;
            mainCamera.enabled = wasCamEnabled;
            cameraDriver.enabled = wasCamDriverEnabled;
            Find.WorldObjects.Remove(mapParent);
            Find.Maps.Remove(tempMap);
        }

        public void MapUpdate(Map map)
        {
            SkyManagerUpdate(this.Map, map.skyManager);
            map.glowGrid.GlowGridUpdate_First();
            map.waterInfo.SetTextures();
            map.mapDrawer.MapMeshDrawerUpdate_First();
            map.mapDrawer.DrawMapMesh();
            map.dynamicDrawManager.DrawDynamicThings();
        }

        public void SkyManagerUpdate(Map original, SkyManager sky)
        {
            MatBases.LightOverlay.color = new Color(1f, 1f, 1f, 0f);
            MatBases.FogOfWar.color = SkyManager.FogOfWarBaseColor;
            sky.curSky = original.skyManager.CurSky;
            sky.curSkyGlowInt = original.skyManager.CurSkyGlow;
            sky.curSky.colors.sky = Color.Lerp(sky.curSky.colors.sky, Color.white, 0.5f);
            SkyTarget originalMapSkyTarget = sky.curSky;
            if (original.Biome != null && original.Biome.disableSkyLighting)
            {
                MatBases.LightOverlay.color = new Color(1f, 1f, 1f, 0f);
                MatBases.FogOfWar.color = original.FogOfWarColor ?? SkyManager.FogOfWarBaseColor;
            }
            else
            {
                MatBases.LightOverlay.color = originalMapSkyTarget.colors.sky;
                Find.CameraColor.saturation = originalMapSkyTarget.colors.saturation;
                Color skyColor = originalMapSkyTarget.colors.sky;
                skyColor.a = 1f;
                skyColor *= original.FogOfWarColor ?? SkyManager.FogOfWarBaseColor;
                MatBases.FogOfWar.color = skyColor;
            }
            Color shadowColor = originalMapSkyTarget.colors.shadow;
            Vector3? overridenShadowVector = original.skyManager.GetOverridenShadowVector();
            if (overridenShadowVector.HasValue)
            {
                sky.SetSunShadowVector(overridenShadowVector.Value);
            }
            else
            {
                sky.SetSunShadowVector(GenCelestial.GetLightSourceInfo(original, GenCelestial.LightType.Shadow).vector);
                shadowColor = Color.Lerp(Color.white, shadowColor, GenCelestial.CurShadowStrength(original));
            }
            GenCelestial.LightInfo lightSourceInfo = GenCelestial.GetLightSourceInfo(original, GenCelestial.LightType.LightingSun);
            GenCelestial.LightInfo lightSourceInfo2 = GenCelestial.GetLightSourceInfo(original, GenCelestial.LightType.LightingMoon);
            Shader.SetGlobalVector(ShaderPropertyIDs.WaterCastVectSun, new Vector4(lightSourceInfo.vector.x, 0f, lightSourceInfo.vector.y, lightSourceInfo.intensity));
            Shader.SetGlobalVector(ShaderPropertyIDs.WaterCastVectMoon, new Vector4(lightSourceInfo2.vector.x, 0f, lightSourceInfo2.vector.y, lightSourceInfo2.intensity));
            Shader.SetGlobalFloat(SkyManager.LightsourceShineSizeReduction, 20f * (1f / originalMapSkyTarget.lightsourceShineSize));
            Shader.SetGlobalFloat(SkyManager.LightsourceShineIntensity, originalMapSkyTarget.lightsourceShineIntensity);
            Shader.SetGlobalFloat(SkyManager.DayPercent, GenLocalDate.DayPercent(original));
            MatBases.SunShadow.color = shadowColor;
            MatBases.SunShadowFade.color = shadowColor;
            sky.UpdateOverlays(originalMapSkyTarget);
        }

        private CellRect SpawnLayout(Map map, IntVec3 position)
        {
            CellRect cellRect = CellRect.CenteredOn(position, layoutDef.Sizes.x, layoutDef.Sizes.z);
            GenOption.GetAllMineableIn(cellRect, map);
            LayoutUtils.CleanRect(layoutDef, map, cellRect, true);
            layoutDef.Generate(cellRect, map, Faction);
            return cellRect;
        }

        public void Impact()
        {
            SpawnLayout(Map, Position);
            Destroy(DestroyMode.Vanish);
        }

        public static void CreateTempMap(IntVec3 size, Map source, out MapParent mapParent, out Map map)
        {
            mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(VGEDefOf.VGE_GravshipGenerationSite);
            mapParent.Tile = source.Tile;
            mapParent.SetFaction(Faction.OfPlayer);
            Find.WorldObjects.Add(mapParent);
            map = MapGenerator.GenerateMap(size, mapParent, mapParent.MapGeneratorDef);
        }

        private static readonly Material MatGravship = MatLoader.LoadMat("Map/Gravship/Gravship");
        private static readonly Material MatGravshipShadow = MatLoader.LoadMat("Map/Gravship/GravshipShadow");
        private static readonly Material MatGravshipDistortion = MatLoader.LoadMat("Map/Gravship/GravshipDistortion");
        private static Material MatGravshipShadowFallback;
        private MaterialPropertyBlock distortionBlock;
        private FleckSystem exhaustFleckSystem;
        private Dictionary<Thing, EventQueue> exhaustTimers = new Dictionary<Thing, EventQueue>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref capturedTexture, "capturedTexture");
            Scribe_Values.Look(ref drawSize, "drawSize");
            Scribe_Values.Look(ref captureCenter, "captureCenter");
            Scribe_Values.Look(ref captureBounds, "captureBounds");
            Scribe_Collections.Look(ref thrusters, "thrusters", LookMode.Deep);
            Scribe_Collections.Look(ref gravFieldExtenderPositions, "gravFieldExtenderPositions", LookMode.Value);
            Scribe_Values.Look(ref enginePos, "enginePos");
            Scribe_Values.Look(ref landingRotation, "landingRotation");
            Scribe_Values.Look(ref launchDirection, "launchDirection");
            Scribe_Values.Look(ref ticksToImpact, "ticksToImpact");
            Scribe_Values.Look(ref ticksToImpactMax, "ticksToImpactMax");
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            DrawGravship(drawLoc + new Vector3(0, 0, -0.5f));
        }

        private void DrawGravship(Vector3 drawLoc)
        {
            if (capturedTexture?.Texture == null) return;

            float progress = 1f - (float)ticksToImpact / (float)ticksToImpactMax;
            progress = progress.RemapClamped(0f, 0.95f, 0f, 1f);
            float height = Mathf.Pow(1f - progress, 5f);

            Vector3 vector;
            Vector3 vector2;
            if (landingRotation == Rot4.North || landingRotation == Rot4.South)
            {
                vector = new Vector3(0f, 0f, 100f * height);
                vector2 = landingRotation.AsQuat * -launchDirection.ToVector3().normalized * 200f * height;
            }
            else
            {
                vector = new Vector3(0f, 0f, 200f * height);
                vector2 = landingRotation.AsQuat * -launchDirection.ToVector3().normalized * 100f * Mathf.Pow(1f - progress, 9f);
            }

            Vector3 groundEffectsCenter = drawLoc + vector2;
            Vector3 gravshipDrawCenter = (drawLoc + vector + vector2);
            gravshipDrawCenter.y = AltitudeLayer.Skyfaller.AltitudeFor();

            Vector3 vector3 = Find.Camera.WorldToViewportPoint(gravshipDrawCenter);
            distortionBlock.SetFloat(ShaderPropertyIDs.Progress, progress);
            distortionBlock.SetFloat(ShaderPropertyGravshipHeight, height);
            distortionBlock.SetVector(ShaderPropertyIDs.DrawPos, vector3);
            distortionBlock.SetFloat(ShaderPropertyIsTakeoff, 0f);
            DrawLayer(MatGravshipDistortion, Find.Camera.transform.position.SetToAltitude(AltitudeLayer.Weather).WithYOffset(0.07317074f), distortionBlock, Find.Camera);

            MatGravship.mainTexture = capturedTexture.Texture;
            MatGravship.color = Color.white;
            MatGravship.SetFloat(ShaderPropertyIDs.Progress, progress);
            MatGravship.SetFloat(ShaderPropertyGravshipHeight, height);
            MatGravship.SetFloat(ShaderPropertyIsTakeoff, 0f);
            GenDraw.DrawQuad(MatGravship, gravshipDrawCenter, landingRotation.AsQuat, this.drawSize);

            MatGravshipShadowFallback.mainTexture = capturedTexture.Texture;
            MatGravshipShadowFallback.SetFloat(ShaderPropertyIDs.Progress, 1f - progress);
            MatGravshipShadowFallback.SetFloat(ShaderPropertyGravshipHeight, height);
            MatGravshipShadowFallback.SetFloat(ShaderPropertyIsTakeoff, 0f);
            MatGravshipShadowFallback.color = MatGravshipShadow.color.WithAlpha(progress.RemapClamped(0.9f, 1f, 1f, 0f));
            float shadowAlpha = progress.RemapClamped(0.9f, 1f, 0.35f, 0f);

            Vector3 shadowPos = (drawLoc + vector2).SetToAltitude(AltitudeLayer.Gas).WithYOffset(0.03658537f);
            float blurOffset = 0.15f;
            Vector3[] offsets = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(blurOffset, 0, 0),
                new Vector3(-blurOffset, 0, 0),
                new Vector3(0, 0, blurOffset),
                new Vector3(0, 0, -blurOffset),
            };

            
            if (progress > 0f && !base.Map.Biome.inVacuum)
            {
                MatGravshipDownwash.SetFloat(ShaderPropertyIDs.Progress, progress);
                MatGravshipDownwash.SetFloat(ShaderPropertyGravshipHeight, height);
                MatGravshipDownwash.SetVector(ShaderPropertyIDs.DrawPos, Find.Camera.WorldToViewportPoint(groundEffectsCenter));
                MatGravshipDownwash.SetFloat(ShaderPropertyIsTakeoff, 0f);
                DrawLayer(MatGravshipDownwash, Find.Camera.transform.position.SetToAltitude(AltitudeLayer.Gas).WithYOffset(0.03658537f), null, Find.Camera);
            }

            foreach (var offset in offsets)
            {
                MatGravshipShadowFallback.color = new Color(0.1f, 0.1f, 0.1f, shadowAlpha / offsets.Length);
                GenDraw.DrawQuad(MatGravshipShadowFallback, shadowPos + offset, Quaternion.identity, this.drawSize * 1.08f);
            }

            if (thrusters.NullOrEmpty()) return;

            Color value = new Color(1f, 1f, 1f, 1f);
            value *= Mathf.Lerp(0.75f, 1f, Mathf.PerlinNoise1D(progress * 100f));
            value.a = Mathf.InverseLerp(0f, 0.1f, (1f - progress));

            foreach (var thruster in thrusters)
            {
                var comp = thruster.TryGetComp<CompGravshipThruster>();
                if (comp != null)
                {
                    var props = comp.Props;
                    float num = (float)thruster.def.size.x * props.flameSize;
                    Vector3 vector4 = thruster.Rotation.AsQuat * props.flameOffsetsPerDirection[thruster.Rotation.AsInt];
                    Vector3 vector5 = GenThing.TrueCenter(thruster.Position, thruster.Rotation, thruster.def.size, 0f) - thruster.Rotation.AsIntVec3.ToVector3() * ((float)thruster.def.size.z * 0.5f + num * 0.5f) + vector4;
                    Vector3 position2 = (gravshipDrawCenter + (vector5 - captureCenter)).SetToAltitude(AltitudeLayer.Skyfaller).WithYOffset(0.07317074f);
                    MaterialRequest req = new MaterialRequest(props.FlameShaderType.Shader);
                    req.renderQueue = 3201;
                    Material mat = MaterialPool.MatFrom(req);
                    thrusterFlameBlock.Clear();
                    thrusterFlameBlock.SetColor("_Color2", value);
                    foreach (ShaderParameter flameShaderParameter in props.flameShaderParameters)
                    {
                        flameShaderParameter.Apply(thrusterFlameBlock);
                    }
                    GenDraw.DrawQuad(mat, position2, landingRotation.AsQuat * thruster.Rotation.AsQuat, num, thrusterFlameBlock);

                    Vector3 vector6 = Find.Camera.WorldToViewportPoint(position2);
                    flareBlock.SetVector(ShaderPropertyIDs.DrawPos, vector6);
                    MatGravshipLensFlare.SetColor("_Color2", value);
                    DrawLayer(MatGravshipLensFlare, Find.Camera.transform.position.SetToAltitude(AltitudeLayer.MetaOverlays).WithYOffset(0.03658537f), flareBlock, Find.Camera);

                    if (props.exhaustSettings.enabled)
                    {
                        if (!exhaustTimers.ContainsKey(thruster))
                        {
                            exhaustFleckSystem.handledDefs.AddUnique(props.exhaustSettings.ExhaustFleckDef);
                            exhaustTimers.Add(thruster, new EventQueue(1f / props.exhaustSettings.emissionsPerSecond));
                        }
                        EventQueue eventQueue = exhaustTimers[thruster];
                        eventQueue.Push(Time.deltaTime);
                        while (eventQueue.Pop())
                        {
                            EmitSmoke(props.exhaustSettings, position2, landingRotation.AsQuat, thruster.Rotation.AsQuat);
                        }
                    }
                }
            }

            MatGravFieldExtenderGlow.SetColor("_Color2", value);
            foreach (IntVec3 gravFieldExtenderPosition in gravFieldExtenderPositions)
            {
                Vector3 vector7 = gravFieldExtenderPosition.ToVector3() + ThingDefOf.GravFieldExtender.graphicData.drawSize.ToVector3() * 0.5f;
                Vector3 position3 = (gravshipDrawCenter + (vector7 - captureCenter)).SetToAltitude(AltitudeLayer.MetaOverlays).WithYOffset(0.07317074f);
                GenDraw.DrawQuad(MatGravFieldExtenderGlow, position3, Quaternion.identity, 8f);
            }

            MatGravEngineGlow.SetColor("_Color2", value);
            Vector3 position4 = (gravshipDrawCenter + (enginePos.ToVector3() + new Vector3(0.5f, 0, 0.5f) - captureCenter)).SetToAltitude(AltitudeLayer.MetaOverlays).WithYOffset(0.07317074f);
            GenDraw.DrawQuad(MatGravEngineGlow, position4, Quaternion.identity, 12.5f);
        }

        private void EmitSmoke(CompProperties_GravshipThruster.ExhaustSettings settings, Vector3 position, Quaternion gravshipRotation, Quaternion thrusterRotation)
        {
            Quaternion quaternion = Quaternion.identity;
            if (settings.inheritThrusterRotation)
            {
                quaternion = thrusterRotation * quaternion;
            }
            if (settings.inheritGravshipRotation)
            {
                quaternion = gravshipRotation * quaternion;
            }
            exhaustFleckSystem.CreateFleck(new FleckCreationData
            {
                def = settings.ExhaustFleckDef,
                spawnPosition = position + quaternion * settings.spawnOffset + Random.insideUnitSphere.WithY(0f).normalized * settings.spawnRadiusRange.RandomInRange,
                scale = settings.scaleRange.RandomInRange,
                velocity = quaternion * Quaternion.Euler(0f, settings.velocityRotationRange.RandomInRange, 0f) * (settings.velocity * settings.velocityMultiplierRange.RandomInRange),
                rotationRate = settings.rotationOverTimeRange.RandomInRange,
                ageTicksOverride = -1
            });
        }

        private void DrawLayer(Material mat, Vector3 position, MaterialPropertyBlock props, Camera camera)
        {
            float num = camera.orthographicSize * 2f;
            Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(num * camera.aspect, 1f, num), pos: position, q: Quaternion.identity);
            Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0, null, 0, props);
        }
    }
}
