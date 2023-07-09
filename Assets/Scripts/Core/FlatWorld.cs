using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatWorld {
    public const float MinBodySize = 0.01f * 0.01f;
    public const float MaxBodySize = 15f * 15f;

    public const float MinDensity = 0.5f;
    public const float MaxDensity = 21.4f;

    public const int MinIterations = 1;
    public const int MaxIterations = 100;

    private List<FlatBody> bodyList;
    private List<(int, int)> contactPairs;
    private Vector3 gravity;

    public int BodyCount => bodyList.Count;

    public FlatWorld() {
        gravity = new Vector3(0, -9.81f);
        bodyList = new List<FlatBody>();
        contactPairs = new List<(int, int)>();
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

    public void Step(float time, int totalIterations) {
        totalIterations = Mathf.Clamp(totalIterations, MinIterations, MaxIterations);

        for (var curIteration = 0; curIteration < totalIterations; curIteration++) {
            contactPairs.Clear();
            StepBodies(time, totalIterations);
            BroadPhase();
            NarrowPhase();
        }
    }

    private void BroadPhase() {
        for (var i = 0; i < bodyList.Count - 1; i++) {
            var bodyA = bodyList[i];
            var aabbA = bodyA.GetAABB();
            for (var j = i + 1; j < bodyList.Count; j++) {
                var bodyB = bodyList[j];

                if (bodyA.isStatic && bodyB.isStatic) continue;

                var aabbB = bodyB.GetAABB();
                if (!Collisions.IntersectAABBs(aabbA, aabbB)) continue;

                contactPairs.Add((i, j));
            }
        }
    }

    private void NarrowPhase() {
        for (var i = 0; i < contactPairs.Count; i++) {
            var pair = contactPairs[i];
            var bodyA = bodyList[pair.Item1];
            var bodyB = bodyList[pair.Item2];

            if (Collisions.Collide(bodyA, bodyB, out var normal, out var depth)) {
                SeperateBodies(bodyA, bodyB, normal * depth);

                Collisions.FindContactPoint(bodyA, bodyB, out var contact1, out var contact2, out var contactCount);
                var contact = new FlatManifold(bodyA, bodyB, normal, depth, contact1, contact2, contactCount);

                ResolveCollision(in contact);
            }
        }
    }

    private void StepBodies(float time, int totalIterations) {
        for (var i = 0; i < bodyList.Count; i++) {
            bodyList[i].Step(time, gravity, totalIterations);
        }
    }

    private void SeperateBodies(FlatBody bodyA, FlatBody bodyB, Vector3 minimumTranslationVector) {
        if (bodyA.isStatic) {
            bodyB.Move(minimumTranslationVector);
        }
        else if (bodyB.isStatic) {
            bodyA.Move(-minimumTranslationVector);
        }
        else {
            bodyA.Move(-minimumTranslationVector * 0.5f);
            bodyB.Move(minimumTranslationVector * 0.5f);
        }
    }

    private void ResolveCollision(in FlatManifold contact) {
        var bodyA = contact.bodyA;
        var bodyB = contact.bodyB;
        var normal = contact.normal;
        var depth = contact.depth;

        var relativeVel = bodyB.LinearVelocity - bodyA.LinearVelocity;

        //if objects moving apart
        if (Vector3.Dot(relativeVel, normal) > 0) {
            return;
        }

        var e = Mathf.Min(bodyA.restitution, bodyB.restitution);

        var j = -(1 + e) * Vector3.Dot(relativeVel, normal);
        j /= (bodyA.invMass + bodyB.invMass);

        var impulse = j * normal;

        bodyA.LinearVelocity -= impulse * bodyA.invMass;
        bodyB.LinearVelocity += impulse * bodyB.invMass;
    }
}

