using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DarkRift;
using DarkRift.Server;
using YamlDotNet.Core.Tokens;

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
    private Dictionary<Tuple<ushort, byte>, AttackEvent> unitsToReceiveDamage;
    public static byte tcpPackageLimit = 20;
    public static byte udpPackageLimit = 30;

    //private bool cleanStopMovementsBlock = false;

    public Room(string uuid, string name, byte maxNoPlayers) {
        this.uuid = uuid;
        this.name = name;
        this.maxNumberOfPlayers = maxNoPlayers;
        this.maxPlayerId = 1;

        this.udpMessageQueue = new Dictionary<IClient, LinkedList<PlayerMessage>>();
        this.tcpMessageQueue = new Dictionary<IClient, LinkedList<PlayerMessage>>();
        this.unitsToReceiveDamage = new Dictionary<Tuple<ushort, byte>, AttackEvent>();

        this.playersIClientMapping = new Dictionary<IClient, byte>();
        this.playersByteMapping = new Dictionary<byte, IClient>();
    }

    public void ClearMemory() {
        this.map = null;
        this.playersIClientMapping.Clear();
        this.playersByteMapping.Clear();
    }

    public void sendInflictedDamagePackets() {
        LinkedList<Tuple<ushort, byte>> garbageAttackEvents = new LinkedList<Tuple<ushort, byte>>();

        foreach(KeyValuePair<Tuple<ushort, byte>, AttackEvent> damageReceiveEvent in this.unitsToReceiveDamage) {
            Player player = RoomMaster.players[this.playersByteMapping[damageReceiveEvent.Key.Item2]];
            Unit victim = null;

            if (player.army.ContainsKey(damageReceiveEvent.Key.Item1)) {
                victim = player.army[damageReceiveEvent.Key.Item1];
            } else if (player.buildings.ContainsKey(damageReceiveEvent.Key.Item1)) {
                victim = player.buildings[damageReceiveEvent.Key.Item1];
            } else {
                garbageAttackEvents.AddLast(damageReceiveEvent.Key);
                continue;
            }

            bool isDead = damageReceiveEvent.Value.isDeadAfterInflictedAttacks(ref victim);

            if (isDead) {
                // notify all players about this unit's death and remove the reference from player's seenBuildings
                PlayerMessage deathMessage = new DeathMessage(damageReceiveEvent.Key.Item1, damageReceiveEvent.Key.Item2);
                foreach(KeyValuePair<IClient, byte> playerMap in this.playersIClientMapping) {
                    if (this.tcpMessageQueue.ContainsKey(playerMap.Key)) {
                        this.tcpMessageQueue.Add(playerMap.Key, new LinkedList<PlayerMessage>());
                    }
                    this.tcpMessageQueue[playerMap.Key].AddLast(deathMessage);

                    // remove the seenBuildings reference
                    Player currentPlayer = RoomMaster.players[playerMap.Key];
                    if (currentPlayer.seenBuildings[damageReceiveEvent.Key.Item2].Contains(damageReceiveEvent.Key.Item1)) {
                        currentPlayer.seenBuildings[damageReceiveEvent.Key.Item2].Remove(damageReceiveEvent.Key.Item1);
                    }
                }

                // schedule this attack event to be removed from the queue
                garbageAttackEvents.AddLast(damageReceiveEvent.Key);

                // remove the unit reference from the server
                if (player.army.ContainsKey(damageReceiveEvent.Key.Item1)) {
                    player.army.Remove(damageReceiveEvent.Key.Item1);
                } else {
                    player.buildings.Remove(damageReceiveEvent.Key.Item1);
                }
            } else {
                // notify all players about this unit's current hp
                PlayerMessage hpUpgrade = new HpUpgradeMessage(damageReceiveEvent.Key.Item2, damageReceiveEvent.Key.Item1, victim.currentHp);
                foreach(KeyValuePair<IClient, byte> playerMap in this.playersIClientMapping) {
                    if (!this.udpMessageQueue.ContainsKey(playerMap.Key)) {
                        this.udpMessageQueue.Add(playerMap.Key, new LinkedList<PlayerMessage>());
                    }
                    this.udpMessageQueue[playerMap.Key].AddLast(hpUpgrade);
                }
            }
        }

        while (garbageAttackEvents.Count > 0) {
            this.unitsToReceiveDamage.Remove(garbageAttackEvents.First.Value);
            garbageAttackEvents.RemoveFirst();
        }
    }

    public void sendDataToPlayersCallback() {
        /*while (this.cleanStopMovementsBlock) {

        }*/

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
                case Tags.PLAYER_BUILD:
                    this.handlePlayerBuild(ref client, ref legacyReader);
                    break;
                case Tags.PLAYER_TECHNOLOGY_UPGRADE:
                    this.handlePlayerTechnologyUpgrade(ref client, ref legacyReader);
                    break;
                case Tags.PLAYER_STOP_UNIT:
                    this.handlePlayerUnitStop(ref client, ref legacyReader);
                    break;
                case Tags.PLAYER_UNIT_DEATH:
                    this.handlePlayerUnitDeath(ref client, ref legacyReader);
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
        Stats unitStats = player.playerStats.map(type);

        Vector3 positionVector = new Vector3(x, 0, z);
        if (Unit.isBuilding(type)) {
            if (!player.buildings.ContainsKey(counterValue)) {
                newUnit = new Unit(positionVector, rotationWhole, rotationFractional, type, unitStats.hp, gridIndex);
                player.buildings.Add(counterValue, newUnit);
                sendSpawnToOthers = true;
            }
        } else if (!player.army.ContainsKey(counterValue)) {
            newUnit = new Unit(positionVector, rotationWhole, rotationFractional, type, unitStats.hp, gridIndex);
            player.army.Add(counterValue, newUnit);
            sendSpawnToOthers = true;
        }

        newUnit.activity = Activities.NONE;

        if (sendSpawnToOthers) {
            HashSet<byte> seenBy = this.whoSeesThisUnitWithSquare(client, newUnit, this.map.getPlayer(gridValue));

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
        ushort unitId = legacyReader.ReadUInt16();
        short wholeX = legacyReader.ReadInt16();
        short fractionalX = legacyReader.ReadInt16();
        short wholeZ = legacyReader.ReadInt16();
        short fractionalZ = legacyReader.ReadInt16();
        short rotationWhole = legacyReader.ReadInt16();
        short rotationFractional = legacyReader.ReadInt16();

        float x = FloatIntConverter.convertInt(wholeX, fractionalX);
        float z = FloatIntConverter.convertInt(wholeZ, fractionalZ);

        try {
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

            int gridValue = this.map.buildCell(unitPlayer, unitId, unit.type);
            PlayerMessage customMessage = new MovementMessage(wholeX, fractionalX, wholeZ, fractionalZ,
                        gridValue, rotationWhole, rotationFractional, Activities.MOVING);

            this.notifyOtherPlayersOnUnitEvent(ref client, unit, unitPlayer, unitId, ref customMessage);
        } catch (KeyNotFoundException) {

        }
    }

    private void handlePlayerUnitRotate(ref IClient client, ref DarkRiftReader legacyReader) {
        ushort unitId = legacyReader.ReadUInt16();
        short wholeY = legacyReader.ReadInt16();
        short fractionalY = legacyReader.ReadInt16();

        byte unitPlayer = this.playersIClientMapping[client];
        Unit unit = null;
        Player player = RoomMaster.players[client];

        try {
            if (player.army.ContainsKey(unitId)) {
                unit = player.army[unitId];
            } else {
                unit = player.buildings[unitId];
            }

            unit.rotationWhole = wholeY;
            unit.rotationFractional = fractionalY;

            Tuple<short, short> posXParts = FloatIntConverter.convertFloat(unit.position.x);
            Tuple<short, short> posZParts = FloatIntConverter.convertFloat(unit.position.z);

            int gridValue = this.map.buildCell(unitPlayer, unitId, unit.type);
            PlayerMessage customMessage = new MovementMessage(posXParts.Item1, posXParts.Item2, posZParts.Item1, posZParts.Item2,
                        gridValue, wholeY, fractionalY, unit.activity);

            this.notifyOtherPlayersOnUnitEvent(ref client, unit, unitPlayer, unitId, ref customMessage);
        } catch (KeyNotFoundException) {

        }
    }

    private void handlePlayerUnitAttack(ref IClient client, ref DarkRiftReader legacyReader) {
        ushort attackerId = (ushort)legacyReader.ReadUInt16();
        ushort victimId = (ushort)legacyReader.ReadUInt16();
        byte victimPlayerId = legacyReader.ReadByte();

        byte attackerPlayerId = this.playersIClientMapping[client];
        Player attackerPlayer = RoomMaster.players[client];
        Player victimPlayer = RoomMaster.players[this.playersByteMapping[victimPlayerId]];

        Unit attackerUnit = null;
        if (attackerPlayer.army.ContainsKey(attackerId)) {
            attackerUnit = attackerPlayer.army[attackerId];
        } else {
            attackerUnit = attackerPlayer.buildings[attackerId];
        }

        Unit victimUnit = null;
        if (victimPlayer.army.ContainsKey(victimId)) {
            victimUnit = victimPlayer.army[victimId];
        } else {
            victimUnit = victimPlayer.buildings[victimId];
        }

        attackerUnit.activity = Activities.ATTACKING;
        PlayerMessage customMessage = new AttackMessage(attackerId, attackerPlayerId);

        try {
            this.notifyOtherPlayersOnUnitEvent(ref client, attackerUnit, attackerPlayerId, attackerId, ref customMessage, sendCustomOnTCP: true);
        } catch (KeyNotFoundException e) {
            Console.WriteLine("Key not found in handlePlayerUnitAttack " + e.ToString());
        }
    }

    private void handlePlayerUnitDeath(ref IClient client, ref DarkRiftReader legacyReader) {
        ushort unitId = legacyReader.ReadUInt16();
        byte playerId = legacyReader.ReadByte();

        // remove the reference from player's tables
        Player player = RoomMaster.players[this.playersByteMapping[playerId]];
        bool isBuildingUnit = false;

        if (player.army.ContainsKey(unitId)) {
            player.army.Remove(unitId);
        } else {
            player.buildings.Remove(unitId);
            isBuildingUnit = true;
        }

        Console.WriteLine("Unit death: " + unitId + " from player: " + playerId);

        if (isBuildingUnit == false) {
            return;
        }

        foreach(IClient otherClient in this.getOtherPlayers(client)) {
            Player otherPlayer = RoomMaster.players[otherClient];

            if (otherPlayer.seenBuildings.ContainsKey(playerId)) {
                if (otherPlayer.seenBuildings[playerId].Contains(unitId)) {
                    otherPlayer.seenBuildings[playerId].Remove(unitId);
                }
            } else {
                continue;
            }
        }
    }

    private void handlePlayerUnitStop(ref IClient client, ref DarkRiftReader legacyReader) {
        ushort unitId = legacyReader.ReadUInt16();
        short wholeX = legacyReader.ReadInt16();
        short fractionalX = legacyReader.ReadInt16();
        short wholeZ = legacyReader.ReadInt16();
        short fractionalZ = legacyReader.ReadInt16();
        byte playerId = this.playersIClientMapping[client];

        // TODO: this shit might produce some network lag
        /*this.cleanStopMovementsBlock = true;
        foreach (KeyValuePair<IClient, LinkedList<PlayerMessage>> message in this.udpMessageQueue) {
            LinkedList<PlayerMessage> toRemove = new LinkedList<PlayerMessage>();

            foreach(PlayerMessage playerMessage in message.Value) {
                if (playerMessage.GetType().IsEquivalentTo(typeof(MovementMessage))) {
                    int gridValue = ((MovementMessage)(playerMessage)).gridValue;
                    if (this.map.getCounterValue(gridValue) == unitId && this.map.getPlayer(gridValue) == playerId) {
                        toRemove.AddLast(playerMessage);
                    }
                }
            }

            while(toRemove.Count > 0) {
                PlayerMessage playerMessage = toRemove.First.Value;
                toRemove.RemoveFirst();

                message.Value.Remove(playerMessage);
            }
        }
        this.cleanStopMovementsBlock = false;*/

        float x = FloatIntConverter.convertInt(wholeX, fractionalX);
        float z = FloatIntConverter.convertInt(wholeZ, fractionalZ);

        int gridIndex = this.map.getGridIndex(x, 0, z);

        Player player = RoomMaster.players[client];
        Unit unit = null;

        try {
            if (player.army.ContainsKey(unitId)) {
                unit = player.army[unitId];
            } else {
                unit = player.buildings[unitId];
            }

            unit.gridIndex = gridIndex;
            unit.position.x = x;
            unit.position.z = z;
            unit.activity = Activities.NONE;

            // remove this unit from the attack avents queue if it exists
            Stats unitStats = player.playerStats.map(unit.type);
            foreach(KeyValuePair<Tuple<ushort, byte>, AttackEvent> attackEvent in this.unitsToReceiveDamage) {
                attackEvent.Value.removeAttacker(new Tuple<ushort, byte, short>(unitId, playerId, (short)(unitStats.attack + unitStats.upgradedAttack)));
            }

            int gridValue = this.map.buildCell(playerId, unitId, unit.type);

            PlayerMessage customMessage = new StopMessage(wholeX, fractionalX, wholeZ, fractionalZ, gridValue);
            this.notifyOtherPlayersOnUnitEvent(ref client, unit, playerId, unitId, ref customMessage, sendCustomOnTCP: true);
        } catch (KeyNotFoundException) {

        }
    }

    private void handlePlayerBuild(ref IClient client, ref DarkRiftReader legacyReader) {
        int gridIndex = legacyReader.ReadInt32();
        int gridValue = legacyReader.ReadInt32();

        ushort unitId = this.map.getCounterValue(gridValue);
        byte buildingType = this.map.getEntityType(gridValue);
        byte playerId = this.map.getPlayer(gridValue);

        Console.WriteLine("Player build: " + unitId + " " + buildingType);

        // create the new entity and add it to its player
        Vector3 position = new Vector3(this.map.getCellPosition(gridIndex));
        Player player = RoomMaster.players[client];
        Stats unitStats = player.playerStats.map(buildingType);

        Unit newUnit = new Unit(position, 0, 0, buildingType, unitStats.hp, gridIndex);
        player.buildings.Add(unitId, newUnit);

        // mark this building on the grid
        Size buildingSize = SizeMapping.map(buildingType);
        this.map.markIndexSquare(gridIndex, buildingSize, buildingType, playerId, unitId);

        // add this build message to clients queues
        PlayerMessage customMessage = new BuildMessage(gridIndex, gridValue);

        try {
            this.notifyOtherPlayersOnUnitEvent(ref client, newUnit, playerId, unitId, ref customMessage, sendCustomOnTCP: true);
        } catch (KeyNotFoundException e) {
            Console.WriteLine(e.ToString());
        }
    }

    private void handlePlayerGatherResource(ref IClient client, ref DarkRiftReader legacyReader) {

    }

    private void handlePlayerTechnologyUpgrade(ref IClient client, ref DarkRiftReader legacyReader) {
        byte upgradeTag = legacyReader.ReadByte();

        Player player = RoomMaster.players[client];
        // ignore this message as it could be a client side bug that sends the same message
        // multiple times
        if (player.technologyUpgrades.Contains(upgradeTag)) {
            return;
        }

        // register the upgrade on the server
        TechnologyUpgrader.upgrade(ref player, upgradeTag);
        player.technologyUpgrades.Add(upgradeTag);

        byte playerId = this.playersIClientMapping[client];
        PlayerMessage customMessage = new UpgradeMessage(playerId, upgradeTag);

        foreach(IClient otherPlayer in this.getOtherPlayers(client)) {
            if (!this.tcpMessageQueue.ContainsKey(otherPlayer)) {
                this.tcpMessageQueue.Add(otherPlayer, new LinkedList<PlayerMessage>());
            }

            this.tcpMessageQueue[otherPlayer].AddLast(customMessage);
        }
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
                    Stats unitStats = currentPlayer.playerStats.map(entityType);
                    Unit newUnit = new Unit(new Vector3(rawPos), 0, 0, entityType, unitStats.hp, entry.Item1);
                    newUnit.activity = Activities.NONE;

                    // compute the maximum field of view for every unit
                    byte thisUnitsFov = (byte)(RoomMaster.players[this.playersByteMapping[playerData.Key]].playerStats.map(entityType).fieldOfView);// * this.map.cellLength);
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

        //this.maximumFieldOfView = (byte)(this.maximumFieldOfView * this.map.cellLength);

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
            }

            using (Message response = Message.Create(Tags.GET_PLAYER_CIVILIZATION, writer)) {
                foreach (KeyValuePair<IClient, byte> player in this.playersIClientMapping) {
                    player.Key.SendMessage(response, SendMode.Reliable);
                }
            }
        }
    }

    private void notifyOtherPlayersOnUnitEvent(ref IClient client, Unit unit, byte unitPlayer, ushort unitId, ref PlayerMessage customMessage, bool sendCustomOnTCP = false) {
        HashSet<byte> seenBy = this.whoSeesThisUnitWithSquare(client, unit, unitPlayer);

        foreach (byte playerId in seenBy) {
            IClient enemyClient = this.playersByteMapping[playerId];

            if (sendCustomOnTCP) {
                if (!this.tcpMessageQueue.ContainsKey(enemyClient)) {
                    this.tcpMessageQueue.Add(enemyClient, new LinkedList<PlayerMessage>());
                }

                this.tcpMessageQueue[enemyClient].AddLast(customMessage);
            } else {
                if (!this.udpMessageQueue.ContainsKey(enemyClient)) {
                    this.udpMessageQueue.Add(enemyClient, new LinkedList<PlayerMessage>());
                }

                this.udpMessageQueue[enemyClient].AddLast(customMessage);
            }            
        }

        Dictionary<byte, HashSet<ushort>> whatThisSees = this.whatThisUnitSees(client, unit);
        foreach (KeyValuePair<byte, HashSet<ushort>> seenEnemies in whatThisSees) {
            Player enemyPlayer = RoomMaster.players[this.playersByteMapping[seenEnemies.Key]];
            foreach (ushort enemyId in seenEnemies.Value) {
                Unit enemyUnit = null;
                if (enemyPlayer.army.ContainsKey(enemyId)) {
                    enemyUnit = enemyPlayer.army[enemyId];
                    int enemyGridValue = this.map.buildCell(seenEnemies.Key, enemyId, enemyUnit.type);

                    Tuple<short, short> posXParts = FloatIntConverter.convertFloat(enemyUnit.position.x);
                    Tuple<short, short> posZParts = FloatIntConverter.convertFloat(enemyUnit.position.z);

                    if (!this.udpMessageQueue.ContainsKey(client)) {
                        this.udpMessageQueue.Add(client, new LinkedList<PlayerMessage>());
                    }

                    this.udpMessageQueue[client].AddLast(new MovementMessage(posXParts.Item1, posXParts.Item2, posZParts.Item1, posZParts.Item2,
                        enemyGridValue, enemyUnit.rotationWhole, enemyUnit.rotationFractional, enemyUnit.activity));
                } else {
                    Player currentPlayer = RoomMaster.players[client];
                    if (!currentPlayer.seenBuildings.ContainsKey(seenEnemies.Key)) {
                        currentPlayer.seenBuildings.Add(seenEnemies.Key, new HashSet<ushort>());
                    }

                    // if this building is seen by the current player, then we'll not send a new building discovery message
                    if (currentPlayer.seenBuildings[seenEnemies.Key].Contains(enemyId)) {
                        continue;
                    } else {
                        enemyUnit = enemyPlayer.buildings[enemyId];
                        int enemyGridValue = this.map.buildCell(seenEnemies.Key, enemyId, enemyUnit.type);

                        if (!this.tcpMessageQueue.ContainsKey(client)) {
                            this.tcpMessageQueue.Add(client, new LinkedList<PlayerMessage>());
                        }

                        this.tcpMessageQueue[client].AddLast(new BuildingDiscoverMessage(enemyUnit.gridIndex, enemyGridValue));

                        // mark this building as discoved by the current player
                        currentPlayer.seenBuildings[seenEnemies.Key].Add(enemyId);
                    }
                }
            }
        }
    }

    private Dictionary<byte, HashSet<ushort>> whatThisUnitSees(IClient unitOwner, Unit currentUnit) {
        Dictionary<byte, HashSet<ushort>> result = new Dictionary<byte, HashSet<ushort>>();
        byte unitPlayer = this.playersIClientMapping[unitOwner];
        Player currentPlayer = RoomMaster.players[unitOwner];

        Tuple<int, int> currentUnitCoords = this.map.getCoordinates(currentUnit.gridIndex);
        int halfFieldOfView = (int)(currentPlayer.playerStats.map(currentUnit.type).fieldOfView * this.map.cellLength);

        // the line start in [0, this.map.gridSize)
        int startLine = currentUnitCoords.Item1 - halfFieldOfView;

        // the line end in [0, this.map.gridSize)
        int endLine = currentUnitCoords.Item1 + halfFieldOfView;

        // the column start in [0, this.map.gridSize)
        int startCol = currentUnitCoords.Item2 - halfFieldOfView;

        // the column end in [0, this.map.gridSize)
        int endCol = currentUnitCoords.Item2 + halfFieldOfView;

        if (Unit.isBuilding(currentUnit.type)) {
            //startLine 
        }

        startLine = (startLine < 0) ? 0 : startLine;
        endLine = (endLine >= this.map.gridSize) ? this.map.gridSize - 1 : endLine;
        startCol = (startCol < 0) ? 0 : startCol;
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
    private HashSet<byte> whoSeesThisUnitWithSquare(IClient unitOwner, Unit modifiedUnit, byte modifiedUnitPlayer) {
        HashSet<byte> seenBy = new HashSet<byte>();

        Tuple<int, int> indexCoords = this.map.getCoordinates(modifiedUnit.gridIndex);

        // the line start in [0, this.map.gridSize)
        int startLine = indexCoords.Item1 - this.maximumFieldOfView;
        startLine = (startLine < 0) ? 0 : startLine;

        // the line end in [0, this.map.gridSize)
        int endLine = indexCoords.Item1 + this.maximumFieldOfView;
        endLine = (endLine >= this.map.gridSize) ? this.map.gridSize - 1 : endLine;

        // the column start in [0, this.map.gridSize)
        int startCol = indexCoords.Item2 - this.maximumFieldOfView;
        startCol = (startCol < 0) ? 0 : startCol;

        // the column end in [0, this.map.gridSize)
        int endCol = indexCoords.Item2 + this.maximumFieldOfView;
        endCol = (endCol >= this.map.gridSize) ? this.map.gridSize - 1 : endCol;

        // the starting column for the first and last lines
        int startIndexLine = startLine * this.map.gridSize;
        int endIndexCol = endLine * this.map.gridSize;

        HashSet<ushort> visited = new HashSet<ushort>();

        for (int line = startIndexLine; line <= endIndexCol; line += this.map.gridSize) {
            for (int index = line + startCol; index <= line + endCol; index++) {
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
                        Unit enemyUnit = enemyPlayer.buildings[unitId];

                        Size buildingSize = SizeMapping.map(unitType);
                        Tuple<int, int> gridCoords = this.map.getCoordinates(enemyUnit.gridIndex);
                        int centerGridIndex = this.map.gridSize * (gridCoords.Item1 + buildingSize.height / 2) +
                            gridCoords.Item2 + buildingSize.width / 2;
                        Vector3 buildingCenter = new Vector3(this.map.getCellPosition(centerGridIndex));

                        float distance = Vector3.distance(buildingCenter, modifiedUnit.position);
                        Stats enemyUnitStats = enemyPlayer.playerStats.map(unitType);
                        if (distance <= (enemyUnitStats.fieldOfView * this.map.cellLength)) {
                            seenBy.Add(playerId);
                        }
                    } else {
                        Unit enemyUnit = enemyPlayer.army[unitId];
                        // it's a moving unit and 0 centered locally
                        float distance = Vector3.distance(enemyUnit.position, modifiedUnit.position);
                        Stats enemyUnitStats = enemyPlayer.playerStats.map(unitType);
                        if (distance <= (enemyUnitStats.fieldOfView * this.map.cellLength)) {
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
}
