using DarkRift;

class WaypointMessage : PlayerMessage {
    private ushort unitId;
    private byte playerId;
    private short wholeX;
    private short fractionalX;
    private short wholeZ;
    private short fractionalZ;

    public WaypointMessage(ushort unitId, byte playerId, short wholeX, short fractionalX,
        short wholeZ, short fractionalZ) {
        this.unitId = unitId;
        this.playerId = playerId;
        this.wholeX = wholeX;
        this.fractionalX = fractionalX;
        this.wholeZ = wholeZ;
        this.fractionalZ = fractionalZ;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_SEND_WAYPOINT);
        writer.Write(this.unitId);
        writer.Write(this.playerId);
        writer.Write(this.wholeX);
        writer.Write(this.fractionalX);
        writer.Write(this.wholeZ);
        writer.Write(this.fractionalZ);
    }
}
