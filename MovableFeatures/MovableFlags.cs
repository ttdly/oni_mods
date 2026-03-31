using System;

namespace MovableFeatures
{
    [Flags]
    public enum MovableFlags
    {
        None = 0,
        
        BannedCrossPlantMove = 1 << 0,
        HaveNeutronium = 1 << 1,
        IsWarpConduit  = 1 << 2,
        LonelyMinion = 1 << 3,
        
        WarpConduit = BannedCrossPlantMove | IsWarpConduit
    }
}