using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShringGrow : MonoBehaviour
{
    public float Grow;
    public float Shring;



        private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "object")
        {

            transform.localScale += new Vector3(Grow, Grow, Grow);

            Destroy(collision.gameObject);

        }

        if (collision.gameObject.tag == "Enemy")
        {

            transform.localScale -= new Vector3(Shring, Shring, Shring);


        }

    }          
    }

