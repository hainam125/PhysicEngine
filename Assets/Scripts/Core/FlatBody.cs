using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeType { Circle = 0, Box =1 }

public class FlatBody
{
    public Vector3 Position { get; private set; }
    private Vector3 linearVelocity;
    private float rotation;
    private float ratationVelocity;

    public readonly float density;
    public readonly float mass;
    public readonly float restitution;
    public readonly float area;

    public readonly bool isStatic;

    public readonly float radius;
    public readonly float width, height;

    public readonly ShapeType shapeType;

    public FlatBody(Vector3 position, float density, float mass, float restitution, float area,
        bool isStatic, float radius, float width, float height, ShapeType shapeType) {

        this.Position = position;
        this.linearVelocity = Vector3.zero;
        this.rotation = 0f;
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
    }

    public void Move(Vector3 amount) {
        Position += amount;
    }

    public void MoveTo(Vector3 position) {
        Position = position;
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
