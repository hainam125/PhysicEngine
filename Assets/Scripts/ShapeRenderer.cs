using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeRenderer
{
    private SpriteRenderer inner;
    private SpriteRenderer body;

    private Transform transform;

    public Vector3 Pos { get => transform.position; set => transform.position = value; }

    public ShapeRenderer(Transform border, Transform body) {
        this.inner = border.GetComponent<SpriteRenderer>();
        this.body = body.GetComponent<SpriteRenderer>();

        this.transform = body;
    }

    public void SetColor(Color color) {
        inner.color = color;
    }

    public void SetBorderColor(Color color) {
        body.color = color;
    }
}
