using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using FlatEngine;

using Debug = UnityEngine.Debug;

public class Game : MonoBehaviour {
    [SerializeField] private float bounciness = 0.5f;
    [SerializeField] private float staticFriction = 0.6f;
    [SerializeField] private float dynamicFriction = 0.4f;
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

        Initialize();

        watch = new Stopwatch();
        sampleTimer = new Stopwatch();
        sampleTimer.Start();
    }

    private void Update() {
        var deltaTime = Time.deltaTime;

        if (Input.GetMouseButtonDown(0)) {
            var width = FlatUtils.RandomFloat(2f, 3f);
            var height = FlatUtils.RandomFloat(2f, 3f);
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            CreateBox(width, height, mousePos, false, FlatUtils.RandomColor());
        }
        if (Input.GetMouseButtonDown(1)) {
            var radius = FlatUtils.RandomFloat(1.25f, 1.5f);
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            CreateCircle(radius, mousePos, false, FlatUtils.RandomColor());
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

        //draw
        removeBodyIndices.Clear();
        for (var i = 0; i < world.BodyCount; i++) {
            if (!world.GetBody(i, out var flatBody)) continue;
            var sr = srList[i];
            sr.Pos = flatBody.Position;
            sr.Rot = flatBody.Angle;
            flatBody.collider.DrawDebug();

            var box = flatBody.GetAABB();
            if (box.Max.y < bottom && !flatBody.isStatic) removeBodyIndices.Add((flatBody, sr));
        }
        for (var i = 0; i < removeBodyIndices.Count; i++) {
            world.RemoveBody(removeBodyIndices[i].Item1);
            srList.Remove(removeBodyIndices[i].Item2);
            Destroy(removeBodyIndices[i].Item2.gameObject);
        }
    }

    private void Initialize() {
        var padding = (right - left) * 0.1f;

        var body = CreateBox(right - left - padding * 2, 3f, new Vector3(0, -10), true, Color.gray);
        body = CreateBox(20f, 2f, new Vector3(-10, 3f), true, Color.cyan);
        body.Rotate(-20 * Mathf.Deg2Rad);
        body = CreateBox(15f, 2f, new Vector3(10, 10f), true, Color.cyan);
        body.Rotate(20 * Mathf.Deg2Rad);
    }

    private FlatBody CreateBox(float width, float height, Vector3 position, bool isStatic, Color color) {
        var mass = width * height;
        var body = FlatBody.CreateBoxBody(width, height, isStatic, mass, bounciness, staticFriction, dynamicFriction);
        body.MoveTo(position);
        world.AddBody(body);
        var sr = factory.CreateRectangle(width, height);
        sr.SetColor(color);
        srList.Add(sr);
        return body;
    }

    private void CreateCircle(float radius, Vector3 position, bool isStatic, Color color) {
        var mass = radius * radius * Mathf.PI;
        var body = FlatBody.CreateCircleBody(radius, isStatic, mass, bounciness, staticFriction, dynamicFriction);
        body.MoveTo(position);
        world.AddBody(body);
        var sr = factory.CreateCircle(radius);
        sr.SetColor(color);
        srList.Add(sr);
    }

    private void OnGUI() {
        GUI.Label(new Rect(20, 0, 200, 30), bodyCountString);
        GUI.Label(new Rect(20, 50, 200, 30), worldStepTimeString);
    }
}
