using DarkRift;

class MoveMessage : PlayerMessage {
    private int gridValue;
    private short wholeX;
    private short fractionalX;
    private short wholeZ;
    private short fractionalZ;

    public MoveMessage(int gridValue, short wholeX, short fractionalX, short wholeZ, short fractionalZ) {
        this.gridValue = gridValue;
        this.wholeX = wholeX;
        this.fractionalX = fractionalX;
        this.wholeZ = wholeZ;
        this.fractionalZ = fractionalZ;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_MOVE);
        writer.Write(this.gridValue);
        writer.Write(this.wholeX);
        writer.Write(this.fractionalX);
        writer.Write(this.wholeZ);
        writer.Write(this.fractionalZ);
    }
}
