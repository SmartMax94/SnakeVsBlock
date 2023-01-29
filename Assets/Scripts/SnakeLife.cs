using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeLife : MonoBehaviour
{
    public int ScoreSnake = 0;

    int TimeSpeed = 10;
    int Buff = 0;
    public GameObject SnakeBody;
    List<GameObject> BodySnake = new List<GameObject>();
    public Vector2 DirectionMod;
    float SpeedMove = 3;

    public
        void AddChank()
    {


        Vector3 Position = this.transform.position;
        if (BodySnake.Count > 0)
        {


            Position = BodySnake[BodySnake.Count - 1].transform.position;
        }
        Position.y+=0.5f;
        GameObject Body = Instantiate(SnakeBody, Position, Quaternion.identity) as GameObject;


        BodySnake.Add(Body);

    }
    void SnakeStep()
    {


        if ((DirectionMod.x != 0) || (DirectionMod.y != 0))
        {
            Rigidbody ComponentRig = GetComponent<Rigidbody>();
            ComponentRig.velocity = new Vector3(DirectionMod.x * SpeedMove, DirectionMod.y * SpeedMove, 0);
            if (BodySnake.Count > 0)
            {
                BodySnake[0].transform.position = transform.position;

                for (int BodyIndex = BodySnake.Count - 1; BodyIndex > 0; BodyIndex--)
                    BodySnake[BodyIndex].transform.position = BodySnake[BodyIndex - 1].transform.position;


            }
        }
    }

    [System.Obsolete]
    public void SnakeDestroy()
    {

        DirectionMod = new Vector2(0, 0);
        //убиваем хвост
        foreach (GameObject o in BodySnake) DestroyObject(o.gameObject);
        //убей голову
        DestroyObject(this.gameObject);

    }

    void Start()
    {
        BodySnake.Clear();

        for (int I = 0; I < 3; I++) AddChank();
    }

    // Update is called once per frame
    void Update()
    {
        Buff++;
        if (Buff > TimeSpeed)
        {
            SnakeStep();
            Buff = 0;
        }
    }
}