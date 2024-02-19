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
    public int crossoverCount;
    public float probabilityOfMutation = 0.055f;

    // Create a list of integers to represent the gene pool (the networks that are selected)
    private List<int> genePool = new List<int>(); // old BUT OK
    
    private int naturalSelectionIndex; // A counter

    // Create an array of neural networks to represent the popultation
    private NeuralNetwork[] population;  // old but OK

    private void Start()
    {
        CreatePopulation();
    }

    private void CreatePopulation()
    {
        // Fetch all of the car controllers.
        controllers = FindObjectsOfType<AutonomousVehicleController>();
        population = new NeuralNetwork[populationStartSize];       // old but ok
        FillPopulationWithRandomValues(population, 0);  // old but ok

        ResetToCurrentGenome();
    }

    private void ResetToCurrentGenome()
    {
        //controller.ResetWithNetwork(population[genomeIndex]); //old but ok
        foreach(AutonomousVehicleController car in controllers)
            car.ResetWithNetwork(population[genomeIndex]);
    }

    // Polymorphism - same method name with different signature.
    private void ResetToCurrentGenome(AutonomousVehicleController car)
    {
        car.ResetWithNetwork(population[genomeIndex]); //old but ok
    }

    // generated a random population
    private void FillPopulationWithRandomValues (NeuralNetwork[] newPopulation, int startingIndex)   
    {
        while (startingIndex < populationStartSize)
        {
            newPopulation[startingIndex] = (new GameObject().AddComponent<NeuralNetwork>());
            newPopulation[startingIndex].Initialise(controller.layerCount, controller.neuronCount);
            startingIndex++;
        }
    }

    public void Death (float fitness, NeuralNetwork network, AutonomousVehicleController car)     // OK
    {

        if (genomeIndex < population.Length -1)
        {

            population[genomeIndex].fitness = fitness;
            genomeIndex++;
            ResetToCurrentGenome(car);

        }
        else
        {
            RePopulate();
        }

    }

    
    private void RePopulate()
    {
        genePool.Clear(); // clears the networks from the previous generation
        generationIndex++;
        naturalSelectionIndex = 0;
        MergeSortPopulation(population, 0, population.Length - 1);

        NeuralNetwork[] newPopulation = SelectBestPopulation();

        PerformCrossover(newPopulation);
        Mutate(newPopulation);

        FillPopulationWithRandomValues(newPopulation, naturalSelectionIndex);

        population = newPopulation;

        genomeIndex = 0;

        ResetToCurrentGenome();

    }

    private void Mutate (NeuralNetwork[] newPopulation)
    {

        // Randomly change 'mutate' the weights of some neural networks - 
        // based on the probability of mutation

        for (int i = 0; i < naturalSelectionIndex; i++) 
        {

            for (int c = 0; c < newPopulation[i].weights1.Count; c++)
            {

                if (Random.Range(0.0f, 1.0f) < probabilityOfMutation)
                {
                    newPopulation[i].weights1[c] = ApplyMutationMatrix(newPopulation[i].weights1[c]);
                }

            }

        }

    }


    List<List<float>> ApplyMutationMatrix(List<List<float>> matrixA)
    {
        // Select a random number of values to be mutated
        int selectionRandom = Random.Range(1, (matrixA.Count * matrixA[0].Count) / 7);
        
        List<List<float>> tempMatrix = matrixA;

        // Perform mutation on random rows and columns
        for (int i = 0; i < selectionRandom; i++)
        {
            int randomRow = Random.Range(0, tempMatrix.Count);
            int randomColumn = Random.Range(0, tempMatrix[0].Count);

            tempMatrix[randomRow][randomColumn] = Mathf.Clamp(tempMatrix[randomRow][randomColumn] + Random.Range(-1f, 1f), -1f, 1f);
        }

        return tempMatrix;

    }

    private void PerformCrossover(NeuralNetwork[] newPopulation)
    {
        for (int i = 0; i < crossoverCount; i += 2)
        {
            int parentIndexA = i;
            int parentIndexB = i + 1;

            // Ensure unique parents if genePool is not empty
            if (genePool.Count >= 1)
            {
                while (parentIndexA == parentIndexB)
                {
                    parentIndexA = genePool[Random.Range(0, genePool.Count)];
                    parentIndexB = genePool[Random.Range(0, genePool.Count)];
                }
            }

            NeuralNetwork firstChild = CreateChild();
            NeuralNetwork secondChild = CreateChild();

            SwapWeightsAndBiases(firstChild, secondChild, parentIndexA, parentIndexB);

            newPopulation[naturalSelectionIndex++] = firstChild;
            newPopulation[naturalSelectionIndex++] = secondChild;
        }
    }

    private NeuralNetwork CreateChild()
    {
        NeuralNetwork child = (new GameObject().AddComponent<NeuralNetwork>());
        child.Initialise(controller.layerCount, controller.neuronCount);
        child.fitness = 0;
        return child;
    }

    private void SwapWeightsAndBiases(NeuralNetwork firstChild, NeuralNetwork secondChild, int parentIndexA, int parentIndexB)
    {
        for (int k = 0; k < firstChild.weights1.Count; k++)
        {
            if (Random.Range(0.0f, 2.0f) < 1f)
            {
                firstChild.weights1[k] = population[parentIndexA].weights1[k];
                secondChild.weights1[k] = population[parentIndexB].weights1[k];
            }
            else
            {
                secondChild.weights1[k] = population[parentIndexA].weights1[k];
                firstChild.weights1[k] = population[parentIndexB].weights1[k];
            }
        }

        for (int k = 0; k < firstChild.biases1.Count; k++)
        {
            if (Random.Range(0.0f, 2.0f) < 1f)
            {
                firstChild.biases1[k] = population[parentIndexA].biases1[k];
                secondChild.biases1[k] = population[parentIndexB].biases1[k];
            }
            else
            {
                secondChild.biases1[k] = population[parentIndexA].biases1[k];
                firstChild.biases1[k] = population[parentIndexB].biases1[k];
            }
        }
    }


    private NeuralNetwork[] SelectBestPopulation()
    {
        NeuralNetwork[] selectedPopulation = new NeuralNetwork[populationStartSize];

        SelectElitePopulation(selectedPopulation);
        SelectWeakPopulation(selectedPopulation);

        return selectedPopulation;
    }

    private void SelectElitePopulation(NeuralNetwork[] selectedPopulation)
    {
        for (int index = 0; index < eliteSelectionCount; index++)
        {
            selectedPopulation[naturalSelectionIndex] = population[index].InitialiseCopy(controller.layerCount, controller.neuronCount);
            selectedPopulation[naturalSelectionIndex].fitness = 0;
            naturalSelectionIndex++;

            int fitnessScaled = Mathf.RoundToInt(population[index].fitness * 10);

            for (int count = 0; count < fitnessScaled; count++)
            {
                genePool.Add(index);
            }
        }
    }

    private void SelectWeakPopulation(NeuralNetwork[] selectedPopulation)
    {
        for (int index = 0; index < weakSelectionAgent; index++)
        {
            int lastIndex = population.Length - 1;
            lastIndex -= index;

            int fitnessScaled = Mathf.RoundToInt(population[lastIndex].fitness * 10);

            for (int count = 0; count < fitnessScaled; count++)
            {
                genePool.Add(lastIndex);
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
            if (leftArr[i].fitness >= rightArr[j].fitness)
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