using PipeSystem;

namespace VanillaGravshipExpanded;

public class OxygenPipeNet : PipeNet
{
    public bool noAtmosphere;

    public override void PostMake()
    {
        base.PostMake();

        noAtmosphere = map.Biome.inVacuum;
    }
}