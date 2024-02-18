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
    
    private int naturallySelected; // A counter

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
        naturallySelected = 0;
        MergeSortPopulation(population, 0, population.Length - 1);

        NeuralNetwork[] newPopulation = PickBestPopulation();

        Crossover(newPopulation);
        Mutate(newPopulation);

        FillPopulationWithRandomValues(newPopulation, naturallySelected);

        population = newPopulation;

        genomeIndex = 0;

        ResetToCurrentGenome();

    }

    private void Mutate (NeuralNetwork[] newPopulation)
    {

        // Randomly change 'mutate' the weights of some neural networks - 
        // based on the probability of mutation

        for (int i = 0; i < naturallySelected; i++) 
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

    private void Crossover (NeuralNetwork[] newPopulation)
    {
        for (int i = 0; i < crossoverCount; i+=2)
        {
            int AIndex = i;
            int BIndex = i + 1;

            if (genePool.Count >= 1)
            {
                for (int l = 0; l < 100; l++)
                {
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];

                    if (AIndex != BIndex)
                        break;
                }
            }

            NeuralNetwork Child1 = (new GameObject().AddComponent<NeuralNetwork>());
            NeuralNetwork Child2 = (new GameObject().AddComponent<NeuralNetwork>());

            Child1.Initialise(controller.layerCount, controller.neuronCount);
            Child2.Initialise(controller.layerCount, controller.neuronCount);

            Child1.fitness = 0;
            Child2.fitness = 0;

            // Randomly swapping 'crossing over' weights in the neural network
            for (int w = 0; w < Child1.weights1.Count; w++) 
            {

                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.weights1[w] = population[AIndex].weights1[w];
                    Child2.weights1[w] = population[BIndex].weights1[w];
                }
                else
                {
                    Child2.weights1[w] = population[AIndex].weights1[w];
                    Child1.weights1[w] = population[BIndex].weights1[w];
                }

            }

            // Randomly swapping 'crossing over' biases in the neural network
            for (int w = 0; w < Child1.biases1.Count; w++) // old loop
            {

                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.biases1[w] = population[AIndex].biases1[w];
                    Child2.biases1[w] = population[BIndex].biases1[w];
                }
                else
                {
                    Child2.biases1[w] = population[AIndex].biases1[w];
                    Child1.biases1[w] = population[BIndex].biases1[w];
                }

            }

            newPopulation[naturallySelected] = Child1;
            naturallySelected++;

            newPopulation[naturallySelected] = Child2;
            naturallySelected++;

        }
    }

    private NeuralNetwork[] PickBestPopulation()
    {
        // Create a temporary array to use in this subroutine
        NeuralNetwork[] newPopulation = new NeuralNetwork[populationStartSize];

        for (int i = 0; i < eliteSelectionCount; i++)
        {
            newPopulation[naturallySelected] = population[i].InitialiseCopy(controller.layerCount, controller.neuronCount);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;
            
            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }

        }

        // add selected weak neural networks to the next generation
        for (int i = 0; i < weakSelectionAgent; i++)
        {
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }

        }

        return newPopulation;

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