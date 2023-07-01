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

        var bodyCount = 10;
        var padding = (right - left) * 0.05f;
        for (var i = 0; i < bodyCount; i++) {
            var type = (ShapeType)Utils.RandomInt(0, 2);

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
            world.AddBody(body);

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

        world.GetBody(0, out var body);

        if(dx!= 0 || dy != 0) {
            var direction = new Vector3(dx, dy).normalized;
            var velocity = direction * speed * deltaTime;
            body.Move(velocity);
        }

        if (Input.GetKeyDown(KeyCode.Q)) body.Rotate(Mathf.PI / 2 * deltaTime);

        world.Step(deltaTime);
        
        for (var i = 0; i < world.BodyCount; i++) {
            if (!world.GetBody(i, out var flatBody)) continue;
            srList[i].Pos = flatBody.Position;
            srList[i].Rot = flatBody.Rotation;
            flatBody.DrawDebug();
        }
    }
}
