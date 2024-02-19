using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using Random = UnityEngine.Random;

public class NeuralNetwork : MonoBehaviour
{

    // Initialise input layer as 0 matrix
    public List<List<float>> inputLayerMatrix = CreateMatrix(1,3);

    // Declare hidden layers as a list of matrices
    public List<List<List<float>>> hiddenLayersList = new List<List<List<float>>>();

    // Initialise output layer as 0 matrix
    public List<List<float>> outputLayerMatrix = CreateMatrix(1,2);

    // Declare weights as a list of matrices
    public List<List<List<float>>> weightsList = new List<List<List<float>>>();

    // Decare biases as a list of floats
    public List<float> biasList = new List<float>();

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


    public void CreateNeuralNetwork (int layerCount, int neuronCount)
    {

        // Reset each component of the neural network
        // so that each element in input and output layer matrices are 0
        // and the biases, weights and hiddenLayers are empty

        ResetMatrix(inputLayerMatrix);
        ResetMatrix(outputLayerMatrix);
        biasList.Clear();
        weightsList.Clear();
        hiddenLayersList.Clear();

        // Create the hidden layers 
        // (populate, the required number of hidden layers with values)
        for (int i = 0; i <= layerCount; i++)
        {

            // Create neurons for each indivdual hidden layer
            List<List<float>> individualHiddenLayer = CreateMatrix(1,neuronCount);
            hiddenLayersList.Add(individualHiddenLayer);
            biasList.Add(Random.Range(-1f, 1f));  

            // Weight matrix is configured so that the first hidden layer
            // holds the correct amount of weights (to be suitable with the input layer)
            if (i == 0)
            {
                List<List<float>> inputLayerToHidden1 = CreateMatrix(3, neuronCount);
                weightsList.Add(inputLayerToHidden1);    
            }

            List<List<float>> HiddenLayersLink = CreateMatrix(neuronCount, neuronCount);
            weightsList.Add(HiddenLayersLink); 
        }

        List<List<float>> weightForOutputLayer = CreateMatrix(neuronCount, 2);
        weightsList.Add(weightForOutputLayer); 
        biasList.Add(Random.Range(-1f, 1f));  

        SetRandomWeights();                 

    }

    public NeuralNetwork DuplicateNetwork (int layerCount, int neuronCount)
    {
        
        NeuralNetwork newNetwork = (new GameObject().AddComponent<NeuralNetwork>());
        
        // Create a new list of matrices to represent the new weights
        List<List<List<float>>> newWeights1 = new List<List<List<float>>>();

        // Loop through the currents weights in this Neural network class and assign these to the new list of weights
        for (int i = 0; i < this.weightsList.Count; i++)        
        {
            List<List<float>> currentWeight1 = CreateMatrix(weightsList[i].Count, weightsList[i][0].Count);

            for (int j = 0; j < currentWeight1.Count; j++)
            {
                for (int k = 0; k < currentWeight1[0].Count; k++)
                {
                    currentWeight1[j][k] = weightsList[i][j][k];
                }
            }

            newWeights1.Add(currentWeight1);
        }

        // Initialise biases
        List<float> newBiases1 = new List<float>();
        newBiases1.AddRange(biasList);
        newNetwork.weightsList = newWeights1;
        newNetwork.biasList = newBiases1;

        newNetwork.ResetHiddenLayers(layerCount, neuronCount); 

        // return newNetwork.
        return newNetwork;
    }

    public void ResetHiddenLayers (int layerCount, int neuronCount)
    {

        // Reset input layer, output layer and hidden layers.
        ResetMatrix(inputLayerMatrix);
        ResetMatrix(outputLayerMatrix);
        hiddenLayersList.Clear();

        // Copy the hidden layers
        for (int i = 0; i <= layerCount; i ++)
        {
            List<List<float>> newHiddenLayer1 = CreateMatrix(1, neuronCount);
            hiddenLayersList.Add(newHiddenLayer1);
        }

    }

    public void SetRandomWeights()
    {
        foreach (var matrix in weightsList)
        {
            foreach (var row in matrix)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    row[i] = Random.Range(-1f, 1f);
                }
            }
        }
    }


    // Function runs the neural network. It returns to values - acceleration and steering.
    public (float, float) ComputeNeuralNetworkOutput (float a, float b, float c)
    {

        // Values of input layer set to input sensor values
        inputLayerMatrix[0][0] = a;     
        inputLayerMatrix[0][1] = b;      
        inputLayerMatrix[0][2] = c;   


        // Tanh used as we would like our output values for steering to be between -1 and 1.
        // It is an activation function to create non-linearity within the neural network 
        // (and so minimise errors).
        // It is also used to ensure that no data is lost throughout propogation.

        HyperbolicTangent(inputLayerMatrix);

        // Values for the first hidden layer are created
        hiddenLayersList[0] = MultiplyMatrices(inputLayerMatrix, weightsList[0]);
        AddBias(hiddenLayersList[0], biasList[0]);
        HyperbolicTangent(hiddenLayersList[0]);

        // Assign values for each hidden layer
        for (int i = 1; i < hiddenLayersList.Count; i++)
        {
            hiddenLayersList[i] = MultiplyMatrices(hiddenLayersList[i-1], weightsList[i]);
            AddBias(hiddenLayersList[i], biasList[i]);
            HyperbolicTangent(hiddenLayersList[i]);
        }

        // Assign the output layer
        outputLayerMatrix = MultiplyMatrices(hiddenLayersList[hiddenLayersList.Count-1], weightsList[weightsList.Count-1]);
        AddBias(outputLayerMatrix, biasList[biasList.Count-1]);
        HyperbolicTangent(outputLayerMatrix);

        // Return the output values
        // Outputs are acceleration and steering
        return (ApplySigmoid(outputLayerMatrix[0][0]), (float)(Mathf.Exp(outputLayerMatrix[0][1]) - Mathf.Exp(-outputLayerMatrix[0][1]))/(Mathf.Exp(outputLayerMatrix[0][1]) + Mathf.Exp(-outputLayerMatrix[0][1])));

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

    private float ApplySigmoid (float s)
    {
        return (1 / (1 + Mathf.Exp(-s)));
    }

}