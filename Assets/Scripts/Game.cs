using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    private Factory factory;

    private List<FlatBody> bodyList;
    private List<ShapeRenderer> srList;
    private float left, right, bottom, top;

    private void Awake() {
        factory = GetComponent<Factory>();
        var cam = FindObjectOfType<Camera>();
        var camPos = cam.transform.position;
        var orthoSize = cam.orthographicSize;
        var ratio = (float)Screen.width / Screen.height;
        top = camPos.y + orthoSize;
        bottom = camPos.y - orthoSize;
        left = camPos.x - orthoSize * ratio;
        right = camPos.x + orthoSize * ratio;

        bodyList = new List<FlatBody>();
        srList = new List<ShapeRenderer>();

        var bodyCount = 10;
        var padding = (right - left) * 0.05f;
        for (var i = 0; i < bodyCount; i++) {
            var type = ShapeType.Box;// (ShapeType)Utils.RandomInt(0, 2);

            FlatBody body = null;
            ShapeRenderer sr = null;
            var x = Utils.RandomFloat(left + padding, right - padding);
            var y = Utils.RandomFloat(bottom + padding, top - padding);

            if (type == ShapeType.Circle) {
                var radius = 1f;
                sr = factory.CreateCircle(radius);
                if (!FlatBody.CreateCircleBody(radius, new Vector3(x, y), 2, false, 0.5f, out body, out var errorMsg)) {
                    Debug.LogError(errorMsg);
                }
            }
            else if (type == ShapeType.Box) {
                var width = 2f;
                var height = 2f;
                sr = factory.CreateRectangle(width, height);
                if (!FlatBody.CreateBoxBody(width, height, new Vector3(x, y), 2, false, 0.5f, out body, out var errorMsg)) {
                    Debug.LogError(errorMsg);
                }
            }
            else {
                Debug.LogError("st wrong!");
            }
            bodyList.Add(body);

            sr.SetColor(Utils.RandomColor());
            srList.Add(sr);
        }
    }

    private void Update() {
        var deltaTime = Time.deltaTime;

        var dx = 0f;
        var dy = 0f;
        var speed = 8f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) dx--;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dx++;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) dy++;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) dy--;

        if(dx!= 0 || dy != 0) {
            var direction = new Vector3(dx, dy).normalized;
            var velocity = direction * speed * deltaTime;
            bodyList[0].Move(velocity);
        }

        if (Input.GetKeyDown(KeyCode.Q)) bodyList[0].Rotate(Mathf.PI / 4);


        for (var i = 0; i < bodyList.Count; i++) {
            //bodyList[i].Rotate(Mathf.PI * 0.5f * deltaTime);
            srList[i].SetBorderColor(Color.white);
        }

        for (var i = 0; i < bodyList.Count - 1; i++) {
            var bodyA = bodyList[i];
            for (var j = i + 1; j < bodyList.Count; j++) {
                var bodyB = bodyList[j];

                if (Collisions.IntersectPolygons(
                    bodyA.GetTransformedVertices(),
                    bodyB.GetTransformedVertices(),
                    out var normal, out var depth)) {


                    srList[i].SetBorderColor(Color.red);
                    srList[j].SetBorderColor(Color.red);

                    bodyA.Move(-depth * 0.5f * normal);
                    bodyB.Move(depth * 0.5f * normal);
                }

                /*if(Collisions.IntersectCircles(
                    bodyA.Position, bodyA.radius,
                    bodyB.Position, bodyB.radius,
                    out var normal, out var depth)) {

                    bodyA.Move(-depth * 0.5f * normal);
                    bodyB.Move(depth * 0.5f * normal);
                }*/
            }
        }

        for (var i = 0; i < bodyList.Count; i++) {
            srList[i].Pos = bodyList[i].Position;
            srList[i].Rot = bodyList[i].Rotation;
            bodyList[i].DrawDebug();
        }
    }
}
