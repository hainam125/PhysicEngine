using UnityEngine;

public static class Collisions {
    public static bool IntersectAABBs(FlatAABB a, FlatAABB b) {
        if (a.Max.x <= b.Min.x || b.Max.x <= a.Min.x ||
            a.Max.y <= b.Min.y || b.Max.y <= a.Min.y) {
            return false;
        }
        return true;
    }

    public static void FindContactPoint(FlatBody bodyA, FlatBody bodyB,
        out Vector3 contact1, out Vector3 contact2, out int contactCount) {

        contactCount = 0;
        contact1 = contact2 = Vector3.zero;

        var shapeTypeA = bodyA.shapeType;
        var shapeTypeB = bodyB.shapeType;

        if (shapeTypeA is ShapeType.Box) {
            if (shapeTypeB is ShapeType.Box) {
                FindContactPoint(bodyA.GetTransformedVertices(), bodyB.GetTransformedVertices(), out contact1, out contact2, out contactCount);
            }
            else if (shapeTypeB is ShapeType.Circle) {
                FindContactPoint(bodyB.Position, bodyB.radius, bodyA.Position, bodyA.GetTransformedVertices(), out contact1);
                contactCount = 1;
            }
        }
        else if (shapeTypeA is ShapeType.Circle) {
            if (shapeTypeB is ShapeType.Box) {
                FindContactPoint(bodyA.Position, bodyA.radius, bodyB.Position, bodyB.GetTransformedVertices(), out contact1);
                contactCount = 1;
            }
            else if (shapeTypeB is ShapeType.Circle) {
                FindContactPoint(bodyA.Position, bodyA.radius, bodyB.Position, out contact1);
                contactCount = 1;
            }
        }
    }

    private static void FindContactPoint(Vector3[] verticesA, Vector3[] verticesB,
        out Vector3 contact1, out Vector3 contact2, out int contactCount) {

        contact1 = contact2 = Vector3.zero;
        contactCount = 0;
        var minDistSqr = float.MaxValue;

        void check(Vector3[] verticesX, Vector3[] verticesY, ref Vector3 c1, ref Vector3 c2, ref int count) {
            for (var i = 0; i < verticesX.Length; i++) {
                var p = verticesX[i];
                for (var j = 0; j < verticesY.Length; j++) {
                    var va = verticesY[j];
                    var vb = verticesY[(j + 1) % verticesY.Length];

                    PointSegmentDistance(p, va, vb, out var distSqr, out var cp);

                    if (Utils.NearlyEqual(distSqr, minDistSqr)) {
                        if (!Utils.NearlyEqual(cp, c1)) {
                            count = 2;
                            c2 = cp;
                        }
                    }
                    else if (distSqr < minDistSqr) {
                        minDistSqr = distSqr;
                        count = 1;
                        c1 = cp;
                    }
                }
            }
        }
        check(verticesA, verticesB, ref contact1, ref contact2, ref contactCount);
        check(verticesB, verticesA, ref contact1, ref contact2, ref contactCount);
    }

    private static void FindContactPoint(Vector3 circleCenter, float circleRadius,
        Vector3 polygonCenter, Vector3[] polygonVertices, out Vector3 cp) {

        cp = Vector3.zero;
        var minDistSqr = float.MaxValue;

        for (var i = 0; i < polygonVertices.Length; i++) {
            var va = polygonVertices[i];
            var vb = polygonVertices[(i + 1) % polygonVertices.Length];

            PointSegmentDistance(circleCenter, va, vb, out var distSqr, out var contact);
            if (distSqr < minDistSqr) {
                minDistSqr = distSqr;
                cp = contact;
            }
        }
    }

    private static void PointSegmentDistance(Vector3 p, Vector3 a, Vector3 b, out float distSqr, out Vector3 closestPoint) {
        var ab = b - a;
        var ap = p - a;
        var proj = Vector3.Dot(ap, ab);
        var abLenSq = ab.sqrMagnitude;
        var d = proj / abLenSq;

        if (d <= 0) closestPoint = a;
        else if (d >= 1) closestPoint = b;
        else closestPoint = a + ab * d;

        distSqr = (p - closestPoint).sqrMagnitude;
    }

    private static void FindContactPoint(Vector3 centerA, float radiusA, Vector3 centerB, out Vector3 cp) {
        var dir = (centerB - centerA).normalized;
        cp = centerA + dir * radiusA;
    }

    //push bodyB out side of bodyA
    public static bool Collide(FlatBody bodyA, FlatBody bodyB, out Vector3 normal, out float depth) {
        normal = Vector3.zero;
        depth = 0f;

        var shapeTypeA = bodyA.shapeType;
        var shapeTypeB = bodyB.shapeType;

        if (shapeTypeA is ShapeType.Box) {
            if (shapeTypeB is ShapeType.Box) {
                return Collisions.IntersectPolygons(
                    bodyA.Position, bodyA.GetTransformedVertices(),
                    bodyB.Position, bodyB.GetTransformedVertices(),
                    out normal, out depth);
            }
            else if (shapeTypeB is ShapeType.Circle) {
                bool result = Collisions.IntersectCirclePolygon(
                    bodyB.Position, bodyB.radius,
                    bodyA.Position, bodyA.GetTransformedVertices(),
                    out normal, out depth);

                normal = -normal;
                return result;
            }
        }
        else if (shapeTypeA is ShapeType.Circle) {
            if (shapeTypeB is ShapeType.Box) {
                return Collisions.IntersectCirclePolygon(
                    bodyA.Position, bodyA.radius,
                    bodyB.Position, bodyB.GetTransformedVertices(),
                    out normal, out depth);
            }
            else if (shapeTypeB is ShapeType.Circle) {
                return Collisions.IntersectCircles(
                    bodyA.Position, bodyA.radius,
                    bodyB.Position, bodyB.radius,
                    out normal, out depth);
            }
        }

        return false;
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
