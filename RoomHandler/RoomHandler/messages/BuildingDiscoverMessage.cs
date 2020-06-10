using DarkRift;
using System;
using System.Collections.Generic;
using System.Text;

class BuildingDiscoverMessage : PlayerMessage {
    private int gridIndex;
    private int gridValue;

    public BuildingDiscoverMessage(int gridIndex, int gridValue) {
        this.gridIndex = gridIndex;
        this.gridValue = gridValue;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_BUILDING_DISCOVER);
        writer.Write(gridIndex);
        writer.Write(gridValue);
    }
}
