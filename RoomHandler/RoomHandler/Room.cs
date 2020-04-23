using System;
using System.Collections.Generic;
using System.Text;
using DarkRift;
using DarkRift.Server;

public class Room {
    // it will work with the instances from RoomMaster and this dictionary will only keep 
    // clients as keys and bool values meaning that a client is room's owner or not
    public Dictionary<IClient, bool> players;
    public string uuid;
    public string name;
    public byte maxNumberOfPlayers;

    private WorldMap map;
    private const byte treesPerPackage = 8;
    private const byte goldChunkSize = 10;
    private const byte stoneChunkSize = 5;

    public Room(string uuid, string name, byte maxNoPlayers) {
        this.uuid = uuid;
        this.name = name;
        this.maxNumberOfPlayers = maxNoPlayers;

        this.players = new Dictionary<IClient, bool>();
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
                result = new Tuple<ushort, float, int, int, int, int>(128, 4f, 1024, random.Next(7, 15), random.Next(1, 3), random.Next(1, 3));
                break;
            case 4:
                result = new Tuple<ushort, float, int, int, int, int>(256, 4f, 2048, random.Next(7, 15), random.Next(1, 3), random.Next(1, 3));
                break;
            case 6:
                result = new Tuple<ushort, float, int, int, int, int>(512, 4f, 4096, random.Next(7, 15), random.Next(1, 3), random.Next(1, 3));
                break;
        }

        return result;
    }

    public void sendWorldToPlayers() {
        // init the world map
        Tuple<ushort, float, int, int, int, int> optimalParams = this.getOptimalWorldParams();
        this.map = new WorldMap(optimalParams.Item1, optimalParams.Item2);

        TerrainGenerator generator = new TerrainGenerator(this.map);
        // 10% of the trees will be randomly positioned
        int noRandom = (int)(optimalParams.Item3 * 0.1f);
        int[] randomTreesPositions = generator.generateRandomPositionedTrees(noRandom);

        // 90% of the trees will be part of forests
        int[] forestsPositions = generator.generateRandomForests(optimalParams.Item3 - noRandom, optimalParams.Item4);

        int noGoldMines = optimalParams.Item5 * Room.goldChunkSize;
        int noStoneMines = optimalParams.Item6 * Room.stoneChunkSize;
        int[] goldPositions = generator.generateRandomMines(noGoldMines);
        int[] stonePositions = generator.generateRandomMines(noStoneMines);

        Random random = new Random();

        // send trees positions in chunks of Room.treesPerPackage
        for (int i = 0; i < optimalParams.Item3; i += Room.treesPerPackage + 1) {
            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                for (int j = i; j <= (i + Room.treesPerPackage) && j < optimalParams.Item3; j++) {
                    // generate a tree type
                    int treeType = random.Next(1, 4);

                    writer.Write((byte)treeType);

                    if (j < noRandom) {
                        writer.Write(randomTreesPositions[j]);
                    } else {
                        writer.Write(forestsPositions[j]);
                    }
                }

                using (Message response = Message.Create(Tags.SEND_TREE_DATA, writer)) {
                    foreach(KeyValuePair<IClient, bool> player in this.players) {
                        player.Key.SendMessage(response, SendMode.Reliable);
                    }
                }
            }
        }

        // send gold positions in chunks
        for (int i = 0; i < noGoldMines; i += Room.goldChunkSize + 1) {
            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                for (int j = i; j <= (i + Room.goldChunkSize) && j < optimalParams.Item5; j++) {
                    writer.Write(goldPositions[j]);
                }

                using (Message response = Message.Create(Tags.SEND_GOLD_DATA, writer)) {
                    foreach (KeyValuePair<IClient, bool> player in this.players) {
                        player.Key.SendMessage(response, SendMode.Reliable);
                    }
                }
            }
        }

        // send stone positions in chunks
        for (int i = 0; i < noGoldMines; i += Room.stoneChunkSize + 1) {
            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                for (int j = i; j <= (i + Room.stoneChunkSize) && j < optimalParams.Item6; j++) {
                    writer.Write(stonePositions[j]);
                }

                using (Message response = Message.Create(Tags.SEND_STONE_DATA, writer)) {
                    foreach (KeyValuePair<IClient, bool> player in this.players) {
                        player.Key.SendMessage(response, SendMode.Reliable);
                    }
                }
            }
        }
    }

    private List<IClient> getOtherPlayers(IClient except) {
        List<IClient> others = new List<IClient>();

        foreach(KeyValuePair<IClient, bool> player in this.players) {
            if (except != player.Key) {
                others.Add(player.Key);
            }
        }

        return others;
    }

    public IClient changeLeader() {
        bool first = true;
        IClient newLeader = null;

        foreach(KeyValuePair<IClient, bool> player in this.players) {
            if (first) {
                newLeader = player.Key;
                this.players[player.Key] = true;
                first = false;
            } else {
                this.players[player.Key] = false;
            }
        }

        return newLeader;
    }

    public void MessageReceived(object sender, MessageReceivedEventArgs e) {
        Logger.print("Message from new room");
        IClient client = e.Client;

        using (Message message = e.GetMessage() as Message) {
            switch (e.Tag) {
                case Tags.PLAYER_MOVE:
                    this.handlePlayerUnitMove(client, message);
                    break;
                case Tags.PLAYER_ATTACK:
                    this.handlePlayerUnitAttack(client, message);
                    break;
                case Tags.PLAYER_SPAWN_UNIT:
                    this.handlePlayerUnitSpawn(client, message);
                    break;
                case Tags.PLAYER_UNIT_DEATH:
                    this.handlePlayerUnitDeath(client, message);
                    break;
                case Tags.PLAYER_STOP_UNIT:
                    this.handlePlayerUnitStop(client, message);
                    break;
                case Tags.PLAYER_BUILD:
                    this.handlePlayerBuild(client, message);
                    break;
                case Tags.PLAYER_ROTATE:
                    this.handlePlayerUnitRotate(client, message);
                    break;
                case Tags.PLAYER_GATHER_RESOURCE:
                    this.handlePlayerGatherResource(client, message);
                    break;
                case Tags.PLAYER_TECHNOLOGY_UPGRADE:
                    this.handlePlayerTechnologyUpgrade(client, message);
                    break;
            }
        }
    }

    private void handlePlayerUnitMove(IClient client, Message message) {
        using (DarkRiftReader reader = message.GetReader()) {
            int newIndex = reader.ReadInt32();

            // TODO: validate
        }
    }

    private void handlePlayerUnitAttack(IClient client, Message message) {
        using (DarkRiftReader reader = message.GetReader()) {
            // TODO: validate
        }
    }

    private void handlePlayerUnitRotate(IClient client, Message message) {
        using (DarkRiftReader reader = message.GetReader()) {
            // TODO: validate
        }
    }

    private void handlePlayerUnitSpawn(IClient client, Message message) {
        using (DarkRiftReader reader = message.GetReader()) {
            // TODO: validate
        }
    }

    private void handlePlayerUnitDeath(IClient client, Message message) {
        using (DarkRiftReader reader = message.GetReader()) {
            // TODO: validate
        }
    }

    private void handlePlayerUnitStop(IClient client, Message message) {
        using (DarkRiftReader reader = message.GetReader()) {
            // TODO: validate
        }
    }

    private void handlePlayerBuild(IClient client, Message message) {
        using (DarkRiftReader reader = message.GetReader()) {
            // TODO: validate
        }
    }

    private void handlePlayerGatherResource(IClient client, Message message) {
        using (DarkRiftReader reader = message.GetReader()) {
            // TODO: validate
        }
    }

    private void handlePlayerTechnologyUpgrade(IClient client, Message message) {
        using (DarkRiftReader reader = message.GetReader()) {
            // TODO: validate
        }
    }
}
