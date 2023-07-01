using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeType { Circle = 0, Box =1 }

public class FlatBody
{
    public Vector3 Position { get; private set; }
    private Vector3 linearVelocity;
    public float Rotation { get; private set; }
    private float ratationVelocity;

    public readonly float density;
    public readonly float mass;
    public readonly float restitution;
    public readonly float area;

    public readonly bool isStatic;

    public readonly float radius;

    public readonly float width, height;
    public readonly int[] triangles;
    private readonly Vector3[] vertices;

    private Vector3[] transformedVertices;
    private bool transformUpdateRequired;

    public readonly ShapeType shapeType;

    public FlatBody(Vector3 position, float density, float mass, float restitution, float area,
        bool isStatic, float radius, float width, float height, ShapeType shapeType) {

        this.Position = position;
        this.linearVelocity = Vector3.zero;
        this.Rotation = 0f;
        this.ratationVelocity = 0f;

        this.density = density;
        this.mass = mass;
        this.restitution = restitution;
        this.area = area;

        this.isStatic = isStatic;
        this.radius = radius;
        this.width = width;
        this.height = height;
        this.shapeType = shapeType;

        if(shapeType == ShapeType.Box) {
            this.vertices = CreateBoxVertices(width, height);
            this.triangles = CreateBoxTriangles();
            this.transformedVertices = new Vector3[this.vertices.Length];
        }
        this.transformUpdateRequired = true;
    }

    public void Move(Vector3 amount) {
        Position += amount;
        transformUpdateRequired = true;
    }

    public void MoveTo(Vector3 position) {
        Position = position;
        transformUpdateRequired = true;
    }

    public void Rotate(float amount) {
        Rotation += amount;
        transformUpdateRequired = true;
    }

    public Vector3[] GetTransformedVertices() {
        if (transformUpdateRequired) {
            var transform = new FlatTransform(Position, Rotation);
            for(var i = 0;  i < vertices.Length; i++) {
                var v = vertices[i];
                transformedVertices[i] = transform.Transform(v);
            }
        }
        transformUpdateRequired = false;
        return transformedVertices;
    }

    public void DrawDebug() {
        if (shapeType == ShapeType.Box) {
            for (var i = 0; i < GetTransformedVertices().Length; i++) {
                var a = transformedVertices[i];
                var b = transformedVertices[(i + 1) % transformedVertices.Length];
                Debug.DrawLine(a, b, Color.white);
            }
        }
    }

    #region Factory
    public static bool CreateCircleBody(float radius, Vector3 position, float density, bool isStatic, float restitution,
        out FlatBody body, out string errorMsg) {

        body = null;
        errorMsg = string.Empty;

        var area = radius * radius * Mathf.PI;
        if (!CheckBodyData(area, density, ref errorMsg, ref restitution, out var mass)) return false;

        body = new FlatBody(position, density, mass, restitution, area, isStatic, radius, 0f, 0f, ShapeType.Circle);
        return true;
    }


    public static bool CreateBoxBody(float width, float height, Vector3 position, float density, bool isStatic, float restitution,
        out FlatBody body, out string errorMsg) {

        body = null;
        errorMsg = string.Empty;

        var area = width * height;
        if (!CheckBodyData(area, density, ref errorMsg, ref restitution, out var mass)) return false;

        body = new FlatBody(position, density, mass, restitution, area, isStatic, 0f, width, height, ShapeType.Box);
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

    private static int[] CreateBoxTriangles() {
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;
        return triangles;
    }

    private static bool CheckBodyData(float area, float density, ref string errorMsg, ref float restitution, out float mass) {
        mass = 0f;
        if (area < Config.MinBodySize) {
            errorMsg = "Area is too small";
            return false;
        }

        if (area > Config.MaxBodySize) {
            errorMsg = "Area is too large";
            return false;
        }

        if (density < Config.MinDensity) {
            errorMsg = "Density is too small";
            return false;
        }

        if (density > Config.MaxDensity) {
            errorMsg = "Density is too large";
            return false;
        }
        restitution = Mathf.Clamp01(restitution);
        mass = density * area;
        return true;
    }
    #endregion
}