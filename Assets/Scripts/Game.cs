using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class Game : MonoBehaviour
{
    private Factory factory;

    private FlatWorld world;
    private List<ShapeRenderer> srList;
    private float left, right, bottom, top;
    private List<(FlatBody, ShapeRenderer)> removeBodyIndices;
    private Stopwatch watch;

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

        removeBodyIndices = new List<(FlatBody, ShapeRenderer)>();
        srList = new List<ShapeRenderer>();
        world = new FlatWorld();

        //CreateRandom();
        CreateGravity();

        watch = new Stopwatch();
    }

    private void CreateGravity() {
        var padding = (right - left) * 0.1f;

        CreateBox(right - left - padding * 2, 3f, new Vector3(0, -10), true, Color.gray);
    }

    private void CreateBox(float width, float height, Vector3 position, bool isStatic, Color color) {
        FlatBody.CreateBoxBody(width, height, position, 1f, isStatic, 0.5f, out var body, out _);
        world.AddBody(body);
        var sr = factory.CreateRectangle(width, height);
        sr.SetColor(color);
        srList.Add(sr);
    }

    private void CreateCircle(float radius, Vector3 position, bool isStatic, Color color) {
        FlatBody.CreateCircleBody(radius, position, 1f, isStatic, 0.5f, out var body, out _);
        world.AddBody(body);
        var sr = factory.CreateCircle(radius);
        sr.SetColor(color);
        srList.Add(sr);
    }

    private void CreateRandom() {
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

        /*
        var dx = 0f;
        var dy = 0f;
        var forceMagnitude = 48f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) dx--;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dx++;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) dy++;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) dy--;

        world.GetBody(0, out var body);

        if(dx!= 0 || dy != 0) {
            //var moveDirection = new Vector3(dx, dy).normalized;
            //var movement = deltaTime * 8f * moveDirection;
            //body.Move(movement);
            var forceDirection = new Vector3(dx, dy).normalized;
            var force = forceDirection * forceMagnitude;
            body.AddForce(force);
        }

        if (Input.GetKeyDown(KeyCode.Q)) body.Rotate(Mathf.PI / 2 * deltaTime);
        */

        if (Input.GetMouseButtonDown(0)) {
            var width = Utils.RandomFloat(1f, 2f);
            var height = Utils.RandomFloat(1f, 2f);
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            CreateBox(width, height, mousePos, false, Utils.RandomColor());
        }
        if (Input.GetMouseButtonDown(1)) {
            var radius = Utils.RandomFloat(0.75f, 1.5f);
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            CreateCircle(radius, mousePos, false, Utils.RandomColor());
        }

        watch.Restart();
        world.Step(deltaTime, 15);
        watch.Stop();

        removeBodyIndices.Clear();
        for (var i = 0; i < world.BodyCount; i++) {
            if (!world.GetBody(i, out var flatBody)) continue;
            var sr = srList[i];
            sr.Pos = flatBody.Position;
            sr.Rot = flatBody.Rotation;
            flatBody.DrawDebug();

            var box = flatBody.GetAABB();
            if (box.Max.y < bottom) removeBodyIndices.Add((flatBody, sr));
        }
        for(var i =0; i < removeBodyIndices.Count; i++) {
            world.RemoveBody(removeBodyIndices[i].Item1);
            srList.Remove(removeBodyIndices[i].Item2);
            Destroy(removeBodyIndices[i].Item2.gameObject);
        }

        //WrapScene();

        if (Input.GetKeyDown(KeyCode.Space)) Debug.Log($"{world.BodyCount} bodies with step time: {watch.Elapsed.TotalMilliseconds}");
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
