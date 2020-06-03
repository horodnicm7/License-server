using DarkRift;

class RotateMessage : PlayerMessage {
    private int gridValue;
    private short wholeY;
    private short fractionalY;
    private byte activity;

    public RotateMessage(int gridValue, short wholeY, short fractionalY, byte activity) {
        this.gridValue = gridValue;
        this.wholeY = wholeY;
        this.fractionalY = fractionalY;
        this.activity = activity;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_ROTATE);
        writer.Write(this.gridValue);
        writer.Write(this.wholeY);
        writer.Write(this.fractionalY);
        writer.Write(this.activity);
    }
}
