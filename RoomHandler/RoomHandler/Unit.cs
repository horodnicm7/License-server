using System;
using System.Collections.Generic;
using System.Text;

public class Unit {
    public Vector3 position;
    public float rotation;
    byte type;

    public Unit(Vector3 position, float rotation, byte type) {
        this.position = position;
        this.rotation = rotation;
        this.type = type;
    }
}
