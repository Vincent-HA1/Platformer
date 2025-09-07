using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public Action Quit;

    [Header("References")]
    [SerializeField] InputHandler inputs;

    [Header("UI References")]
    [SerializeField] Button resumeButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button quitButton;
    [SerializeField] GameObject background;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject optionsScreen;
    [SerializeField] UIBar masterBar;


    public bool paused {get; private set;}

    // Start is called before the first frame update
    void Start()
    {
        resumeButton.onClick.AddListener(ClosePauseScreen);
        optionsButton.onClick.AddListener(OpenOptionsScreen);
        quitButton.onClick.AddListener(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelManager.cannotAct && !paused) return;
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
        if (Input.GetKeyDown("k"))
        {
            if (optionsScreen.activeInHierarchy)
            {
                //close it
                optionsScreen.SetActive(false);
                pauseMenu.SetActive(true);
                EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
            }
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
        background.SetActive(true);
        optionsScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        paused = true;
        LevelManager.cannotAct = true;
    }

    void ClosePauseScreen()
    {
        optionsScreen.SetActive(false);
        background.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        paused = false;
        LevelManager.cannotAct = false;
        EventSystem.current.SetSelectedGameObject(null);
    }

    void OpenOptionsScreen()
    {
        optionsScreen.SetActive(true);
        pauseMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(masterBar.leftArrow.gameObject);
    }

    void QuitGame()
    {
        Quit?.Invoke();
    }

}
