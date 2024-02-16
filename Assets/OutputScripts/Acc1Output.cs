using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Acc1Output : MonoBehaviour
{
    public Text outputAcceleration;
    public Text outputSteering;
    public Text outputRuntime;
    
    void Update()
    {
        outputAcceleration.text = FindObjectOfType<GeneticManager>().controllers[0].a.ToString();
        outputSteering.text = FindObjectOfType<GeneticManager>().controllers[0].t.ToString();
        outputRuntime.text = FindObjectOfType<GeneticManager>().controllers[0].timeSinceStart.ToString();
    }
   
   
}

