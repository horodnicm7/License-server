using System.Collections;
using System.Collections.Generic;
using System;

/*
 * CONVENTIONS:
 * 1. the map is centered in (0, 0, 0)
 * 2. the map has square form
 */
public class WorldMap {
    public ushort worldLength;
    public float cellLength;
    public ushort gridSize;

    public static int keyCounter = 1;

    private int[] world;
    private float halfCellLength;
    private ushort halfWorldLength;
    private int halfGridSize;

    public WorldMap(ushort worldLength = 1024, float cellLength = 0.5f) {
        this.worldLength = worldLength;
        this.cellLength = cellLength;

        this.gridSize = (ushort)(this.worldLength / this.cellLength);
        this.world = new int[this.gridSize * this.gridSize];
        this.halfCellLength = this.cellLength / 2;
        this.halfWorldLength = (ushort)(this.worldLength / 2);
        this.halfGridSize = this.gridSize / 2;
    }

    public void buildGridFromTerrain() {

    }

    public Tuple<float, float> getCellCenter(float x, float y, float z) {
        Tuple<float, float> result;

        // TODO: might need to invert these if you rotate the world
        int wholePartX = (int)x;
        int wholePartZ = (int)z;

        float moveX = this.halfCellLength;
        float moveZ = this.halfCellLength;

        if (wholePartX < 0) {
            moveX *= -1;
        }

        if (wholePartZ < 0) {
            moveZ *= -1;
        }

        result = new Tuple<float, float>(wholePartX + moveX, wholePartZ + moveZ);

        return result;
    }

    public Tuple<float, float, float> getCellPosition(int index) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        float x, y, z;

        // TODO: change this if you'll add terrain height
        y = 0;

        //Debug.Log("Half: " + this.halfWorldLength + " " + this.halfGridSize + " " + this.gridSize);

        if (line <= this.halfGridSize) {
            // Z coord will be positive
            z = this.halfWorldLength - (line * this.cellLength + this.halfCellLength);
        } else {
            // Z coord will be negative
            z = this.halfWorldLength - (line * this.cellLength + this.halfCellLength);
        }

        if (col <= this.halfGridSize) {
            // X coord will be negative
            x = -(col * this.cellLength - this.halfCellLength);
        } else {
            // X coord will be positive
            x = (col * this.cellLength + this.halfCellLength) - this.halfWorldLength;
        }

        return new Tuple<float, float, float>(x, y, z);
    }

    public int getGridIndex(float x, float y, float z) {
        // the Y coord will be ignored, it's here only as a convention
        int index = 0;

        // check for out of world coordinates
        if (x > this.halfWorldLength || x < -this.halfWorldLength ||
            z > this.halfWorldLength || x < -this.halfWorldLength) {

            return -1;
        }

        // the normal case
        // TODO: might need to invert these if you rotate the world
        float wholePartX;
        float wholePartZ;

        if (x < 0) {
            wholePartX = Math.Abs(x - this.halfCellLength);
        } else {
            wholePartX = x + this.halfWorldLength;
        }

        if (z < 0) {
            wholePartZ = Math.Abs(z) + this.halfWorldLength;
        } else {
            wholePartZ = this.halfWorldLength - z;
        }

        int line = (int)(wholePartZ / this.cellLength);
        int col = (int)(wholePartX / this.cellLength);

        index = line * this.gridSize + col;

        return index;
    }

    public int getUpperCell(int index) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        if (line - 1 < 0) {
            return -1;
        }

        return (line - 1) * this.gridSize + col;
    }

    public int getLeftCell(int index) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        if (col - 1 < 0) {
            return -1;
        }

        return line * this.gridSize + (col - 1);
    }

    public int getRightCell(int index) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        if (col + 1 >= this.gridSize) {
            return -1;
        }

        return line * this.gridSize + (col + 1);
    }

    public int getLowerCell(int index) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        if (line + 1 >= this.gridSize) {
            return -1;
        }

        return (line + 1) * this.gridSize + col;
    }

    public int getCell(int index) {
        return this.world[index];
    }

    public int getPlayer(int cell) {
        return cell >> 24;
    }

    /*public ushort getEntityType(ushort cell) {
        return (ushort)((cell << 8) >> 8);
    }*/

    public int buildCell(int player, int id) {
        return player | id;
    }

    public void markCell(int index, int player, int entityId) {
        this.world[index] = this.buildCell(player, entityId);
    }

    public bool isFreeIndexCell(int index) {
        if (index < 0) {
            return false;
        }

        return this.getCell(index) == 0;
    }

    public bool isFreeCell(int cell) {
        return cell == 0;
    }
}
