using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{

    // Define an Autonomous Vehicle Controller object
    public AutonomousVehicleController controller; // old BUT OK
    public AutonomousVehicleController[] controllers; // old BUT OK

    public int generationIndex = 0;
    public int genomeIndex = 0;
    public int populationStartSize = 85;
    public int eliteSelectionCount = 8;
    public int weakSelectionAgent = 3;
    public int crossoverCount = 13;
    public float probabilityOfMutation = 0.055f;

    // Create a list of integers to represent the gene pool (the networks that are selected)
    private List<int> selectedNetworks = new List<int>(); // old BUT OK
    
    private int naturalSelectionIndex; // A counter

    // Create an array of neural networks to represent the popultation
    private NeuralNetwork[] vehiclePopulation;  // old but OK

    private void Start()
    {
        // Fetch all of the vehicle controllers.
        controllers = FindObjectsOfType<AutonomousVehicleController>();
        vehiclePopulation = new NeuralNetwork[populationStartSize];       // old but ok
        GenerateRandomPopulation(vehiclePopulation, 0);  // old but ok

        AutoControllerReset();
    }

    private void AutoControllerReset()
    {
        foreach(AutonomousVehicleController vehicle in controllers)
            vehicle.SetNetworkToInitialState(vehiclePopulation[genomeIndex]);
    }

    // Polymorphism - same method name with different signature.
    private void AutoControllerReset(AutonomousVehicleController vehicle)
    {
        vehicle.SetNetworkToInitialState(vehiclePopulation[genomeIndex]); //old but ok
    }

    // generated a random population
    private void GenerateRandomPopulation(NeuralNetwork[] updatedVehiclePopulation, int startingIndex)   
    {
        while (startingIndex < populationStartSize)
        {
            updatedVehiclePopulation[startingIndex] = (new GameObject().AddComponent<NeuralNetwork>());
            updatedVehiclePopulation[startingIndex].CreateNeuralNetwork(controller.layerCount, controller.neuronCount);
            startingIndex++;
        }
    }

    public void ResetAfterCollision (float fitnessValue, NeuralNetwork neuralNetwork, AutonomousVehicleController vehicle)     // OK
    {

        if (genomeIndex < vehiclePopulation.Length -1)
        {
            vehiclePopulation[genomeIndex].fitnessValue = fitnessValue;
            genomeIndex++;
            AutoControllerReset(vehicle);
        }
        else
        {
            GenerateNextPopulation();
        }

    }

    
    private void GenerateNextPopulation()
    {
        selectedNetworks.Clear(); // clears the networks from the previous generation
        generationIndex++;
        naturalSelectionIndex = 0;
        MergeSortPopulation(vehiclePopulation, 0, vehiclePopulation.Length - 1);

        NeuralNetwork[] updatedVehiclePopulation = SelectNextGeneration();

        PerformCrossover(updatedVehiclePopulation);
        PerformMutation(updatedVehiclePopulation);

        GenerateRandomPopulation(updatedVehiclePopulation, naturalSelectionIndex);

        vehiclePopulation = updatedVehiclePopulation;

        genomeIndex = 0;

        AutoControllerReset();

    }

    private void PerformMutation (NeuralNetwork[] updatedVehiclePopulation)
    {

        // Randomly change 'mutate' the weights of some neural networks - 
        // based on the probability of mutation

        for (int currentIndex = 0; currentIndex < naturalSelectionIndex; currentIndex++) 
        {

            for (int currentWeight = 0; currentWeight < updatedVehiclePopulation[currentIndex].weightsList.Count; currentWeight++)
            {

                if (Random.Range(0.0f, 1.0f) < probabilityOfMutation)
                {
                    updatedVehiclePopulation[currentIndex].weightsList[currentWeight] = ApplyMutation(updatedVehiclePopulation[currentIndex].weightsList[currentWeight]);
                }

            }

        }

    }


    List<List<float>> ApplyMutation(List<List<float>> matrixA)
    {
        // Select a random number of values to be mutated
        int selectionRandom = Random.Range(1, (matrixA.Count * matrixA[0].Count) / 7);
        
        List<List<float>> tempMatrix = matrixA;

        // Perform mutation on random rows and columns
        for (int i = 0; i < selectionRandom; i++)
        {
            int rowNumber = Random.Range(0, tempMatrix.Count);
            int elementNumber = Random.Range(0, tempMatrix[0].Count);

            tempMatrix[rowNumber][elementNumber] = Mathf.Clamp(tempMatrix[rowNumber][elementNumber] + Random.Range(-1f, 1f), -1f, 1f);
        }

        return tempMatrix;

    }

    private void PerformCrossover(NeuralNetwork[] updatedVehiclePopulation)
    {
        for (int i = 0; i < crossoverCount; i += 2)
        {
            int parentIndexA = i;
            int parentIndexB = i + 1;

            // Ensure unique parents if selectedNetworks is not empty
            if (selectedNetworks.Count >= 1)
            {
                parentIndexA = selectedNetworks[Random.Range(0, selectedNetworks.Count)];
                parentIndexB = selectedNetworks[Random.Range(0, selectedNetworks.Count)];

                while (parentIndexA == parentIndexB)
                {
                    parentIndexA = selectedNetworks[Random.Range(0, selectedNetworks.Count)];
                    parentIndexB = selectedNetworks[Random.Range(0, selectedNetworks.Count)];
                }
            }

            NeuralNetwork firstChild = CreateChild();
            NeuralNetwork secondChild = CreateChild();

            SwapWeightsAndBiases(firstChild, secondChild, parentIndexA, parentIndexB);

            updatedVehiclePopulation[naturalSelectionIndex++] = firstChild;
            updatedVehiclePopulation[naturalSelectionIndex++] = secondChild;
        }
    }

    private NeuralNetwork CreateChild()
    {
        NeuralNetwork child = (new GameObject().AddComponent<NeuralNetwork>());
        child.CreateNeuralNetwork(controller.layerCount, controller.neuronCount);
        child.fitnessValue = 0;
        return child;
    }

    private void SwapWeightsAndBiases(NeuralNetwork firstChild, NeuralNetwork secondChild, int parentIndexA, int parentIndexB)
    {
        for (int k = 0; k < firstChild.weightsList.Count; k++)
        {
            if (Random.Range(0.0f, 2.0f) < 1f)
            {
                firstChild.weightsList[k] = vehiclePopulation[parentIndexA].weightsList[k];
                secondChild.weightsList[k] = vehiclePopulation[parentIndexB].weightsList[k];
            }
            else
            {
                secondChild.weightsList[k] = vehiclePopulation[parentIndexA].weightsList[k];
                firstChild.weightsList[k] = vehiclePopulation[parentIndexB].weightsList[k];
            }
        }

        for (int k = 0; k < firstChild.biasList.Count; k++)
        {
            if (Random.Range(0.0f, 2.0f) < 1f)
            {
                firstChild.biasList[k] = vehiclePopulation[parentIndexA].biasList[k];
                secondChild.biasList[k] = vehiclePopulation[parentIndexB].biasList[k];
            }
            else
            {
                secondChild.biasList[k] = vehiclePopulation[parentIndexA].biasList[k];
                firstChild.biasList[k] = vehiclePopulation[parentIndexB].biasList[k];
            }
        }
    }


    private NeuralNetwork[] SelectNextGeneration()
    {
        NeuralNetwork[] selectedPopulation = new NeuralNetwork[populationStartSize];

        SelectStrongPopulation(selectedPopulation);
        SelectWeakPopulation(selectedPopulation);

        return selectedPopulation;
    }

    private void SelectStrongPopulation(NeuralNetwork[] selectedPopulation)
    {
        for (int index = 0; index < eliteSelectionCount; index++)
        {
            selectedPopulation[naturalSelectionIndex] = vehiclePopulation[index].DuplicateNetwork(controller.layerCount, controller.neuronCount);
            selectedPopulation[naturalSelectionIndex].fitnessValue = 0;
            naturalSelectionIndex++;

            int fitnessScaled = Mathf.RoundToInt(vehiclePopulation[index].fitnessValue * 10);

            for (int count = 0; count < fitnessScaled; count++)
            {
                selectedNetworks.Add(index);
            }
        }
    }

    private void SelectWeakPopulation(NeuralNetwork[] selectedPopulation)
    {
        for (int index = 0; index < weakSelectionAgent; index++)
        {
            int lastIndex = vehiclePopulation.Length - 1;
            lastIndex -= index;

            int fitnessScaled = Mathf.RoundToInt(vehiclePopulation[lastIndex].fitnessValue * 10);

            for (int count = 0; count < fitnessScaled; count++)
            {
                selectedNetworks.Add(lastIndex);
            }
        }
    }


    private static void MergeSortPopulation(NeuralNetwork[] arr, int left, int right) // done, to be annotated
    {
        if (left < right)
        {
            int middle = (left + right) / 2;

            MergeSortPopulation(arr, left, middle);
            MergeSortPopulation(arr, middle + 1, right);

            Merge(arr, left, middle, right);
        }
    }

    private static void Merge(NeuralNetwork[] arr, int left, int middle, int right)  // done, to be annotated
    {
        int n1 = middle - left + 1;
        int n2 = right - middle;

        NeuralNetwork[] leftArr = new NeuralNetwork[n1];
        NeuralNetwork[] rightArr = new NeuralNetwork[n2];

        for (int x = 0; x < n1; ++x)
        {
            leftArr[x] = arr[left + x];
        }
            
        for (int y = 0; y < n2; ++y)
        {
            rightArr[y] = arr[middle + 1 + y];
        }
            
        int i = 0, j = 0;

        int k = left;
        while (i < n1 && j < n2)
        {
            if (leftArr[i].fitnessValue >= rightArr[j].fitnessValue)
            {
                arr[k] = leftArr[i];
                i++;
            }
            else
            {
                arr[k] = rightArr[j];
                j++;
            }
            k++;
        }

        while (i < n1)
        {
            arr[k] = leftArr[i];
            i++;
            k++;
        }

        while (j < n2)
        {
            arr[k] = rightArr[j];
            j++;
            k++;
        }
    }

}