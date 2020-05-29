using DarkRift;

class SpawnMesage : PlayerMessage {
    private int gridIndex;
    private int gridValue;

    public SpawnMesage(int gridIndex, int gridValue) {
        this.gridIndex = gridIndex;
        this.gridValue = gridValue;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_SPAWN_UNIT);
        writer.Write(this.gridIndex);
        writer.Write(this.gridValue);
    }
}
