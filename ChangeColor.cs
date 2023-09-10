using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    public float scaleGrowSpeed = 5;
    private int moveCubeLayer;

    private void Start()
    {
        moveCubeLayer = LayerMask.GetMask("MoveCube");
    }

    private void Update()
    {
        transform.localScale += Vector3.one * Time.deltaTime * scaleGrowSpeed;
        if (transform.localScale.x > 50) Destroy(gameObject);
    }
}
