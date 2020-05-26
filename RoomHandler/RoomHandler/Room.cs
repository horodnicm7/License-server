using System;
using System.Collections.Generic;
using System.Text;
using DarkRift;
using DarkRift.Server;

public class Room {
    // it will work with the instances from RoomMaster and this dictionary will only keep 
    // clients as keys and bool values meaning that a client is room's owner or not
    public Dictionary<IClient, byte> players;
    public string uuid;
    public string name;
    public byte maxNumberOfPlayers;
    public IClient leader;
    public byte maxPlayerId; // used for player id incrementation

    private WorldMap map;
    private const byte treesPerPackage = 8;
    private const byte goldChunkSize = 10;
    private const byte stoneChunkSize = 5;

    public Room(string uuid, string name, byte maxNoPlayers) {
        this.uuid = uuid;
        this.name = name;
        this.maxNumberOfPlayers = maxNoPlayers;
        this.maxPlayerId = 1;

        this.players = new Dictionary<IClient, byte>();
    }

    public void ClearMemory() {
        this.map = null;
        this.players.Clear();
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

        switch (this.players.Count) {
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
                foreach (KeyValuePair<IClient, byte> player in this.players) {
                    player.Key.SendMessage(response, SendMode.Reliable);
                }
            }
        }

        TerrainGenerator generator = new TerrainGenerator(this.map);

        // generate players data
        Dictionary<byte, List<Tuple<int, int>>> playersData = generator.generatePlayers((byte)(this.players.Count));

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
                        byte treeType = (byte) (this.map.getEntityType(this.map.getCell(randomTreesPositions[j])) - EntityType.TREE_TYPE1 + 1);
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
                    foreach(KeyValuePair<IClient, byte> player in this.players) {
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
                    foreach (KeyValuePair<IClient, byte> player in this.players) {
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
                    foreach (KeyValuePair<IClient, byte> player in this.players) {
                        player.Key.SendMessage(response, SendMode.Reliable);
                    }
                }
            }
        }

        // notice the clients that all terrain data has been sent
        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            using (Message response = Message.Create(Tags.DONE_SENDING_TERRAIN, writer)) {
                foreach (KeyValuePair<IClient, byte> player in this.players) {
                    player.Key.SendMessage(response, SendMode.Reliable);
                }
            }
        }

        // send data to every player
        foreach(KeyValuePair<byte, List<Tuple<int, int>>> playerData in playersData) {
            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                byte length = (byte)(playerData.Value.Count);
                writer.Write(length);

                // TODO: this might be optimized to quit sending the player ID. Basically, entry.item2 will be 3 bytes,
                // because we'll skip the first one (player ID)
                foreach (Tuple<int, int> entry in playerData.Value) {
                    writer.Write(entry.Item1);
                    writer.Write(entry.Item2);
                }

                // TODO: send this data only to its player
                using (Message response = Message.Create(Tags.SEND_PLAYER_DATA, writer)) {
                    foreach (KeyValuePair<IClient, byte> player in this.players) {
                        // if the units belong to the current player
                        if (player.Value == playerData.Key) {
                            player.Key.SendMessage(response, SendMode.Reliable);
                        }
                    }
                }
            }
        }

        // notice the clients that the game can start
        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            using (Message response = Message.Create(Tags.DONE_INIT_WORLD, writer)) {
                foreach (KeyValuePair<IClient, byte> player in this.players) {
                    player.Key.SendMessage(response, SendMode.Reliable);
                }
            }
        }

        // notice the players about other players civilizations
        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            foreach(KeyValuePair<IClient, byte> playerPair in this.players) {
                Player player = RoomMaster.players[playerPair.Key];
                writer.Write(playerPair.Value);
                writer.Write(player.civilization);
            }

            using (Message response = Message.Create(Tags.GET_PLAYER_CIVILIZATION, writer)) {
                foreach (KeyValuePair<IClient, byte> player in this.players) {
                    player.Key.SendMessage(response, SendMode.Reliable);
                }
            }
        }
    }

    public List<IClient> getOtherPlayers(IClient except) {
        List<IClient> others = new List<IClient>();

        foreach(KeyValuePair<IClient, byte> player in this.players) {
            if (except != player.Key) {
                others.Add(player.Key);
            }
        }

        return others;
    }

    public void changeLeader() {
        foreach(KeyValuePair<IClient, byte> player in this.players) {
            this.leader = player.Key;
            break;
        }
    }

    public void MessageReceived(object sender, MessageReceivedEventArgs e) {
        Logger.print("Message from new room");
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

    }

    private void handlePlayerUnitMove(ref IClient client, ref DarkRiftReader legacyReader) {
        
    }

    private void handlePlayerUnitAttack(ref IClient client, ref DarkRiftReader legacyReader) {

    }

    private void handlePlayerUnitRotate(ref IClient client, ref DarkRiftReader legacyReader) {

    }

    private void handlePlayerUnitDeath(ref IClient client, ref DarkRiftReader legacyReader) {

    }

    private void handlePlayerUnitStop(ref IClient client, ref DarkRiftReader legacyReader) {

    }

    private void handlePlayerBuild(ref IClient client, ref DarkRiftReader legacyReader) {

    }

    private void handlePlayerGatherResource(ref IClient client, ref DarkRiftReader legacyReader) {

    }

    private void handlePlayerTechnologyUpgrade(ref IClient client, ref DarkRiftReader legacyReader) {

    }
}
