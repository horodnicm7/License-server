using System;
using System.Collections.Generic;
using System.Text;
using DarkRift.Server;
using DarkRift;

public class RoomMaster : Plugin {
    public override bool ThreadSafe => false;
    public override Version Version => new Version(1, 0, 0);
    private Dictionary<IClient, Player> players;
    private Dictionary<string, Room> rooms;

    public RoomMaster(PluginLoadData pluginLoadData) : base(pluginLoadData) {
        this.players = new Dictionary<IClient, Player>();

        ClientManager.ClientConnected += ClientConnected;
        ClientManager.ClientDisconnected += ClientDisconnect;
    }

    private void ClientConnected(object sender, ClientConnectedEventArgs e) {
        Logger.print("New connection " + e.Client.ID);

        // create an empty instance for this client, as we'll ask for details later
        this.players.Add(e.Client, null);
        e.Client.MessageReceived += MessageReceived;
    }

    private void MessageReceived(object sender, MessageReceivedEventArgs e) {
        IClient client = e.Client;

        using (Message message = e.GetMessage() as Message) {
            using (DarkRiftReader reader = message.GetReader()) {
                switch (e.Tag) {
                    case Tags.RECEIVE_PLAYER_NAME:
                        string playerName = reader.ReadString();
                        this.players[client] = new Player(playerName);
                        break;
                    case Tags.CREATE_ROOM:
                        if (this.players[client] == null) {
                            Logger.print("Create room before getting player name, for client " + client.ID);
                        }

                        // get final info for creating a room
                        string roomName = reader.ReadString();
                        string uuid = Guid.NewGuid().ToString();

                        this.rooms.Add(uuid, new Room(uuid, roomName));
                        break;
                }
            }
        }
    }

    private void ClientDisconnect(object sender, ClientDisconnectedEventArgs e) {
        Console.WriteLine("Disconnect " + e.Client.ID);
    }
}
