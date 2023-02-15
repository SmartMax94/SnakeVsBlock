using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour

{
    public GameController gameController;
    public GameObject startMenu;


    // Start is called before the first frame update
    void Start()
    {
        gameController.OnStateChanged += UpdateUI;  
    }

    
    void UpdateUI(GameController.GameState state)
    {
        startMenu.SetActive( state == GameController.GameState.Start);
    }
}
