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

    private double totalWorldStepTime;
    private int totalBodyCount;
    private int totalSampleCount;
    private Stopwatch sampleTimer;
    private string bodyCountString, worldStepTimeString;

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

        CreateGravity();

        watch = new Stopwatch();
        sampleTimer = new Stopwatch();
        sampleTimer.Start();
    }

    private void CreateGravity() {
        var padding = (right - left) * 0.1f;

        var body = CreateBox(right - left - padding * 2, 3f, new Vector3(0, -10), true, Color.gray);
        /*body = CreateBox(20f, 2f, new Vector3(-10, 3f), true, Color.cyan);
        body.Rotate(-20 * Mathf.Deg2Rad);
        body = CreateBox(15f, 2f, new Vector3(10, 10f), true, Color.cyan);
        body.Rotate(20 * Mathf.Deg2Rad);*/
    }

    private FlatBody CreateBox(float width, float height, Vector3 position, bool isStatic, Color color) {
        FlatBody.CreateBoxBody(width, height, 1f, isStatic, 0.5f, out var body, out _);
        body.MoveTo(position);
        world.AddBody(body);
        var sr = factory.CreateRectangle(width, height);
        sr.SetColor(color);
        srList.Add(sr);
        return body;
    }

    private void CreateCircle(float radius, Vector3 position, bool isStatic, Color color) {
        FlatBody.CreateCircleBody(radius, 1f, isStatic, 0.5f, out var body, out _);
        body.MoveTo(position);
        world.AddBody(body);
        var sr = factory.CreateCircle(radius);
        sr.SetColor(color);
        srList.Add(sr);
    }

    private void Update() {
        var deltaTime = Time.deltaTime;

        if (Input.GetMouseButtonDown(0)) {
            var width = Utils.RandomFloat(2f, 3f);
            var height = Utils.RandomFloat(2f, 3f);
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            CreateBox(width, height, mousePos, false, Utils.RandomColor());
        }
        if (Input.GetMouseButtonDown(1)) {
            var radius = Utils.RandomFloat(1.25f, 1.5f);
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            CreateCircle(radius, mousePos, false, Utils.RandomColor());
        }

        if (sampleTimer.Elapsed.TotalSeconds > 1d) {
            bodyCountString = "BodyCount: " + System.Math.Round((double)totalBodyCount / totalSampleCount, 4).ToString();
            worldStepTimeString = "StepTime: " + System.Math.Round(totalWorldStepTime / totalSampleCount, 5).ToString();
            totalWorldStepTime = 0;
            totalBodyCount = 0;
            totalSampleCount = 0;
            sampleTimer.Restart();
        }

        watch.Restart();
        world.Step(deltaTime, 15);
        watch.Stop();

        totalWorldStepTime += watch.ElapsedMilliseconds;
        totalBodyCount += world.BodyCount;
        totalSampleCount++;

        removeBodyIndices.Clear();
        for (var i = 0; i < world.BodyCount; i++) {
            if (!world.GetBody(i, out var flatBody)) continue;
            var sr = srList[i];
            sr.Pos = flatBody.Position;
            sr.Rot = flatBody.Angle;
            //flatBody.DrawDebug();

            var box = flatBody.GetAABB();
            if (box.Max.y < bottom && !flatBody.isStatic) removeBodyIndices.Add((flatBody, sr));
        }
        for (var i = 0; i < removeBodyIndices.Count; i++) {
            world.RemoveBody(removeBodyIndices[i].Item1);
            srList.Remove(removeBodyIndices[i].Item2);
            Destroy(removeBodyIndices[i].Item2.gameObject);
        }

    }

    private void OnGUI() {
        GUI.Label(new Rect(20, 0, 200, 30), bodyCountString);
        GUI.Label(new Rect(20, 50, 200, 30), worldStepTimeString);
    }
}
