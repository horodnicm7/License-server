
using DarkRift;

class VillagerGatherMessage : PlayerMessage {
    private ushort unitId;
    private byte playerId;
    private byte resourceType;

    public VillagerGatherMessage(ushort unitId, byte playerId, byte resourceType) {
        this.unitId = unitId;
        this.playerId = playerId;
        this.resourceType = resourceType;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_VILLAGER_GATHER);
        writer.Write(this.unitId);
        writer.Write(this.playerId);
        writer.Write(this.resourceType);
    }
}
