using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Acc1Output : CarController
{
    public Text outputAcceleration;
    public Text outputSteering;
    public Text outputRuntime;
    
    void Update()
    {
        outputAcceleration.text = a.ToString();
        outputSteering.text = t.ToString();
        outputRuntime.text = timeSinceStart.ToString();

    }
   
   
}

