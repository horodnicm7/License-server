using DarkRift;

class ResourceExhaustionMessage : PlayerMessage {
    private ushort unitId;

    public ResourceExhaustionMessage(ushort unitId) {
        this.unitId = unitId;
    }


    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.RESOURCE_EXHAUST);
        writer.Write(this.unitId);
    }
}
