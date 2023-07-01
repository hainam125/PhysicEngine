using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatWorld {
    public const float MinBodySize = 0.01f * 0.01f;
    public const float MaxBodySize = 15f * 15f;

    public const float MinDensity = 0.5f;
    public const float MaxDensity = 21.4f;

    private List<FlatBody> bodyList;
    private Vector3 gravity;

    public int BodyCount => bodyList.Count;

    public FlatWorld() {
        gravity = new Vector3(0, 9.81f);
        bodyList = new List<FlatBody>();
    }

    public void AddBody(FlatBody body) {
        bodyList.Add(body);
    }

    public bool RemoveBody(FlatBody body) {
        return bodyList.Remove(body);
    }

    public bool GetBody(int index, out FlatBody body) {
        body = null;
        if (index < 0 || index >= bodyList.Count) return false;
        body = bodyList[index]; ;
        return true;
    }

    public void Step(float time) {
        //movement
        for (var i = 0; i < bodyList.Count; i++) {
            bodyList[i].Step(time);
        }

        //collision
        for (var i = 0; i < bodyList.Count - 1; i++) {
            var bodyA = bodyList[i];
            for (var j = i + 1; j < bodyList.Count; j++) {
                var bodyB = bodyList[j];

                if (Collide(bodyA, bodyB, out var normal, out var depth)) {
                    bodyA.Move(-normal * depth / 2f);
                    bodyB.Move(normal * depth / 2f);

                    ResolveCollision(bodyA, bodyB, normal, depth);
                }
            }
        }
    }

    private void ResolveCollision(FlatBody bodyA, FlatBody bodyB, Vector3 normal, float depth) {
        var relativeVel = bodyB.LinearVelocity - bodyA.LinearVelocity;
        var e = Mathf.Min(bodyA.restitution, bodyB.restitution);

        var j = -(1 + e) * Vector3.Dot(relativeVel, normal);
        j /= (1 / bodyA.mass + 1 / bodyB.mass);

        bodyA.LinearVelocity -= j / bodyA.mass * normal;
        bodyB.LinearVelocity += j / bodyB.mass * normal;
    }

    //push bodyB out side of bodyA
    private bool Collide(FlatBody bodyA, FlatBody bodyB, out Vector3 normal, out float depth) {
        normal = Vector3.zero;
        depth = 0f;

        var shapeTypeA = bodyA.shapeType;
        var shapeTypeB = bodyB.shapeType;

        if (shapeTypeA is ShapeType.Box) {
            if (shapeTypeB is ShapeType.Box) {
                return Collisions.IntersectPolygons(
                    bodyA.GetTransformedVertices(), bodyB.GetTransformedVertices(),
                    out normal, out depth);
            }
            else if (shapeTypeB is ShapeType.Circle) {
                bool result = Collisions.IntersectCirclePolygon(
                    bodyB.Position, bodyB.radius, bodyA.GetTransformedVertices(),
                    out normal, out depth);

                normal = -normal;
                return result;
            }
        }
        else if (shapeTypeA is ShapeType.Circle) {
            if (shapeTypeB is ShapeType.Box) {
                return Collisions.IntersectCirclePolygon(
                    bodyA.Position, bodyA.radius, bodyB.GetTransformedVertices(),
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
}

