using DarkRift;

class StopMessage : PlayerMessage {
    /*private short wholeX;
    private short fractionalX;
    private short wholeZ;
    private short fractionalZ;
    private int gridValue;

    public StopMessage(short wholeX, short fractionalX, short wholeZ, short fractionalZ, int gridValue) {
        this.wholeX = wholeX;
        this.fractionalX = fractionalX;
        this.wholeZ = wholeZ;
        this.fractionalZ = fractionalZ;
        this.gridValue = gridValue;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_STOP_UNIT);
        writer.Write(this.gridValue);
        writer.Write(this.wholeX);
        writer.Write(this.fractionalX);
        writer.Write(this.wholeZ);
        writer.Write(this.fractionalZ);
    }*/

    private ushort unitId;
    private byte playerId;
    byte activity;

    public StopMessage(ushort unitId, byte playerId, byte activity) {
        this.unitId = unitId;
        this.playerId = playerId;
        this.activity = activity;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_STOP_UNIT);
        writer.Write(this.unitId);
        writer.Write(this.playerId);
        writer.Write(this.activity);
    }
}

