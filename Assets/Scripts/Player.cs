using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    public bool isMoving = false;
    private Vector3 targetPosition;
    private float speed = 0.5f;

    

    private void Start()
    {
        
    }





    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            SetTargetPosition();



        }
        if (isMoving)
        {
            Move();

        }

    }
    private void SetTargetPosition()
    {
        targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPosition.z = transform.position.z;

        isMoving= true;




    }
    private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        if (transform.position == targetPosition )
        {
            isMoving = false;
        }
    }


}

