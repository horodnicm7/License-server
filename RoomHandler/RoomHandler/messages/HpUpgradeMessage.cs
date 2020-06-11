
using DarkRift;

class HpUpgradeMessage : PlayerMessage {
    private byte victimPlayerId;
    private ushort victimId;
    private short newHp;

    public HpUpgradeMessage(byte playerId, ushort unitId, short hp) {
        this.victimPlayerId = playerId;
        this.victimId = unitId;
        this.newHp = hp;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_UNIT_HP_UPGRADE);
        writer.Write(this.victimId);
        writer.Write(this.newHp);
        writer.Write(this.victimPlayerId);
    }
}

