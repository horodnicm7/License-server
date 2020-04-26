using System.Collections.Generic;
using System;

public class TerrainGenerator {
    private float minimum;
    private float maximum;
    private static Random random = new Random();

    private WorldMap worldMap;

    public ushort counter = 0;

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
         * A gold/stone mine will take 4 tiles:
         *      |  |  |
         *      |  |  |
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

                    /*Tuple<float, float, float> midPos = this.worldMap.getCellPosition(index);
                    bool found = false;
                    float centerX = 0, centerZ = 0;

                    if (this.worldMap.isFreeIndexCell(top)) {
                        if (this.worldMap.isFreeIndexCell(left)) {
                            // 5 - 2 - 1- 4
                            int topLeft = this.worldMap.getUpperCellIndex(left);
                            if (this.worldMap.isFreeIndexCell(topLeft)) {
                                found = true;
                                Tuple<float, float, float> leftPos = this.worldMap.getCellPosition(left);
                                Tuple<float, float, float> topPos = this.worldMap.getCellPosition(top);
                                centerX = (leftPos.Item1 + midPos.Item1) / 2;
                                centerZ = (topPos.Item3 + midPos.Item3) / 2;
                            }
                        } else if (this.worldMap.isFreeIndexCell(right)) {
                            // 5 - 2 - 3 - 6
                            int topRight = this.worldMap.getUpperCellIndex(right);
                            if (this.worldMap.isFreeIndexCell(topRight)) {
                                found = true;
                                Tuple<float, float, float> rightPos = this.worldMap.getCellPosition(right);
                                Tuple<float, float, float> topPos = this.worldMap.getCellPosition(top);
                                centerX = (rightPos.Item1 + midPos.Item1) / 2;
                                centerZ = (topPos.Item3 + midPos.Item3) / 2;
                            }
                        }
                    } else if (this.worldMap.isFreeIndexCell(bottom)) {
                        if (this.worldMap.isFreeIndexCell(left)) {
                            // 5 - 8 - 7 - 4
                            int bottomLeft = this.worldMap.getLowerCellIndex(left);
                            if (this.worldMap.isFreeIndexCell(bottomLeft)) {
                                found = true;
                                Tuple<float, float, float> leftPos = this.worldMap.getCellPosition(left);
                                Tuple<float, float, float> bottomPos = this.worldMap.getCellPosition(bottom);
                                centerX = (leftPos.Item1 + midPos.Item1) / 2;
                                centerZ = (bottomPos.Item3 + midPos.Item3) / 2;
                            }
                        } else if (this.worldMap.isFreeIndexCell(right)) {
                            // 5 - 6 - 8 - 9
                            int bottomRight = this.worldMap.getLowerCellIndex(right);
                            if (this.worldMap.isFreeIndexCell(bottomRight)) {
                                found = true;
                                Tuple<float, float, float> rightPos = this.worldMap.getCellPosition(right);
                                Tuple<float, float, float> bottomPos = this.worldMap.getCellPosition(bottom);
                                centerX = (rightPos.Item1 + midPos.Item1) / 2;
                                centerZ = (bottomPos.Item3 + midPos.Item3) / 2;
                            }
                        }
                    }

                    if (found) {
                        minePositions[i] = index;

                        this.markNewEntity(index, (isGold) ? EntityType.GOLD_MINE : EntityType.STONE_MINE);
                        break;
                    }*/
                }
            }
        }

        return minePositions;
    }

    private Tuple<float, float, float> getRandomPosition() {
        // TODO: must be moved on the server
        float x = (float)(TerrainGenerator.random.NextDouble()) * (this.maximum - this.minimum) + this.minimum;
        float y = 0;
        float z = (float)(TerrainGenerator.random.NextDouble()) * (this.maximum - this.minimum) + this.minimum;

        return new Tuple<float, float, float>(x, y, z);
    }
}
