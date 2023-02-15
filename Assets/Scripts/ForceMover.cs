using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    public GameObject brush;
    public bool isPushed;

    void Update()
    {
        if (isPushed)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            transform.position = new Vector3(ray.origin.x, 0, ray.origin.y);
        }
    }

    void OnMouseDown()
    {
        isPushed = true;
    }

    void OnMouseUp()
    {
        isPushed = false;
    }
}


