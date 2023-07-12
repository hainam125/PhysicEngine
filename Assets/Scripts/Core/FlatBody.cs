using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeType { Circle = 0, Box =1 }

public class FlatBody
{
    public Vector3 Position { get; private set; }
    public Vector3 LinearVelocity { get; internal set; }
    public float Angle { get; private set; }
    public float AngularVelocity { get; internal set; }

    private Vector3 force;

    public readonly ShapeType shapeType;
    public readonly bool isStatic;
    public readonly float density;
    public readonly float mass;
    public readonly float invMass;
    public readonly float restitution;
    public readonly float area;
    public readonly float inertia;
    public readonly float invInertia;
    public readonly float staticFriction, dynamicFriction;

    public readonly float radius;
    public readonly float width, height;
    public readonly int[] triangles;
    private readonly Vector3[] vertices;

    private Vector3[] transformedVertices;
    private bool transformUpdateRequired;

    private FlatAABB aabb;
    private bool aabbUpdateRequired;


    public FlatBody(float density, float mass, float restitution, float area,
        bool isStatic, float radius, float width, float height, ShapeType shapeType) {

        this.Position = Vector3.zero;
        this.LinearVelocity = Vector3.zero;
        this.Angle = 0f;
        this.AngularVelocity = 0f;
        this.force = Vector3.zero;

        this.shapeType = shapeType;
        this.density = density;
        this.restitution = restitution;
        this.area = area;
        this.isStatic = isStatic;

        this.radius = radius;
        this.width = width;
        this.height = height;

        this.mass = mass;
        this.inertia = CalculateRotationalInertia();

        this.staticFriction = 0.6f;
        this.dynamicFriction = 0.4f;

        if (!isStatic) {
            this.invMass = 1f / mass;
            this.invInertia = 1f / inertia;
        }
        else {
            this.invMass = 0f;
            this.invInertia = 0f;
        }

        if (shapeType == ShapeType.Box) {
            this.vertices = CreateBoxVertices(width, height);
            this.transformedVertices = new Vector3[this.vertices.Length];
        }
        this.transformUpdateRequired = true;
        this.aabbUpdateRequired = true;
    }

    //https://en.wikipedia.org/wiki/List_of_moments_of_inertia
    private float CalculateRotationalInertia() {
        if (shapeType == ShapeType.Circle) {
            return 0.5f * mass * radius * radius;
        }
        else if (shapeType == ShapeType.Box) {
            return (1f / 12f) * mass * (width * width + height * height);
        }
        else {
            throw new System.ArgumentOutOfRangeException(shapeType + "is invalid!");
        }
    }

    internal void Step(float time, Vector3 gravity, int iterations) {
        if (isStatic) return;

        time /= iterations;

        //LinearVelocity += force / mass * time;

        LinearVelocity += gravity * time;

        Position += LinearVelocity * time;
        Angle += AngularVelocity * time;

        force = Vector3.zero;
        transformUpdateRequired = true;
        aabbUpdateRequired = true;
    }

    public void Move(Vector3 amount) {
        Position += amount;
        transformUpdateRequired = true;
        aabbUpdateRequired = true;
    }

    public void MoveTo(Vector3 position) {
        Position = position;
        transformUpdateRequired = true;
        aabbUpdateRequired = true;
    }

    public void Rotate(float amount) {
        Angle += amount;
        transformUpdateRequired = true;
        aabbUpdateRequired = true;
    }

    public void AddForce(Vector3 amount) {
        force = amount;
    }

    public Vector3[] GetTransformedVertices() {
        if (transformUpdateRequired) {
            var transform = new FlatTransform(Position, Angle);
            for(var i = 0;  i < vertices.Length; i++) {
                var v = vertices[i];
                transformedVertices[i] = transform.Transform(v);
            }
        }
        transformUpdateRequired = false;
        return transformedVertices;
    }

    public FlatAABB GetAABB() {
        if (aabbUpdateRequired) {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            if (shapeType == ShapeType.Box) {
                var vertices = GetTransformedVertices();

                for (int i = 0; i < vertices.Length; i++) {
                    var v = vertices[i];

                    if (v.x < minX) { minX = v.x; }
                    if (v.x > maxX) { maxX = v.x; }
                    if (v.y < minY) { minY = v.y; }
                    if (v.y > maxY) { maxY = v.y; }
                }
            }
            if (shapeType == ShapeType.Circle) {
                minX = Position.x - radius;
                minY = Position.y - radius;
                maxX = Position.x + radius;
                maxY = Position.y + radius;
            }
            aabb = new FlatAABB(minX, minY, maxX, maxY);
        }
        aabbUpdateRequired = false;
        return aabb;
    }

    public void DrawDebug() {
        if (shapeType == ShapeType.Box) {
            for (var i = 0; i < GetTransformedVertices().Length; i++) {
                var a = transformedVertices[i];
                var b = transformedVertices[(i + 1) % transformedVertices.Length];
                Debug.DrawLine(a, b, Color.white);
            }
        }
        //Debug.DrawLine(aabb.Min, aabb.Max, Color.black);
    }

    #region Factory
    public static bool CreateCircleBody(float radius, float density, bool isStatic, float restitution,
        out FlatBody body, out string errorMsg) {

        body = null;
        errorMsg = string.Empty;

        var area = radius * radius * Mathf.PI;
        if (!CheckBodyData(area, density, ref errorMsg, ref restitution, out var mass)) return false;

        body = new FlatBody(density, mass, restitution, area, isStatic, radius, 0f, 0f, ShapeType.Circle);
        return true;
    }


    public static bool CreateBoxBody(float width, float height, float density, bool isStatic, float restitution,
        out FlatBody body, out string errorMsg) {

        body = null;
        errorMsg = string.Empty;

        var area = width * height;
        if (!CheckBodyData(area, density, ref errorMsg, ref restitution, out var mass)) return false;

        body = new FlatBody(density, mass, restitution, area, isStatic, 0f, width, height, ShapeType.Box);
        return true;
    }

    private static Vector3[] CreateBoxVertices(float width, float height) {
        var left = -width / 2f;
        var right = left + width;
        var bottom = -height / 2f;
        var top = bottom + height;

        var vertices = new Vector3[4];
        vertices[0] = new Vector3(left, top);
        vertices[1] = new Vector3(right, top);
        vertices[2] = new Vector3(right, bottom);
        vertices[3] = new Vector3(left, bottom);

        return vertices;
    }

    private static bool CheckBodyData(float area, float density, ref string errorMsg, ref float restitution, out float mass) {
        mass = 0f;
        if (area < FlatWorld.MinBodySize) {
            errorMsg = "Area is too small";
            return false;
        }

        if (area > FlatWorld.MaxBodySize) {
            errorMsg = "Area is too large";
            return false;
        }

        if (density < FlatWorld.MinDensity) {
            errorMsg = "Density is too small";
            return false;
        }

        if (density > FlatWorld.MaxDensity) {
            errorMsg = "Density is too large";
            return false;
        }
        restitution = Mathf.Clamp01(restitution);
        mass = density * area;
        return true;
    }
    #endregion
}
