﻿using System;
using System.Collections.Generic;
using System.Text;

public class Unit {
    public Vector3 position;
    public Vector3 waypoint;
    public short rotationWhole;
    public int gridIndex;
    public byte type;
    public byte activity;
    public short currentHp;

    public Unit(Vector3 position, short rotationWhole, byte type, short currentHp, int gridIndex,
        Vector3 waypoint = null) {
        this.position = position;
        this.rotationWhole = rotationWhole;
        this.type = type;

        this.gridIndex = gridIndex;
        this.currentHp = currentHp;
        this.activity = Activities.NONE;

        if (waypoint == null) {
            this.waypoint = position;
        } else {
            this.waypoint = waypoint;
        }
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
