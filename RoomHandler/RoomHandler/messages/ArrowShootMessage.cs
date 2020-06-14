using DarkRift;

class ArrowShootMessage : PlayerMessage {
    ushort shooterUnitId;
    byte shooterPlayerId;
    ushort victimUnitId;
    byte victimPlayerId;
    short rotationWhole;
    short rotationFractional;

    public ArrowShootMessage(ushort shooterUnitId, byte shooterPlayerId, ushort victimUnitId,
        byte victimPlayerId, short rotationWhole, short rotationFractional) {
        this.shooterUnitId = shooterUnitId;
        this.shooterPlayerId = shooterPlayerId;
        this.victimUnitId = victimUnitId;
        this.victimPlayerId = victimPlayerId;
        this.rotationWhole = rotationWhole;
        this.rotationFractional = rotationFractional;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_SEND_PROJECTILE);
        writer.Write(this.shooterUnitId);
        writer.Write(this.shooterPlayerId);
        writer.Write(this.victimUnitId);
        writer.Write(this.victimPlayerId);
        writer.Write(this.rotationWhole);
        writer.Write(this.rotationFractional);
    }
}
