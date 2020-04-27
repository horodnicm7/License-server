using System.Collections;
using System.Collections.Generic;
using System;

public class TerrainGenerator {
    private float minimum;
    private float maximum;
    private static Random random = new Random();

    private WorldMap worldMap;

    public ushort counter = 0;
    public static ushort playerSquareTolerance = 10;
    public static float acceptedDistanceBetweenPlayers = 50f;
    public static byte playerSquareSize = 50;

    public TerrainGenerator(WorldMap worldMap) {
        this.worldMap = worldMap;
        this.minimum = -this.worldMap.worldLength / 2;
        this.maximum = -1 * minimum;
    }

    public void markNewEntity(Tuple<float, float, float> position, byte entityType) {
        int index = this.worldMap.getGridIndex(position.Item1, position.Item2, position.Item3);

        int cell = this.worldMap.getCell(index);
        if (this.worldMap.isFreeCell(cell)) {
            this.worldMap.markCell(index, 0, entityType, this.counter);
            this.counter++;
        }
    }

    public void markNewEntity(int position, byte entityType) {
        int cell = this.worldMap.getCell(position);
        if (this.worldMap.isFreeCell(cell)) {
            this.worldMap.markCell(position, 0, entityType, this.counter);
            this.counter++;
        }
    }

    public int[] generateRandomPositionedTrees(int numberOfTrees, int maxIter = 100) {
        int[] positions = new int[numberOfTrees];
        int randomLimit = this.worldMap.gridSize * this.worldMap.gridSize;

        for (int i = 0; i < numberOfTrees; i++) {
            int iter = 0;
            while (true) {
                if (iter >= maxIter) {
                    // Debug.Log("Cannot generate random tree. No more random positions!");
                    return null;
                }

                // TODO: check for grid integrity
                int index = TerrainGenerator.random.Next(randomLimit);
                if (this.worldMap.isFreeIndexCell(index)) {
                    int treeType = TerrainGenerator.random.Next(1, 4);
                    this.markNewEntity(index, (byte)treeType);

                    positions[i] = index;
                    break;
                }

                iter++;
            }
        }

        return positions;
    }

    public int[] generateRandomForests(int numberOfTrees, int numberOfForests = 10, int maxIter = 100) {
        int[] positions = new int[numberOfTrees];

        // randomly choose between 5 and 9 forests
        // int forestsNumber = this.random.Next(5, 10);
        int averageForestSize = numberOfTrees / numberOfForests;

        int[] forestSizes = new int[numberOfForests];

        // randomly genereate forestNumber - 1 forest sizes
        int totalNoTrees = numberOfTrees;
        int lowerRandomLimit = (int)(averageForestSize / 1.5f);
        int upperRandomLimit = (int)(averageForestSize * 1.3f);
        int arrayLength = 0;

        // generate a size for every forest
        for (int i = 0; i < numberOfForests - 1; i++) {
            int forestSize;
            if (totalNoTrees < upperRandomLimit) {
                forestSize = TerrainGenerator.random.Next((int)(totalNoTrees / 1.3f), totalNoTrees);
            } else {
                forestSize = TerrainGenerator.random.Next(lowerRandomLimit, upperRandomLimit);
            }

            if (totalNoTrees - forestSize <= 0) {
                forestSizes[i] = totalNoTrees;
                totalNoTrees -= forestSize;
                Console.WriteLine("Out of trees");
                break;
            }

            forestSizes[i] = forestSize;
            totalNoTrees -= forestSize;
            arrayLength++;
        }

        // append the remaining trees to the last forest. Usually, this will be the largest one
        if (totalNoTrees > 0) {
            forestSizes[arrayLength] = totalNoTrees;
        }

        // start creating the last forest, as it is most probably the largest one
        int treeIndex = 0;
        Console.WriteLine("Actual number of forests: " + (arrayLength + 1));

        for (int i = arrayLength; i >= 0; i--) {
            int forestSize = forestSizes[i];
            //Debug.Log("Forest size current: " + forestSize);
            int addedUntilNow = 0;
            Queue<int> queue = new Queue<int>();

            int startIndex;
            while (true) {
                // TODO: force these indexes to be uniformly distributed over the map. Now, usually, they
                // are scattered on some map quarter
                startIndex = TerrainGenerator.random.Next(0, this.worldMap.gridSize * this.worldMap.gridSize);

                if (this.worldMap.isFreeIndexCell(startIndex)) {
                    break;
                }
            }

            queue.Enqueue(startIndex);

            while (queue.Count > 0) {
                int gridIndex = queue.Dequeue();
                positions[treeIndex] = gridIndex;

                int treeType = TerrainGenerator.random.Next(1, 4);
                this.markNewEntity(gridIndex, (byte) treeType);
                treeIndex++;

                addedUntilNow++;
                if (addedUntilNow >= forestSize) {
                    break;
                }

                int[] neighbours = {
                    this.worldMap.getUpperCellIndex(gridIndex),
                    this.worldMap.getRightCellIndex(gridIndex),
                    this.worldMap.getLowerCellIndex(gridIndex),
                    this.worldMap.getLeftCellIndex(gridIndex)
                };

                // add noise to the normal traverse for obtaining iregular looking forests. We'll simply ignore a random 
                // chosen direction
                int noiseIndex = TerrainGenerator.random.Next(4);
                neighbours[noiseIndex] = -1;

                for (int j = 0; j < 4; j++) {
                    if (neighbours[j] != -1 && this.worldMap.isFreeIndexCell(neighbours[j])) {
                        queue.Enqueue(neighbours[j]);
                    }
                }
            }

        }

        return positions;
    }

    public int[] generateRandomMines(int noMines, bool isGold = true) {
        /*
         * A gold/stone mine will take 9 tiles:
         *      |  |  |  |
         *      |  |  |  |
         *      |  |  |  |
         * The mine's center will be in its center (viewing it from the game scene perspective)
         * 
         * CONVENTION: after a mine is exhausted, it will still be there, but as a dead one and still as 
         * a navigation obstacle
         */
        int[] minePositions = new int[noMines];

        for (int i = 0; i < noMines; i++) {
            int index;
            while (true) {
                index = TerrainGenerator.random.Next(0, this.worldMap.gridSize * this.worldMap.gridSize);

                if (this.worldMap.isFreeIndexCell(index)) {
                    /*
                     * | 1 | 2 | 3 |
                     * | 4 | 5 | 6 |
                     * | 7 | 8 | 9 | 
                     */
                    int top = this.worldMap.getUpperCellIndex(index);
                    int right = this.worldMap.getRightCellIndex(index);
                    int bottom = this.worldMap.getLowerCellIndex(index);
                    int left = this.worldMap.getLeftCellIndex(index);
                    int topLeft = this.worldMap.getLeftCellIndex(top);
                    int topRight = this.worldMap.getRightCellIndex(top);
                    int bottomRight = this.worldMap.getRightCellIndex(bottom);
                    int bottomLeft = this.worldMap.getLeftCellIndex(bottom);

                    int[] neededIndexes = new int[8] {
                        top, right, bottom, left, topLeft, topRight, bottomLeft, bottomRight
                    };

                    bool available = true;
                    for (int j = 0; j < 8; j++) {
                        if (!this.worldMap.isFreeIndexCell(neededIndexes[j])) {
                            available = false;
                            break;
                        }
                    }

                    if (available) {
                        minePositions[i] = index;
                        byte entityType = (isGold) ? EntityType.GOLD_MINE : EntityType.STONE_MINE;

                        for (int j = 0; j < 8; j++) {
                            this.markNewEntity(neededIndexes[j], entityType);
                        }
                        
                        break;
                    }
                }
            }
        }

        return minePositions;
    }

    public bool markBuilding(int centerIndex, byte width, byte height, byte type, byte player, ushort counter) {
        // get 2D grid coordinates
        Tuple<int, int> figureCenter = this.worldMap.getCoordinates(centerIndex);
        int line = figureCenter.Item1;
        int col = figureCenter.Item2;

        // compute lengths
        byte halfHorizontalDist = (byte)((width - 1) / 2);
        byte halfVerticalDist = (byte)((height - 1) / 2);

        // get the 4 corners
        int topLeft = this.worldMap.indexFromCoordinates(line - halfVerticalDist, col - halfHorizontalDist);
        int topRight = this.worldMap.indexFromCoordinates(line - halfVerticalDist, col + halfHorizontalDist);
        int bottomLeft = this.worldMap.indexFromCoordinates(line + halfVerticalDist, col - halfHorizontalDist);
        int bottomRight = this.worldMap.indexFromCoordinates(line + halfVerticalDist, col + halfHorizontalDist);

        // check if you didn't fall out of the map
        if (topLeft < 0 || topRight < 0 || bottomLeft < 0 || bottomRight < 0) {
            return false;
        }

        // try every index that needs to be marked and check if it's available
        int idx = 0;
        line -= halfVerticalDist;
        col -= halfHorizontalDist;

        int[] indexesToMark = new int[width * height];

        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++, idx++) {
                int newGridIndex = this.worldMap.indexFromCoordinates(line + i, col + j);

                // can't place building here, as it's obstructed
                if (!this.worldMap.isFreeIndexCell(newGridIndex)) {
                    return false;
                }

                indexesToMark[idx] = newGridIndex;
            }
        }

        // if it gets to here, then it's safe to mark this building on grid
        for (int i = 0; i < width * height; i++) {
            this.worldMap.markCell(indexesToMark[i], player, type, counter);
        }

        // signal that the building was successfuly placed
        return true;
    }

    public Dictionary<byte, List<Tuple<int, int>>> generatePlayers(byte noPlayers) {
        Dictionary<byte, List<Tuple<int, int>>> result = new Dictionary<byte, List<Tuple<int, int>>>();

        // generate a random number of workers between 2 and 4 (inclusive)
        int noWorkers = TerrainGenerator.random.Next(2, 5);

        // generate a random number of houses between 2 and 4 (inclusive)
        int noHouses = TerrainGenerator.random.Next(2, 5);

        // generate flags for having in this order: barracks, defense tower
        bool hasBarracks = (TerrainGenerator.random.Next(2) == 1);
        bool hasTower = (TerrainGenerator.random.Next(2) == 1);

        int randCenterStart = TerrainGenerator.playerSquareSize * this.worldMap.gridSize + TerrainGenerator.playerSquareSize;
        int totalGridSize = this.worldMap.gridSize * this.worldMap.gridSize;
        int randCenterMax = totalGridSize - this.worldMap.gridSize * TerrainGenerator.playerSquareSize;

        for (byte playerId = 1, i = 0; i < noPlayers; playerId *= 2, i++) {
            bool foundCenter = false;
            int centerIndex = 0;

            // try to find a suitable area to place the current player's units
            while (!foundCenter) {
                foundCenter = true;

                centerIndex = TerrainGenerator.random.Next(randCenterStart, randCenterMax);

                // get 2D grid coordinates
                Tuple<int, int> figureCenter = this.worldMap.getCoordinates(centerIndex);
                int line = figureCenter.Item1 - TerrainGenerator.playerSquareSize;
                int col = figureCenter.Item2 - TerrainGenerator.playerSquareSize;

                ushort allowedError = 0;

                for (int k = 0; i < TerrainGenerator.playerSquareSize; k++) {
                    for (int l = 0; l < TerrainGenerator.playerSquareSize; l++) {
                        int newGridIndex = this.worldMap.indexFromCoordinates(line + k, col + l);

                        // can't place building here, as it's obstructed
                        if (!this.worldMap.isFreeIndexCell(newGridIndex)) {
                            allowedError++;
                        }

                        if (allowedError > TerrainGenerator.playerSquareTolerance) {
                            foundCenter = false;
                            break;
                        }
                    }

                    if (!foundCenter) {
                        break;
                    }
                }
            }

            // find
            Tuple<int, int> center = this.worldMap.getCoordinates(centerIndex);
            int centerLine = center.Item1;
            int centerCol = center.Item2;

            int squareMaxLine = centerLine + (TerrainGenerator.playerSquareSize - 1) / 2;
            int squareMinLine = centerLine - (squareMaxLine - centerLine);
            int squareMinCol = centerCol - (squareMaxLine - centerLine);
            int squareMaxCol = centerCol + (squareMaxLine - centerLine);

            if (hasBarracks) {
                int gridIndex = 0;
                Size barracksSize = SizeMapping.map(EntityType.BARRACKS);

                while (true) {
                    int randomLine = TerrainGenerator.random.Next(squareMinLine, squareMaxLine + 1);
                    int randomCol = TerrainGenerator.random.Next(squareMinCol, squareMaxCol);

                    gridIndex = this.worldMap.indexFromCoordinates(randomLine, randomCol);

                    if (this.markBuilding(gridIndex, barracksSize.width, barracksSize.height, EntityType.BARRACKS, playerId, this.counter)) {
                        this.counter++;
                        break;
                    }
                }
            }
        }

        return result;
    }

    private Tuple<float, float, float> getRandomPosition() {
        // TODO: must be moved on the server
        float x = (float)(TerrainGenerator.random.NextDouble()) * (this.maximum - this.minimum) + this.minimum;
        float y = 0;
        float z = (float)(TerrainGenerator.random.NextDouble()) * (this.maximum - this.minimum) + this.minimum;

        return new Tuple<float, float, float>(x, y, z);
    }
}
