using System;
using UnityEngine;

public static class Utils {
    private static System.Random Rand => new System.Random();

    public static int RandomInt(int min, int max) {
        return Rand.Next(min, max);
    }

    public static bool RandomBoolean() {
        return RandomInt(0, 2) == 1;
    }

    public static float RandomFloat(float min, float max) {
        return (float)(Rand.NextDouble() * (max - min) + min);
    }

    public static Color RandomColor() {
        return UnityEngine.Random.ColorHSV();
    }

    private const float SmallDistance = 0.0005f;
    public static bool NearlyEqual(float a, float b) {
        return Mathf.Abs(a - b) < SmallDistance;//meter
    }

    public static bool NearlyEqual(Vector3 a, Vector3 b) {
        return (a - b).sqrMagnitude < SmallDistance * SmallDistance;
    }

    public static Vector3 PerpXY(this Vector3 v) {
        return new Vector3(-v.y, v.x);
    }

    public static float Cross(Vector3 a, Vector3 b) {
        // cz = ax * by − ay * bx
        return a.x * b.y - a.y * b.x;
    }
}
