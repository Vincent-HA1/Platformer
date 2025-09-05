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

    EventSystem eventSystem;
    GameObject currentlySelectedGameobject;
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
        }
    }

    private void LateUpdate()
    {
        if (!eventSystem.enabled) return;
        //Check for UI navigation
        if (eventSystem.currentSelectedGameObject != currentlySelectedGameobject)
        {
            //If there was a previously seleced object, this means the choice has changed, so play the sound
            if(currentlySelectedGameobject != null)
            {
                uiAudioSource.PlayOneShot(uiMoveSound);
            }
            currentlySelectedGameobject = eventSystem.currentSelectedGameObject; //set the object to what is currently selected
        }
        else if(eventSystem.currentSelectedGameObject == null)
        {
            //The UI screen has closed, so set this to null
            currentlySelectedGameobject = null;
        }
    }
    void OnEnable()
    {
        // subscribe safely (action may be null if not configured)
        if (submitAction != null) submitAction.performed += OnSubmit;
    }

    void OnDisable()
    {
        if (submitAction != null) submitAction.performed -= OnSubmit;
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (eventSystem.currentSelectedGameObject == null || !eventSystem.enabled) return;
        Debug.Log("Submit performed");
        uiAudioSource.PlayOneShot(uiConfirmSound);
    }


}
