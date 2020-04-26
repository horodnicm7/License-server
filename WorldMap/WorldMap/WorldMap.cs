using System;

/*
 * CONVENTIONS:
 * 1. the map is centered in (0, 0, 0)
 * 2. the map has squared form
 * 
 * One cell has the following form:
 * |1 byte = entity type|1 byte = player id|2 bytes = key counter|
 */
public class WorldMap {
    public ushort worldLength;
    public float cellLength;
    public ushort gridSize;

    public static ushort keyCounter = 1;

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

    public int getUpperCellIndex(int index) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        if (line - 1 < 0) {
            return -1;
        }

        return (line - 1) * this.gridSize + col;
    }

    public int getLeftCellIndex(int index) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        if (col - 1 < 0) {
            return -1;
        }

        return line * this.gridSize + (col - 1);
    }

    public int getRightCellIndex(int index) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        if (col + 1 >= this.gridSize) {
            return -1;
        }

        return line * this.gridSize + (col + 1);
    }

    public int getLowerCellIndex(int index) {
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

    public byte getPlayer(int cell) {
        return (byte)((cell << 8) >> 24);
    }

    public byte getEntityType(int cell) {
        return (byte)(cell >> 24);
    }

    public ushort getCounterValue(int cell) {
        return (ushort)((cell << 16) >> 16);
    }

    public int buildCell(byte player, ushort counter, byte entityType) {
        return (int)((int)(entityType << 24) | (int)(player << 16) | (int)(counter));
    }

    public void markCell(int index, byte player, byte entityType, ushort entityId) {
        this.world[index] = this.buildCell(player, entityId, entityType);
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

    public Tuple<int, int> getCoordinates(int index) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        return new Tuple<int, int>(line, col);
    }

    public int indexFromCoordinates(int line, int col) {
        if (line < 0 || line >= this.gridSize || col < 0 || col >= this.gridSize) {
            return -1;
        }

        return line * this.gridSize + col;
    }
}
