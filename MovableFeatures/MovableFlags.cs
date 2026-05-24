using System;

namespace MovableFeatures
{
    [Flags]
    public enum MovableFlags
    {
        None = 0,
        
        BannedCrossPlantMove = 1 << 0,
        HaveNeutronium = 1 << 1,
        WarpConduit  = 1 << 2 | BannedCrossPlantMove | JustCreateNew,
        LonelyMinionHouse = 1 << 3,
        JustCreateNew = 1 << 4,
        SapTree = 1 << 5 | JustCreateNew,
        HaveRadiationEmitter = 1 << 6,
        LonelyMinionMailbox = 1 << 7,
        IsGravitasCreatureManipulator = 1 << 8,
        IsGeyser =  1 << 9 | HaveNeutronium,
    }
}