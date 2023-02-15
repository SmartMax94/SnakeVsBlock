using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A library of classes to be used by the other scripts. */

//The brains of the snake.
[System.Serializable]
public class NeuralNet {

    //Number of hidden layers
    public int hiddenLayers;

    //Nodes per hidden layer
    public int hiddenNodes;

    //Number of input nodes
    public int inputNodes;

    //Number of output nodes
    public int outputNodes;

    //Weights Matrix
    public List<WeightMatrix> wms = new List<WeightMatrix>();

    //Rate of mutation
    public float mutationRate;

    //How many moves can the snake make without food before dying ?
    public int movesWithoutFood;

    //Initializing network
    public NeuralNet (SnakeManager sm) {
        
        hiddenLayers = sm.netConfig.hiddenLayers;
        hiddenNodes = sm.netConfig.hiddenNodes;
        inputNodes = sm.netConfig.inputNodes;
        outputNodes = sm.netConfig.outputNodes;
        mutationRate = sm.netConfig.mutationRate;
        movesWithoutFood = sm.movesWithoutFood;

        //Adding input Matrix
        wms.Add (new WeightMatrix (hiddenNodes, inputNodes + 1));

        //Adding hidden layer matricies
        for (int i = 0; i < hiddenLayers; i++) {
            wms.Add (new WeightMatrix (hiddenNodes, hiddenNodes + 1));
        }

        //Adding output matrix
        wms.Add (new WeightMatrix (outputNodes, hiddenNodes + 1));

        //Set random weights
        foreach (WeightMatrix rWM in wms) {
            CoreFunctions.core.setRandomWeights (rWM, new Vector2 (-1f, 1f));
        }

    }
    

}

//A class used when exporting matricies.
//Unity and xml don't allow for exporting classes with constructos and multi dimensional arrays, thus, this class is being used to accomodate the data form the "WeightMatrix" class in order for it to be exported.
public class WeightMatrixExport {
    public float[] matrix;
    public int rows;
    public int columns;
}

//Weights matrix template.
[System.Serializable]
public class WeightMatrix {

    public float[,] matrix;
    public int rows;
    public int columns;

    //Constructor
    public WeightMatrix (int r, int c) {

        rows = r;
        columns = c;
        matrix = new float [rows, columns];

    }

 }

//Snake population template.
[System.Serializable]
public class SnakePopulation {

    public Snake[] snakes;
    public Snake bestSnake;

    public int populationSize;
    public float bestScore;
    public int currentGen;

    //Constructor
    public SnakePopulation (int size, SnakeManager sm) {

        //Initializing snake population
        populationSize = size;
        snakes = new Snake [populationSize];

        //Assigning a new snake to each spot in the array
        for (int i = 0; i < snakes.Length; i++) {
            snakes[i] = new Snake (sm);
        }

    }

}


//Individual snake template
[System.Serializable]
public class Snake {

    //How many fruits has the snake eaten?
    public int size;

    //How well is the snake doing overall?
    public float fitness;

    //How many moves does it have until it dies?
    public int movesLeft;

    //Is the snake dead?
    public bool dead;

    //In which direction is the snake currently facing?
    public int direction;

    //Current position
    public Vector2 currentPosition;

    //Fruit position
    public Vector2 fruitPosition;

    //The tiles occupied by the snake
    public List<Vector2> occupiedTiles = new List<Vector2>();

    //Snake's previous coordinates
    public List<List<Vector2>> replayCoordinates = new List<List<Vector2>>();
    public List<Vector2> fruitReplayCoordinates = new List<Vector2>();

    //The neural network resposible for the snake
    public NeuralNet nn;

    public SnakeManager sm;

    public string cause;

    //Constructor
    public Snake (SnakeManager sm) {

        //Creating a new neural network for our snake
        NeuralNet n = new NeuralNet (sm);

        nn = n;

        size = 2;
        fitness = 0;
        dead = false;
        movesLeft = SnakeManager.core.movesWithoutFood;

        direction = 1;
        currentPosition = SnakeManager.core.Grid [1000];
        
        var newDir = 0;

        switch (direction) {
            case 0:
                newDir = 1;
                break;
            case 1:
                newDir = 0;
                break;
            case 2:
                newDir = 3;
                break;
            default:
                newDir = 2;
                break;
        }

        var t1 = SnakeManager.core.nextPosition (newDir, currentPosition);
        var t2 = SnakeManager.core.nextPosition (newDir, t1);

        occupiedTiles.Add (t1);
        occupiedTiles.Add (t2);

        fruitPosition = SnakeManager.core.Grid [Random.Range (0, SnakeManager.core.Grid.Count)];

        //Making sure that the fruit didn't spawn on the snake
        while (fruitPosition == currentPosition) {
            fruitPosition = SnakeManager.core.Grid [Random.Range (0, SnakeManager.core.Grid.Count)];
        }

    }

}

//Used to store all the config value of the neural nets (all neural nets are sharing the same config values).
//An instance of this function can be accessed via the inspector from SnakeManager.cs.
[System.Serializable]
public class NetConfig {
    public int hiddenLayers, hiddenNodes, inputNodes, outputNodes;
    public Vector2 randomWeightLimits;
    public float mutationRate;
}
