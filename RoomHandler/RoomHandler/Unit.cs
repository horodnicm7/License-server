using System;
using System.Collections.Generic;
using System.Text;

public class Unit {
    public Vector3 position;
    public short rotationWhole;
    public short rotationFractional;
    public int gridIndex;

    private byte type;

    public Unit(Vector3 position, short rotationWhole, short rotationFractional, byte type) {
        this.position = position;
        this.rotationWhole = rotationWhole;
        this.rotationFractional = rotationFractional;
        this.type = type;
    }
}
