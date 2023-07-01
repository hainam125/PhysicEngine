using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal struct FlatTransform
{
    public readonly float posX, posY;
    public readonly float sin, cos;

    public readonly static FlatTransform Zero = new FlatTransform(0f, 0f, 0f);

    public FlatTransform(Vector3 position, float angle) {
        this.posX = position.x;
        this.posY = position.y;
        this.sin = Mathf.Sin(angle);
        this.cos = Mathf.Cos(angle);
    }

    public FlatTransform(float posX, float posY, float angle) {
        this.posX = posX;
        this.posY = posY;
        this.sin = Mathf.Sin(angle);
        this.cos = Mathf.Cos(angle); 
    }

    internal Vector3 Transform(Vector3 v) {
        float rx = cos * v.x - sin * v.y;
        float ry = sin * v.x + cos * v.y;

        float tx = rx + posX;
        float ty = ry + posY;
        return new Vector3(tx, ty);
    }
}
