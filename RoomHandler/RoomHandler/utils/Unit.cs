using System;
using System.Collections.Generic;
using System.Text;

public class Unit {
    public Vector3 position;
    public short rotationWhole;
    public short rotationFractional;
    public int gridIndex;
    public byte type;

    public Unit(Vector3 position, short rotationWhole, short rotationFractional, byte type, int gridIndex) {
        this.position = position;
        this.rotationWhole = rotationWhole;
        this.rotationFractional = rotationFractional;
        this.type = type;

        this.gridIndex = gridIndex;
    }

    public static bool isBuilding(byte unitType) {
        return Unit.isInBounds(unitType, 1, 30);
    }

    public static bool isSoldier(byte unitType) {
        return Unit.isInBounds(unitType, 31, 98);
    }

    public static bool isVillager(byte unitType) {
        return unitType == EntityType.VILLAGER;
    }

    private static bool isInBounds(byte type, byte a, byte b) {
        return type >= a && type <= b;
    }
}
