using System;
using System.Collections.Generic;

public static class SizeMapping {
    private static Dictionary<byte, Size> mapping = new Dictionary<byte, Size> {
        { EntityType.BARRACKS, new Size(5, 5) },
        { EntityType.GOLD_MINE, new Size(3, 3) },
        { EntityType.STONE_MINE, new Size(3, 3) },
        { EntityType.TREE_TYPE1, new Size(3, 3) },
        { EntityType.TREE_TYPE2, new Size(3, 3) },
        { EntityType.TREE_TYPE3, new Size(3, 3) },
        { EntityType.WALL, new Size(2, 2) },
        { EntityType.GATE, new Size(6, 2) },
        { EntityType.HOUSE, new Size(3, 3) },
        { EntityType.GUARD_TOWER, new Size(2, 2) },
        { EntityType.VILLAGER, new Size(1, 1) }
    };

    public static Size map(byte type) {
        if (!SizeMapping.mapping.ContainsKey(type)) {
            return null;
        }

        return SizeMapping.mapping[type];
    }
}
