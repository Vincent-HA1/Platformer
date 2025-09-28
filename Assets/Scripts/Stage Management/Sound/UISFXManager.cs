using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class UISFXManager : MonoBehaviour
{
    [Header("UI Sounds")]
    [SerializeField] AudioClip uiMoveSound;
    [SerializeField] AudioClip uiConfirmSound;

    AudioSource uiAudioSource;
    InputSystemUIInputModule uiModule;

    // cached action instances (from the module or refs)
    private InputAction submitAction;
    private InputAction navigateAction;


    EventSystem eventSystem;
    GameObject currentlySelectedGameobject;

    bool navigated = false;
    void Awake()
    {
        uiAudioSource = GetComponent<AudioSource>();
        eventSystem = EventSystem.current;
        if (eventSystem != null)
            uiModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        // If you're using InputSystemUIInputModule, prefer those actions (they are the active ones).
        if (uiModule != null)
        {
            // InputActionProperty -> .action returns the live InputAction instance
            submitAction = uiModule.submit.action;
            navigateAction = uiModule.move.action;
        }
    }

    private void LateUpdate()
    {
        if (!eventSystem.enabled) return;
        //Check for UI navigation
        if (eventSystem.currentSelectedGameObject != currentlySelectedGameobject && eventSystem.currentSelectedGameObject != null)
        {
            //If there was a previously seleced object, this means the choice has changed, so play the sound
            if (currentlySelectedGameobject != null && navigated) //check if player has moved as well, or if this was a manual selection
            {
                uiAudioSource.PlayOneShot(uiMoveSound);
            }
            currentlySelectedGameobject = eventSystem.currentSelectedGameObject; //set the object to what is currently selected
        }
        else if (eventSystem.currentSelectedGameObject == null)
        {
            //The UI screen has closed, so set this to null
            currentlySelectedGameobject = null;
        }
        navigated = false;
    }
    void OnEnable()
    {
        // subscribe safely (action may be null if not configured)
        if (submitAction != null) submitAction.performed += OnSubmit;
        if (navigateAction != null) navigateAction.performed += OnNavigate;

    }

    void OnDisable()
    {
        if (submitAction != null) submitAction.performed -= OnSubmit;
        if (navigateAction != null) navigateAction.performed -= OnNavigate;
    }

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        navigated = true;
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (eventSystem.currentSelectedGameObject == null || !eventSystem.enabled) return;
        uiAudioSource.PlayOneShot(uiConfirmSound);
    }


}
