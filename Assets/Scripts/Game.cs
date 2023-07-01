using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    private Factory factory;

    private FlatWorld world;
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

        srList = new List<ShapeRenderer>();
        world = new FlatWorld();

        var bodyCount = 25;
        var padding = (right - left) * 0.05f;

        for (var i = 0; i < bodyCount; i++) {
            var type = (ShapeType)Utils.RandomInt(0, 2);

            FlatBody body = null;
            ShapeRenderer sr = null;
            var x = Utils.RandomFloat(left + padding, right - padding);
            var y = Utils.RandomFloat(bottom + padding, top - padding);

            var isStatic = i > 0 && Utils.RandomBoolean();

            if (type == ShapeType.Circle) {
                var radius = 1f;
                sr = factory.CreateCircle(radius);
                if (!FlatBody.CreateCircleBody(radius, new Vector3(x, y), 2, isStatic, 0.5f, out body, out var errorMsg)) {
                    Debug.LogError(errorMsg);
                }
            }
            else if (type == ShapeType.Box) {
                var width = 1.77f;
                var height = 1.77f;
                sr = factory.CreateRectangle(width, height);
                if (!FlatBody.CreateBoxBody(width, height, new Vector3(x, y), 2, isStatic, 0.5f, out body, out var errorMsg)) {
                    Debug.LogError(errorMsg);
                }
            }
            else {
                Debug.LogError("st wrong!");
            }
            world.AddBody(body);

            if (isStatic) {
                sr.SetColor(Color.green);
                sr.SetBorderColor(Color.red);
            }
            else {
                sr.SetColor(Utils.RandomColor());
            }
            srList.Add(sr);
        }
    }

    private void Update() {
        var deltaTime = Time.deltaTime;

        var dx = 0f;
        var dy = 0f;
        var forceMagnitude = 48f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) dx--;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dx++;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) dy++;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) dy--;

        world.GetBody(0, out var body);

        if(dx!= 0 || dy != 0) {
            var forceDirection = new Vector3(dx, dy).normalized;
            var force = forceDirection * forceMagnitude;
            body.AddForce(force);
        }

        if (Input.GetKeyDown(KeyCode.Q)) body.Rotate(Mathf.PI / 2 * deltaTime);

        world.Step(deltaTime);
        
        for (var i = 0; i < world.BodyCount; i++) {
            if (!world.GetBody(i, out var flatBody)) continue;
            srList[i].Pos = flatBody.Position;
            srList[i].Rot = flatBody.Rotation;
            flatBody.DrawDebug();
        }

        WrapScene();
    }

    private void WrapScene() {
        var camWidth = right - left;
        var camHeight = top - bottom;
        for (var i = 0; i < world.BodyCount; i++) {
            if (!world.GetBody(i, out var body)) continue;
            if (body.Position.x < left) body.MoveTo(body.Position + new Vector3(camWidth, 0f));
            if (body.Position.x > right) body.MoveTo(body.Position - new Vector3(camWidth, 0f));
            if (body.Position.y < bottom) body.MoveTo(body.Position + new Vector3(0f, camHeight));
            if (body.Position.y > top) body.MoveTo(body.Position - new Vector3(0f, camHeight));
        }
    }
}
