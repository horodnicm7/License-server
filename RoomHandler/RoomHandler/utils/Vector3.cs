using System;
using System.Collections.Generic;

public class Vector3 {
    public float x, y, z;
    public Vector3(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3(Tuple<float, float, float> position) {
        this.x = position.Item1;
        this.y = position.Item2;
        this.z = position.Item3;
    }

    public static float distance(Vector3 a, Vector3 b) {
        return (float)Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2) + Math.Pow(a.z - b.z, 2));
    }
}
