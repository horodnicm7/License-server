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
                result = new Tuple<ushort, float, int, int, int, int>(128, 4f, 1024, random.Next(7, 15), random.Next(15, 20), random.Next(15, 20));
                break;
            case 4:
                result = new Tuple<ushort, float, int, int, int, int>(256, 4f, 2048, random.Next(7, 15), random.Next(15, 20), random.Next(15, 20));
                break;
            case 6:
                result = new Tuple<ushort, float, int, int, int, int>(512, 4f, 4096, random.Next(7, 15), random.Next(15, 20), random.Next(15, 20));
                break;
        }

        return result;
    }

    public void sendWorldToPlayers() {
        Tuple<ushort, float, int, int, int, int> optimalParams = this.getOptimalWorldParams();
        this.map = new WorldMap(optimalParams.Item1, optimalParams.Item2);

        TerrainGenerator generator = new TerrainGenerator(this.map);
        // 10% of the trees will be randomly positioned
        int noRandom = (int)(optimalParams.Item3 * 0.1f);
        int[] randomTreesPositions = generator.generateRandomPositionedTrees(noRandom);

        // 90% of the trees will be part of forests
        int[] forestsPositions = generator.generateRandomForests(optimalParams.Item3 - noRandom, optimalParams.Item4);

        int[] goldPositions = generator.generateRandomMines(optimalParams.Item5);
        int[] stonePositions = generator.generateRandomMines(optimalParams.Item6);
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
