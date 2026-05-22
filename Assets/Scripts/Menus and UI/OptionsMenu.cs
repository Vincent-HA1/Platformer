using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject keyboardConfigScreen;
    [SerializeField] GameObject gamepadConfigScreen;
    [SerializeField] GameObject parentUIScreen;
    [SerializeField] GameObject optionsScreen;
    [SerializeField] GameObject optionsMenu;
    [SerializeField] Button optionsButton;
    [SerializeField] Button firstOptionsButton;
    [SerializeField] Button keyboardConfigButton;
    [SerializeField] Button gamepadConfigButton;
    [SerializeField] Button firstKeyboardConfigRebind;
    [SerializeField] Button firstGamepadConfigRebind;


    Image backgroundImage;
    InputHandler inputHandler;
    EventSystem eventSystem;

    // Start is called before the first frame update
    void Start()
    {
        backgroundImage = GetComponent<Image>();
        //eventSystem = EventSystem.current;
        inputHandler = FindObjectOfType<InputHandler>();
        optionsButton.onClick.AddListener(() => CloseScreen(parentUIScreen, optionsMenu, firstOptionsButton));
        keyboardConfigButton.onClick.AddListener(OpenKeyboardConfig);
        gamepadConfigButton.onClick.AddListener(OpenGamepadConfig);
        backgroundImage.enabled = false;
        StartCoroutine(SetEventSystem());
    }

    IEnumerator SetEventSystem()
    {
        yield return null;
        //Wait for Event system to finish updating with rebindings and input actions
        yield return new WaitUntil(() => EventSystem.current != null);
        eventSystem = EventSystem.current;
    }

    // Update is called once per frame
    void Update()
    {
        if (RebindManager.rebinding) return;
        if (optionsMenu.activeInHierarchy)
        {
            backgroundImage.enabled = true;
        }
        //If back pressed, close the correct screen
        if (inputHandler.cancelPressed)
        {
            if (keyboardConfigScreen.activeInHierarchy)
            {
                CloseScreen(keyboardConfigScreen, optionsMenu, keyboardConfigButton);
            }
            else if (gamepadConfigScreen.activeInHierarchy)
            {
                CloseScreen(gamepadConfigScreen, optionsMenu, gamepadConfigButton);
            }
            else if (optionsMenu.activeInHierarchy)
            {
                CloseScreen(optionsMenu, parentUIScreen, optionsButton);
                backgroundImage.enabled = false;
            }
        }
    }


    void OpenKeyboardConfig()
    {
        keyboardConfigScreen.SetActive(true);
        optionsMenu.SetActive(false);
        eventSystem.SetSelectedGameObject(firstKeyboardConfigRebind.gameObject);
    }

    void OpenGamepadConfig()
    {
        gamepadConfigScreen.SetActive(true);
        optionsMenu.SetActive(false);
        eventSystem.SetSelectedGameObject(firstGamepadConfigRebind.gameObject);
    }

    void CloseScreen(GameObject screenToClose, GameObject screenToGoTo, Button buttonToSelect)
    {
        screenToClose.SetActive(false);
        screenToGoTo.SetActive(true);
        eventSystem.SetSelectedGameObject(buttonToSelect.gameObject);
        if (inputHandler.cancelPressed) inputHandler.cancelPressed = false; //prevent any race conditions by blocking this input from doing anything else
    }
}
