using DarkRift;

class SpawnMessage : PlayerMessage {
    private ushort id;
    private byte player;
    private short wholeX;
    private short fractionalX;
    private short wholeZ;
    private short fractionalZ;
    private short rotation;
    private byte type;

    public SpawnMessage(ushort id, byte player, short wX, short fX, short wZ, short fZ, short rot, byte type) {
        this.id = id;
        this.player = player;
        this.wholeX = wX;
        this.fractionalX = fX;
        this.wholeZ = wZ;
        this.fractionalZ = fZ;
        this.rotation = rot;
        this.type = type;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_SPAWN_UNIT);
        writer.Write(this.id);
        writer.Write(this.player);
        writer.Write(this.type);
        writer.Write(this.wholeX);
        writer.Write(this.fractionalX);
        writer.Write(this.wholeZ);
        writer.Write(this.fractionalZ);
        writer.Write(this.rotation);
    }
}

