using System;
using System.Collections.Generic;
using System.Text;

public class Player {
    public Dictionary<string, Unit> army;
    public Dictionary<string, Unit> buildings;
    public string name;
    public string roomUUID;
    public byte civilization;

    // metadata
    private string ipAddress;
    private short ping;

    public Player(string name) {
        this.name = name;

        this.army = new Dictionary<string, Unit>();
        this.buildings = new Dictionary<string, Unit>();
    }
}
