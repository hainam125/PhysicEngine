using UnityEngine;

namespace FlatEngine {
    public enum ShapeType { Circle = 0, Box = 1 }

    public class FlatBody {
        public Vector3 Position { get; private set; }
        public Vector3 LinearVelocity { get; internal set; }
        public float Angle { get; private set; }
        public float AngularVelocity { get; internal set; }

        private Vector3 force;

        public readonly ShapeType shapeType;
        public readonly FlatCollider collider;

        public readonly bool isStatic;
        public readonly float mass;
        public readonly float inertia;
        public readonly float restitution;
        public readonly float staticFriction, dynamicFriction;

        internal readonly float invMass;
        internal readonly float invInertia;

        private FlatBody(bool isStatic, FlatCollider collider,
            float mass, float restitution, float staticFriction, float dynamicFriction) {

            this.Position = Vector3.zero;
            this.LinearVelocity = Vector3.zero;
            this.Angle = 0f;
            this.AngularVelocity = 0f;
            this.force = Vector3.zero;

            this.restitution = restitution;
            this.isStatic = isStatic;
            this.staticFriction = staticFriction;
            this.dynamicFriction = dynamicFriction;

            this.collider = collider;
            this.collider.Setup(this);
            this.shapeType = collider.shapeType;

            this.mass = mass;
            this.inertia = collider.CalculateRotationalInertia();

            if (!isStatic) {
                this.invMass = 1f / mass;
                this.invInertia = 1f / inertia;
            }
            else {
                this.invMass = 0f;
                this.invInertia = 0f;
            }
            collider.SetDirty();
        }

        internal void Step(float time, Vector3 gravity, int iterations) {
            if (isStatic) return;

            time /= iterations;

            LinearVelocity += force / mass * time;

            LinearVelocity += gravity * time;

            Position += LinearVelocity * time;
            Angle += AngularVelocity * time;

            force = Vector3.zero;
            collider.SetDirty();
        }

        public void Move(Vector3 amount) {
            Position += amount;
            collider.SetDirty();
        }

        public void MoveTo(Vector3 position) {
            Position = position;
            collider.SetDirty();
        }

        public void Rotate(float amount) {
            Angle += amount;
            collider.SetDirty();
        }

        public void AddForce(Vector3 amount) {
            force = amount;
        }

        internal FlatAABB GetAABB() => collider.GetAABB();

        #region Factory
        public static FlatBody CreateCircleBody(float radius, bool isStatic, float mass,
            float restitution, float staticFriction, float dynamicFriction) {

            var collider = new FlatCircleCollider(radius);
            return new FlatBody(isStatic, collider, mass, restitution, staticFriction, dynamicFriction);
        }

        public static FlatBody CreateBoxBody(float width, float height, bool isStatic, float mass,
            float restitution, float staticFriction, float dynamicFriction) {

            var collider = new FlatBoxCollider(width, height);
            return new FlatBody(isStatic, collider, mass, restitution, staticFriction, dynamicFriction);
        }
        #endregion
    }
}