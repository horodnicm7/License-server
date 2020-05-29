using System;
using System.Collections.Generic;

public static class SizeMapping {
    private static Dictionary<byte, Size> mapping = new Dictionary<byte, Size> {
        { EntityType.BARRACKS, new Size(10, 10) },
        { EntityType.GOLD_MINE, new Size(3, 3) },
        { EntityType.STONE_MINE, new Size(3, 3) },
        { EntityType.TREE_TYPE1, new Size(3, 3) },
        { EntityType.TREE_TYPE2, new Size(3, 3) },
        { EntityType.TREE_TYPE3, new Size(3, 3) },
        { EntityType.HOUSE, new Size(6, 6) },
        { EntityType.GUARD_TOWER, new Size(5, 5) },
        { EntityType.VILLAGER, new Size(1, 1) },
        { EntityType.SWORDSMAN, new Size(1, 1) },
        { EntityType.BARN, new Size(7, 7) },
        { EntityType.BLACKSMITH, new Size(10, 10) },
        { EntityType.CASTLE, new Size(12, 12) },
        { EntityType.KEEP, new Size(12, 12) },
        { EntityType.SIEGE_WORKSHOP, new Size(10, 10) },
        { EntityType.STONE_GATE, new Size(9, 3) },
        { EntityType.STONE_WALL, new Size(9, 3) },
        { EntityType.STONE_WALL_CORNER, new Size(4, 4) },
        { EntityType.WOOD_WALL, new Size(9, 3) },
        { EntityType.WOOD_CAMP, new Size(6, 6) },
        { EntityType.STABLES, new Size(10, 10) },
        { EntityType.CAVALRY_SWORD, new Size(2, 2) },
        { EntityType.ELITE_SWORDSMAN, new Size(1, 1) },
        { EntityType.TOWN_HALL, new Size(12, 12) },
        { EntityType.SPEARMAN, new Size(1, 1) },
        { EntityType.MINE_CAMP, new Size(6, 6) },
        { EntityType.CATAPULT, new Size(3, 3) },
        { EntityType.ARCHER, new Size(1, 1) },
        { EntityType.ARCHERY_RANGE, new Size(10, 10) }
    };

    public static Size map(byte type) {
        if (!SizeMapping.mapping.ContainsKey(type)) {
            return null;
        }

        return SizeMapping.mapping[type];
    }
}
