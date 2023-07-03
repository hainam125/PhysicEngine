using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Collisions {

    public static bool IntersectCirclePolygon(Vector3 circleCenter, float circleRadius, Vector3[] vertices,
        out Vector3 normal, out float depth) {

        var polygonCenter = FindArithmeticMean(vertices);
        return IntersectCirclePolygon(circleCenter, circleRadius, polygonCenter, vertices, out normal, out depth);
    }

    public static bool IntersectCirclePolygon(Vector3 circleCenter, float circleRadius,
         Vector3 polygonCenter, Vector3[] vertices,
        out Vector3 normal, out float depth) {

        normal = Vector3.zero;
        depth = float.MaxValue;

        var axis = Vector3.zero;

        bool check(Vector3 lAxis, ref Vector3 lNormal, ref float lDepth) {
            ProjectVertices(vertices, lAxis, out var minA, out var maxA);
            ProjectCircle(circleCenter, circleRadius, lAxis, out var minB, out var maxB);

            if (minA >= maxB || minB >= maxA) return false;

            var axisDepth = Mathf.Min(maxB - minA, maxA - minB);
            if (axisDepth < lDepth) {
                lDepth = axisDepth;
                lNormal = lAxis;
            }
            return true;
        }

        for (var i = 0; i < vertices.Length; i++) {
            var va = vertices[i];
            var vb = vertices[(i + 1) % vertices.Length];

            var edge = vb - va;
            axis = new Vector3(-edge.y, edge.x).normalized;

            if (!check(axis, ref normal, ref depth)) return false;
        }

        var cpIndex = FindClosestPointOnPolygon(circleCenter, vertices);
        var cp = vertices[cpIndex];
        axis = (cp - circleCenter).normalized;
        if (!check(axis, ref normal, ref depth)) return false;

        var direction = polygonCenter - circleCenter;

        if (Vector3.Dot(direction, normal) < 0) normal = -normal;

        return true;
    }

    private static int FindClosestPointOnPolygon(Vector3 circleCenter, Vector3[] vertices) {
        var result = -1;
        var minDistSqr = float.MaxValue;
        for (var i = 0; i < vertices.Length; i++) {
            var distSqr = Vector3.SqrMagnitude(vertices[i] - circleCenter);
            if (distSqr < minDistSqr) {
                minDistSqr = distSqr;
                result = i;
            }
        }
        return result;
    }

    private static void ProjectCircle(Vector3 center, float radius, Vector3 axis, out float min, out float max) {
        var directionAndRadius = axis * radius;
        var p1 = center + directionAndRadius;
        var p2 = center - directionAndRadius;

        min = Vector3.Dot(p1, axis);
        max = Vector3.Dot(p2, axis);

        if (min > max) {
            var t = min;
            min = max;
            max = t;
        }
    }

    public static bool IntersectPolygons(Vector3[] verticesA, Vector3[] verticesB,
        out Vector3 normal, out float depth) {

        var centerA = FindArithmeticMean(verticesA);
        var centerB = FindArithmeticMean(verticesB);
        return IntersectPolygons(centerA, verticesA, centerB, verticesB, out normal, out depth);
    }

    public static bool IntersectPolygons(Vector3 centerA, Vector3[] verticesA, Vector3 centerB, Vector3[] verticesB,
        out Vector3 normal, out float depth) {

        normal = Vector3.zero;
        depth = float.MaxValue;

        bool check(Vector3[] vertices, ref Vector3 lNormal, ref float lDepth) {
            for (var i = 0; i < vertices.Length; i++) {
                var va = vertices[i];
                var vb = vertices[(i + 1) % vertices.Length];

                var edge = vb - va;
                var axis = new Vector3(-edge.y, edge.x).normalized;

                ProjectVertices(verticesA, axis, out var minA, out var maxA);
                ProjectVertices(verticesB, axis, out var minB, out var maxB);

                if (minA >= maxB || minB >= maxA) return false;

                var axisDepth = Mathf.Min(maxB - minA, maxA - minB);
                if (axisDepth < lDepth) {
                    lDepth = axisDepth;
                    lNormal = axis;
                }
            }
            return true;
        }

        if (!check(verticesA, ref normal, ref depth)) return false;
        if (!check(verticesB, ref normal, ref depth)) return false;

        var direction = centerB - centerA;

        if (Vector3.Dot(direction, normal) < 0) normal = -normal;

        return true;
    }

    private static Vector3 FindArithmeticMean(Vector3[] vertices) {
        float sumX = 0f, sumY = 0f;

        for(var i = 0; i < vertices.Length; i++) {
            sumX += vertices[i].x;
            sumY += vertices[i].y;
        }

        return new Vector3(sumX / vertices.Length, sumY / vertices.Length);
    }

    private static void ProjectVertices(Vector3[] vertices, Vector3 axis, out float min, out float max) {
        min = float.MaxValue;
        max = float.MinValue;

        for(var i = 0; i < vertices.Length; i++) {
            var v = vertices[i];
            var proj = Vector3.Dot(v, axis);
            if (proj < min) min = proj;
            if (proj > max) max = proj;
        }
    }

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
