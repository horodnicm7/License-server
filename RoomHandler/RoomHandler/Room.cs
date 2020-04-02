using System;
using System.Collections.Generic;
using System.Text;
using DarkRift;
using DarkRift.Server;

public class Room {
    // it will work with the instances from RoomMaster and this dictionary will only keep 
    // clients as keys and bool values meaning that a client is room's owner or not
    public Dictionary<IClient, bool> players;
    string uuid;
    string name;

    public Room(string uuid, string name) {
        this.uuid = uuid;
        this.name = name;
    }

    public void MessageReceived(object sender, MessageReceivedEventArgs e) {

    }
}
