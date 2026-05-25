using System.Collections.Generic;
using System.Reflection;
using CLog = GlobalUtil.Logger;
using static MovableFeatures.MovableFlags;

namespace MovableFeatures
{
    public class PatchRegistry
    {
        public static Dictionary<MethodBase, PatchContext> PatchContexts = new Dictionary<MethodBase, PatchContext>();

        public static readonly Dictionary<Tag, PatchContext> BuildingMovableContexts =
            new Dictionary<Tag, PatchContext>();

        public static readonly Dictionary<Tag, PatchContext>
            EntityMovableContexts = new Dictionary<Tag, PatchContext>();

        private static void RegisterBuilding<TPatchType>(
            string prefabId,
            MovableFlags flags = MovableFlags.None)
        {
            if (prefabId == null) return;

            var patchContext = new PatchContext
            {
                Flags = flags
            };
            BuildingMovableContexts.Add(new Tag(prefabId), patchContext);
#if DEBUG && false
            CLog.Info($"已注册 {typeof(TPatchType)}");
#endif
        }

        private static void RegisterEntity<TPatchType>(
            string prefabId,
            MovableFlags flags = MovableFlags.None)
        {
            if (prefabId == null) return;

            var patchContext = new PatchContext
            {
                Flags = flags
            };
            EntityMovableContexts.Add(new Tag(prefabId), patchContext);
#if DEBUG && false
            CLog.Info($"已注册 {typeof(TPatchType)}");
#endif
        }

        public static void RegisterPatches()
        {
            // 打印仓
            RegisterBuilding<HeadquartersConfig>(HeadquartersConfig.ID, BannedCrossPlantMove);
            // 便携式打印舱
            RegisterBuilding<ExobaseHeadquartersConfig>(ExobaseHeadquartersConfig.ID, BannedCrossPlantMove);
            // 时空裂口开放器
            RegisterBuilding<TemporalTearOpenerConfig>(TemporalTearOpenerConfig.ID);
            // 供给传送器输出端
            RegisterBuilding<WarpConduitSenderConfig>(WarpConduitSenderConfig.ID, WarpConduit);
            // 供给传送器输入端
            RegisterBuilding<WarpConduitReceiverConfig>(WarpConduitReceiverConfig.ID, WarpConduit);
            // 神经振荡仪
            RegisterEntity<GeneShufflerConfig>("GeneShuffler");
            // 反熵热量中和器
            RegisterBuilding<MassiveHeatSinkConfig>(MassiveHeatSinkConfig.ID);
            // 睡衣柜 
            RegisterBuilding<GravitasContainerConfig>(GravitasContainerConfig.ID);
            // 柴堆
            RegisterBuilding<WoodStorageConfig>(WoodStorageConfig.ID);
            // 储油石
            RegisterEntity<OilWellConfig>(OilWellConfig.ID);
            // 试验体52B
            RegisterEntity<SapTreeConfig>(SapTreeConfig.ID, MovableFlags.SapTree);
            // 辐射蜂巢
            RegisterEntity<BaseBeeHiveConfig>(BaseBeeHiveConfig.ID);
            // 流明石英
            RegisterEntity<PinkRockConfig>("PinkRock");
            // 低温箱3000
            RegisterEntity<CryoTankConfig>(CryoTankConfig.ID);
            // 传送器
            RegisterEntity<WarpPortalConfig>(WarpPortalConfig.ID, BannedCrossPlantMove);
            // 接收器
            RegisterEntity<WarpReceiverConfig>(WarpReceiverConfig.ID, BannedCrossPlantMove);
            // 地热排气孔
            RegisterEntity<GeothermalVentConfig>(GeothermalVentConfig.ID, IsGeothermalVent);
            // 地热热泵
            RegisterEntity<GeothermalControllerConfig>(GeothermalControllerConfig.ID, IsGeothermalController);
            // 坠毁卫星1
            RegisterEntity<PropSurfaceSatellite1Config>(PropSurfaceSatellite1Config.ID);
            // 坠毁卫星2
            RegisterEntity<PropSurfaceSatellite2Config>(PropSurfaceSatellite2Config.ID);
            // 坠毁卫星3
            RegisterEntity<PropSurfaceSatellite3Config>(PropSurfaceSatellite3Config.ID);
            // 小动物衍变器
            RegisterBuilding<GravitasCreatureManipulatorConfig>(GravitasCreatureManipulatorConfig.ID);
            // 梦境合成仪器
            RegisterBuilding<MegaBrainTankConfig>(MegaBrainTankConfig.ID);
            // LED灯
            RegisterBuilding<GravitasLabLightConfig>(GravitasLabLightConfig.ID);
            // 神秘隐士
            RegisterBuilding<LonelyMinionHouseConfig>(LonelyMinionHouseConfig.ID, MovableFlags.LonelyMinionHouse);
            RegisterBuilding<LonelyMinionMailboxConfig>(LonelyMinionMailboxConfig.ID, MovableFlags.LonelyMinionMailbox);
            // 远古标本
            RegisterEntity<FossilSiteConfig_Ice>(FossilSiteConfig_Ice.ID);
            RegisterEntity<FossilSiteConfig_Resin>(FossilSiteConfig_Resin.ID);
            RegisterEntity<FossilSiteConfig_Rock>(FossilSiteConfig_Rock.ID);
            RegisterBuilding<FossilDigSiteConfig>(FossilDigSiteConfig.ID);
            // 生物织构仪
            RegisterBuilding<MorbRoverMakerConfig>(MorbRoverMakerConfig.ID);
            // 打印截能仪器
            RegisterBuilding<HijackedHeadquartersConfig>(HijackedHeadquartersConfig.ID);
            // 展厅门
            RegisterBuilding<POIDlc2ShowroomDoorConfig>(POIDlc2ShowroomDoorConfig.ID);
            // 安全门
            RegisterBuilding<POIDoorInternalConfig>(POIDoorInternalConfig.ID);
            // 安全门2
            RegisterBuilding<POIBunkerExteriorDoor>(POIBunkerExteriorDoor.ID);
        }

        public class PatchContext
        {
            public MovableFlags Flags { get; set; }
        }
    }
}