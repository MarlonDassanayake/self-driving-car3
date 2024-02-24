using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NeuralNetwork))]
public class AutonomousVehicleController : MonoBehaviour
{
    private Vector3 positionOrigin, rotationOrigin;
    private NeuralNetwork neuralNetwork;

    public float vehicleAccelerationValue, vehicleSteeringDirection;

    public float elapsedTime = 0f;

    public int layerCount = 1;
    public int neuronCount = 10;

    public float scaleFactorSensor = 0.1f;
    public float scaleFactorSpeed = 0.2f;
    public float scaleFactorDistance = 1.4f;
    public float calculatedFitnessValue;

    private Vector3 updatedPosition;
    public float distanceValue;
    public float speedValue;

    public float rightRayValue,straightRayValue,leftRayValue;

    private void Awake() 
    {
        positionOrigin = transform.position;
        rotationOrigin = transform.eulerAngles;
        neuralNetwork = GetComponent<NeuralNetwork>();
    }

    public void SetNetworkToInitialState (NeuralNetwork previousNeuralNetwork)
    {
        neuralNetwork = previousNeuralNetwork;
        SetValuesToInitialState();
    }

    

    public void SetValuesToInitialState() 
    {

        elapsedTime = 0.0f;
        distanceValue = 0.0f;
        speedValue = 0.0f;
        updatedPosition = positionOrigin;
        calculatedFitnessValue = 0.0f;
        transform.position = positionOrigin;
        transform.eulerAngles = rotationOrigin;
    }

    private void OnCollisionEnter (Collision collision) 
    {
        GameObject.FindObjectOfType<GeneticAlgorithm>().ResetAfterCollision(calculatedFitnessValue, neuralNetwork, this);
    }

    private void FixedUpdate() 
    {
        GetSensorValues();
        updatedPosition = transform.position;
        (vehicleAccelerationValue, vehicleSteeringDirection) = neuralNetwork.ComputeNeuralNetworkOutput(rightRayValue, straightRayValue, leftRayValue);
        VehicleDriver(vehicleAccelerationValue, vehicleSteeringDirection);
        elapsedTime += Time.deltaTime;
        UpdateVehicleData();
    }

    private void UpdateVehicleData() 
    {
        UpdateDistanceValue();
        UpdateSpeedValue();
        UpdateCalculatedFitnessValue();

        if (calculatedFitnessValue >= 1000)
        {
            GameObject.FindObjectOfType<GeneticAlgorithm>().ResetAfterCollision(calculatedFitnessValue, neuralNetwork, this);
        }
    }

    private void UpdateDistanceValue() 
    {
        distanceValue += Vector3.Distance(transform.position, updatedPosition);
    }

    private void UpdateSpeedValue() 
    {
        speedValue = distanceValue / elapsedTime;
    }

    private void UpdateCalculatedFitnessValue() 
    {
        float averageRayValue = (rightRayValue + straightRayValue + leftRayValue) / 3f;
        calculatedFitnessValue = (distanceValue * scaleFactorDistance) + (speedValue * scaleFactorSpeed) + (averageRayValue * scaleFactorSensor);
    }


    private void GetSensorValues() {
        // Define ray directions for right, straight, and left
        Vector3[] directions = 
        { 
            transform.forward + transform.right, 
            transform.forward, 
            transform.forward - transform.right 
        };

        // Define ray lengths corresponding to each direction
        float[] rayLengths = { 40f, 80f, 40f };

        // Perform raycasting for each direction
        for (int i = 0; i < directions.Length; i++) 
        {
            Ray ray = new Ray(transform.position, directions[i]);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) 
            {
                float standardDistance = hit.distance / rayLengths[i];
                Debug.DrawLine(ray.origin, hit.point, Color.red);
                
                // Assign the distance value based on direction
                switch (i) 
                {
                    case 0:
                        rightRayValue = standardDistance;
                        break;
                    case 1:
                        straightRayValue = standardDistance;
                        break;
                    case 2:
                        leftRayValue = standardDistance;
                        break;
                }
            }
        }
    }


    public void VehicleDriver(float vehicleAcceleration, float vehicleSteering) 
    {
        Vector3 displace = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, vehicleAcceleration * 11.4f), 0.02f);
        displace = transform.TransformDirection(displace);
        transform.position += displace;

        if (transform.position.y > 52.5f) 
        {
            transform.position = new Vector3(transform.position.x, 52.5f, transform.position.z);
        }

        transform.eulerAngles += new Vector3(0, (vehicleSteering * 90) * 0.02f, 0);
    }


}