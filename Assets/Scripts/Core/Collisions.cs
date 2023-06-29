using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Collisions {
    public static bool IntersectCircles(Vector3 centerA, float radiusA, Vector3 centerB, float radiusB,
        out Vector3 normal, out float depth) {

        normal = Vector3.zero;
        depth = 0f;

        var distance = (centerA - centerB).magnitude;
        var radii = radiusA + radiusB;

        if (distance >= radii) {
            return false;
        }

        normal = (centerB - centerA) / distance;
        depth = radii - distance;

        return true;
    }
}
