using DarkRift;

class SpawnMesage : PlayerMessage {
    private int gridIndex;
    private int gridValue;
    private short wholeRotationY;
    private short fractionalRotationY;

    public SpawnMesage(int gridIndex, int gridValue, short wholeRotationY, short fractionalRotationY) {
        this.gridIndex = gridIndex;
        this.gridValue = gridValue;
        this.wholeRotationY = wholeRotationY;
        this.fractionalRotationY = fractionalRotationY;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_SPAWN_UNIT);
        writer.Write(this.gridIndex);
        writer.Write(this.gridValue);
        writer.Write(this.wholeRotationY);
        writer.Write(this.fractionalRotationY);
    }
}
