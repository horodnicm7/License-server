
using DarkRift;

class DeathMessage : PlayerMessage {
    private ushort unitId;
    private byte playerId;

    public DeathMessage(ushort unitId, byte playerId) {
        this.unitId = unitId;
        this.playerId = playerId;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_UNIT_DEATH);
        writer.Write(this.unitId);
        writer.Write(this.playerId);
    }
}

