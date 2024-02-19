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
        Death();
    }

    private void FixedUpdate() {

        InputSensors();
        lastPosition = transform.position;


        (vehicleAccelerationValue, vehicleSteeringDirection) = network.RunNetwork(rightRayValue, straightRayValue, leftRayValue);


        MoveCar(vehicleAccelerationValue, vehicleSteeringDirection);

        elapsedTime += Time.deltaTime;

        CalculateFitness();

        //a = 0;
        //t = 0;


    }

    private void Death ()
    {
        GameObject.FindObjectOfType<GeneticAlgorithm>().ResetAfterCollision(calculatedFitnessValue, network, this);
    }

    private void CalculateFitness() {

        distanceValue += Vector3.Distance(transform.position,lastPosition);
        speedValue = distanceValue/elapsedTime;

       calculatedFitnessValue = (distanceValue*scaleFactorDistance)+(speedValue*scaleFactorSpeed)+(((rightRayValue+straightRayValue+leftRayValue)/3)*scaleFactorSensor);

        // if (elapsedTime > 20 && calculatedFitnessValue < 40) {
        //     Death();
        // }

        if (calculatedFitnessValue >= 1000)
        {
            // At this point we could save the network to a JSON
            // This is also where the network stops when the fitness is too good
            Death();
        }

    }

    private void InputSensors() {

        Vector3 a = (transform.forward+transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward-transform.right);

        Ray r = new Ray(transform.position,a);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit)) {
            rightRayValue = hit.distance/40;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = b;

        if (Physics.Raycast(r, out hit)) {
            straightRayValue = hit.distance/80;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = c;

        if (Physics.Raycast(r, out hit)) {
            leftRayValue = hit.distance/40;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

    }

    private Vector3 inp;
    public void MoveCar (float v, float h) {
        inp = Vector3.Lerp(Vector3.zero,new Vector3(0,0,v*11.4f),0.02f);
        inp = transform.TransformDirection(inp);
        transform.position += inp;

        if (transform.position.y > 52.5f)
        {
            transform.position = new Vector3(transform.position.x, 52.5f, transform.position.z);
        }

        transform.eulerAngles += new Vector3(0, (h*90)*0.02f,0);
    }

}