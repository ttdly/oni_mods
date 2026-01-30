using System;
using System.Collections.Generic;

namespace MovableFeatures
{
    public class GridZLayerLookup
    {
        private static readonly Dictionary<float, Grid.SceneLayer> Table;

        static GridZLayerLookup()
        {
            Table = new Dictionary<float, Grid.SceneLayer>();
            var values = (Grid.SceneLayer[])Enum.GetValues(typeof(Grid.SceneLayer));
            if (values.Length == 0) return;
            foreach (var sceneLayer in values)
            {
                var z = Grid.GetLayerZ(sceneLayer);
                Table.Add(z, sceneLayer);
            }
        }

        public static Grid.SceneLayer Lookup(float z)
        {
           return (Table.TryGetValue(z, out var layer)) ? layer : Grid.SceneLayer.Building;
        }
    }
}