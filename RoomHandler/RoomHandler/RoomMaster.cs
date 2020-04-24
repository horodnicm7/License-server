using System;
using System.Collections.Generic;
using System.Text;
using DarkRift.Server;
using DarkRift;
using DarkRift.Server.Plugins.LogWriters;

public class RoomMaster : Plugin {
    public override bool ThreadSafe => false;
    public override Version Version => new Version(1, 0, 0);
    public static Dictionary<IClient, Player> players;
    private Dictionary<string, Room> rooms;

    private Dictionary<IClient, List<Message>> messageQueue;

    public static int PACKET_SIZE_LIMIT = 32000;

    public RoomMaster(PluginLoadData pluginLoadData) : base(pluginLoadData) {
        RoomMaster.players = new Dictionary<IClient, Player>();

        ClientManager.ClientConnected += ClientConnected;
        ClientManager.ClientDisconnected += ClientDisconnect;
        
        this.rooms = new Dictionary<string, Room>();

        //this.testStuff();
    }

    private void testStuff() {
        this.rooms.Add("123-456-789", new Room("123-456-789", "A", 2));
        this.rooms.Add("123-456-78", new Room("123-456-78", "B", 4));
        this.rooms.Add("123-456-7", new Room("123-456-7", "C", 6));
        this.rooms.Add("123-456-", new Room("123-456-", "D", 6));
        this.rooms.Add("123-456", new Room("123-456", "E", 4));
        this.rooms.Add("123-45", new Room("123-45", "F", 2));
        this.rooms.Add("123-4", new Room("123-4", "G", 4));
        this.rooms.Add("123-", new Room("123-", "H", 6));
        this.rooms.Add("123", new Room("123", "I", 6));
        this.rooms.Add("12", new Room("12", "J", 6));
        this.rooms.Add("1", new Room("1", "A", 6));

        Player p1 = new Player("P1");
        Player p2 = new Player("P2");
        Player p3 = new Player("P3");
        Player p4 = new Player("P4");
        Player p5 = new Player("P5");
        Player p6 = new Player("P6");
        Player p7 = new Player("P7");
        Player p8 = new Player("P8");


    }

    private void ClientConnected(object sender, ClientConnectedEventArgs e) {
        Logger.print("New connection " + e.Client.ID);

        // create an empty instance for this client, as we'll ask for details later
        RoomMaster.players.Add(e.Client, null);

        e.Client.MessageReceived += this.MessageReceived;
    }

    private void MessageReceived(object sender, MessageReceivedEventArgs e) {
        IClient client = e.Client;

        Logger.print("Message received from: " + client.ID);

        using (Message message = e.GetMessage() as Message) {
            Logger.print("Message tag: " + message.Tag + "   e tag: " + e.Tag);
            using (DarkRiftReader reader = message.GetReader()) {
                switch (e.Tag) {
                    case Tags.RECEIVE_PLAYER_NAME:
                        string playerName = reader.ReadString();
                        RoomMaster.players[client] = new Player(playerName);
                        Logger.print("Joining: " + client.ID + " " + playerName);
                        break;
                    case Tags.CREATE_ROOM:
                        if (RoomMaster.players[client] == null) {
                            Logger.print("Create room before getting player name, for client " + client.ID);
                        }

                        // get final info for creating a room
                        string[] roomInfo = reader.ReadString().Split('>');
                        string roomName = roomInfo[1];
                        byte numberOfPlayers = Byte.Parse(roomInfo[0]);
                        
                        string uuid = Guid.NewGuid().ToString();

                        Logger.print("UUID: " + uuid);
                        Logger.print("Room name: " + roomName);
                        Logger.print("No players: " + numberOfPlayers);

                        // create a new room, register it and set the current client as owner
                        Room newRoom = new Room(uuid, roomName, numberOfPlayers);
                        newRoom.players.Add(client, true);
                        RoomMaster.players[client].roomUUID = uuid;

                        this.rooms.Add(uuid, newRoom);

                        // send the UUID to client
                        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                            writer.Write(uuid);
                            message.Serialize(writer);
                        }
                        message.Tag = Tags.SEND_ROOM_IDENTIFIER;
                        client.SendMessage(message, SendMode.Reliable);

                        // # = this player is room's leader
                        string playersList = "";//"Dan\n#Lucifer\nIonut\nAlexandru\nTerminator\nWTF\nAAAALO\nBarabula\n";
                        foreach (KeyValuePair<IClient, bool> player in newRoom.players) {
                            playersList += RoomMaster.players[player.Key].name + "\n";
                        }

                        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                            writer.Write(playersList);

                            using (Message response = Message.Create(Tags.CAN_JOIN_ROOM, writer))
                                client.SendMessage(response, SendMode.Reliable);
                        }

                        break;
                    case Tags.REQUEST_ROOMS_LIST:
                        this.sentRoomsListToClient(client);
                        break;
                    case Tags.JOIN_ROOM:
                        string roomId = reader.ReadString();
                        Room thisRoom = this.rooms[roomId];

                        Logger.print("Client trying " + client.ID + " to join: " + thisRoom.players.Count);
                        Logger.print("Client trying " + client.ID + " to join: " + this.rooms.ContainsKey(roomId));

                        if (thisRoom.players.Count == thisRoom.maxNumberOfPlayers || !this.rooms.ContainsKey(roomId)) {
                            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                                using (Message response = Message.Create(Tags.ROOM_NOT_AVAILABLE, writer))
                                    client.SendMessage(response, SendMode.Reliable);
                            }
                        } else {
                            // # = this player is room's leader
                            // Add this player to the room
                            thisRoom.players.Add(client, false);
                            string playersList1 = "";
                            foreach (KeyValuePair<IClient, bool> player in thisRoom.players) {
                                Logger.print("PLM");
                                Logger.print(RoomMaster.players[player.Key].ToString());
                                playersList1 = playersList1 + RoomMaster.players[player.Key].name + "\n";
                            }

                            Logger.print("List of players: " + playersList1);

                            foreach(KeyValuePair<IClient, bool> player in thisRoom.players) {
                                using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                                    writer.Write(playersList1);

                                    using (Message response = Message.Create(Tags.CAN_JOIN_ROOM, writer))
                                        player.Key.SendMessage(response, SendMode.Reliable);
                                }
                            }
                        }
                        break;
                    case Tags.START_GAME:
                        string roomUuid = reader.ReadString();

                        // TODO: check if player is room leader and the room is full

                        // set the message received handler to the room's one
                        client.MessageReceived -= this.MessageReceived;
                        client.MessageReceived += this.rooms[roomUuid].MessageReceived;

                        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                            using (Message response = Message.Create(Tags.CAN_START_GAME, writer)) {
                                foreach (KeyValuePair<IClient, bool> player in this.rooms[roomUuid].players) {
                                    player.Key.SendMessage(response, SendMode.Reliable);
                                }
                            }
                        }
                        this.rooms[roomUuid].sendWorldToPlayers();
                        break;
                    case Tags.LEAVE_ROOM:
                        string roomToLeaveId = reader.ReadString();
                        Room room = this.rooms[roomToLeaveId];
                        
                        if (room.players.Count == 1) {
                            this.rooms.Remove(roomToLeaveId);
                        }
                        break;
                }
            }
        }
    }

    private void sentRoomsListToClient(IClient client) {
        string roomsList = "";
        foreach (KeyValuePair<string, Room> roomEntry in this.rooms) {
            string newEntry = roomEntry.Key + ">" + roomEntry.Value.name + ">" +
                roomEntry.Value.players.Count + ">" + roomEntry.Value.maxNumberOfPlayers + "\n";

            if (roomsList.Length + newEntry.Length > RoomMaster.PACKET_SIZE_LIMIT) {
                break;
            }

            roomsList = roomsList + newEntry;
        }

        Logger.print("Rooms list: \n" + roomsList);

        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            writer.Write(roomsList);

            using (Message message = Message.Create(Tags.SEND_ROOMS_LIST, writer)) {
                client.SendMessage(message, SendMode.Reliable);
            }
        }
    }

    private void ClientDisconnect(object sender, ClientDisconnectedEventArgs e) {
        Console.WriteLine("Disconnect " + e.Client.ID);
    }
}
