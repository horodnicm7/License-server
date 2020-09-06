using System;
using System.Collections.Generic;
using System.Text;

public class PlayerStats {
    private Dictionary<byte, Stats> unitStats;

    public PlayerStats() {
        this.unitStats = new Dictionary<byte, Stats>() {
            { EntityType.SWORDSMAN, new Stats(hp: 75, attack: 8, upgradedAttack: 0, range: 1, fieldOfView: 20, swordArmor: 1, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.SPEARMAN, new Stats(hp: 55, attack: 9, upgradedAttack: 0, range: 2, fieldOfView: 20, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.ELITE_SWORDSMAN, new Stats(hp: 80, attack: 14, upgradedAttack: 0, range: 1, fieldOfView: 20, swordArmor: 2, arrowArmor: 2, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.CATAPULT, new Stats(hp: 100, attack: 150, upgradedAttack: 0, range: 8, fieldOfView: 40, swordArmor: 2, arrowArmor: 2, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.ARCHER, new Stats(hp: 45, attack: 5, upgradedAttack: 0, range: 8, fieldOfView: 28, swordArmor: 0, arrowArmor: 1, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.VILLAGER, new Stats(hp: 40, attack: 2, upgradedAttack: 0, range: 1, fieldOfView: 16, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },

            { EntityType.BARRACKS, new Stats(hp: 1500, attack: 0, upgradedAttack: 0, range: 0, fieldOfView: 20, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.HOUSE, new Stats(hp: 900, attack: 0, upgradedAttack: 0, range: 0, fieldOfView: 12, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.GUARD_TOWER, new Stats(hp: 2350, attack: 5, upgradedAttack: 0, range: 16, fieldOfView: 40, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.CASTLE, new Stats(hp: 6800, attack: 9, upgradedAttack: 0, range: 8, fieldOfView: 48, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.KEEP, new Stats(hp: 4550, attack: 7, upgradedAttack: 0, range: 8, fieldOfView: 48, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.BLACKSMITH, new Stats(hp: 1700, attack: 0, upgradedAttack: 0, range: 0, fieldOfView: 20, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.STONE_WALL, new Stats(hp: 1800, attack: 0, upgradedAttack: 0, range: 0, fieldOfView: 16, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.STONE_WALL_CORNER, new Stats(hp: 1800, attack: 0, upgradedAttack: 0, range: 0, fieldOfView: 12, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.STONE_GATE, new Stats(hp: 3000, attack: 0, upgradedAttack: 0, range: 0, fieldOfView: 16, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.WOOD_WALL, new Stats(hp: 900, attack: 0, upgradedAttack: 0, range: 0, fieldOfView: 16, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.MINE_CAMP, new Stats(hp: 1200, attack: 0, upgradedAttack: 0, range: 0, fieldOfView: 16, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.ARCHERY_RANGE, new Stats(hp: 1900, attack: 0, upgradedAttack: 0, range: 0, fieldOfView: 20, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.BARN, new Stats(hp: 1100, attack: 0, upgradedAttack: 0, range: 0, fieldOfView: 16, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) },
            { EntityType.TOWN_HALL, new Stats(hp: 2500, attack: 0, upgradedAttack: 0, range: 0, fieldOfView: 40, swordArmor: 0, arrowArmor: 0, upgradedSwordArmor: 0, upgradedArrowArmor: 0, level: 0) }
        };
    }

    public Stats map(byte id) {
        if (!this.unitStats.ContainsKey(id)) {
            return null;
        }

        return this.unitStats[id];
    }

    public void upgradeStats(byte id, Stats stats) {
        if (!this.unitStats.ContainsKey(id)) {
            return;
        }

        this.unitStats[id] = stats;
    }

    public void upgradeArrowArmor(byte id, byte upgrade) {
        if (!this.unitStats.ContainsKey(id)) {
            return;
        }

        this.unitStats[id].upgradedArrowArmor += upgrade;
    }

    public void upgradeSwordArmor(byte id, byte upgrade) {
        if (!this.unitStats.ContainsKey(id)) {
            return;
        }

        this.unitStats[id].upgradedSwordArmor += upgrade;
    }

    public void upgradeAttack(byte id, short upgrade) {
        if (!this.unitStats.ContainsKey(id)) {
            return;
        }

        this.unitStats[id].upgradedAttack += upgrade;
    }

    public void upgradeRange(byte id, byte upgrade) {
        if (!this.unitStats.ContainsKey(id)) {
            return;
        }

        this.unitStats[id].upgradedRange += upgrade;
    }

    public void upgradeFieldOfView(byte id, byte upgrade) {
        if (!this.unitStats.ContainsKey(id)) {
            return;
        }

        this.unitStats[id].fieldOfView = upgrade;
    }

    public void upgradeLevel(byte id, byte upgrade) {
        if (!this.unitStats.ContainsKey(id)) {
            return;
        }

        this.unitStats[id].level = upgrade;
    }

    public void upgradeHp(byte id, short upgrade) {
        if (!this.unitStats.ContainsKey(id)) {
            return;
        }

        this.unitStats[id].hp = upgrade;
    }

    public void upgradeStandardAttack(byte id, short upgrade) {
        if (!this.unitStats.ContainsKey(id)) {
            return;
        }

        this.unitStats[id].attack = upgrade;
    }

    public void upgradeStandardSwordArmor(byte id, byte upgrade) {
        if (!this.unitStats.ContainsKey(id)) {
            return;
        }

        this.unitStats[id].swordArmor = upgrade;
    }

    public void upgradeStandardArrowArmor(byte id, byte upgrade) {
        if (!this.unitStats.ContainsKey(id)) {
            return;
        }

        this.unitStats[id].arrowArmor = upgrade;
    }
}
