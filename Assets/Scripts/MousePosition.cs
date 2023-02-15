using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePosition : MonoBehaviour
{
    public float speed = 3;
    private void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mouse = new Vector3(Input.GetAxis("Mouse X") * speed * Time.deltaTime, 0, 0);
            transform.Translate(mouse * speed);

        }
    }

}

