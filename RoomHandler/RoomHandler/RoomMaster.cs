using System;
using System.Collections.Generic;
using System.Text;
using DarkRift.Server;
using DarkRift;
using DarkRift.Server.Plugins.LogWriters;
using System.Timers;
using System.IO;
using YamlDotNet.RepresentationModel;

public class RoomMaster : Plugin {
    public override bool ThreadSafe => false;
    public override Version Version => new Version(1, 0, 0);
    public static Dictionary<IClient, Player> players;
    private Dictionary<string, Room> rooms;

    private Dictionary<IClient, List<Message>> messageQueue;

    public static int PACKET_SIZE_LIMIT = 32000;

    private System.Timers.Timer packetSendTimer;
    private System.Timers.Timer damagePacketsTimer;

    public RoomMaster(PluginLoadData pluginLoadData) : base(pluginLoadData) {
        RoomMaster.players = new Dictionary<IClient, Player>();

        ClientManager.ClientConnected += ClientConnected;
        ClientManager.ClientDisconnected += ClientDisconnect;
        
        this.rooms = new Dictionary<string, Room>();

        this.startRoomsTimers();
    }

    ~RoomMaster() {
        this.packetSendTimer.Stop();
        this.packetSendTimer.Dispose();
    }

    private void startRoomsTimers() {
        this.packetSendTimer = new System.Timers.Timer(100);
        this.packetSendTimer.Elapsed += onRoomsTimerEvent;
        this.packetSendTimer.AutoReset = true;
        this.packetSendTimer.Enabled = true;

        this.damagePacketsTimer = new System.Timers.Timer(1000);
        this.damagePacketsTimer.Elapsed += onDamagePacketsTimerEvent;
        this.damagePacketsTimer.AutoReset = true;
        this.damagePacketsTimer.Enabled = true;
    }

    private void onRoomsTimerEvent(Object source, ElapsedEventArgs e) {
        foreach(KeyValuePair<string, Room> room in this.rooms) {
            room.Value.sendDataToPlayersCallback();
        }
    }

    private void onDamagePacketsTimerEvent(Object source, ElapsedEventArgs e) {
        foreach (KeyValuePair<string, Room> room in this.rooms) {
            room.Value.sendInflictedDamagePackets();
        }
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

                        // create a new room, register it and set the current client as owner
                        Room newRoom = new Room(uuid, roomName, numberOfPlayers);
                        newRoom.playersIClientMapping.Add(client, newRoom.maxPlayerId);
                        newRoom.playersByteMapping.Add(newRoom.maxPlayerId, client);
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
                        foreach (KeyValuePair<IClient, byte> player in newRoom.playersIClientMapping) {
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

                        if (thisRoom.playersIClientMapping.Count == thisRoom.maxNumberOfPlayers || !this.rooms.ContainsKey(roomId)) {
                            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                                using (Message response = Message.Create(Tags.ROOM_NOT_AVAILABLE, writer))
                                    client.SendMessage(response, SendMode.Reliable);
                            }
                        } else {
                            // # = this player is room's leader
                            // Add this player to the room
                            thisRoom.playersIClientMapping.Add(client, thisRoom.maxPlayerId);
                            thisRoom.playersByteMapping.Add(thisRoom.maxPlayerId, client);
                            thisRoom.maxPlayerId *= 2;
                            string playersList1 = "";
                            foreach (KeyValuePair<IClient, byte> player in thisRoom.playersIClientMapping) {
                                Logger.print(RoomMaster.players[player.Key].ToString());
                                playersList1 = playersList1 + RoomMaster.players[player.Key].name + ">" + player.Value + "\n";
                            }

                            foreach(KeyValuePair<IClient, byte> player in thisRoom.playersIClientMapping) {
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
                        byte civilizationId = reader.ReadByte();
                        Room currentRoom = this.rooms[roomUuid];

                        // TODO: check if player is room leader and the room is full
                        if (currentRoom.leader == client) {
                            // check if all players are ready to start
                            foreach(KeyValuePair<IClient, byte> playerMap in currentRoom.playersIClientMapping) {
                                Player currentPlayer = RoomMaster.players[playerMap.Key];
                                if (currentPlayer.ready == false && playerMap.Key != client) {
                                    return;
                                }
                            }

                            Player roomLeader = RoomMaster.players[client];
                            roomLeader.ready = true;
                            roomLeader.civilization = civilizationId;

                            // set the message received handler to the room's one
                            foreach (KeyValuePair<IClient, byte> roomPlayer in currentRoom.playersIClientMapping) {
                                roomPlayer.Key.MessageReceived -= this.MessageReceived;
                                roomPlayer.Key.MessageReceived += this.rooms[roomUuid].MessageReceived;
                            }

                            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                                using (Message response = Message.Create(Tags.CAN_START_GAME, writer)) {
                                    foreach (KeyValuePair<IClient, byte> player in this.rooms[roomUuid].playersIClientMapping) {
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
                        
                        if (room.playersIClientMapping.Count == 1) {
                            this.rooms.Remove(roomToLeaveId);
                        }
                        break;
                    case Tags.KICK_PLAYER_FROM_LOBBY:

                        break;
                    case Tags.READY_TO_START:
                        byte civilization = reader.ReadByte();

                        Player preparedPlayer = RoomMaster.players[client];
                        preparedPlayer.civilization = civilization;
                        preparedPlayer.ready = true;
                        break;
                }
            }
        }
    }

    private void sentRoomsListToClient(IClient client) {
        string roomsList = "";
        foreach (KeyValuePair<string, Room> roomEntry in this.rooms) {
            string newEntry = roomEntry.Key + ">" + roomEntry.Value.name + ">" +
                roomEntry.Value.playersIClientMapping.Count + ">" + roomEntry.Value.maxNumberOfPlayers + "\n";

            if (roomsList.Length + newEntry.Length > RoomMaster.PACKET_SIZE_LIMIT) {
                break;
            }

            roomsList = roomsList + newEntry;
        }

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
        if (room.playersIClientMapping.Count == 1) {
            room.ClearMemory();
            this.rooms.Remove(player.roomUUID);
            RoomMaster.players.Remove(client);
        }
    }
}
