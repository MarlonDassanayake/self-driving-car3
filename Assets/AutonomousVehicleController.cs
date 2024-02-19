using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NeuralNetwork))]
public class AutonomousVehicleController : MonoBehaviour
{
    private Vector3 startPosition, startRotation;
    private NeuralNetwork network;

    public float vehicleAccelerationValue, vehicleSteeringDirection;

    public float elapsedTime = 0f;

    public int layerCount = 1;
    public int neuronCount = 10;

    public float scaleFactorSensor = 0.1f;
    public float scaleFactorSpeed = 0.2f;
    public float scaleFactorDistance = 1.4f;
    public float calculatedFitnessValue;

    private Vector3 lastPosition;
    public float distanceValue;
    public float speedValue;

    public float rightRayValue,straightRayValue,leftRayValue;

    private void Awake() {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        network = GetComponent<NeuralNetwork>();

        
    }

    public void ResetWithNetwork (NeuralNetwork net)
    {
        network = net;
        Reset();
    }

    

    public void Reset() {

        elapsedTime = 0f;
        distanceValue = 0f;
        speedValue = 0f;
        lastPosition = startPosition;
        calculatedFitnessValue = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    private void OnCollisionEnter (Collision collision) {
        GameObject.FindObjectOfType<GeneticAlgorithm>().ResetAfterCollision(calculatedFitnessValue, network, this);
    }

    private void FixedUpdate() {

        GetSensorValues();
        lastPosition = transform.position;


        (vehicleAccelerationValue, vehicleSteeringDirection) = network.RunNetwork(rightRayValue, straightRayValue, leftRayValue);


        VehicleDriver(vehicleAccelerationValue, vehicleSteeringDirection);

        elapsedTime += Time.deltaTime;

        CalculateFitness();


    }

    private void CalculateFitness() {

        distanceValue += Vector3.Distance(transform.position,lastPosition);
        speedValue = distanceValue/elapsedTime;

       calculatedFitnessValue = (distanceValue*scaleFactorDistance)+(speedValue*scaleFactorSpeed)+(((rightRayValue+straightRayValue+leftRayValue)/3)*scaleFactorSensor);

        if (calculatedFitnessValue >= 1000)
        {
            // At this point we could save the network to a JSON
            // This is also where the network stops when the fitness is too good
            GameObject.FindObjectOfType<GeneticAlgorithm>().ResetAfterCollision(calculatedFitnessValue, network, this);;
        }

    }

    private void GetSensorValues() {
        // Define ray directions for right, straight, and left
        Vector3[] directions = { 
            transform.forward + transform.right, 
            transform.forward, 
            transform.forward - transform.right 
        };

        // Define ray lengths corresponding to each direction
        float[] rayLengths = { 40f, 80f, 40f };

        // Perform raycasting for each direction
        for (int i = 0; i < directions.Length; i++) {
            Ray ray = new Ray(transform.position, directions[i]);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                float standardDistance = hit.distance / rayLengths[i];
                Debug.DrawLine(ray.origin, hit.point, Color.red);
                
                // Assign the distance value based on direction
                switch (i) {
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
        Vector3 movement = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, vehicleAcceleration * 11.4f), 0.02f);
        movement = transform.TransformDirection(movement);
        transform.position += movement;

        if (transform.position.y > 52.5f) {
            transform.position = new Vector3(transform.position.x, 52.5f, transform.position.z);
        }

        transform.eulerAngles += new Vector3(0, (vehicleSteering * 90) * 0.02f, 0);
    }


}