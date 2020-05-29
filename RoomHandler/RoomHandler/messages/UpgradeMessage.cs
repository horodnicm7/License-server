using DarkRift;

public class UpgradeMessage : PlayerMessage {
    private byte playerId;
    private byte upgradeId;

    public UpgradeMessage(byte playerId, byte upgradeId) {
        this.playerId = playerId;
        this.upgradeId = upgradeId;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_TECHNOLOGY_UPGRADE);
        writer.Write(this.playerId);
        writer.Write(this.upgradeId);
    }
}
