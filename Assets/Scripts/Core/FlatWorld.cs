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
    private List<FlatManifold> contactList;
    private List<Vector3> contactPointList;
    private Vector3 gravity;

    public int BodyCount => bodyList.Count;

    public FlatWorld() {
        gravity = new Vector3(0, -9.81f);
        bodyList = new List<FlatBody>();
        contactList = new List<FlatManifold>();
        contactPointList = new List<Vector3>();
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

    public void Step(float time, int iterations) {
        iterations = Mathf.Clamp(iterations, MinIterations, MaxIterations);
        for(var it = 0; it < iterations; it++) {
            //movement
            for (var i = 0; i < bodyList.Count; i++) {
                bodyList[i].Step(time, gravity, iterations);
            }

            contactList.Clear();
            //collision
            for (var i = 0; i < bodyList.Count - 1; i++) {
                var bodyA = bodyList[i];
                var aabbA = bodyA.GetAABB();
                for (var j = i + 1; j < bodyList.Count; j++) {
                    var bodyB = bodyList[j];

                    if (bodyA.isStatic && bodyB.isStatic) continue;

                    var aabbB = bodyB.GetAABB();
                    if (!Collisions.IntersectAABBs(aabbA, aabbB)) continue;

                    if (Collisions.Collide(bodyA, bodyB, out var normal, out var depth)) {
                        if (bodyA.isStatic) {
                            bodyB.Move(normal * depth);
                        }
                        else if (bodyB.isStatic) {
                            bodyA.Move(-normal * depth);
                        }
                        else {
                            bodyA.Move(-normal * depth * 0.5f);
                            bodyB.Move(normal * depth * 0.5f);
                        }

                        Collisions.FindContactPoint(bodyA, bodyB, out var contact1, out var contact2, out var contactCount);
                        var contact = new FlatManifold(bodyA, bodyB, normal, depth, contact1, contact2, contactCount);
                        contactList.Add(contact);
                    }
                }
            }

            contactPointList.Clear();
            for (var i =0; i < contactList.Count; i++) {
                var contact = contactList[i];
                if (contact.contactCount > 0) {
                    if (!contactPointList.Contains(contact.contact1)) {
                        contactPointList.Add(contact.contact1);
                    }
                    if (contact.contactCount > 1 && !contactPointList.Contains(contact.contact2)) {
                        contactPointList.Add(contact.contact2);
                    }
                }
                ResolveCollision(contact);
            }
            foreach (var point in contactPointList) {
                Debug.DrawRay(point - Vector3.up * 0.25f, Vector3.up * 0.5f, Color.green, 0.05f);
            }
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

