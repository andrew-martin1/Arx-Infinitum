using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Crosshair : MonoBehaviour
{
    public LayerMask targetMask;
    public Color dotHighlightColor;
    Color originalDotColor;
    public SpriteRenderer dot;

    private void Start()
    {
        Cursor.visible = false;
        originalDotColor = dot.color;
    }

    private void Update()
    {
        transform.Rotate(Vector3.forward * -40 * Time.deltaTime);
    }

    public void DetectTargets(Ray ray)
    {
        if (Physics.Raycast(ray, 100, targetMask))
        {
            dot.color = dotHighlightColor;
        }
        else
        {
            dot.color = originalDotColor;
        }
    }
}

