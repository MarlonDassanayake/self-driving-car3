using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticManager : MonoBehaviour
{
    [Header("References")]

    // Define a car controller object
    public CarController controller; // old BUT OK
    public CarController[] controllers; // old BUT OK

    [Header("Controls")]
    public int initialPopulation = 85;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.055f;

    [Header("Crossover Controls")]
    public int bestAgentSelection = 8;
    public int worstAgentSelection = 3;
    public int numberToCrossover;

    // Create a list of integers to represent the gene pool (the networks that are selected)
    private List<int> genePool = new List<int>(); // old BUT OK
    
    private int naturallySelected; // A counter

    // Create an array of neural networks to represent the popultation
    private NNet[] population;  // old but OK

    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome = 0;

    private void Start()
    {
        CreatePopulation();
    }

    private void CreatePopulation()
    {
        // Fetch all of the car controllers.
        controllers = FindObjectsOfType<CarController>();
        population = new NNet[initialPopulation];       // old but ok
        FillPopulationWithRandomValues(population, 0);  // old but ok

        ResetToCurrentGenome();
    }

    private void ResetToCurrentGenome()
    {
        //controller.ResetWithNetwork(population[currentGenome]); //old but ok
        foreach(CarController car in controllers)
            car.ResetWithNetwork(population[currentGenome]);
    }

    // Polymorphism - same method name with different signature.
    private void ResetToCurrentGenome(CarController car)
    {
        car.ResetWithNetwork(population[currentGenome]); //old but ok
    }

    // generated a random population
    private void FillPopulationWithRandomValues (NNet[] newPopulation, int startingIndex)   
    {
        while (startingIndex < initialPopulation)
        {
            newPopulation[startingIndex] = (new GameObject().AddComponent<NNet>());
            newPopulation[startingIndex].Initialise(controller.LAYERS, controller.NEURONS);
            startingIndex++;
        }
    }

    public void Death (float fitness, NNet network, CarController car)     // OK
    {

        if (currentGenome < population.Length -1)
        {

            population[currentGenome].fitness = fitness;
            currentGenome++;
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
        currentGeneration++;
        naturallySelected = 0;
        MergeSortPopulation(population, 0, population.Length - 1);

        NNet[] newPopulation = PickBestPopulation();

        Crossover(newPopulation);
        Mutate(newPopulation);

        FillPopulationWithRandomValues(newPopulation, naturallySelected);

        population = newPopulation;

        currentGenome = 0;

        ResetToCurrentGenome();

    }

    private void Mutate (NNet[] newPopulation)
    {

        // Randomly change 'mutate' the weights of some neural networks - based on the mutation rate

        for (int i = 0; i < naturallySelected; i++) 
        {

            for (int c = 0; c < newPopulation[i].weights1.Count; c++)
            {

                if (Random.Range(0.0f, 1.0f) < mutationRate)
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

    private void Crossover (NNet[] newPopulation)
    {
        for (int i = 0; i < numberToCrossover; i+=2)
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

            NNet Child1 = (new GameObject().AddComponent<NNet>());
            NNet Child2 = (new GameObject().AddComponent<NNet>());

            Child1.Initialise(controller.LAYERS, controller.NEURONS);
            Child2.Initialise(controller.LAYERS, controller.NEURONS);

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

    private NNet[] PickBestPopulation()
    {
        // Create a temporary array to use in this subroutine
        NNet[] newPopulation = new NNet[initialPopulation];

        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPopulation[naturallySelected] = population[i].InitialiseCopy(controller.LAYERS, controller.NEURONS);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;
            
            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }

        }

        // add selected worst neural networks to the next generation
        for (int i = 0; i < worstAgentSelection; i++)
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


    private static void MergeSortPopulation(NNet[] arr, int left, int right) // done, to be annotated
    {
        if (left < right)
        {
            int middle = (left + right) / 2;

            MergeSortPopulation(arr, left, middle);
            MergeSortPopulation(arr, middle + 1, right);

            Merge(arr, left, middle, right);
        }
    }

    private static void Merge(NNet[] arr, int left, int middle, int right)  // done, to be annotated
    {
        int n1 = middle - left + 1;
        int n2 = right - middle;

        NNet[] leftArr = new NNet[n1];
        NNet[] rightArr = new NNet[n2];

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