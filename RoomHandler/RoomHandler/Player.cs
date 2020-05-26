using System;
using System.Collections.Generic;
using System.Text;

public class Player {
    public Dictionary<ushort, Unit> army;
    public Dictionary<ushort, Unit> buildings;
    public string name;
    public string roomUUID;
    public byte civilization;

    // metadata
    private string ipAddress;
    private short ping;

    public Player(string name) {
        this.name = name;

        this.army = new Dictionary<ushort, Unit>();
        this.buildings = new Dictionary<ushort, Unit>();
    }
}
