using DarkRift;

class StopMessage : PlayerMessage {
    private ushort unitId;
    private short wholeX;
    private short fractionalX;
    private short wholeZ;
    private short fractionalZ;

    public StopMessage(short wholeX, short fractionalX, short wholeZ, short fractionalZ, ushort unitId) {
        this.unitId = unitId;
        this.wholeX = wholeX;
        this.fractionalX = fractionalX;
        this.wholeZ = wholeZ;
        this.fractionalZ = fractionalZ;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_STOP_UNIT);
        writer.Write(this.wholeX);
        writer.Write(this.fractionalX);
        writer.Write(this.wholeZ);
        writer.Write(this.fractionalZ);
        writer.Write(this.unitId);
    }
}

