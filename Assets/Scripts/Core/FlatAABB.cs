using UnityEngine;

public readonly struct FlatAABB
{
    public readonly Vector3 Min;
    public readonly Vector3 Max;

    public FlatAABB(Vector3 min, Vector3 max) {
        Min = min;
        Max = max;
    }

    public FlatAABB(float minX, float minY, float maxX, float maxY) {
        Min = new Vector3(minX, minY);
        Max = new Vector3(maxX, maxY);
    }
}
