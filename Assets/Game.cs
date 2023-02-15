using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour
{
    //Fields

    public GameObject CategoryWorld;
    public Sprite WhiteQuad;
    public TextMeshProUGUI ScoreText;

    private Cell[,] Cells;
    private List<Cell> Snake;

    private int SizeX  = 25;
    private int SizeY = 15;

    private float SnakeSpeed = 0.6f;
    private Direction SnakeDirection = Direction.Up;
    private bool SnakeBlockControl = false;

    private bool GameIsStarted = false;
    private int Score = 0;
    


    enum Direction
    {
        Up, Down, Left, Right
    }

    private void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
       GameIsStarted= true;
        Score = 0;
        ScoreText.text = "Your score: " + Score;
        SnakeDirection = Direction.Up;
    }
}






