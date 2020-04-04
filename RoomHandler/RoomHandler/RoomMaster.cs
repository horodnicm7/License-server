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

        Logger.print("Message received from: " + client.ID);

        using (Message message = e.GetMessage() as Message) {
            Logger.print("Message tag: " + message.Tag + "   e tag: " + e.Tag);
            using (DarkRiftReader reader = message.GetReader()) {
                switch (e.Tag) {
                    case Tags.RECEIVE_PLAYER_NAME:
                        string playerName = reader.ReadString();
                        RoomMaster.players[client] = new Player(playerName);
                        break;
                    case Tags.CREATE_ROOM:
                        if (RoomMaster.players[client] == null) {
                            Logger.print("Create room before getting player name, for client " + client.ID);
                        }

                        // get final info for creating a room
                        string roomName = reader.ReadString();
                        string uuid = Guid.NewGuid().ToString();

                        Logger.print("UUID: " + uuid);
                        Logger.print("Room name: " + roomName);

                        // create a new room, register it and set the current client as owner
                        Room newRoom = new Room(uuid, roomName);
                        newRoom.players.Add(client, true);
                        RoomMaster.players[client].roomUUID = uuid;

                        this.rooms.Add(uuid, newRoom);

                        // set the message received handler to the room's one
                        client.MessageReceived -= this.MessageReceived;
                        client.MessageReceived += newRoom.MessageReceived;

                        // send the UUID to client
                        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                            writer.Write(uuid);
                            message.Serialize(writer);
                        }
                        message.Tag = Tags.SEND_ROOM_IDENTIFIER;

                        client.SendMessage(message, SendMode.Reliable);
                        break;
                }
            }
        }
    }

    private void ClientDisconnect(object sender, ClientDisconnectedEventArgs e) {
        Console.WriteLine("Disconnect " + e.Client.ID);
    }
}
