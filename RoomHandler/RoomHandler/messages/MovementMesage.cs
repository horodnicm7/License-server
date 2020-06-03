﻿using DarkRift;

class MovementMessage : PlayerMessage {
    private short wholeX;
    private short fractionalX;
    private short wholeZ;
    private short fractionalZ;

    private int gridValue;
    private short wholeRotationY;
    private short fractionalRotationY;
    private byte activity;

    public MovementMessage(short wholeX, short fractionalX, short wholeZ, short fractionalZ,
        int gridValue, short wholeRotationY, short fractionalRotationY, byte activity) {
        this.wholeX = wholeX;
        this.fractionalX = fractionalX;
        this.wholeZ = wholeZ;
        this.fractionalZ = fractionalZ;
        this.gridValue = gridValue;
        this.wholeRotationY = wholeRotationY;
        this.fractionalRotationY = fractionalRotationY;
        this.activity = activity;
    }

    public override void serialize(ref DarkRiftWriter writer) {
        writer.Write(Tags.PLAYER_MOVE);
        writer.Write(this.wholeX);
        writer.Write(this.fractionalX);
        writer.Write(this.wholeZ);
        writer.Write(this.fractionalZ);
        writer.Write(this.gridValue);
        writer.Write(this.wholeRotationY);
        writer.Write(this.fractionalRotationY);
        writer.Write(this.activity);
    }
}
