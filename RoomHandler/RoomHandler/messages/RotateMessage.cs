using DarkRift;

class RotateMessage : PlayerMessage {
    private int gridValue;
    private short wholeY;
    private short fractionalY;

    public RotateMessage(int gridValue, short wholeY, short fractionalY) {
        this.gridValue = gridValue;
        this.wholeY = wholeY;
        this.fractionalY = fractionalY;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_ROTATE);
        writer.Write(gridValue);
        writer.Write(wholeY);
        writer.Write(fractionalY);
    }
}
