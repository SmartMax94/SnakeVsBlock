using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayGame : MonoBehaviour { 


    public string sceneName;
public Button playButton;

    
    void Start()
    {
    playButton.onClick.AddListener(TaskOnClick);    
    }

    // Update is called once per frame
    void TaskOnClick()
    {
        SceneManager.LoadScene(sceneName);  
    }
}
