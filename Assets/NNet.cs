using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;
using System;

using Random = UnityEngine.Random;

public class NNet : MonoBehaviour
{

    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 3); // old

    // Initialise input layer as 0 matrix
    public List<List<float>> inputLayer1 = CreateMatrix(1,3);

    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>(); // old

    // Declare hidden layers as a list of matrices
    public List<List<List<float>>> hiddenLayers1 = new List<List<List<float>>>();


    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2); // old

    // Initialise output layer as 0 matrix
    public List<List<float>> outputLayer1 = CreateMatrix(1,2);

    public List<Matrix<float>> weights = new List<Matrix<float>>(); //old

    // Declare weights as a list of matrices
    public List<List<List<float>>> weights1 = new List<List<List<float>>>();

    public List<float> biases = new List<float>(); // old

    // Decare biases as a list of floats
    public List<float> biases1 = new List<float>();

    // Declare fitness variable
    public float fitness;

    // Create a nested list structure containing 0 in every element to represent a 0 matrix

    public static List<List<float>> CreateMatrix(int rows, int columns)
    {
        List<List<float>> matrix = new List<List<float>>();
        for (int i = 0; i < rows; i++)
        {
            List<float> row = new List<float>();

            for (int j = 0; j < columns; j++)
            {
                row.Add(0.0f);
            }
            matrix.Add(row);
        }

        return matrix;
    }

    // Reset a matrix so every element is a 0

    public static void ResetMatrix(List<List<float>> matrix)
    {
        for (int i = 0; i < matrix.Count; i++)
        {
            for (int j = 0; j < matrix[i].Count; j++)
            {
                matrix[i][j] = 0.0f;
            }
        }
    }

    // Reset a list of matrices so every element is a 0
    // This subroutine may be obsolete (it's not used I think)

    public static void ResetListOfMatrices(List<List<List<float>>> listOfMatrices)
    {

        for (int z = 0; z < listOfMatrices.Count; z++)
        {
            ResetMatrix(listOfMatrices[z]);
        }
    }


    public void Initialise (int hiddenLayerCount, int hiddenNeuronCount)
    {

        inputLayer.Clear();   // old
        hiddenLayers.Clear(); // old
        outputLayer.Clear();  // old
        weights.Clear();      // old
        biases.Clear();       // old

        // Reset each component of the nerual network
        // so that each element in input and output layer matrices are 0
        // and the biases, weights and hiddenLayers are empty

        ResetMatrix(inputLayer1);
        ResetMatrix(outputLayer1);
        biases1.Clear();
        weights1.Clear();
        hiddenLayers1.Clear();

        // Create the hidden layers 
        // (populate, the required number of hidden layers with values)
        for (int i = 0; i <= hiddenLayerCount; i++)
        {

            Matrix<float> f = Matrix<float>.Build.Dense(1, hiddenNeuronCount); //old
            hiddenLayers.Add(f);                                               //old
            biases.Add(Random.Range(-1f, 1f));                                 //old

            // Create neurons for each indivdual hidden layer
            List<List<float>> individualHiddenLayer = CreateMatrix(1,hiddenNeuronCount);
            hiddenLayers1.Add(individualHiddenLayer);
            biases1.Add(Random.Range(-1f, 1f));  

            // Weight matrix is configured so that the first hidden layer
            // holds the correct amount of weights (to be suitable with the input layer)
            if (i == 0)
            {
                Matrix<float> inputToH1 = Matrix<float>.Build.Dense(3, hiddenNeuronCount); // old
                weights.Add(inputToH1);                                                    // old

                List<List<float>> inputLayerToHidden1 = CreateMatrix(3, hiddenNeuronCount);
                weights1.Add(inputLayerToHidden1);    
            }

            Matrix<float> HiddenToHidden = Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount); // old
            weights.Add(HiddenToHidden);                                                                // old

            List<List<float>> HiddenLayersLink = CreateMatrix(hiddenNeuronCount, hiddenNeuronCount);
            weights1.Add(HiddenLayersLink); 
        }

        Matrix<float> OutputWeight = Matrix<float>.Build.Dense(hiddenNeuronCount, 2); // old
        weights.Add(OutputWeight);                                                  // old
        biases.Add(Random.Range(-1f, 1f));                                          // old

        List<List<float>> weightForOutputLayer = CreateMatrix(hiddenNeuronCount, 2);
        weights1.Add(weightForOutputLayer); 
        biases1.Add(Random.Range(-1f, 1f));  

        RandomiseAllWeights();                 // adapted, done (+ name changed done)

    }

    public NNet InitialiseCopy (int hiddenLayerCount, int hiddenNeuronCount)
    {
        NNet n = (new GameObject().AddComponent<NNet>());           // old
        NNet newNetwork = (new GameObject().AddComponent<NNet>());

        List<Matrix<float>> newWeights = new List<Matrix<float>>(); // old
        
        // Create a new list of matrices to represent the new weights
        List<List<List<float>>> newWeights1 = new List<List<List<float>>>();

        for (int i = 0; i < this.weights.Count; i++)        // old loop
        {
            Matrix<float> currentWeight = Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);

            for (int x = 0; x < currentWeight.RowCount; x++)
            {
                for (int y = 0; y < currentWeight.ColumnCount; y++)
                {
                    currentWeight[x, y] = weights[i][x, y];
                }
            }

            newWeights.Add(currentWeight);
        }

        // Loop through the currents weights in this Neural network class and assign these to the new list of weights
        // I THINK ADDRANGE() CAN BE USED TO DO THIS! , or see comment on vd 3 'bravo'
        for (int i = 0; i < this.weights1.Count; i++)        
        {
            List<List<float>> currentWeight1 = CreateMatrix(weights1[i].Count, weights1[i][0].Count);

            for (int x = 0; x < currentWeight1.Count; x++)
            {
                for (int y = 0; y < currentWeight1[0].Count; y++)
                {
                    currentWeight1[x][y] = weights1[i][x][y];
                }
            }

            newWeights1.Add(currentWeight1);
        }

        List<float> newBiases = new List<float>(); //old
        newBiases.AddRange(biases); //old
        n.weights = newWeights; //old
        n.biases = newBiases;   //old

        // Initialise biases
        List<float> newBiases1 = new List<float>();
        newBiases1.AddRange(biases1);
        newNetwork.weights1 = newWeights1;
        newNetwork.biases1 = newBiases1;
        newNetwork.weights = newWeights; // maybe fix
        newNetwork.biases = newBiases;      // maybe fix

        n.weights1 = newWeights1;
        n.biases1 = newBiases1;

        n.InitialiseHidden(hiddenLayerCount, hiddenNeuronCount); // old

        newNetwork.InitialiseHidden(hiddenLayerCount, hiddenNeuronCount); 

        // return n;               // to be repalced with return newNetwork.
        return newNetwork;
    }

    public void InitialiseHidden (int hiddenLayerCount, int hiddenNeuronCount)
    {
        inputLayer.Clear();     // old
        hiddenLayers.Clear();   // old
        outputLayer.Clear();    // old

        // Reset input layer, output layer and hidden layers.
        ResetMatrix(inputLayer1);
        ResetMatrix(outputLayer1);
        hiddenLayers1.Clear();

        // Copy the hidden layers
        for (int i = 0; i < hiddenLayerCount + 1; i ++)
        {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNeuronCount); // old
            hiddenLayers.Add(newHiddenLayer);                                               // old

            List<List<float>> newHiddenLayer1 = CreateMatrix(1, hiddenNeuronCount);
            hiddenLayers1.Add(newHiddenLayer1);

        }

    }

    public void RandomiseAllWeights()
    {

        for (int i = 0; i < weights.Count; i++)                 // old loop
        {
            for (int x = 0; x < weights[i].RowCount; x++)
            {
                for (int y = 0; y < weights[i].ColumnCount; y++)
                {
                    weights[i][x, y] = Random.Range(-1f, 1f);
                }
            }
        }

        // Sets a random value in each element of each weight matrix

        //foreach (var matrix in weights1)
        //{
        //    foreach (var row in matrix)
        //    {
        //        for (int i = 0; i < row.Count; i++)
        //        {
        //            row[i] = Random.Range(-1f, 1f);
        //        }
        //    }
        //}

        for (int i = 0; i < weights1.Count; i++)                 
        {
            for (int x = 0; x < weights1[i].Count; x++)
            {
                for (int y = 0; y < weights1[i][0].Count; y++)
                {
                    weights1[i][x][y] = Random.Range(-1f, 1f);
                }
            }
        }

    }

    // Function runs the neural network. It returns to values - acceleration and steering.
    public (float, float) RunNetwork (float a, float b, float c)
    {
        inputLayer[0, 0] = a;       // old
        inputLayer[0, 1] = b;      // old
        inputLayer[0, 2] = c;      // old

        // Values of input layer set to input sensor values
        inputLayer1[0][0] = a;     
        inputLayer1[0][1] = b;      
        inputLayer1[0][2] = c;   

        inputLayer = inputLayer.PointwiseTanh(); // old

        // Tanh used as we would like our output values for steering to be between -1 and 1.
        // It is an activation function to create non-linearity within the neural network 
        // (and so minimise errors).
        // It is also used to ensure that no data is lost throughout propogation.

        HyperbolicTangent(inputLayer1);

        hiddenLayers[0] = ((inputLayer * weights[0]) + biases[0]).PointwiseTanh(); // old

        //test line
        List<List<float>> testerTester = MultiplyMatrices(inputLayer1, weights1[0]);

        // Values for the first hidden layer are created
        hiddenLayers1[0] = MultiplyMatrices(inputLayer1, weights1[0]);
        AddBias(hiddenLayers1[0], biases1[0]);
        HyperbolicTangent(hiddenLayers1[0]);

        // Old loop
        for (int i = 1; i < hiddenLayers.Count; i++) 
        {
            hiddenLayers[i] = ((hiddenLayers[i - 1] * weights[i]) + biases[i]).PointwiseTanh(); 
        }

        // Assign values for each hidden layer
        for (int i = 1; i < hiddenLayers1.Count; i++)
        {
            hiddenLayers1[i] = MultiplyMatrices(hiddenLayers1[i-1], weights1[i]);
            AddBias(hiddenLayers1[i], biases1[i]);
            HyperbolicTangent(hiddenLayers1[i]);
        }

        outputLayer = ((hiddenLayers[hiddenLayers.Count-1]*weights[weights.Count-1])+biases[biases.Count-1]).PointwiseTanh(); //old

        // Assign the output layer
        outputLayer1 = MultiplyMatrices(hiddenLayers1[hiddenLayers1.Count-1], weights1[weights1.Count-1]);
        AddBias(outputLayer1, biases1[biases1.Count-1]);
        HyperbolicTangent(outputLayer1);

        // Return the output values
        // Outputs are acceleration and steering
        //return (Sigmoid(outputLayer[0,0]), (float)Math.Tanh(outputLayer[0,1])); // old

        // UNCOMMENT LATER
        return (Sigmoid(outputLayer1[0][0]), (float)(Mathf.Exp(outputLayer1[0][1]) - Mathf.Exp(-outputLayer1[0][1]))/(Mathf.Exp(outputLayer1[0][1]) + Mathf.Exp(-outputLayer1[0][1])));

    }

    public static void AddBias(List<List<float>> matrix, float bias)
    {
        for (int i = 0; i < matrix.Count; i++)
        {
            for (int j = 0; j < matrix[i].Count; j++)
            {
                matrix[i][j] = matrix[i][j] + bias;
            }
        }
    }

    public static List<List<float>> MultiplyMatrices(List<List<float>> matrixA, List<List<float>> matrixB)
    {

        // Check if matrices can be multiplied
        if (matrixA[0].Count != matrixB.Count)
        {
            throw new ArgumentException("Matrix multiplication is not defined for the dimensions.");
        }
    
        List<List<float>> resultMatrix = new List<List<float>>(matrixA.Count);
    
        // Calculate the value of each element as a result of matrix multiplication
        for (int i = 0; i < matrixA.Count; i++)
        {
            resultMatrix.Add(new List<float>(matrixB[0].Count));
            for (int j = 0; j < matrixB[0].Count; j++)
            {
                float currentSum = 0.0f;
                for (int k = 0; k < matrixA[0].Count; k++)
                {
                    currentSum += matrixA[i][k] * matrixB[k][j];
                }
                resultMatrix[i].Add(currentSum);
            }
        }
    
        return resultMatrix;
    }

    public static List<List<float>> AddMatrices(List<List<float>> matrixA, List<List<float>> matrixB)   // obsolete
    {

        // Check if matrices can be added
        if ((matrixA.Count != matrixB.Count)||(matrixA[0].Count != matrixB[0].Count))
        {
            throw new ArgumentException("Matrix addition is not defined for the dimensions.");
        }
    
        List<List<float>> resultMatrix = CreateMatrix(matrixA.Count, matrixA[0].Count);
    
        // Calculate the value of each element as a result of matrix addition
        for (int i = 0; i < matrixA.Count; i++)
        {
            for (int j = 0; j < matrixA[0].Count; j++)
            {
                resultMatrix[i][j] = matrixA[i][j]+matrixB[i][j];
            }
        }
    
        return resultMatrix;
    }

    // Applies the tanh activation function to every element in a 2D matrix
    public static void HyperbolicTangent(List<List<float>> matrix)
    {
        for (int i = 0; i < matrix.Count; i++)
        {
            for (int j = 0; j < matrix[i].Count; j++)
            {
                float element = matrix[i][j];
                matrix[i][j] = (Mathf.Exp(element) - Mathf.Exp(-element))/(Mathf.Exp(element) + Mathf.Exp(-element));
            }
        }
    }

    private float Sigmoid (float s)
    {
        return (1 / (1 + Mathf.Exp(-s)));
    }

}