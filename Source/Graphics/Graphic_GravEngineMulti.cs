using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class Graphic_GravEngineMulti : Graphic_Multi, IGravEngineGraphic
{
    public Graphic_Multi cooldownGraphic;

    public CachedMaterial OrbNormalMat { get; private set; }
    public CachedMaterial OrbCooldownMat { get; private set; }

    public override Material MatAt(Rot4 rot, Thing thing = null)
    {
        if (thing is not Building_GravEngine gravEngine || Find.TickManager.TicksGame >= gravEngine.cooldownCompleteTick)
            return base.MatAt(rot, thing);
        return cooldownGraphic.MatAt(rot, thing);
    }

    public override Material MatSingleFor(Thing thing)
    {
        if (thing is not Building_GravEngine gravEngine || Find.TickManager.TicksGame >= gravEngine.cooldownCompleteTick)
            return base.MatSingleFor(thing);
        return cooldownGraphic.MatSingleFor(thing);
    }

    public override void TryInsertIntoAtlas(TextureAtlasGroup groupKey)
    {
        base.TryInsertIntoAtlas(groupKey);
        cooldownGraphic.TryInsertIntoAtlas(groupKey);
    }

    public override void Init(GraphicRequest req)
    {
        OrbNormalMat = new CachedMaterial($"{req.path}_Orb", ShaderDatabase.Cutout);
        OrbCooldownMat = new CachedMaterial($"{req.path}_Orb_Cooldown", ShaderDatabase.Cutout);

        base.Init(req);
        req.path += "_Cooldown";

        cooldownGraphic = new Graphic_Multi();
        cooldownGraphic.Init(req);
    }

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        => GraphicDatabase.Get<Graphic_GravEngineMulti>(path, newShader, drawSize, newColor, newColorTwo, data);

    public override string ToString()
        => $"{nameof(Graphic_GravEngineMulti)}(base=({base.ToString()}), cooldown=({cooldownGraphic}))";
}