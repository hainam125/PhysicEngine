using UnityEngine;

public readonly struct FlatManifold {
    public readonly FlatBody bodyA;
    public readonly FlatBody bodyB;
    public readonly Vector3 normal;
    public readonly float depth;
    public readonly Vector3 contact1;
    public readonly Vector3 contact2;
    public readonly int contactCount;

    public FlatManifold(FlatBody bodyA, FlatBody bodyB, Vector3 normal, float depth,
        Vector3 contact1, Vector3 contact2, int contactCount) {
        this.bodyA = bodyA;
        this.bodyB = bodyB;
        this.normal = normal;
        this.depth = depth;
        this.contact1 = contact1;
        this.contact2 = contact2;
        this.contactCount = contactCount;
    }

    public Vector3 GetContact(int i) {
        if (i == 1) return contact2;
        return contact1;
    }
}
