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
    public int gridSquareSize;

    public WorldMap(ushort worldLength = 128, float cellLength = 0.5f) {
        this.worldLength = worldLength;
        this.cellLength = cellLength;

        this.gridSize = (ushort)(this.worldLength / this.cellLength);
        this.world = new int[this.gridSize * this.gridSize];
        this.halfCellLength = this.cellLength / 2;
        this.halfWorldLength = (ushort)(this.worldLength / 2);
        this.halfGridSize = this.gridSize / 2;
        this.gridSquareSize = this.gridSize * this.gridSize;
    }

    public void buildGridFromTerrain() {

    }

    public Tuple<float, float> getCellCenter(float x, float y, float z) {
        int wholePartX = (int)x;
        int wholePartZ = (int)z;

        return new Tuple<float, float>(wholePartX + this.halfCellLength, wholePartZ + this.halfCellLength);
    }

    public Tuple<float, float, float> getCellPosition(int index) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        float x, y, z;
        x = col * this.cellLength + this.halfCellLength;
        // TODO: change this if you'll add terrain height
        y = 0;
        z = line * this.cellLength + this.halfCellLength;

        return new Tuple<float, float, float>(x, y, z);
    }

    public int getGridIndex(float x, float y, float z) {
        int wholePartX = (int)x;
        int wholePartZ = (int)z;

        int col = (int)(wholePartX / this.cellLength);
        int line = (int)(wholePartZ / this.cellLength);

        return line * this.gridSize + col;
    }

    public int getUpperCellIndex(int index, bool fromCenter = false, int distV = 0, int distH = 0) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        if (fromCenter) {
            line -= 2 * distV;
        }

        line--;

        if (line < distV) {
            return -1;
        }

        return line * this.gridSize + col;
    }

    public int getLeftCellIndex(int index, bool fromCenter = false, int distV = 0, int distH = 0) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        if (fromCenter) {
            col -= 2 * distH;
        }

        col--;

        if (col < distH) {
            return -1;
        }

        return line * this.gridSize + col;
    }

    public int getRightCellIndex(int index, bool fromCenter = false, int distV = 0, int distH = 0) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        if (fromCenter) {
            col += 2 * distH;
        }

        col++;

        if (col >= this.gridSize - distH) {
            return -1;
        }

        return line * this.gridSize + col;
    }

    public int getLowerCellIndex(int index, bool fromCenter = false, int distV = 0, int distH = 0) {
        int line = index / this.gridSize;
        int col = index % this.gridSize;

        if (fromCenter) {
            line += 2 * distV;
        }

        line++;

        if (line >= this.gridSize - distV) {
            return -1;
        }

        return line * this.gridSize + col;
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

    public bool isFreeIndexSquare(int index, Size size, bool fromCenter = false, int distV = 0, int distH = 0) {
        Tuple<int, int> gridPos = this.getCoordinates(index);

        if (fromCenter) {
            gridPos = new Tuple<int, int>(gridPos.Item1 - distV, gridPos.Item2 - distH);
        }

        for (int i = 0; i < size.height; i++) {
            for (int j = 0; j < size.width; j++) {
                int newIndex = this.indexFromCoordinates(gridPos.Item1 + i, gridPos.Item2 + j);

                if (!this.isFreeIndexCell(newIndex)) {
                    return false;
                }
            }
        }

        return true;
    }

    public void markIndexSquare(int index, Size size, byte entityType, byte playerId, ushort counter,
        bool fromCenter = false, int distV = 0, int distH = 0) {

        Tuple<int, int> gridPos = this.getCoordinates(index);

        if (fromCenter) {
            gridPos = new Tuple<int, int>(gridPos.Item1 - distV, gridPos.Item2 - distH);
        }

        for (int i = 0; i < size.height; i++) {
            for (int j = 0; j < size.width; j++) {
                int newIndex = this.indexFromCoordinates(gridPos.Item1 + i, gridPos.Item2 + j);

                this.markCell(newIndex, playerId, entityType, counter);
            }
        }
    }

    public void cleanMarkedIndexSquare(int index, Size size, byte playerId, bool fromCenter = false, int distV = 0, int distH = 0) {
        Tuple<int, int> gridPos = this.getCoordinates(index);

        if (fromCenter) {
            gridPos = new Tuple<int, int>(gridPos.Item1 - distV, gridPos.Item2 - distH);
        }

        for (int i = 0; i < size.height; i++) {
            for (int j = 0; j < size.width; j++) {
                int newIndex = this.indexFromCoordinates(gridPos.Item1 + i, gridPos.Item2 + j);
                int cell = this.getCell(newIndex);

                if (cell > 0 && this.getPlayer(cell) == playerId) {
                    this.world[newIndex] = 0;
                }
            }
        }
    }
}
