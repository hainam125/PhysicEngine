using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlatEngine {
    public class FlatWorld {
        internal const int MinIterations = 1;
        internal const int MaxIterations = 100;

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
                    if (!FlatCollisions.IntersectAABBs(aabbA, aabbB)) continue;

                    contactPairs.Add((i, j));
                }
            }
        }

        private void NarrowPhase() {
            for (var i = 0; i < contactPairs.Count; i++) {
                var pair = contactPairs[i];
                var bodyA = bodyList[pair.Item1];
                var bodyB = bodyList[pair.Item2];

                if (FlatCollisions.Collide(bodyA, bodyB, out var normal, out var depth)) {
                    SeperateBodies(bodyA, bodyB, normal * depth);

                    FlatCollisions.FindContactPoint(bodyA, bodyB, out var contact1, out var contact2, out var contactCount);
                    var contact = new FlatManifold(bodyA, bodyB, normal, depth, contact1, contact2, contactCount);

                    ResolveCollisionWithRotationAndFriction(in contact);
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

        private void ResolveCollisionBasic(in FlatManifold contact) {
            var bodyA = contact.bodyA;
            var bodyB = contact.bodyB;
            var normal = contact.normal;

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

        private void ResolveCollisionWithRotation(in FlatManifold contact) {
            var bodyA = contact.bodyA;
            var bodyB = contact.bodyB;
            var normal = contact.normal;
            var contactCount = contact.contactCount;

            var e = Mathf.Min(bodyA.restitution, bodyB.restitution);

            var aLinearVelocity = bodyA.LinearVelocity;
            var aAngularVelocity = bodyA.AngularVelocity;
            var bLinearVelocity = bodyB.LinearVelocity;
            var bAngularVelocity = bodyB.AngularVelocity;

            for (var i = 0; i < contactCount; i++) {
                var rA = contact.GetContact(i) - bodyA.Position;
                var rB = contact.GetContact(i) - bodyB.Position;
                var raPerp = rA.PerpXY();
                var rbPerp = rB.PerpXY();

                var angularLinearVelA = aAngularVelocity * raPerp;
                var angularLinearVelB = bAngularVelocity * rbPerp;

                var relativeVel =
                    (bLinearVelocity + angularLinearVelB) -
                    (aLinearVelocity + angularLinearVelA);

                var contactVelMag = Vector3.Dot(relativeVel, normal);

                //if objects moving apart
                if (contactVelMag > 0) {
                    continue;
                }

                var raPerpDotN = Vector3.Dot(raPerp, normal);
                var rbPerpDotN = Vector3.Dot(rbPerp, normal);

                var denom = bodyA.invMass + bodyB.invMass +
                    raPerpDotN * raPerpDotN * bodyA.invInertia +
                    rbPerpDotN * rbPerpDotN * bodyB.invInertia;

                var j = -(1 + e) * contactVelMag;
                j /= denom;
                j /= contactCount;

                var impulse = j * normal;


                bodyA.LinearVelocity -= impulse * bodyA.invMass;
                bodyA.AngularVelocity -= FlatUtils.Cross(rA, impulse) * bodyA.invInertia;
                bodyB.LinearVelocity += impulse * bodyB.invMass;
                bodyB.AngularVelocity += FlatUtils.Cross(rB, impulse) * bodyB.invInertia;
            }
        }


        private void ResolveCollisionWithRotationAndFriction(in FlatManifold contact) {
            var bodyA = contact.bodyA;
            var bodyB = contact.bodyB;
            var normal = contact.normal;
            var contactCount = contact.contactCount;

            var e = Mathf.Min(bodyA.restitution, bodyB.restitution);

            var sf = (bodyA.staticFriction + bodyB.staticFriction) * 0.5f;
            var df = (bodyA.dynamicFriction + bodyB.dynamicFriction) * 0.5f;

            var aLinearVelocity = bodyA.LinearVelocity;
            var aAngularVelocity = bodyA.AngularVelocity;
            var bLinearVelocity = bodyB.LinearVelocity;
            var bAngularVelocity = bodyB.AngularVelocity;

            float j1 = 0f, j2 = 0f;
            void setJ(int i, float value) {
                if (i == 0) j1 = value;
                else j2 = value;
            }
            float getJ(int i) => i == 0 ? j1 : j2;

            for (var i = 0; i < contactCount; i++) {
                var rA = contact.GetContact(i) - bodyA.Position;
                var rB = contact.GetContact(i) - bodyB.Position;
                var raPerp = rA.PerpXY();
                var rbPerp = rB.PerpXY();

                var angularLinearVelA = aAngularVelocity * raPerp;
                var angularLinearVelB = bAngularVelocity * rbPerp;

                var relativeVel =
                    (bLinearVelocity + angularLinearVelB) -
                    (aLinearVelocity + angularLinearVelA);

                var contactVelMag = Vector3.Dot(relativeVel, normal);

                //if objects moving apart
                if (contactVelMag > 0) {
                    continue;
                }

                var raPerpDotN = Vector3.Dot(raPerp, normal);
                var rbPerpDotN = Vector3.Dot(rbPerp, normal);

                var denom = bodyA.invMass + bodyB.invMass +
                    raPerpDotN * raPerpDotN * bodyA.invInertia +
                    rbPerpDotN * rbPerpDotN * bodyB.invInertia;

                var j = -(1 + e) * contactVelMag;
                j /= denom;
                j /= contactCount;

                var impulse = j * normal;

                bodyA.LinearVelocity -= impulse * bodyA.invMass;
                bodyA.AngularVelocity -= FlatUtils.Cross(rA, impulse) * bodyA.invInertia;
                bodyB.LinearVelocity += impulse * bodyB.invMass;
                bodyB.AngularVelocity += FlatUtils.Cross(rB, impulse) * bodyB.invInertia;

                setJ(i, j);
            }

            //friction
            for (var i = 0; i < contactCount; i++) {
                var rA = contact.GetContact(i) - bodyA.Position;
                var rB = contact.GetContact(i) - bodyB.Position;
                var raPerp = rA.PerpXY();
                var rbPerp = rB.PerpXY();

                var angularLinearVelA = aAngularVelocity * raPerp;
                var angularLinearVelB = bAngularVelocity * rbPerp;

                var relativeVel =
                    (bLinearVelocity + angularLinearVelB) -
                    (aLinearVelocity + angularLinearVelA);

                var tangent = relativeVel - Vector3.Dot(relativeVel, normal) * normal;

                if (FlatUtils.NearlyEqual(tangent, Vector3.zero)) continue;
                else tangent.Normalize();

                var raPerpDotT = Vector3.Dot(raPerp, tangent);
                var rbPerpDotT = Vector3.Dot(rbPerp, tangent);

                var denom = bodyA.invMass + bodyB.invMass +
                    raPerpDotT * raPerpDotT * bodyA.invInertia +
                    rbPerpDotT * rbPerpDotT * bodyB.invInertia;

                var jt = -Vector3.Dot(relativeVel, tangent);
                jt /= denom;
                jt /= contactCount;

                var j = getJ(i);

                Vector3 frictionImpulse;

                //coulomb's law
                if (Mathf.Abs(jt) <= j * sf) {
                    frictionImpulse = jt * tangent;
                }
                else {
                    frictionImpulse = -j * tangent * df;
                }

                bodyA.LinearVelocity -= frictionImpulse * bodyA.invMass;
                bodyA.AngularVelocity -= FlatUtils.Cross(rA, frictionImpulse) * bodyA.invInertia;
                bodyB.LinearVelocity += frictionImpulse * bodyB.invMass;
                bodyB.AngularVelocity += FlatUtils.Cross(rB, frictionImpulse) * bodyB.invInertia;
            }
        }
    }

}