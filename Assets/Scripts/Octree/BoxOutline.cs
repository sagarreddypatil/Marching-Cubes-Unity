using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxOutline : MonoBehaviour
{
    public float size = 1f;
    public int depth = 0;

    void Start()
    {
    }

    void Update()
    {
        DrawBox();
    }

    void DrawBox()
    {
        // generate color hue based on size
        Random.InitState(depth);
        float hue = Random.value;
        Color boxColor = Color.HSVToRGB(hue, 1, 1);

        Debug.DrawLine(transform.position + new Vector3(0, 0, 0), transform.position + new Vector3(size, 0, 0), boxColor, 0f, true);
        Debug.DrawLine(transform.position + new Vector3(size, 0, 0), transform.position + new Vector3(size, size, 0), boxColor, 0f, true);
        Debug.DrawLine(transform.position + new Vector3(size, size, 0), transform.position + new Vector3(0, size, 0), boxColor, 0f, true);
        Debug.DrawLine(transform.position + new Vector3(0, size, 0), transform.position + new Vector3(0, 0, 0), boxColor, 0f, true);

        Debug.DrawLine(transform.position + new Vector3(0, 0, size), transform.position + new Vector3(size, 0, size), boxColor, 0f, true);
        Debug.DrawLine(transform.position + new Vector3(size, 0, size), transform.position + new Vector3(size, size, size), boxColor, 0f, true);
        Debug.DrawLine(transform.position + new Vector3(size, size, size), transform.position + new Vector3(0, size, size), boxColor, 0f, true);
        Debug.DrawLine(transform.position + new Vector3(0, size, size), transform.position + new Vector3(0, 0, size), boxColor, 0f, true);

        Debug.DrawLine(transform.position + new Vector3(0, 0, 0), transform.position + new Vector3(0, 0, size), boxColor, 0f, true);
        Debug.DrawLine(transform.position + new Vector3(size, 0, 0), transform.position + new Vector3(size, 0, size), boxColor, 0f, true);
        Debug.DrawLine(transform.position + new Vector3(size, size, 0), transform.position + new Vector3(size, size, size), boxColor, 0f, true);
        Debug.DrawLine(transform.position + new Vector3(0, size, 0), transform.position + new Vector3(0, size, size), boxColor, 0f, true);
    }
}