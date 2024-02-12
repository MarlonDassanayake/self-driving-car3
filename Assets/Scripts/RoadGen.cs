using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{
    public Transform startPos;
    public GameObject[] PrefabsRoad; // Hold various type of roads
    public Vector3 prevPos;
    int count = 0;
    string exitDirection = "North";
    string oldExitDirection = "North";
    int nextPlanePosition;
    float xDisplacement;
    float zDisplacement;
    int rotation;


    void Start()
    {
        prevPos = startPos.position;
    }
    void Update()
    {
        if (count > 100) return;

        switch (exitDirection)
        {
            case "North":
                nextPlanePosition = Random.RandomRange(0, 3);
                break;
            case "West":
                nextPlanePosition = Random.RandomRange(3, 5);
                break;
            case "East":
                nextPlanePosition = Random.RandomRange(4, 6);
                break;
        }

        switch (nextPlanePosition)
        {
            case 0:
                if (oldExitDirection=="West")
                {
                    xDisplacement = -32.5f;
                }
                else
                {
                    xDisplacement = 20f;
                }
                zDisplacement = 42.5f;
                rotation = 0;
                oldExitDirection = exitDirection;
                exitDirection = "North" ;
                break;
            case 1:
                if (oldExitDirection=="West")
                {
                    xDisplacement = -52.5f;
                }
                else
                {
                    xDisplacement = 0f;
                }
                zDisplacement = 0f;
                rotation = -90;
                oldExitDirection = exitDirection;
                exitDirection = "West" ;
                break;
            case 2:
                if (oldExitDirection=="West")
                {
                    xDisplacement = 0f;
                }
                else
                {
                    xDisplacement = 52.5f;
                }
                zDisplacement = 0f;
                rotation = 180;
                oldExitDirection = exitDirection;
                exitDirection = "East" ;
                break;
            case 3:
                if (oldExitDirection=="West")
                {
                    xDisplacement = -42.5f;
                    zDisplacement = 20f;
                }
                else
                {
                    xDisplacement = 0f;
                    zDisplacement = 52.5f;
                }
                rotation = 90;
                oldExitDirection = exitDirection;
                exitDirection = "North" ;
                break;
            case 4:
                if (exitDirection=="West")
                {
                    if (oldExitDirection=="West")
                    {
                        xDisplacement = -42.5f;
                        zDisplacement = 20f;
                    }
                    else
                    {
                        xDisplacement = 0f;
                        zDisplacement = 52.5f;
                    }
                    oldExitDirection = exitDirection;
                    exitDirection = "West" ;
                }
                else
                {
                    if (oldExitDirection=="East")
                    {
                        xDisplacement = 42.5f;
                        zDisplacement = -20f;
                    }
                    else
                    {
                        xDisplacement = 42.5f;
                        zDisplacement = 32.5f;
                    }
                    oldExitDirection = exitDirection;
                    exitDirection = "East" ;
                }
                rotation = 90;
                break;
            case 5:
                if (oldExitDirection=="East")
                {
                    xDisplacement = 0f;
                    zDisplacement = 0f;
                }
                else
                {
                    xDisplacement = 0f;
                    zDisplacement = 52.5f;
                }
                rotation = 0;
                oldExitDirection = exitDirection;
                exitDirection = "North" ;
                break;

        }

        Vector3 currentPos = new Vector3(prevPos.x + xDisplacement, prevPos.y, prevPos.z + zDisplacement);
        
        Instantiate(
            PrefabsRoad[nextPlanePosition],
            currentPos,
            Quaternion.Euler(0, rotation, 0)
        );
        prevPos = currentPos;
        count++;

    }
}