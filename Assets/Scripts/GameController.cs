using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum GameState { Start, Play, lose, game_over}
    public event System.Action<GameState> OnStateChanged;
    private GameState state;
    public GameState State { get => state; set { state = value; OnStateChanged?.Invoke(state); } }

   public void StartGame()
    {
        State = GameState.Play;
    

    
        
    }
}
