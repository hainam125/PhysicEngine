using UnityEngine;

public class Factory : MonoBehaviour
{
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform squarePrefab;

    private readonly float borderSize = 0.25f;

    public ShapeRenderer CreateCircle(float radius) {
        var body = Instantiate(circlePrefab);
        body.localScale = Vector3.one * radius * 2;

        var inner = Instantiate(circlePrefab);
        inner.localScale = body.localScale - Vector3.one * borderSize;
        inner.SetParent(body);
        inner.position -= Vector3.forward * 0.01f;
        return new ShapeRenderer(inner, body);
    }

    public ShapeRenderer CreateRectangle(float width, float height) {
        var body = Instantiate(squarePrefab);
        var scale = body.localScale = new Vector3(width, height, 1f);

        var inner = Instantiate(squarePrefab);
        scale.x -= borderSize;
        scale.y -= borderSize;
        inner.localScale = scale;
        inner.SetParent(body);
        inner.position -= Vector3.forward * 0.01f;

        return new ShapeRenderer(inner, body);
    }
}
