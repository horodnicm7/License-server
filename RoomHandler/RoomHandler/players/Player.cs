using System;
using System.Collections.Generic;
using System.Text;

public class Player {
    public Dictionary<ushort, Unit> army;
    public Dictionary<ushort, Unit> buildings;
    public Dictionary<byte, HashSet<ushort>> seenBuildings;
    public PlayerStats playerStats;
    public string name;
    public string roomUUID;
    public byte civilization;
    public HashSet<byte> technologyUpgrades;

    // metadata
    private string ipAddress;
    private short ping;

    public Player(string name) {
        this.name = name;

        this.army = new Dictionary<ushort, Unit>();
        this.buildings = new Dictionary<ushort, Unit>();
        this.playerStats = new PlayerStats();
        this.technologyUpgrades = new HashSet<byte>();
        this.seenBuildings = new Dictionary<byte, HashSet<ushort>>();
    }
}
