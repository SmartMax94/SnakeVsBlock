using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/* This script manages the initiation of the project and the subsequent simulations. */

public class SnakeManager : MonoBehaviour {

    public static SnakeManager core;

    //Core elements

    //The Prefab that will be comprising our snake
    public GameObject SnakeTilePrefab;
    //The Prefab that will be comprising our grid
    public GameObject GridTilePrefab;
    //The Prefab of the fruit tile
    public GameObject FruitTilePrefab;
    //The Parent of our fruit
    public GameObject FruitParent;
    //The parent object of our grid tiles
    public GameObject GridParent;
    //The parent object of our snake tiles
    public GameObject SnakeParent;
    //Grid Canvas
    public GameObject GridCanvas;

    //The size of the grid (X*Y Tiles big)
    public Vector2 GridSize;

    //Score Label
    public Text scoreLabel;

    //The size of each cell/tile of the grid. This size should be identical to the size of the Grid Tile Prefab.
    public Vector2 CellSize;

    //The tiles of the visible snake
    public List<Vector2> SnakeTiles = new List<Vector2>();

    //The coordinates of each grid tile
    public List<Vector2> Grid = new List<Vector2>();

    public List<List<List<Vector2>>> replayCoordinates = new List<List<List<Vector2>>>();
    public List<List<Vector2>> fruitReplayCoordinates = new List<List<Vector2>>();

    //Main camera object
    private Camera mainCam;

    //The visible fruit gameObject
    private GameObject fruit;

    //How many moves can a snake do without food?
    public int movesWithoutFood;

    //For how many generations should the game continue?
    public int gens;

    //Population per generation
    public int population;

    public int parents;

    //Network coroutine
    Coroutine networkRoutine;

    //Draw routine
    Coroutine drawRoutine;

    //Neural net config
    public NetConfig netConfig = new NetConfig();

    //The speed at which the snake moves when drawn
    public float snakeDrawSpeed = 0.01f;

    [HideInInspector]
    public SnakePopulation p;

    //Drawing each step that a specific snake took during its lifespan.
    IEnumerator DrawRoute (Vector2 fruitPos) {

        //Destroying existing fruits
        foreach (Transform g in FruitParent.transform) {
            Destroy (g.gameObject);
        }

        //Destroying existing snake tiles
        foreach (Transform g in SnakeParent.transform) {
            Destroy (g.gameObject);
        }

        //A list to be filled with snake tiles
        List<Vector2> currentlyDrawn = new List<Vector2>();

        //A list to be filled with tiles designated for removal
        List<Vector2> toRemove = new List <Vector2>();

        //The coordinates of the last snake's tiles
        List<List<Vector2>> replayCoordinatesLast = replayCoordinates [replayCoordinates.Count - 1];
        List<Vector2> fruitReplayCoordinatesLast = fruitReplayCoordinates [fruitReplayCoordinates.Count - 1];

        Vector2 fruitPrev = new Vector2 (-999, -999);

        //For each replay coordinates list
        for (int i = 0; i < replayCoordinatesLast.Count; i++) {

            //Destroy any invalid coordinates
            foreach (Vector2 v2 in currentlyDrawn) {
                if (!replayCoordinatesLast[i].Contains (v2)) {
                    toRemove.Add (v2);
                    foreach (Transform t in SnakeParent.transform) {
                        if ((Vector2) t.position == v2) {
                            Destroy (t.gameObject);
                        }
                    }
                }
            }

            foreach (Vector2 v2 in toRemove) {
                currentlyDrawn.Remove (v2);
            }
            
            //Draw the snake based on the newest coordinates
            foreach (Vector2 v2 in replayCoordinatesLast[i]) {

                if (!currentlyDrawn.Contains (v2)) {

                    currentlyDrawn.Add (v2);

                    Instantiate (SnakeTilePrefab, v2, Quaternion.identity, SnakeParent.transform);

                }

            }

            //Destroy fruit if it has been eaten
            if (fruitPrev != fruitReplayCoordinatesLast [i]) {

                foreach (Transform g in FruitParent.transform) {
                    Destroy (g.gameObject);
                }
                
                //Instantiate new fruit if the prevous one was eaten
                Instantiate (FruitTilePrefab, fruitReplayCoordinatesLast [i], Quaternion.identity, FruitParent.transform);
            }

            yield return new WaitForSeconds (snakeDrawSpeed);

        }

    }

    void resetExternal () {
        ExternalFunctions.core.autoStartNextGen = false;
        ExternalFunctions.core.startNextGen = false;
        ExternalFunctions.core.autoStartNextGenObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Auto Start Next Gen:Off";
        ExternalFunctions.core.startNextGenObject.SetActive (false);
        ExternalFunctions.core.clearDebug();
    }

    void Start () {

        //Initializing Camera
        mainCam = Camera.main;

        //Generating Greed
        GenerateGrid ();

        p = new SnakePopulation (population, SnakeManager.core);

        resetExternal ();

    }

    public void importAndStartEmulation () {

        if (networkRoutine != null) {
            StopCoroutine (networkRoutine);
        }

        resetExternal();

        p = new SnakePopulation (population, SnakeManager.core);

        ExternalFunctions.core.import ();

        //Start Emulation
        networkRoutine = StartCoroutine(startNetwork());

    }

    public void startNewEmulation () {

        p = new SnakePopulation (population, SnakeManager.core);

        if (networkRoutine != null) {
            StopCoroutine (networkRoutine);
        }

        resetExternal();

        //Start Emulation
        networkRoutine = StartCoroutine(startNetwork());
        
    }

    //Starting network.
    IEnumerator startNetwork () {

        ExternalFunctions.core.debug ("Net Started");
        
        //Running emulations for each egenartion of snakes
        for (int i = 0; i < gens; i++) {
            
            p.currentGen += 1;

            ExternalFunctions.core.debug ("Gen " + p.currentGen.ToString());

            var counter = 0;

            ExternalFunctions.core.debug (p.snakes.Length);

            for (int i2 = 0; i2 < p.snakes.Length; i2++) {
                
                StartCoroutine (emulateGame(p, i2, counter));

                yield return new WaitForEndOfFrame ();
                
            }

            //Waiting for all snakes to die before moving to the new generation
            while (snakesAlive (p)) {
                yield return new WaitForEndOfFrame ();
            }

            ExternalFunctions.core.debug ("Previous best snake got score " + p.snakes[0].fitness.ToString());
            
            //Getting the best snake of this generation
            Snake [] orderedSnakes = p.snakes.OrderBy (x => x.fitness).ToArray();

            p.bestSnake = orderedSnakes [orderedSnakes.Length - 1];
            p.bestScore = p.bestSnake.fitness;

            replayCoordinates.Add (p.bestSnake.replayCoordinates);
            fruitReplayCoordinates.Add (p.bestSnake.fruitReplayCoordinates);

            //scoreLabel.text = "Best score in gen [" + p.currentGen.ToString() + "] is " + p.bestScore.ToString();
            ExternalFunctions.core.debug ("Best score in gen [" + p.currentGen.ToString() + "] is " + p.bestSnake.size.ToString() + " with fitness " + p.bestSnake.fitness.ToString());
            ExternalFunctions.core.debug (p.bestSnake.cause);

            if (drawRoutine != null) {
                StopCoroutine (drawRoutine);
            }
            
            drawRoutine = StartCoroutine (DrawRoute(p.bestSnake.fruitPosition));

            //At this point all of our snakes have died and we can start the process of breeding
            Snake [] snakes2 = new Snake [p.snakes.Length];
            snakes2 [0] = CoreFunctions.core.copySnake (p.bestSnake);

            for (int i2 = 1; i2 < p.snakes.Length; i2++) {

                Snake rouletteResult1 = rouletteSelection (orderedSnakes, false);
                Snake rouletteResult2 = rouletteSelection (orderedSnakes, false);

                //Making sure that no snake can mate with itself
                while (rouletteResult1 == rouletteResult2) {
                    rouletteResult2 = rouletteSelection (orderedSnakes, true);
                    yield return new WaitForEndOfFrame ();
                }

                Snake newSnake = CoreFunctions.core.snakeCrossover (rouletteResult1, rouletteResult2);
                newSnake = CoreFunctions.core.mutateSnake (newSnake);

                snakes2 [i2] = newSnake;

            }

            //Replacing with new generation
            p.snakes = snakes2;

            //If autoStartNextGen is false, we must wait for the player to start the next generation manually
            if (!ExternalFunctions.core.autoStartNextGen) {

                ExternalFunctions.core.startNextGenObject.SetActive (true);

                while (!ExternalFunctions.core.startNextGen) {

                    yield return new WaitForSeconds (0.1f);

                }

                ExternalFunctions.core.startNextGenObject.SetActive (false);
                ExternalFunctions.core.startNextGen = false;

            }

        }

    }

    Snake rouletteSelection (Snake [] snakes, bool pickRnadom) {

        float totalFitness = 0;

        //Storing all the snakes in a list
        for (int i = snakes.Length - parents; i < snakes.Length; i++) {
            totalFitness += snakes [i].fitness < 0 ? 0 : snakes [i].fitness;
        }
        
        if (!pickRnadom) {

            float rand = Random.Range (0, totalFitness);

            float current = 0f;

            foreach (Snake s in snakes) {

                current += s.fitness < 0 ? 0 : s.fitness;

                if (current > rand) {

                    return s;

                }

            }

        } else {

            return snakes [Random.Range (snakes.Length - parents, snakes.Length - 1)];

        }
  
        return snakes [Random.Range (snakes.Length - parents, snakes.Length - 1)];
        
    }

    //Emulation a life of a snake
    IEnumerator emulateGame (SnakePopulation p, int i, int counter) {

        //Creating a new list to store our inputs
        float[] inputs = new float [p.snakes [i].nn.inputNodes];

        //If the snake is not dead, we need to decide in which direction the snake should move.
        //First we need to see where our snake is currently facing.
        //The snake cannot move backwards, but is can always move to its front, left, and right.
        //However, the front, left and right changes as the snake moves.
        //To remedy this issue, we are deciding these directions based on where it wants to move.
        //By default 0 is up, 1 is down, 2 is left and 3 is right.
        //However, if the snake decides to move right, for example, its previous left will become up, right will become down, and front - right.
        while (!p.snakes[i].dead) {

            var front = 0;
            var right = 0;
            var left = 0;

            switch (p.snakes[i].direction) {
                case 0:
                    front = 0;
                    left = 2;
                    right = 3;
                    break;
                case 1:
                    front = 1;
                    left = 3;
                    right = 2;
                    break;  
                case 2:
                    front = 2;
                    left = 1;
                    right = 0;
                    break;
                default:
                    front = 3;
                    left = 0;
                    right = 1;
                    break;
            }

            //checking if our snake can move in the chosen directions and if there are any fruits there.
            inputs [0] = canMoveInDirection (front, p.snakes[i]) == true ? 1 : 0;
            inputs [1] = canMoveInDirection (left, p.snakes[i]) == true ? 1 : 0;
            inputs [2] = canMoveInDirection (right, p.snakes[i]) == true ? 1 : 0;
            inputs [3] = fruitInDirection2 (front, p.snakes[i]) == true ? 1 : 0;
            inputs [4] = fruitInDirection2 (left, p.snakes[i]) == true ? 1 : 0;
            inputs [5] = fruitInDirection2 (right, p.snakes[i]) == true ? 1 : 0;
            

            //Aften the knows in which direction is what, we run the decide function to allows the network to calculate where to move.
            var turn = CoreFunctions.core.decide (inputs, p.snakes[i].nn);
            var newDir = front;

            switch (turn) {
                case 0:
                    //Turn left
                    newDir = left;
                    break;
                case 1:
                    //Continue forward
                    newDir = front;
                    //Direction remains unchanged
                    break;
                default:
                    //Turn right
                    newDir = right;
                    break;
            }

            //Assigning chosen direction as new direction
            p.snakes[i].direction = newDir;

            //Moving in that direction
            move (p, i);

            yield return new WaitForEndOfFrame ();

        }
               
    }

    //This function will return true if at least one snake from the generation is alive
    public bool snakesAlive (SnakePopulation p) {

        bool alive = false;

        foreach (Snake s in p.snakes) {

            if (!s.dead) {
                alive = true;
                break;
            }

        }

        return alive;
    }

    //Checking if the snake can move in a specific direction
    public bool canMoveInDirection (int dir, Snake s) {
        
        //Getting the coordinates of the next position based on direction
        Vector2 goalPosition = nextPosition (dir, s.currentPosition);

        //Checking whether the destination tile is already occupied by the tail of the snake
        bool tileOccupied = false;

        foreach (Vector2 v in s.occupiedTiles) {
            if (v == goalPosition) {
                tileOccupied = true;
                break;
            }
        }

        bool isTile = false;

        foreach (Vector2 v in Grid) {
            if (v == goalPosition) {
                isTile = true;
                break;
            }
        }
        
        return isTile && !tileOccupied ? true : false;

    }


    public Vector2 distanceToFruit (Snake s) {
        return new Vector2 (Mathf.Abs (s.currentPosition.x - s.fruitPosition.x), Mathf.Abs (s.currentPosition.y - s.fruitPosition.y));
    }
    
    public bool fruitInDirection2 (int dir, Snake s) {

        Vector2 fruitPosition = s.fruitPosition;
        Vector2 currentPosition = s.currentPosition;

        bool fruitFound = false;

        //Looking for fruit based on direction
        switch (dir) {

            case 0:

                //Snake goes up
                if (fruitPosition.y > currentPosition.y) {
                    fruitFound = true;
                }

                break;

            case 1:

                //Snake goes down
                if (fruitPosition.y < currentPosition.y) {
                    fruitFound = true;
                }

                break;

            case 2:

                //Snake goes left
                if (fruitPosition.x < currentPosition.x) {
                    fruitFound = true;
                }

                break;

            default:

                //Snake goes right
                if (fruitPosition.x > currentPosition.x) {
                    fruitFound = true;
                }
                
                break;

        }

        return fruitFound;

    }

    //Finding next position based on coordinates
    public Vector2 nextPosition (int dir, Vector2 curPos) {

        Vector2 currentPosition = curPos;
        Vector2 goalPosition = new Vector2 (-1, -1);

        switch (dir) {

            case 0:
                goalPosition = new Vector2 (currentPosition.x, currentPosition.y + CellSize.y);
                break;

            case 1:
                goalPosition = new Vector2 (currentPosition.x, currentPosition.y - CellSize.y);
                break;

            case 2:
                goalPosition = new Vector2 (currentPosition.x - CellSize.x, currentPosition.y);
                break;

            case 3:
                goalPosition = new Vector2 (currentPosition.x + CellSize.x, currentPosition.y);
                break;

        }

        return goalPosition;

    }

    //Generating grid
    void GenerateGrid () {

        //Getting grid size
        float gridX = CellSize.x * GridSize.x;
        float gridY = GridSize.y * CellSize.y;

        //Resizing canvas
        GridCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2 (gridX, gridY);
        GridCanvas.transform.position = new Vector2 (CellSize.x, CellSize.y);

        //Adjusting camera
        
        //Getting grid centre
        Vector2 gridCentre = new Vector2 ((gridX)/2, (gridY)/2);

        //Setting camera position to grid centre
        mainCam.transform.position = new Vector3 (gridCentre.x, gridCentre.y, mainCam.transform.position.z);

        //Getting camera size based on grid size
        float cameraSize = gridX > gridY ? gridX : gridY;

        mainCam.orthographicSize = cameraSize/2;

        //Clearing Grid
        Grid.Clear();

        //For each X element
        for (int f1 = 1; f1 < GridSize.x; f1++) {
            
            //For each Y element
            for (int f2 = 1; f2 < GridSize.y; f2++) {

                //Generate a grid cell
                
                //Calculating coordinates
                Vector2 newCell = new Vector2 (f1*CellSize.x, f2*CellSize.y);

                //Adding the calculated coordinates to the Grid list.
                Grid.Add (newCell);

                //Instantiating the tile prefab.
                Instantiate (GridTilePrefab, newCell, Quaternion.identity, GridParent.transform);

            }

        }

    }

    //Snake movement & generation
    void move (SnakePopulation p, int i) {

        //While the snake is alive
        if (!p.snakes[i].dead) {
                      
            //Getting the next position of the snake
            Vector2 goalPosition = nextPosition (p.snakes[i].direction, p.snakes[i].currentPosition);

            //Keeping track of previous size of the snake
            var prevSize = p.snakes[i].size;

            if (!canMoveInDirection (p.snakes[i].direction, p.snakes[i])) {

                //If the snake cannot move in the direction it has chosen, also mark it as dead.
                p.snakes[i].dead = true;
                p.snakes[i].cause = "Could not move in direction";

                p.snakes[i].fitness -= 1000;

                //Marking the previous tile as occupied
                p.snakes[i].occupiedTiles.Add (p.snakes[i].currentPosition);

                //Moving
                p.snakes[i].currentPosition = goalPosition;

                //Removing last tile (tip of the tail) if the size has expanded
                if (p.snakes[i].occupiedTiles.Count > 0 && (p.snakes[i].size == prevSize)) {

                    if (p.snakes[i].occupiedTiles.Count > 0) {
                        p.snakes[i].occupiedTiles = p.snakes[i].occupiedTiles.GetRange (1, p.snakes[i].occupiedTiles.Count - 1);
                    }
                
                }

                //Adding a new list of replay coordinates
                List<Vector2> temp = new List<Vector2>();

                foreach (Vector2 v2 in p.snakes[i].occupiedTiles) {
                    temp.Add (v2);
                }

                temp.Add (goalPosition);

                p.snakes[i].replayCoordinates.Add (temp);
                p.snakes[i].fruitReplayCoordinates.Add (p.snakes[i].fruitPosition);

                //Subtracting a move
                p.snakes[i].movesLeft -= 1;


            } else {

                //Calculating fitness based on current and next position
                //Is the snake coming closer to the fruit?

                if (Vector2.Distance (p.snakes[i].currentPosition, p.snakes[i].fruitPosition) > Vector2.Distance (goalPosition, p.snakes[i].fruitPosition)) {
                    p.snakes[i].fitness += 10;
                } else {
                    p.snakes[i].fitness -= 10;
                }

                //Marking the previous tile as occupied
                p.snakes[i].occupiedTiles.Add (p.snakes[i].currentPosition);

                //Moving
                p.snakes[i].currentPosition = goalPosition;

                //Eat apple if there is one
                if (p.snakes[i].fruitPosition == p.snakes[i].currentPosition) {

                    //Change apple's position
                    p.snakes[i].fruitPosition = Grid [Random.Range (0, Grid.Count - 1)];

                    while (p.snakes[i].fruitPosition == p.snakes[i].currentPosition || p.snakes[i].occupiedTiles.Contains (p.snakes[i].fruitPosition)) {
                        p.snakes[i].fruitPosition = Grid [Random.Range (0, Grid.Count - 1)];
                    }  

                    //Increase the size and fitness of the snake
                    p.snakes[i].size += 1;

                    //Reset moves
                    p.snakes[i].movesLeft = movesWithoutFood;

                    p.snakes[i].fitness += 2000;

                }
                
                //Removing last tile (tip of the tail) if the size has expanded
                if (p.snakes[i].occupiedTiles.Count > 0 && (p.snakes[i].size == prevSize)) {

                    if (p.snakes[i].occupiedTiles.Count > 0) {
                        p.snakes[i].occupiedTiles = p.snakes[i].occupiedTiles.GetRange (1, p.snakes[i].occupiedTiles.Count - 1);
                    }
                
                }

                //Adding a new list of replay coordinates
                List<Vector2> temp = new List<Vector2>();

                foreach (Vector2 v2 in p.snakes[i].occupiedTiles) {
                    temp.Add (v2);
                }

                temp.Add (goalPosition);

                p.snakes[i].replayCoordinates.Add (temp);
                p.snakes[i].fruitReplayCoordinates.Add (p.snakes[i].fruitPosition);

                //Subtracting a move
                p.snakes[i].movesLeft -= 1;

                //If the snake has no moves left, mark it as dead.
                if (p.snakes[i].movesLeft <= 0) {
                    p.snakes[i].dead = true;
                    p.snakes[i].cause = "No moves left.";
                    p.snakes[i].fitness -= 1000;
                }
            }


        }

    }

    void Awake () {
        if (core == null) {
            core = this;
        }
    }

}
