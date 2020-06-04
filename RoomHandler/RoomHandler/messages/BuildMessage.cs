using DarkRift;

class BuildMessage : PlayerMessage {
    private int gridIndex;
    private int gridValue;

    public BuildMessage(int gridIndex, int gridValue) {
        this.gridIndex = gridIndex;
        this.gridValue = gridValue;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_BUILD);
        writer.Write(this.gridIndex);
        writer.Write(this.gridValue);
    }
}

