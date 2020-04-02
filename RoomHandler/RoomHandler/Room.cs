using System;
using System.Collections.Generic;
using System.Text;

namespace RoomHandler {
    class Room {
        public Dictionary<string, Player> players;
        string uuid;
        string name;

        public Room(string uuid, string name) {
            this.uuid = uuid;
            this.name = name;
        }
    }
}
