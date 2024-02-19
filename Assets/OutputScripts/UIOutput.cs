using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOutput : MonoBehaviour
{
    public Text outputAcceleration;
    public Text outputSteering;
    public Text outputRuntime;
    
    public float time1 = 0.0f;
    public float time2 = 0.0f;
    public Stack runtimesStack = new Stack();
    public List<float> runtimesList = new List<float>();

    public bool forwardButtonClicked = false;
    public int backClickCount = 0;
    public int forwardClickCount = 0;
    public Stack runtimesStackCopy = new Stack();
    public Text outputPastRuntimes;
    public float lastPopped = 0.0f;
    
    public int lastElement = 0;
    

    void Update()
    {
        outputAcceleration.text = FindObjectOfType<GeneticAlgorithm>().controllers[0].vehicleAccelerationValue.ToString();
        outputSteering.text = FindObjectOfType<GeneticAlgorithm>().controllers[0].vehicleSteeringDirection.ToString();
        outputRuntime.text = FindObjectOfType<GeneticAlgorithm>().controllers[0].elapsedTime.ToString();

        if (backClickCount==0 && forwardClickCount==0)
        {
            outputPastRuntimes.text = FindObjectOfType<GeneticAlgorithm>().controllers[0].elapsedTime.ToString();
        }

        time1 = time2;
        time2 = FindObjectOfType<GeneticAlgorithm>().controllers[0].elapsedTime;

        if (time1>time2)
        {
            runtimesStack.Push(time1);
            runtimesList.Add(time1);
        }

        try
        {
            if (forwardButtonClicked)
            {
                lastElement = runtimesList.IndexOf(lastPopped);
                outputPastRuntimes.text = runtimesList[lastElement-backClickCount+forwardClickCount].ToString();
            }

        }
        catch
        {}

    }

    public void BackClick()
    {
        if (backClickCount==0)
        {
            runtimesStackCopy = runtimesStack;
        }
        if (!forwardButtonClicked)
        {
            lastPopped = runtimesStackCopy.Pop();
            outputPastRuntimes.text = lastPopped.ToString();
        }
        backClickCount++;
    }

    public void ForwardClick()
    {
        forwardClickCount++;
        if (!forwardButtonClicked)
        {
            backClickCount = 0;
            forwardButtonClicked = true;
        }
    }
   
   public class Stack
    {
        static int MAXSize = 1000000;
        public int top;
        int currentItemIndex;
        float[] stack = new float[MAXSize];
 
        public bool IsEmpty()
        {
            return top == -1;
        }
 
        public Stack()
        {
            top = -1;
        }
 
        public void Push (float item)
        {
            stack[++top] = item;
        }
 
        public float Pop()
        {
            if (top == -1)
            {
                // Would "Stack Underflow", 
                // but instead just return last thing;
                return stack[top+1];
            }
            else
            {
                return stack[top--];
            }
        }
           
    }
   
}

