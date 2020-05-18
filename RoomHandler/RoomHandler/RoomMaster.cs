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
    }

    private void ClientConnected(object sender, ClientConnectedEventArgs e) {
        Logger.print("New connection " + e.Client.ID);

        // create an empty instance for this client, as we'll ask for details later
        RoomMaster.players.Add(e.Client, null);

        e.Client.MessageReceived += this.MessageReceived;
    }

    private void MessageReceived(object sender, MessageReceivedEventArgs e) {
        IClient client = e.Client;

        using (Message message = e.GetMessage() as Message) {
            using (DarkRiftReader reader = message.GetReader()) {
                switch (e.Tag) {
                    case Tags.RECEIVE_PLAYER_NAME:
                        string playerName = reader.ReadString();
                        RoomMaster.players[client] = new Player(playerName);
                        Logger.print("Joining: " + client.ID + " " + playerName);
                        break;
                    case Tags.CREATE_ROOM:
                        if (!RoomMaster.players.ContainsKey(client)) {
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
                        newRoom.players.Add(client, newRoom.maxPlayerId);
                        newRoom.leader = client;
                        RoomMaster.players[client].roomUUID = uuid;
                        newRoom.maxPlayerId *= 2;

                        this.rooms.Add(uuid, newRoom);

                        // send the UUID to client
                        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                            writer.Write(uuid);
                            message.Serialize(writer);
                        }
                        message.Tag = Tags.SEND_ROOM_IDENTIFIER;
                        client.SendMessage(message, SendMode.Reliable);

                        // # = this player is room's leader
                        string playersList = ""; //String.Format("Dan>1\n{0}>2\n#Ionut>4\nAlexandru>8\n", RoomMaster.players[client].name.Substring(1, RoomMaster.players[client].name.Length - 1));
                        foreach (KeyValuePair<IClient, byte> player in newRoom.players) {
                            playersList += RoomMaster.players[player.Key].name + ">" + player.Value + "\n";
                        }

                        //playersList += "Ionut>2\nAlexandru>4\nIoana>8\n";

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

                        Logger.print("Client trying " + client.ID + " to join: " + this.rooms.ContainsKey(roomId));

                        if (thisRoom.players.Count == thisRoom.maxNumberOfPlayers || !this.rooms.ContainsKey(roomId)) {
                            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                                using (Message response = Message.Create(Tags.ROOM_NOT_AVAILABLE, writer))
                                    client.SendMessage(response, SendMode.Reliable);
                            }
                        } else {
                            // # = this player is room's leader
                            // Add this player to the room
                            thisRoom.players.Add(client, thisRoom.maxPlayerId);
                            thisRoom.maxPlayerId *= 2;
                            string playersList1 = "";
                            foreach (KeyValuePair<IClient, byte> player in thisRoom.players) {
                                Logger.print(RoomMaster.players[player.Key].ToString());
                                playersList1 = playersList1 + RoomMaster.players[player.Key].name + ">" + player.Value + "\n";
                            }

                            foreach(KeyValuePair<IClient, byte> player in thisRoom.players) {
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
                        if (this.rooms[roomUuid].leader == client) {
                            // set the message received handler to the room's one
                            client.MessageReceived -= this.MessageReceived;
                            client.MessageReceived += this.rooms[roomUuid].MessageReceived;

                            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                                using (Message response = Message.Create(Tags.CAN_START_GAME, writer)) {
                                    foreach (KeyValuePair<IClient, byte> player in this.rooms[roomUuid].players) {
                                        player.Key.SendMessage(response, SendMode.Reliable);
                                    }
                                }
                            }
                            this.rooms[roomUuid].sendWorldToPlayers();
                        }
                        break;
                    case Tags.LEAVE_ROOM:
                        string roomToLeaveId = reader.ReadString();
                        Room room = this.rooms[roomToLeaveId];
                        
                        if (room.players.Count == 1) {
                            this.rooms.Remove(roomToLeaveId);
                        }
                        break;
                    case Tags.KICK_PLAYER_FROM_LOBBY:

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
        IClient client = e.Client;

        Player player = RoomMaster.players[client];
        Room room = this.rooms[player.roomUUID];

        // if this is the last player, then remove every reference to its rooms and data
        if (room.players.Count == 1) {
            room.ClearMemory();
            this.rooms.Remove(player.roomUUID);
            RoomMaster.players.Remove(client);
        }
    }
}
