using System;
using System.Collections.Generic;
using System.Text;
using DarkRift.Server;

namespace RoomHandler {
    class RoomMaster : Plugin {
        public override bool ThreadSafe => false;

        public override Version Version => new Version(1, 0, 0);

        public RoomMaster(PluginLoadData pluginLoadData) : base(pluginLoadData) {

        }
    }
}
