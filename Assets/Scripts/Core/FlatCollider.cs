using UnityEngine;

namespace FlatEngine {
    public abstract class FlatCollider {
        public abstract ShapeType shapeType { get; }
        protected FlatBody body;
        protected FlatAABB aabb;
        protected bool aabbUpdateRequired;

        //https://en.wikipedia.org/wiki/List_of_moments_of_inertia
        internal abstract float CalculateRotationalInertia();

        internal void Setup(FlatBody body) {
            this.body = body;
        }

        internal FlatAABB GetAABB() {
            if (aabbUpdateRequired) {
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;
                CalculateMinMax(ref minX, ref minY, ref maxX, ref maxY);
                aabb = new FlatAABB(minX, minY, maxX, maxY);
            }
            aabbUpdateRequired = false;
            return aabb;
        }

        protected abstract void CalculateMinMax(ref float minX, ref float minY, ref float maxX, ref float maxY);

        internal virtual void SetDirty() {
            this.aabbUpdateRequired = true;
        }

        internal virtual void DrawDebug() {
            //Debug.DrawLine(aabb.Min, aabb.Max, Color.black);
        }

        internal virtual Vector3[] GetTransformedVertices() {
            throw new System.NotImplementedException();
        }

        internal virtual float GetRadius() {
            throw new System.NotImplementedException();
        }
    }

    public class FlatCircleCollider : FlatCollider {
        public override ShapeType shapeType => ShapeType.Circle;
        public readonly float radius;

        public FlatCircleCollider(float radius) {
            this.radius = radius;
        }

        internal override float CalculateRotationalInertia() {
            return 0.5f * body.mass * radius * radius;
        }

        protected override void CalculateMinMax(ref float minX, ref float minY, ref float maxX, ref float maxY) {
            minX = body.Position.x - radius;
            minY = body.Position.y - radius;
            maxX = body.Position.x + radius;
            maxY = body.Position.y + radius;
        }

        internal override float GetRadius() => radius;
    }

    public class FlatBoxCollider : FlatCollider {
        public override ShapeType shapeType => ShapeType.Box;
        public readonly float width, height;
        private readonly Vector3[] vertices;
        private Vector3[] transformedVertices;
        private bool transformUpdateRequired;

        public FlatBoxCollider(float width, float height) {
            this.width = width;
            this.height = height;

            this.vertices = CreateBoxVertices(width, height);
            this.transformedVertices = new Vector3[this.vertices.Length];
        }

        internal override float CalculateRotationalInertia() {
            return (1f / 12f) * body.mass * (width * width + height * height);
        }

        protected override void CalculateMinMax(ref float minX, ref float minY, ref float maxX, ref float maxY) {
            var vertices = GetTransformedVertices();

            for (int i = 0; i < vertices.Length; i++) {
                var v = vertices[i];

                if (v.x < minX) { minX = v.x; }
                if (v.x > maxX) { maxX = v.x; }
                if (v.y < minY) { minY = v.y; }
                if (v.y > maxY) { maxY = v.y; }
            }
        }

        internal override Vector3[] GetTransformedVertices() {
            if (transformUpdateRequired) {
                var transform = new FlatTransform(body.Position, body.Angle);
                for (var i = 0; i < vertices.Length; i++) {
                    var v = vertices[i];
                    transformedVertices[i] = transform.Transform(v);
                }
            }
            transformUpdateRequired = false;
            return transformedVertices;
        }

        internal override void SetDirty() {
            base.SetDirty();
            this.transformUpdateRequired = true;
        }

        internal override void DrawDebug() {
            for (var i = 0; i < GetTransformedVertices().Length; i++) {
                var a = transformedVertices[i];
                var b = transformedVertices[(i + 1) % transformedVertices.Length];
                Debug.DrawLine(a, b, Color.white);
            }
            base.DrawDebug();
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
    }
}