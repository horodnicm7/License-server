using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DarkRift;
using DarkRift.Server;

public class Room {
    // it will work with the instances from RoomMaster and this dictionary will only keep 
    // clients as keys and bool values meaning that a client is room's owner or not
    public Dictionary<IClient, byte> playersIClientMapping;
    public Dictionary<byte, IClient> playersByteMapping;
    public string uuid;
    public string name;
    public byte maxNumberOfPlayers;
    public IClient leader;
    public byte maxPlayerId; // used for player id incrementation

    private WorldMap map;
    private const byte treesPerPackage = 8;
    private const byte goldChunkSize = 10;
    private const byte stoneChunkSize = 5;

    private byte maximumFieldOfView = 0;

    private Dictionary<IClient, LinkedList<PlayerMessage>> tcpMessageQueue;
    private Dictionary<IClient, LinkedList<PlayerMessage>> udpMessageQueue;
    public static byte tcpPackageLimit = 20;
    public static byte udpPackageLimit = 30;

    public Room(string uuid, string name, byte maxNoPlayers) {
        this.uuid = uuid;
        this.name = name;
        this.maxNumberOfPlayers = maxNoPlayers;
        this.maxPlayerId = 1;

        this.udpMessageQueue = new Dictionary<IClient, LinkedList<PlayerMessage>>();
        this.tcpMessageQueue = new Dictionary<IClient, LinkedList<PlayerMessage>>();

        this.playersIClientMapping = new Dictionary<IClient, byte>();
        this.playersByteMapping = new Dictionary<byte, IClient>();
    }

    public void ClearMemory() {
        this.map = null;
        this.playersIClientMapping.Clear();
        this.playersByteMapping.Clear();
    }

    public void sendDataToPlayersCallback() {
        foreach(KeyValuePair<IClient, LinkedList<PlayerMessage>> playerQueue in this.udpMessageQueue) {
            DarkRiftWriter udpWriter = DarkRiftWriter.Create();
            byte sent = 0;

            while (sent <= Room.udpPackageLimit && playerQueue.Value.Count > 0) {
                PlayerMessage playerMessage = playerQueue.Value.First.Value;
                playerMessage.serialize(ref udpWriter);

                playerQueue.Value.RemoveFirst();
                sent++;
            }

            // if there are messages in this queue to send
            if (sent > 0) {
                Console.WriteLine("Send to " + this.playersIClientMapping[playerQueue.Key] + " " + sent + " UDP messages");
                using (Message response = Message.Create(Tags.MIXED_MESSAGE, udpWriter)) {
                    playerQueue.Key.SendMessage(response, SendMode.Unreliable);
                }
            }
        }

        foreach(KeyValuePair<IClient, LinkedList<PlayerMessage>> playerQueue in this.tcpMessageQueue) {
            DarkRiftWriter tcpWriter = DarkRiftWriter.Create();

            byte sent = 0;

            while (sent <= Room.tcpPackageLimit && playerQueue.Value.Count > 0) {
                PlayerMessage playerMessage = playerQueue.Value.First.Value;
                playerMessage.serialize(ref tcpWriter);

                playerQueue.Value.RemoveFirst();
                sent++;
            }

            // if there are messages in this queue to send
            if (sent > 0) {
                Console.WriteLine("Send to " + this.playersIClientMapping[playerQueue.Key] + " " + sent + " TCP messages");
                using (Message response = Message.Create(Tags.MIXED_MESSAGE, tcpWriter)) {
                    playerQueue.Key.SendMessage(response, SendMode.Reliable);
                }
            }
        }
    }

    public List<IClient> getOtherPlayers(IClient except) {
        List<IClient> others = new List<IClient>();

        foreach(KeyValuePair<IClient, byte> player in this.playersIClientMapping) {
            if (except != player.Key) {
                others.Add(player.Key);
            }
        }

        return others;
    }

    public void changeLeader() {
        foreach(KeyValuePair<IClient, byte> player in this.playersIClientMapping) {
            this.leader = player.Key;
            break;
        }
    }

    public void MessageReceived(object sender, MessageReceivedEventArgs e) {
        IClient client = e.Client;

        using (Message message = e.GetMessage() as Message) {
            DarkRiftReader reader = message.GetReader();
            switch (e.Tag) {
                case Tags.MIXED_MESSAGE:
                    this.handleMixedMessage(ref client, ref reader);
                    break;
                case Tags.PLAYER_MOVE:
                    this.handlePlayerUnitMove(ref client, ref reader);
                    break;
                case Tags.PLAYER_ATTACK:
                    this.handlePlayerUnitAttack(ref client, ref reader);
                    break;
                case Tags.PLAYER_SPAWN_UNIT:
                    this.handlePlayerUnitSpawn(ref client, ref reader);
                    break;
                case Tags.PLAYER_UNIT_DEATH:
                    this.handlePlayerUnitDeath(ref client, ref reader);
                    break;
                case Tags.PLAYER_STOP_UNIT:
                    this.handlePlayerUnitStop(ref client, ref reader);
                    break;
                case Tags.PLAYER_BUILD:
                    this.handlePlayerBuild(ref client, ref reader);
                    break;
                case Tags.PLAYER_ROTATE:
                    this.handlePlayerUnitRotate(ref client, ref reader);
                    break;
                case Tags.PLAYER_GATHER_RESOURCE:
                    this.handlePlayerGatherResource(ref client, ref reader);
                    break;
                case Tags.PLAYER_TECHNOLOGY_UPGRADE:
                    this.handlePlayerTechnologyUpgrade(ref client, ref reader);
                    break;
            }
        }
    }

    private void handleMixedMessage(ref IClient client, ref DarkRiftReader legacyReader) {     
        while(legacyReader.Position != legacyReader.Length) {
            byte tag = legacyReader.ReadByte();

            switch (tag) {
                case Tags.PLAYER_SPAWN_UNIT:
                    this.handlePlayerUnitSpawn(ref client, ref legacyReader);
                    break;
                case Tags.PLAYER_ATTACK:
                    this.handlePlayerUnitAttack(ref client, ref legacyReader);
                    break;
                case Tags.PLAYER_MOVE:
                    this.handlePlayerUnitMove(ref client, ref legacyReader);
                    break;
                case Tags.PLAYER_ROTATE:
                    this.handlePlayerUnitRotate(ref client, ref legacyReader);
                    break;
            }
        }
    }

    private void handlePlayerUnitSpawn(ref IClient client, ref DarkRiftReader legacyReader) {
        short wholeX = legacyReader.ReadInt16();
        short fractionalX = legacyReader.ReadInt16();
        short wholeZ = legacyReader.ReadInt16();
        short fractionalZ = legacyReader.ReadInt16();
        int gridValue = legacyReader.ReadInt32();
        short rotationWhole = legacyReader.ReadInt16();
        short rotationFractional = legacyReader.ReadInt16();

        float x = FloatIntConverter.convertInt(wholeX, fractionalX);
        float z = FloatIntConverter.convertInt(wholeZ, fractionalZ);

        int gridIndex = this.map.getGridIndex(x, 0, z);

        ushort counterValue = this.map.getCounterValue(gridValue);
        Player player = RoomMaster.players[client];

        byte type = this.map.getEntityType(gridValue);
        bool sendSpawnToOthers = false;
        Unit newUnit = null;

        Vector3 positionVector = new Vector3(x, 0, z);
        if (Unit.isBuilding(type)) {
            if (!player.buildings.ContainsKey(counterValue)) {
                newUnit = new Unit(positionVector, rotationWhole, rotationFractional, type, gridIndex);
                player.buildings.Add(counterValue, newUnit);
                sendSpawnToOthers = true;
            }
        } else if (!player.army.ContainsKey(counterValue)) {
            newUnit = new Unit(positionVector, rotationWhole, rotationFractional, type, gridIndex);
            player.army.Add(counterValue, newUnit);
            sendSpawnToOthers = true;
        }

        newUnit.activity = Activities.NONE;

        if (sendSpawnToOthers) {
            HashSet<byte> seenBy = this.whoSeesThisUnitWithSquare(client, newUnit);

            foreach(byte playerId in seenBy) {
                IClient enemyClient = this.playersByteMapping[playerId];
                if (!this.tcpMessageQueue.ContainsKey(enemyClient)) {
                    this.tcpMessageQueue.Add(enemyClient, new LinkedList<PlayerMessage>());
                }

                this.tcpMessageQueue[enemyClient].AddLast(new MovementMessage(wholeX, fractionalX, wholeZ, fractionalZ,
                    gridValue, rotationWhole, rotationFractional, Activities.NONE));
            }
        }
    }

    private void handlePlayerUnitMove(ref IClient client, ref DarkRiftReader legacyReader) {
        ushort unitId = (ushort)legacyReader.ReadInt16();
        short wholeX = legacyReader.ReadInt16();
        short fractionalX = legacyReader.ReadInt16();
        short wholeZ = legacyReader.ReadInt16();
        short fractionalZ = legacyReader.ReadInt16();
        short rotationWhole = legacyReader.ReadInt16();
        short rotationFractional = legacyReader.ReadInt16();

        float x = FloatIntConverter.convertInt(wholeX, fractionalX);
        float z = FloatIntConverter.convertInt(wholeZ, fractionalZ);

        //Console.WriteLine("Unit " + unitId + " move: " + wholeX + "," + fractionalX + " " + wholeZ + "," + fractionalZ + "    " + x + " " + z);
        Player player = RoomMaster.players[client];
        Unit unit = player.army[unitId];

        byte unitPlayer = this.playersIClientMapping[client];

        int oldIndex = unit.gridIndex;

        this.map.cleanMarkedIndexSquare(unit.gridIndex, SizeMapping.map(unit.type), unitPlayer);
        unit.position.x = x;
        unit.position.z = z;
        unit.gridIndex = this.map.getGridIndex(x, unit.position.y, z);
        unit.rotationWhole = rotationWhole;
        unit.rotationFractional = rotationFractional;
        unit.activity = Activities.MOVING;
        this.map.markCell(unit.gridIndex, this.playersIClientMapping[client], unit.type, unitId);

        if (oldIndex != unit.gridIndex) {
            Console.WriteLine(unit.gridIndex);
        }

        HashSet<byte> seenBy = this.whoSeesThisUnitWithSquare(client, unit);
        //Console.WriteLine("Seen by: " + seenBy.Count);
        int gridValue = this.map.buildCell(unitPlayer, unitId, unit.type);
        //Console.Write("Foll enemies see: ");
        foreach (byte playerId in seenBy) {
            IClient enemyClient = this.playersByteMapping[playerId];

            //Console.Write(playerId + ", ");

            if (!this.udpMessageQueue.ContainsKey(enemyClient)) {
                this.udpMessageQueue.Add(enemyClient, new LinkedList<PlayerMessage>());
            }

            this.udpMessageQueue[enemyClient].AddLast(new MovementMessage(wholeX, fractionalX, wholeZ, fractionalZ,
                    gridValue, rotationWhole, rotationFractional, Activities.MOVING));
        }
        //Console.WriteLine();

        //Console.Write("What this unit sees: ");
        Dictionary<byte, HashSet<ushort>> whatThisSees = this.whatThisUnitSees(client, unit);
        foreach(KeyValuePair<byte, HashSet<ushort>> seenEnemies in whatThisSees) {
            Player enemyPlayer = RoomMaster.players[this.playersByteMapping[seenEnemies.Key]];
            foreach(ushort enemyId in seenEnemies.Value) {
                Unit enemyUnit = null;
                if (enemyPlayer.army.ContainsKey(enemyId)) {
                    enemyUnit = enemyPlayer.army[enemyId];
                } else {
                    enemyUnit = enemyPlayer.buildings[enemyId];
                }

                int enemyGridValue = this.map.buildCell(seenEnemies.Key, enemyId, enemyUnit.type);
                Tuple<short, short> posXParts = FloatIntConverter.convertFloat(enemyUnit.position.x);
                Tuple<short, short> posZParts = FloatIntConverter.convertFloat(enemyUnit.position.z);

                if (!this.udpMessageQueue.ContainsKey(client)) {
                    this.udpMessageQueue.Add(client, new LinkedList<PlayerMessage>());
                }

                //Console.Write(enemyId + ", ");

                this.udpMessageQueue[client].AddLast(new MovementMessage(posXParts.Item1, posXParts.Item2, posZParts.Item1, posZParts.Item2,
                    enemyGridValue, enemyUnit.rotationWhole, enemyUnit.rotationFractional, enemyUnit.activity));
            }
        }
        //Console.WriteLine();
        //Console.WriteLine();
    }

    private Dictionary<byte, HashSet<ushort>> whatThisUnitSees(IClient unitOwner, Unit currentUnit) {
        Dictionary<byte, HashSet<ushort>> result = new Dictionary<byte, HashSet<ushort>>();
        byte unitPlayer = this.playersIClientMapping[unitOwner];
        Player currentPlayer = RoomMaster.players[unitOwner];

        Tuple<int, int> currentUnitCoords = this.map.getCoordinates(currentUnit.gridIndex);
        int halfFieldOfView = (int)(currentPlayer.playerStats.map(currentUnit.type).fieldOfView * this.map.cellLength);

        // the line start in [0, this.map.gridSize)
        int startLine = currentUnitCoords.Item1 - halfFieldOfView;
        startLine = (startLine < 0) ? 0 : startLine;

        // the line end in [0, this.map.gridSize)
        int endLine = currentUnitCoords.Item1 + halfFieldOfView;
        endLine = (endLine >= this.map.gridSize) ? this.map.gridSize - 1 : endLine;

        // the column start in [0, this.map.gridSize)
        int startCol = currentUnitCoords.Item2 - halfFieldOfView;
        startCol = (startCol < 0) ? 0 : startCol;

        // the column end in [0, this.map.gridSize)
        int endCol = currentUnitCoords.Item2 + halfFieldOfView;
        endCol = (endCol >= this.map.gridSize) ? this.map.gridSize - 1 : endCol;

        // the starting column for the first and last lines
        int startIndex = startLine * this.map.gridSize + startCol;
        int endIndex = endLine * this.map.gridSize + startCol;

        HashSet<ushort> visited = new HashSet<ushort>();

        for (int line = startIndex; line <= endIndex; line += this.map.gridSize) {
            for (int index = line; index <= line + endCol; index++) {
                int cell = this.map.getCell(index);

                if (!this.map.isFreeCell(cell)) {
                    byte enemyPlayerId = this.map.getPlayer(cell);
                    // if it's and environment entity or this cell contains the current unit
                    if (enemyPlayerId == 0 || unitPlayer == enemyPlayerId) {
                        continue;
                    }

                    ushort enemyUnitId = this.map.getCounterValue(cell);
                    if (visited.Contains(enemyUnitId)) {
                        continue;
                    }

                    if (!result.ContainsKey(enemyPlayerId)) {
                        result.Add(enemyPlayerId, new HashSet<ushort>());
                    }

                    result[enemyPlayerId].Add(enemyUnitId);
                    visited.Add(enemyUnitId);
                }
            }
        }

        return result;
    }

    /*
     *  This method uses a square with a length equal to the maximum fieldOfView for any player. It checks
     *  every cell in this square to see if the current unit is visible to that one
     */
    private HashSet<byte> whoSeesThisUnitWithSquare(IClient unitOwner, Unit modifiedUnit) {
        byte modifiedUnitPlayer = this.map.getPlayer(this.map.getCell(modifiedUnit.gridIndex));
        HashSet<byte> seenBy = new HashSet<byte>();

        Tuple<int, int> indexCoords = this.map.getCoordinates(modifiedUnit.gridIndex);
        int halfFieldOfView = this.maximumFieldOfView;

        // the line start in [0, this.map.gridSize)
        int startLine = indexCoords.Item1 - halfFieldOfView;
        startLine = (startLine < 0) ? 0 : startLine;

        // the line end in [0, this.map.gridSize)
        int endLine = indexCoords.Item1 + halfFieldOfView;
        endLine = (endLine >= this.map.gridSize) ? this.map.gridSize - 1 : endLine;

        // the column start in [0, this.map.gridSize)
        int startCol = indexCoords.Item2 - halfFieldOfView;
        startCol = (startCol < 0) ? 0 : startCol;

        // the column end in [0, this.map.gridSize)
        int endCol = indexCoords.Item2 + halfFieldOfView;
        endCol = (endCol >= this.map.gridSize) ? this.map.gridSize - 1 : endCol;

        // the starting column for the first and last lines
        int startIndex = startLine * this.map.gridSize + startCol;
        int endIndex = endLine * this.map.gridSize + startCol;

        HashSet<ushort> visited = new HashSet<ushort>();

        for (int line = startIndex; line <= endIndex; line += this.map.gridSize) {
            for (int index = line; index <= line + endCol; index++) {
                int cell = this.map.getCell(index);

                if (!this.map.isFreeCell(cell)) {
                    byte playerId = this.map.getPlayer(cell);
                    // if it's and environment entity or there's already an enemy unit that sees this one or this unit 
                    // has the same player ID as the modified one
                    if (playerId == 0 || seenBy.Contains(playerId) || modifiedUnitPlayer == playerId) {
                        continue;
                    }

                    ushort unitId = this.map.getCounterValue(cell);
                    if (visited.Contains(unitId)) {
                        continue;
                    }

                    byte unitType = this.map.getEntityType(cell);
                    Player enemyPlayer = RoomMaster.players[this.playersByteMapping[playerId]];

                    // TODO: treat buildings differently
                    if (Unit.isBuilding(unitType)) {

                    } else {
                        Unit enemyUnit = enemyPlayer.army[unitId];
                        // it's a moving unit and 0 centered locally
                        float distance = Vector3.distance(enemyUnit.position, modifiedUnit.position);
                        Stats enemyUnitStats = enemyPlayer.playerStats.map(unitType);
                        if (distance <= enemyUnitStats.fieldOfView) {
                            seenBy.Add(playerId);
                        }
                    }

                    visited.Add(unitId);
                }
            }
        }

        return seenBy;
    }

    /*
     * For every player, check if there's a unit who sees this one
     */
    private HashSet<IClient> whoSeesThisUnitGreedy(IClient unitOwner, Unit modifiedUnit) {
        List<IClient> otherPlayers = this.getOtherPlayers(unitOwner);
        HashSet<IClient> seenBy = new HashSet<IClient>();

        foreach (IClient otherClient in otherPlayers) {
            Player otherPlayer = RoomMaster.players[otherClient];

            // we need only one enemy unit for each player to see the modified unit
            foreach (KeyValuePair<ushort, Unit> unit in otherPlayer.army) {
                float distance = Vector3.distance(modifiedUnit.position, unit.Value.position);
                Stats unitStats = otherPlayer.playerStats.map(unit.Value.type);

                if (distance <= unitStats.fieldOfView) {
                    if (!seenBy.Contains(otherClient)) {
                        seenBy.Add(otherClient);
                        break;
                    }
                }
            }
        }

        return seenBy;
    }

    private void handlePlayerUnitRotate(ref IClient client, ref DarkRiftReader legacyReader) {
        ushort unitId = (ushort)legacyReader.ReadInt16();
        short wholeY = legacyReader.ReadInt16();
        short fractionalY = legacyReader.ReadInt16();

        byte unitPlayer = this.playersIClientMapping[client];
        Unit unit = null;
        Player player = RoomMaster.players[client];
        if (player.army.ContainsKey(unitId)) {
            unit = player.army[unitId];
        } else {
            unit = player.buildings[unitId];
        }

        unit.rotationWhole = wholeY;
        unit.rotationFractional = fractionalY;

        HashSet<byte> seenBy = this.whoSeesThisUnitWithSquare(client, unit);
        int gridValue = this.map.buildCell(unitPlayer, unitId, unit.type);
        foreach (byte playerId in seenBy) {
            IClient enemyClient = this.playersByteMapping[playerId];

            if (!this.udpMessageQueue.ContainsKey(enemyClient)) {
                this.udpMessageQueue.Add(enemyClient, new LinkedList<PlayerMessage>());
            }

            this.udpMessageQueue[enemyClient].AddLast(new RotateMessage(gridValue, wholeY, fractionalY, unit.activity));
        }

        Dictionary<byte, HashSet<ushort>> whatThisSees = this.whatThisUnitSees(client, unit);
        foreach (KeyValuePair<byte, HashSet<ushort>> seenEnemies in whatThisSees) {
            Player enemyPlayer = RoomMaster.players[this.playersByteMapping[seenEnemies.Key]];
            foreach (ushort enemyId in seenEnemies.Value) {
                Unit enemyUnit = null;
                if (enemyPlayer.army.ContainsKey(enemyId)) {
                    enemyUnit = enemyPlayer.army[enemyId];
                } else {
                    enemyUnit = enemyPlayer.buildings[enemyId];
                }

                int enemyGridValue = this.map.buildCell(seenEnemies.Key, enemyId, enemyUnit.type);
                Tuple<short, short> posXParts = FloatIntConverter.convertFloat(enemyUnit.position.x);
                Tuple<short, short> posZParts = FloatIntConverter.convertFloat(enemyUnit.position.z);

                if (!this.udpMessageQueue.ContainsKey(client)) {
                    this.udpMessageQueue.Add(client, new LinkedList<PlayerMessage>());
                }

                this.udpMessageQueue[client].AddLast(new MovementMessage(posXParts.Item1, posXParts.Item2, posZParts.Item1, posZParts.Item2,
                    enemyGridValue, enemyUnit.rotationWhole, enemyUnit.rotationFractional, enemyUnit.activity));
            }
        }
    }

    private void handlePlayerUnitAttack(ref IClient client, ref DarkRiftReader legacyReader) {

    }

    private void handlePlayerUnitDeath(ref IClient client, ref DarkRiftReader legacyReader) {
        short unitIndex = legacyReader.ReadInt16();

        Console.WriteLine("Unit death: " + unitIndex);
    }

    private void handlePlayerUnitStop(ref IClient client, ref DarkRiftReader legacyReader) {

    }

    private void handlePlayerBuild(ref IClient client, ref DarkRiftReader legacyReader) {

    }

    private void handlePlayerGatherResource(ref IClient client, ref DarkRiftReader legacyReader) {

    }

    private void handlePlayerTechnologyUpgrade(ref IClient client, ref DarkRiftReader legacyReader) {

    }

    private Tuple<ushort, float, int, int, int, int> getOptimalWorldParams() {
        /*
         * 1st value = world length
         * 2nd value = cell size
         * 3rd value = number of trees
         * 4th value = number of forests
         * 5th value = number of gold mines
         * 6th value = number of stone mines
         */
        Tuple<ushort, float, int, int, int, int> result = null;
        Random random = new Random();

        switch (this.playersIClientMapping.Count) {
            case 2:
                result = new Tuple<ushort, float, int, int, int, int>(128, 0.25f, 256, random.Next(7, 15), random.Next(1, 3), random.Next(1, 3));
                break;
            case 4:
                result = new Tuple<ushort, float, int, int, int, int>(128, 4f, 2048, random.Next(7, 15), random.Next(1, 3), random.Next(1, 3));
                break;
            case 6:
                result = new Tuple<ushort, float, int, int, int, int>(128, 4f, 4096, random.Next(7, 15), random.Next(1, 3), random.Next(1, 3));
                break;
            case 1:
                result = new Tuple<ushort, float, int, int, int, int>(128, 0.25f, 256, random.Next(7, 15), random.Next(1, 3), random.Next(1, 3));
                break;
        }

        return result;
    }

    public void sendWorldToPlayers() {
        // init the world map
        Tuple<ushort, float, int, int, int, int> optimalParams = this.getOptimalWorldParams();
        this.map = new WorldMap(optimalParams.Item1, optimalParams.Item2);

        // send general world data to players
        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            writer.Write(optimalParams.Item1); // world length
            writer.Write(optimalParams.Item2); // cell size

            using (Message response = Message.Create(Tags.SEND_WORLD_DATA, writer)) {
                foreach (KeyValuePair<IClient, byte> player in this.playersIClientMapping) {
                    player.Key.SendMessage(response, SendMode.Reliable);
                }
            }
        }

        TerrainGenerator generator = new TerrainGenerator(this.map);

        // generate players data
        Dictionary<byte, List<Tuple<int, int>>> playersData = generator.generatePlayers((byte)(this.playersIClientMapping.Count));

        // 10% of the trees will be randomly positioned
        int noRandom = (int)(optimalParams.Item3 * 0.05f);
        int[] randomTreesPositions = generator.generateRandomPositionedTrees(noRandom);

        // 90% of the trees will be part of forests
        int[] forestsPositions = generator.generateRandomForests(optimalParams.Item3 - noRandom, optimalParams.Item4);

        int noGoldMines = optimalParams.Item5 * Room.goldChunkSize;
        int noStoneMines = optimalParams.Item6 * Room.stoneChunkSize;
        int[] goldPositions = generator.generateRandomMines(noGoldMines);
        int[] stonePositions = generator.generateRandomMines(noStoneMines, isGold: false);

        Random random = new Random();

        // send trees positions in chunks of Room.treesPerPackage
        for (int i = 0; i < optimalParams.Item3; i += Room.treesPerPackage) {
            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                for (int j = i; j < (i + Room.treesPerPackage) && j < optimalParams.Item3; j++) {
                    if (j < noRandom) {
                        byte treeType = (byte)(this.map.getEntityType(this.map.getCell(randomTreesPositions[j])) - EntityType.TREE_TYPE1 + 1);
                        ushort counter = this.map.getCounterValue(this.map.getCell(randomTreesPositions[j]));

                        writer.Write(treeType);
                        writer.Write(counter);
                        writer.Write(randomTreesPositions[j]);
                    } else {
                        byte treeType = (byte)(this.map.getEntityType(this.map.getCell(forestsPositions[j - noRandom])) - EntityType.TREE_TYPE1 + 1);
                        ushort counter = this.map.getCounterValue(this.map.getCell(forestsPositions[j - noRandom]));

                        writer.Write(treeType);
                        writer.Write(counter);
                        writer.Write(forestsPositions[j - noRandom]);
                    }
                }

                using (Message response = Message.Create(Tags.SEND_TREE_DATA, writer)) {
                    foreach (KeyValuePair<IClient, byte> player in this.playersIClientMapping) {
                        player.Key.SendMessage(response, SendMode.Reliable);
                    }
                }
            }
        }

        // send gold positions in chunks
        for (int i = 0; i < noGoldMines; i += Room.goldChunkSize) {
            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                for (int j = i; j < (i + Room.goldChunkSize) && j < noGoldMines; j++) {
                    ushort counter = this.map.getCounterValue(this.map.getCell(goldPositions[i]));
                    writer.Write(counter);
                    writer.Write(goldPositions[j]);
                }

                using (Message response = Message.Create(Tags.SEND_GOLD_DATA, writer)) {
                    foreach (KeyValuePair<IClient, byte> player in this.playersIClientMapping) {
                        player.Key.SendMessage(response, SendMode.Reliable);
                    }
                }
            }
        }

        // send stone positions in chunks
        for (int i = 0; i < noStoneMines; i += Room.stoneChunkSize) {
            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                for (int j = i; j < (i + Room.stoneChunkSize) && j < noStoneMines; j++) {
                    ushort counter = this.map.getCounterValue(this.map.getCell(stonePositions[i]));
                    writer.Write(counter);
                    writer.Write(stonePositions[j]);
                }

                using (Message response = Message.Create(Tags.SEND_STONE_DATA, writer)) {
                    foreach (KeyValuePair<IClient, byte> player in this.playersIClientMapping) {
                        player.Key.SendMessage(response, SendMode.Reliable);
                    }
                }
            }
        }

        // notice the clients that all terrain data has been sent
        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            using (Message response = Message.Create(Tags.DONE_SENDING_TERRAIN, writer)) {
                foreach (KeyValuePair<IClient, byte> player in this.playersIClientMapping) {
                    player.Key.SendMessage(response, SendMode.Reliable);
                }
            }
        }

        // send data to every player
        foreach (KeyValuePair<byte, List<Tuple<int, int>>> playerData in playersData) {
            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                byte length = (byte)(playerData.Value.Count);
                writer.Write(length);

                Player currentPlayer = RoomMaster.players[this.playersByteMapping[playerData.Key]];

                // TODO: this might be optimized to quit sending the player ID. Basically, entry.item2 will be 3 bytes,
                // because we'll skip the first one (player ID)
                foreach (Tuple<int, int> entry in playerData.Value) {
                    writer.Write(entry.Item1);
                    writer.Write(entry.Item2);

                    // add this unit to its player data structure
                    byte entityType = this.map.getEntityType(entry.Item2);
                    ushort entityId = this.map.getCounterValue(entry.Item2);

                    Tuple<float, float, float> rawPos = this.map.getCellPosition(entry.Item1);
                    Unit newUnit = new Unit(new Vector3(rawPos), 0, 0, entityType, entry.Item1);
                    newUnit.activity = Activities.NONE;

                    // compute the maximum field of view for every unit
                    byte thisUnitsFov = RoomMaster.players[this.playersByteMapping[playerData.Key]].playerStats.map(entityType).fieldOfView;
                    this.maximumFieldOfView = Math.Max(thisUnitsFov, this.maximumFieldOfView);

                    if (Unit.isBuilding(entityType)) {
                        currentPlayer.buildings.Add(entityId, newUnit);
                    } else {
                        currentPlayer.army.Add(entityId, newUnit);
                    }
                }

                using (Message response = Message.Create(Tags.SEND_PLAYER_DATA, writer)) {
                    IClient player = this.playersByteMapping[playerData.Key];
                    player.SendMessage(response, SendMode.Reliable);
                }
            }
        }

        this.maximumFieldOfView = (byte)(this.maximumFieldOfView * this.map.cellLength);

        // notice the clients that the game can start
        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            using (Message response = Message.Create(Tags.DONE_INIT_WORLD, writer)) {
                foreach (KeyValuePair<IClient, byte> player in this.playersIClientMapping) {
                    player.Key.SendMessage(response, SendMode.Reliable);
                }
            }
        }

        // notice the players about other players civilizations
        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            foreach (KeyValuePair<IClient, byte> playerPair in this.playersIClientMapping) {
                Player player = RoomMaster.players[playerPair.Key];
                writer.Write(playerPair.Value);
                writer.Write(player.civilization);
                Console.WriteLine("Civilization: " + playerPair.Value + " " + player.civilization);
            }

            using (Message response = Message.Create(Tags.GET_PLAYER_CIVILIZATION, writer)) {
                foreach (KeyValuePair<IClient, byte> player in this.playersIClientMapping) {
                    Console.WriteLine("Send civ message to " + player.Key + " " + player.Value);
                    player.Key.SendMessage(response, SendMode.Reliable);
                }
            }
        }
    }
}
