using System;
using UnityEngine;

public class Utils {
    private static System.Random GetRandom() => new System.Random((int)(DateTime.Now.ToFileTime()%100000000));

    public static int RandomInt(int min, int max) {
        return GetRandom().Next(min, max);
    }

    public static bool RandomBoolean() {
        return RandomInt(0, 2) == 1;
    }

    public static float RandomFloat(float min, float max) {
        return (float)(GetRandom().NextDouble() * (max - min) + min);
    }

    public static Color RandomColor() {
        return UnityEngine.Random.ColorHSV();
    }

    public static bool NearlyEqual(float a, float b) {
        return Mathf.Abs(a - b) < 0.0005f;//meter
    }

    public static bool NearlyEqual(Vector3 a, Vector3 b) {
        return NearlyEqual(a.x, b.x) && NearlyEqual(a.y, b.y);
    }
}
