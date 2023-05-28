using UnityEngine;

public class CoordArrays {
    public Vector3[] Positions;
    public Color[] Colors;

    public long Length {
        get {
            return Positions.Length;
        }
    }

    public CoordArrays(long len) {
        Positions = new Vector3[len];
        Colors = new Color[len];
    }
}
