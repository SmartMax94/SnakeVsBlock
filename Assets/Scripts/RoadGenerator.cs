using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    private List<GameObject> ReadyRoad = new List<GameObject>();
    [Header("Все участки дорог")]
    public GameObject[] Road;
    public bool[] roadNumbers;


    [Header("Текущая длина дороги")]
    public int currentRoadLength = 0;

    [Header("Максимальная длина дороги")]
    public int maximumRoadLength = 6;

    [Header("Дистанция между дорогами")]
    public float distanceBetweenRoads ;


    [Header("Скорость дороги")]
    public float speedRoad = 5;

    public float maximumPositionZ = -15;

    public Vector3 waitingZona = new Vector3(0, 0, -40);



    private int currentRoadNumber = -1;
    private int lastRoadNumber = -1;


    [Header("Статус генерации")]
    public string roadGenerationStatus = "Generation";

    private void FixedUpdate()
    {
        if (roadGenerationStatus == "Generation")
        {

            if (currentRoadLength != maximumRoadLength)
            {
                currentRoadNumber = Random.Range(0, Road.Length);

                if (currentRoadNumber != lastRoadNumber)
                {

                    if (currentRoadNumber < Road.Length / 2)
                    {


                        if (roadNumbers[currentRoadNumber] != true)
                        {
                            if (lastRoadNumber != (Road.Length / 2) + currentRoadNumber)
                            {
                                RoadCreation();
                            }
                            else if (lastRoadNumber == (Road.Length / 2) + currentRoadNumber && currentRoadLength == Road.Length - 1)
                            {

                                RoadCreation();


                            }

                        }


                    }

                    else if (currentRoadNumber >= Road.Length / 2)

                    {
                        if (roadNumbers[currentRoadNumber] != true) { }
                        {
                            if (lastRoadNumber != currentRoadNumber - (Road.Length / 2))
                            {
                                RoadCreation();

                            }
                            else if (lastRoadNumber == currentRoadNumber - (Road.Length / 2) && currentRoadLength == Road.Length - 1)
                            {

                                RoadCreation();


                            }
                        }


                    }

                }


            }

            MovingRoad();
            if (ReadyRoad.Count != 0)
            {
                RemovingRoad();



            }    
        }



    }

    private  void MovingRoad()
    {
foreach (GameObject readyRoad in ReadyRoad)
        {
            readyRoad.transform.localPosition -= new Vector3(0f, 0f, speedRoad * Time.fixedDeltaTime);


        }



    }

    private void RemovingRoad()
    {
if (ReadyRoad[0].transform.localPosition.z < maximumPositionZ) 
        
        
        {
            int i;
            i = ReadyRoad[0].GetComponent<Road>().number;
            roadNumbers[i] = false;
            ReadyRoad[0].transform.localPosition = waitingZona;
            ReadyRoad.RemoveAt(0);
            currentRoadLength--;
        
        
        }

    }



    private void RoadCreation()
    {
        if (ReadyRoad.Count > 0)
        {
            Road[currentRoadNumber].transform.localPosition = ReadyRoad[ReadyRoad.Count - 1].transform.position + new Vector3(0f, 0f, distanceBetweenRoads);



        }
        else if (ReadyRoad.Count == 0)
        {



            Road[currentRoadNumber].transform.localPosition = new Vector3(0f, 0f, 0f);
        }

        Road[currentRoadNumber].GetComponent<Road>().number  = currentRoadNumber;
        roadNumbers[currentRoadNumber] = true;
        lastRoadNumber= currentRoadNumber;
        ReadyRoad.Add(Road[currentRoadNumber]);

        currentRoadLength++;

    }
}

