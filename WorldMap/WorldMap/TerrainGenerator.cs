using System.Collections;
using System.Collections.Generic;
using System;

public class TerrainGenerator {
    private float minimum;
    private float maximum;
    private static Random random = new Random();

    private WorldMap worldMap;

    public ushort counter = 0;
    public static ushort playerSquareTolerance = 150;
    public static float acceptedDistanceBetweenPlayers = 50f;
    public int playerSquareSize = 50;

    public TerrainGenerator(WorldMap worldMap) {
        this.worldMap = worldMap;
        this.minimum = -this.worldMap.worldLength / 2;
        this.maximum = -1 * minimum;

        //this.playerSquareSize = (int)(this.playerSquareSize / this.worldMap.cellLength);
    }

    public void markNewEntity(Tuple<float, float, float> position, byte entityType) {
        int index = this.worldMap.getGridIndex(position.Item1, position.Item2, position.Item3);

        int cell = this.worldMap.getCell(index);
        if (this.worldMap.isFreeCell(cell)) {
            this.worldMap.markCell(index, 0, entityType, this.counter);
            this.counter++;
        }
    }

    public void markNewEntity(int gridPosition, Size size, byte entityType, bool fromCenter = false, int distH = 0, int distV = 0) {
        Tuple<int, int> gridPos = this.worldMap.getCoordinates(gridPosition);

        if (fromCenter) {
            gridPos = new Tuple<int, int>(gridPos.Item1 - distV, gridPos.Item2 - distH);
        }

        for (int i = 0; i < size.height; i++) {
            for (int j = 0; j < size.width; j++) {
                int index = this.worldMap.indexFromCoordinates(gridPos.Item1 + i, gridPos.Item2 + j);

                if (this.worldMap.isFreeIndexCell(index)) {
                    this.worldMap.markCell(index, 0, entityType, this.counter);
                } else {
                    Console.WriteLine("Positioned (" + this.worldMap.getCell(index) + " " + this.worldMap.getCounterValue(this.worldMap.getCell(index)) + ")");
                }
            }
        }

        this.counter++;
    }

    public int[] generateRandomPositionedTrees(int numberOfTrees, int maxIter = 100) {
        int[] positions = new int[numberOfTrees];
        int randomLimit = this.worldMap.gridSize * this.worldMap.gridSize;
        Size size = SizeMapping.map(EntityType.TREE_TYPE1);

        int distV = (size.height - 1) / 2;
        int distH = (size.width - 1) / 2;

        for (int i = 0; i < numberOfTrees; i++) {
            int iter = 0;
            while (true) {
                if (iter >= maxIter) {
                    // Debug.Log("Cannot generate random tree. No more random positions!");
                    return null;
                }

                // TODO: check for grid integrity
                int index = TerrainGenerator.random.Next(randomLimit);
                if (this.worldMap.isFreeIndexSquare(index, size, fromCenter: true, distH: distH, distV: distV)) {
                    int treeType = EntityType.TREE_TYPE1 + TerrainGenerator.random.Next(1, 4) - 1; 
                    this.markNewEntity(index, size, (byte)treeType, fromCenter: true, distH: distH, distV: distV);

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

        Size size = SizeMapping.map(EntityType.TREE_TYPE1);

        int distV = (size.height - 1) / 2;
        int distH = (size.width - 1) / 2;

        for (int i = arrayLength; i >= 0; i--) {
            int forestSize = forestSizes[i];
            //Debug.Log("Forest size current: " + forestSize);
            int addedUntilNow = 0;
            Queue<int> queue = new Queue<int>();

            int startIndex;
            while (true) {
                startIndex = TerrainGenerator.random.Next(0, this.worldMap.gridSize * this.worldMap.gridSize);

                if (this.worldMap.isFreeIndexSquare(startIndex, size, fromCenter: true, distV: distV, distH: distH)) {
                    break;
                }
            }

            queue.Enqueue(startIndex);

            while (queue.Count > 0) {
                int gridIndex = queue.Dequeue();
                
                int[] neighbours = {
                    this.worldMap.getUpperCellIndex(gridIndex, fromCenter: true, distV: distV),
                    this.worldMap.getRightCellIndex(gridIndex, fromCenter: true, distH: distH),
                    this.worldMap.getLowerCellIndex(gridIndex, fromCenter: true, distV: distV),
                    this.worldMap.getLeftCellIndex(gridIndex, fromCenter: true, distH: distH)
                };

                // add noise to the normal traverse for obtaining iregular looking forests. We'll simply ignore a random 
                // chosen direction
                int noiseIndex = TerrainGenerator.random.Next(4);
                neighbours[noiseIndex] = -1;

                bool full = false;

                for (int j = 0; j < 4; j++) {
                    if (neighbours[j] != -1 && this.worldMap.isFreeIndexSquare(neighbours[j], size, fromCenter: true, distV: distV, distH: distH)) {
                        queue.Enqueue(neighbours[j]);

                        addedUntilNow++;
                        positions[treeIndex] = neighbours[j];

                        int treeType = EntityType.TREE_TYPE1 + TerrainGenerator.random.Next(1, 4) - 1;
                        this.markNewEntity(neighbours[j], size, (byte)treeType, fromCenter: true, distV: distV, distH: distH);
                        treeIndex++;

                        if (addedUntilNow >= forestSize) {
                            full = true;
                            break;
                        }
                    }
                }

                if (full) {
                    break;
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
        Size size = SizeMapping.map(EntityType.GOLD_MINE);

        int distH = (size.width - 1) / 2;
        int distV = (size.height - 1) / 2;

        for (int i = 0; i < noMines; i++) {
            int index;
            while (true) {
                index = TerrainGenerator.random.Next(0, this.worldMap.gridSize * this.worldMap.gridSize);

                if (this.worldMap.isFreeIndexSquare(index, size, fromCenter: true, distV: distV, distH: distH)) {
                    minePositions[i] = index;
                    byte entityType = (isGold) ? EntityType.GOLD_MINE : EntityType.STONE_MINE;

                    this.markNewEntity(index, size, entityType, fromCenter: true, distH: distH, distV: distV);
                    break;
                }
            }
        }

        return minePositions;
    }

    public bool markEntity(int centerIndex, byte width, byte height, byte type, byte player, ushort counter) {
        /*
         * Convention:
         *  - the centerIndex MUST BE the top left corner
         * Approach: compute a list of indexes (if possible) where to place this entity and place it if it's possible.
         * Returns true if the entity could be placed, false otherwise.
         */
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
        /*line -= halfVerticalDist;
        col -= halfHorizontalDist;*/

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

    private List<int> getStartIndexes(byte width, byte height, int startLine, int endLine, int startCol, int endCol) {
        // TODO: implement this
        /* Explanation: this method will return a list of top-left corner indexes where big cells of size height x width 
         * could be placed, starting from there
         */
        List<int> result = new List<int>();

        for (int i = startLine * this.worldMap.gridSize; i < (startLine + height) * this.worldMap.gridSize; i += this.worldMap.gridSize) {
            
        }

        return result;
    }

    private int placeEntity(byte type, byte playerId, Rectangle shape) {
        /*
         * Get a random grid index and start to place the entity there
         */
        int gridIndex = 0;
        Size entitySize = SizeMapping.map(type);

        while (true) {
            int randomLine = TerrainGenerator.random.Next(shape.topLeftLine, shape.bottomRightLine + 1);
            int randomCol = TerrainGenerator.random.Next(shape.topLeftCol, shape.bottomRightCol + 1);

            gridIndex = this.worldMap.indexFromCoordinates(randomLine, randomCol);

            /*if (this.markEntity(gridIndex, (byte) entitySize.width, (byte) entitySize.height, EntityType.BARRACKS, playerId, this.counter)) {
                this.counter++;
                break;
            }*/

            if (this.worldMap.isFreeIndexSquare(gridIndex, entitySize)) {
                this.markNewEntity(gridIndex, entitySize, type);
                break;
            }
        }

        return gridIndex;
    }

    private int[] getMatchingRectangles(Size size, int howMany, int allowedTolerance = 0, int startLine = -1, int startCol = -1, int endLine = -1, int endCol = -1) {
        int[] result = new int[howMany];
        int completed = 0;
        bool satisfying = false;

        startLine = (startLine < 0) ? 0 : startLine;
        startCol = (startCol < 0) ? 0 : startCol;
        endLine = (endLine < 0) ? this.worldMap.gridSize : endLine;
        endCol = (endCol < 0) ? this.worldMap.gridSize : endCol;

        // TODO: optimization: keep precomputed values for number of obstacles on the first and last lines of your window
        /*Tuple<int, int>[] line = new Tuple<int, int>[endCol - startCol + 1];
        Tuple<int, int>[] column = new Tuple<int, int>[endLine - startLine + 1];
        bool precomputed = false;*/

        for (int i = startLine; i < endLine - size.height; i++) {
            Console.WriteLine("Test: " + i);
            for (int j = startCol; j < endCol - size.width; j++) {
                bool isCandidate = true;
                int occupiedHere = 0;
                int gridIndex = 0;

                for (int k = 0; k < size.height; k++) {
                    for (int l = 0; l < size.width; l++) {
                        gridIndex = this.worldMap.indexFromCoordinates(i + k, j + l);

                        if (!this.worldMap.isFreeIndexCell(gridIndex)) {
                            occupiedHere++;

                            if (occupiedHere > allowedTolerance) {
                                isCandidate = false;
                                break;
                            }
                        }
                    }

                    if (!isCandidate) {
                        break;
                    }
                }

                if (isCandidate) {
                    result[completed++] = gridIndex;

                    if (completed >= howMany) {
                        satisfying = true;
                        break;
                    }
                }
            }

            if (satisfying) {
                break;
            }
        }

        return result;
    }

    public Dictionary<byte, List<Tuple<int, int>>> generatePlayers(byte noPlayers) {
        Dictionary<byte, List<Tuple<int, int>>> result = new Dictionary<byte, List<Tuple<int, int>>>();

        // generate a random number of workers between 2 and 4 (inclusive)
        int noWorkers = TerrainGenerator.random.Next(2, 5);

        // generate a random number of houses between 2 and 4 (inclusive)
        int noHouses = TerrainGenerator.random.Next(2, 5);

        // generate flags for having in this order: barracks, defense tower
        bool hasBarracks = true;// (TerrainGenerator.random.Next(2) == 1);
        bool hasTower = true; // (TerrainGenerator.random.Next(2) == 1);
        int noSoldiers = TerrainGenerator.random.Next(1, 10);

        int randCenterStart = this.playerSquareSize * this.worldMap.gridSize + this.playerSquareSize;
        int totalGridSize = this.worldMap.gridSize * this.worldMap.gridSize;
        int randCenterMax = totalGridSize - this.worldMap.gridSize * this.playerSquareSize;

        for (byte playerId = 1, i = 0; i < noPlayers; playerId *= 2, i++) {
            List<Tuple<int, int>> playerData = new List<Tuple<int, int>>();

            // try to find a suitable area to place the current player's units, with a certain degree of liberty
            int centerIndex = TerrainGenerator.random.Next(this.worldMap.gridSize * 5 + (int)(this.playerSquareSize / this.worldMap.cellLength),
                this.worldMap.gridSquareSize - this.worldMap.gridSize * this.playerSquareSize);
            Console.WriteLine("Player center index: " + centerIndex);

            // place barracks
            Tuple<int, int> center = this.worldMap.getCoordinates(centerIndex);
            int centerLine = center.Item1;
            int centerCol = center.Item2;

            int squareMaxLine = centerLine + (this.playerSquareSize - 1) / 2;
            int squareMinLine = centerLine - (squareMaxLine - centerLine);
            int squareMinCol = centerCol - (squareMaxLine - centerLine);
            int squareMaxCol = centerCol + (squareMaxLine - centerLine);
            Rectangle playerSquare = new Rectangle(squareMinLine, squareMinCol, squareMaxLine, squareMaxCol);

            if (hasBarracks) {
                int gridIndex = this.placeEntity(EntityType.BARRACKS, playerId, playerSquare);

                int value = this.worldMap.buildCell(playerId, (ushort)(this.counter - 1), EntityType.BARRACKS);
                playerData.Add(new Tuple<int, int>(gridIndex, value));
            }

            // place tower
            if (hasTower) {
                int gridIndex = this.placeEntity(EntityType.GUARD_TOWER, playerId, playerSquare);
                Size towerSize = SizeMapping.map(EntityType.GUARD_TOWER);

                int value = this.worldMap.buildCell(playerId, (ushort)(this.counter - 1), EntityType.GUARD_TOWER);
                playerData.Add(new Tuple<int, int>(gridIndex, value));
            }

            // place soldiers
            for (int w = 0; w < noSoldiers; w++) {
                int gridIndex = this.placeEntity(EntityType.SWORDSMAN, playerId, playerSquare);
                Size soldierSize = SizeMapping.map(EntityType.SWORDSMAN);

                int value = this.worldMap.buildCell(playerId, (ushort)(this.counter - 1), EntityType.SWORDSMAN);
                playerData.Add(new Tuple<int, int>(gridIndex, value));
            }

            // place houses
            Size houseSize = SizeMapping.map(EntityType.HOUSE);
            for (int w = 0; w < noHouses; w++) {
                int gridIndex = this.placeEntity(EntityType.HOUSE, playerId, playerSquare);

                int value = this.worldMap.buildCell(playerId, (ushort)(this.counter - 1), EntityType.HOUSE);
                playerData.Add(new Tuple<int, int>(gridIndex, value));
            }

            // place villagers
            Size workerSize = SizeMapping.map(EntityType.VILLAGER);
            for (int w = 0; w < noWorkers; w++) {
                int gridIndex = this.placeEntity(EntityType.VILLAGER, playerId, playerSquare);

                int value = this.worldMap.buildCell(playerId, (ushort)(this.counter - 1), EntityType.VILLAGER);
                playerData.Add(new Tuple<int, int>(gridIndex, value));
            }

            result.Add(playerId, playerData);
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
