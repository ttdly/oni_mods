using System.Collections.Generic;
using System.Reflection;
using CLog = GlobalUtil.Logger;

namespace MovableFeatures
{
    public class PatchRegistry
    {
        public enum GameObjectType
        {
            Building = 0,
            Entity = 1
        }

        public static Dictionary<MethodBase, PatchContext> PatchContexts = new Dictionary<MethodBase, PatchContext>();
        public static Dictionary<Tag, PatchContext> BuildingMovableContexts = new Dictionary<Tag, PatchContext>();
        public static Dictionary<Tag, PatchContext> EntityMovableContexts = new Dictionary<Tag, PatchContext>();
        
        public static void Register<TPatchType>(
            GameObjectType gameObjectType = GameObjectType.Entity,
            bool crossMove = true,
            bool geyser = false,
            bool warpConduit = false,
            bool lonelyMinion = false,
            string prefabId = null)
        {
            if (prefabId == null) return;

            var patchContext = new PatchContext
            {
                GameObjectType = gameObjectType,
                CrossMove = crossMove,
                Neutronium = geyser,
                WarpConduit = warpConduit,
                LonelyMinion = lonelyMinion
            };
            switch (gameObjectType)
            {
                case GameObjectType.Building:
                    BuildingMovableContexts.Add(new Tag(prefabId), patchContext);            
                    break;
                case GameObjectType.Entity:
                    EntityMovableContexts.Add(new Tag(prefabId), patchContext);
                    break;
                default:
                    CLog.Error($"不支持的注册类型 {gameObjectType}");
                    break;
            }
#if DEBUG
            CLog.Info($"已注册 {typeof(TPatchType)}");
#endif
        }

        public static void RegisterPatches()
        {
            // 打印仓
            Register<HeadquartersConfig>(GameObjectType.Building, false, prefabId: HeadquartersConfig.ID);
            // 便携式打印舱
            Register<ExobaseHeadquartersConfig>(GameObjectType.Building, false, prefabId: ExobaseHeadquartersConfig.ID);
            // 时空裂口开放器
            Register<TemporalTearOpenerConfig>(GameObjectType.Building, prefabId: TemporalTearOpenerConfig.ID);
            // 供给传送器输出端
            Register<WarpConduitSenderConfig>(GameObjectType.Building, warpConduit: true,
                prefabId: WarpConduitSenderConfig.ID);
            // 供给传送器输入端
            Register<WarpConduitReceiverConfig>(GameObjectType.Building, warpConduit: true,
                prefabId: WarpConduitReceiverConfig.ID);
            // 神经振荡仪
            Register<GeneShufflerConfig>(prefabId: "GeneShuffler");
            // 反熵热量中和器
            Register<MassiveHeatSinkConfig>(GameObjectType.Building, prefabId: MassiveHeatSinkConfig.ID);
            // 睡衣柜 
            Register<GravitasContainerConfig>(GameObjectType.Building, prefabId: GravitasContainerConfig.ID);
            // 柴堆
            Register<WoodStorageConfig>(GameObjectType.Building, prefabId: WoodStorageConfig.ID);
            // 储油石
            Register<OilWellConfig>(prefabId: OilWellConfig.ID);
            // 试验体52B
            Register<SapTreeConfig>(prefabId: SapTreeConfig.ID);
            // 辐射蜂巢
            Register<BaseBeeHiveConfig>(prefabId: BaseBeeHiveConfig.ID);
            // 流明石英
            Register<PinkRockConfig>(prefabId: "PinkRock");
            // 低温箱3000
            Register<CryoTankConfig>(prefabId: CryoTankConfig.ID);
            // 传送器
            Register<WarpPortalConfig>(prefabId: WarpPortalConfig.ID);
            // 接收器
            Register<WarpReceiverConfig>(prefabId: WarpReceiverConfig.ID);
            // 地热排气孔
            Register<GeothermalVentConfig>(prefabId: GeothermalVentConfig.ID);
            // 地热热泵
            Register<GeothermalControllerConfig>(GameObjectType.Building, prefabId: GeothermalControllerConfig.ID);
            // 坠毁卫星1
            Register<PropSurfaceSatellite1Config>(prefabId: PropSurfaceSatellite1Config.ID);
            // 坠毁卫星2
            Register<PropSurfaceSatellite2Config>(prefabId: PropSurfaceSatellite2Config.ID);
            // 坠毁卫星3
            Register<PropSurfaceSatellite3Config>(prefabId: PropSurfaceSatellite3Config.ID);
            // 小动物衍变器
            Register<GravitasCreatureManipulatorConfig>(GameObjectType.Building, 
                prefabId: GravitasCreatureManipulatorConfig.ID);
            // 梦境合成仪器
            Register<MegaBrainTankConfig>(GameObjectType.Building, prefabId: MegaBrainTankConfig.ID);
            // 神秘隐士
            Register<LonelyMinionHouseConfig>(GameObjectType.Building, lonelyMinion: true,
                prefabId: LonelyMinionHouseConfig.ID);
            // 远古标本
            Register<FossilSiteConfig_Ice>(prefabId: FossilSiteConfig_Ice.ID);
            Register<FossilSiteConfig_Resin>(prefabId: FossilSiteConfig_Resin.ID);
            Register<FossilSiteConfig_Rock>(prefabId: FossilSiteConfig_Rock.ID);
            Register<FossilDigSiteConfig>(GameObjectType.Building, prefabId: FossilDigSiteConfig.ID);
            // 生物织构仪
            Register<MorbRoverMakerConfig>(GameObjectType.Building, prefabId: MorbRoverMakerConfig.ID);
            // 打印截能仪器
            Register<HijackedHeadquartersConfig>(GameObjectType.Building, prefabId: HijackedHeadquartersConfig.ID);
        }
        
        public class PatchContext
        {
            public GameObjectType GameObjectType { get; set; }
            public bool CrossMove { get; set; }
            public bool Neutronium { get; set; }
            public bool WarpConduit { get; set; }
            public bool LonelyMinion { get; set; }
        }

    }
}