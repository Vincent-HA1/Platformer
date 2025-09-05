using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public Action Quit;

    [Header("References")]
    [SerializeField] InputHandler inputs;

    [Header("UI References")]
    [SerializeField] Button resumeButton;
    [SerializeField] Button quitButton;
    [SerializeField] GameObject pauseMenu;

    public bool paused {get; private set;}

    // Start is called before the first frame update
    void Start()
    {
        resumeButton.onClick.AddListener(ClosePauseScreen);
        quitButton.onClick.AddListener(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelManager.cannotAct) return;
        if (inputs.pausePressed)
        {
            if (paused)
            {
                ClosePauseScreen();
            }
            else 
            {
                PauseGame();
            }
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        paused = true;
        LevelManager.cannotAct = true;
    }

    void ClosePauseScreen()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        paused = false;
        LevelManager.cannotAct = false;
        EventSystem.current.SetSelectedGameObject(null);
    }

    void QuitGame()
    {
        Quit?.Invoke();
    }

}
