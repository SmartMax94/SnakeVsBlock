using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A list of functions that mainly deal with matrix manipulations and computation. */

public class CoreFunctions : MonoBehaviour {

    //Creating a reference to an instance of this class. This reference will make the instance accesible from any other script, foregoing the need to access an object beforehand.
    public static CoreFunctions core;

    //After the reference has been declared, we need to assign a script instance to it at runtime.
    void Awake () {
        if (core == null) {
            core = this;
        }
    }

    //This function allows the snake to make descisions about the direction in which to move.
    public int decide (float[] inputsArray, NeuralNet nn) {

        //First, we get a list of all the inputs. Since the inputs are originally in array form, we need to convert them into a one dimensional matrix.
        WeightMatrix inputs = arrayToMatrix (inputsArray);

        //We also append a bias to the matrix that we generated.
        //This matrix will be modified during the propagation.
        WeightMatrix modified = addBias (inputs);

        //Since the input nodes have been dealt with, we proceed to dealing with the hidden layers.
        for (int i = 0; i < nn.hiddenLayers; i++) {
            //Calculating the dot product between the outputs of the previous layer and the following weights.
            WeightMatrix productWithPrevious = dot (nn.wms [i], modified);
            //Now, we need to active the resultant values. In this case we will be using RELU for activation (to be described later). You can use any activation function later on.
            WeightMatrix activated = activate (productWithPrevious);
            //After all the nodes have been activated, we simply add a bias to the resultant matrix.
            modified = addBias (activated);
        }

        //Repeating the previous process without adding a bias.
        //The resultant matrix (after activation), will be our output matrix.
        WeightMatrix outputProduct = dot (nn.wms [nn.wms.Count - 1], modified);
        WeightMatrix outputActivated = activate (outputProduct);

        //Now that we have an output matrix we need to convert it into an array so that it is easier to work with.
        //If you wish you can skip this step and keep using the output matrix instead.
        float[] weightedDirections = matrixToArray (outputActivated);

        //Deciding direction based on outputs.
        
        //The following is a simple piece of code that determines which array element has the highter value.
        //The index of the element with the highest value is returned by this function and is used as an indicator towards the direction in which the snake should move.
        float tempMax = weightedDirections [0];
        int tempDir = 0;

        for (int i = 0; i < weightedDirections.Length; i++) {

            float d = weightedDirections [i];

            if (d > tempMax) {
                tempMax = d;
                tempDir = i;
            }

        }

        return tempDir;

        /* It is worth mentioning that the index values in themselves have no meaning. The meaning is given to these values by the function which uses this method.
        In other words, you can freely decide which index value is going to correspond to a specific direction and the Neural Network will learn to act accordingly. */

    }

    //Fills a matrix with random weights. The limits of the random function depend of the "limits" parameter.
    public void setRandomWeights (WeightMatrix rWM, Vector2 limits) {

        for (int i1 = 0; i1 < rWM.rows; i1 ++) {

            for (int i2 = 0; i2 < rWM.columns; i2++) {

                rWM.matrix [i1, i2] = Random.Range (limits.x, limits.y);

            }
                
        }

    }

    //Returns the dot product of two matricies.
    public WeightMatrix dot (WeightMatrix a, WeightMatrix b) {

        //A matrix that will be containing the results of our dot multiplication.
        WeightMatrix product = new WeightMatrix (a.rows, b.columns);

        //Ensuring that the number of rows in a is equal to the number of columns in b, this is a very basic rule/requirement of "dot" multiplication.
        if (b.rows == a.columns) {

            //We will be multiplying the values of each matrix, adding up the results and saving them into our product matrix.
            //The following is a basic dot product algorithm, you can read about dot multiplication online if interested.
            for (int i1 = 0; i1 < a.rows; i1++) {

                for (int i2 = 0; i2 < b.columns; i2++) {

                    float sum = 0;

                    for (int i3 = 0; i3 < a.columns; i3 ++) {
                        sum += a.matrix [i1, i3] * b.matrix [i3, i2];
                    }
                    
                    product.matrix [i1, i2] = sum;
                }

            }

        }

        return product;

        /* It is worth mentioning that the reason why we use do multiplication is because it does exactly what we want it to do.
        Essentially, a node in a neural network is the activation of the sum of the products between all the previous nodes and their corresponding weights that connect to the node in question (plus some bias).
        The product matrix that is being returned by this function is essentially a matrix of such sums (excluding the activation and bias, as those are added/applied later). */
    }


    //Activation function.
    //This functions simply returns the greater value between zero and the one provided.
    //There are many activation functions, and you are free to use another one if you feel that it could increase the performance.
    public float relu (float num) {
        return Mathf.Max (0, num);
    }

    //Activating all nodes of the network using relu.
    //This functions simply goes through all the nodes of a matrix and applies relu to them.
    public WeightMatrix activate (WeightMatrix naWM) {

        WeightMatrix aWM = new WeightMatrix (naWM.rows, naWM.columns);

        for (int i1 = 0; i1 < naWM.rows; i1++) {

            for (int i2 = 0; i2 < naWM.columns; i2++) {

                aWM.matrix [i1, i2] = relu (naWM.matrix [i1, i2]);

            }

        }

        return aWM;

    }

    //Crossover between two snakes.
    //This function is used to create new snakes by mixing the weights values of their parents.
    //The mixing itself is done by the matrixCrossover function, this function simply assigns the result of said mixing to the child.
    public Snake snakeCrossover (Snake parent1, Snake parent2) {

        //Creating a new snake.
        Snake child = new Snake (SnakeManager.core);
        //Creatig a new neural network for our new snake (its Brain essentially).
        NeuralNet childNN = new NeuralNet (SnakeManager.core);

        //For each matrix of weights of the child snake.
        for (int i = 0; i < childNN.wms.Count; i++) {
            //Crossover the parents.
            childNN.wms [i] = matrixCrossover (parent1.nn.wms [i], parent2.nn.wms [i]);
        }

        //Assign the new neural network to the snake.
        child.nn = childNN;

        return child;


    }

    //Crossover between two matricies.
    public WeightMatrix matrixCrossover (WeightMatrix parent1, WeightMatrix parent2) {

        //We first creating a new child matrix
        WeightMatrix childMatrix = new WeightMatrix (parent1.rows, parent1.columns);

        //For each matrix element of parent 1 and 2.
        for (int i1 = 0; i1 < parent1.rows; i1++) {

            for (int i2 = 0; i2 < parent1.columns; i2++) {

                //Get a random value between 0 and 1.
                var r = Random.Range (0.0f,1.0f);

                //We will be using a threshold of 0.5 for this project, however, you can chance the threshold to whatever you want.
                //The threshold signifies the chance of either parent's genes being passed to their child.
                //For example, if the threshold is 0.2, then the chances of snake 1 of passing their genes is 20%, while the chances of snake 2 is 80%.
                var threshold = 0.5f;
                
                //You can flip <= to >, this will make snake 1 have the advantage when the threshold is lowered.
                if (r <= threshold) {
                    childMatrix.matrix [i1, i2] = parent1.matrix [i1, i2];
                } else {
                    childMatrix.matrix [i1, i2] = parent2.matrix [i1, i2];
                }

            }

        }

        return childMatrix;

    }

    //This function is used to randomly alter the weights of a given snake's matricies.
    //This function is uused alognside with crossover to introduce diversity into the snake population in order to avoid stagnation.
    //You can choose to not use this function if you feel like it is not improving our results.
    public Snake mutateSnake (Snake s) {

        //Creating a new snake which will inherit the mutated version of the original's genes.
        Snake mutated = new Snake (SnakeManager.core);
        //Creating a neural network for it.
        NeuralNet nn = new NeuralNet (SnakeManager.core);

        //For each weight matrix of the snake
        for (int i = 0; i < s.nn.wms.Count; i++) {

            //Go through each node of said matrix
            for (int i1 = 0; i1 < s.nn.wms [i].rows; i1 ++) {

                for (int i2 = 0; i2 < s.nn.wms [i].columns; i2 ++) {

                    //Getting a random value between 0 and 1
                    var r = Random.Range (0.00f, 1.00f);

                    //If the random value is lower than our mutation rate (which is basically a threshold in this case), we assign a random value to the node.
                    if (r < s.nn.mutationRate) {

                        nn.wms [i].matrix [i1, i2] = Random.Range (-1f, 1f);

                    } else {

                        //Otherwise, the node remains the same.
                        nn.wms [i].matrix [i1, i2] = s.nn.wms [i].matrix [i1, i2];

                    }

                }

            }

        }

        //Assigning neural network and returning the result.
        mutated.nn = nn;
        return mutated;

        /* When it comes to mutation, it is worth mentioning that the mutation rate should be small. The highter the mutation rate, the more
        "random" will the snakes be, making crossover less impactful. */

    }

    //creating a copy of a snake. The only importart thing here is to pass the trained neural network on.
    public Snake copySnake (Snake s) {

        Snake copy = new Snake (SnakeManager.core);
        copy.nn = copyNet (s.nn);

        return copy;
    }

    NeuralNet copyNet (NeuralNet nn) {

        NeuralNet copy = new NeuralNet (SnakeManager.core);

        for (int i1 = 0; i1 < nn.wms.Count; i1++) {

            for (int i2 = 0; i2 < nn.wms [i1].rows; i2++) {

                for (int i3 = 0; i3 < nn.wms [i1].columns; i3++) {

                    copy.wms [i1].matrix [i2, i3] = nn.wms [i1].matrix [i2, i3];

                }

            }

        }

        return copy;

    }


    //Conventing any array to a one dimensional matrix.
    public WeightMatrix arrayToMatrix (float[] a) {

        WeightMatrix wm = new WeightMatrix (a.Length, 1);

        for (int i = 0; i < a.Length; i++) {
            wm.matrix [i, 0] = a [i];
        }

        return wm;

    }

    //Convert any matrix to an array.
    public float[] matrixToArray (WeightMatrix wm) {

        List<float> l1 = new List<float>();

        for (int i1 = 0; i1 < wm.rows; i1++) {

            for (int i2 = 0; i2 < wm.columns; i2++) {

               l1.Add (wm.matrix [i1, i2]);

            }
            
        }

        return l1.ToArray();


    }

    //Adding bias to a matrix.
    //In this case we will simply use 1 as the bias.
    public WeightMatrix addBias (WeightMatrix wm) {

        WeightMatrix bWM = new WeightMatrix (wm.rows + 1, 1);

        for (int i = 0; i < wm.rows; i++) {
            bWM.matrix [i, 0] = wm.matrix [i, 0];
        }

        bWM.matrix [wm.rows, 0] = 1;

        return bWM;

    }

}