
using DarkRift;

class AttackMessage : PlayerMessage {
    private ushort unitId;
    private byte playerId;

    public AttackMessage(ushort unitId, byte playerId) {
        this.unitId = unitId;
        this.playerId = playerId;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_ATTACK);
        writer.Write(this.unitId);
        writer.Write(this.playerId);
    }
}
