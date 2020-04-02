using System;
using System.Collections.Generic;
using System.Text;
using DarkRift.Server;

public class RoomMaster : Plugin {
    public override bool ThreadSafe => false;

    public override Version Version => new Version(1, 0, 0);

    public RoomMaster(PluginLoadData pluginLoadData) : base(pluginLoadData) {
        ClientManager.ClientConnected += ClientConnected;
        ClientManager.ClientDisconnected += ClientDisconnect;
    }

    private void ClientConnected(object sender, ClientConnectedEventArgs e) {

    }

    private void ClientDisconnect(object sender, ClientDisconnectedEventArgs e) {

    }
}
