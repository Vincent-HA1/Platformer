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
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction clickAction;

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
            navigateAction = uiModule.move.action;
            submitAction = uiModule.submit.action;
            clickAction = uiModule.leftClick.action;
        }
    }

    private void LateUpdate()
    {
        if (!eventSystem.enabled) return;
        if (eventSystem.currentSelectedGameObject != currentlySelectedGameobject)
        {
            //If there was a previously seleced object, this means the choice has changed, so play the sound
            if(currentlySelectedGameobject != null)
            {
                uiAudioSource.PlayOneShot(uiMoveSound);
            }
            currentlySelectedGameobject = eventSystem.currentSelectedGameObject; //set it initially
        }
        else if(eventSystem.currentSelectedGameObject == null)
        {
            currentlySelectedGameobject = null;
        }
    }
    void OnEnable()
    {
        // subscribe safely (action may be null if not configured)
        if (navigateAction != null) navigateAction.performed += OnNavigate;
        if (submitAction != null) submitAction.performed += OnSubmit;
        if (clickAction != null) clickAction.performed += OnClick;
    }

    void OnDisable()
    {
        if (navigateAction != null) navigateAction.performed -= OnNavigate;
        if (submitAction != null) submitAction.performed -= OnSubmit;
        if (clickAction != null) clickAction.performed -= OnClick;
    }

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        //if (eventSystem.currentSelectedGameObject == null) return;
        //print(currentlySelectedGameobject);
        //print(eventSystem.currentSelectedGameObject);
        //if(currentlySelectedGameobject != eventSystem.currentSelectedGameObject)
        //{
        //    currentlySelectedGameobject = eventSystem.currentSelectedGameObject; //means we have moved to a new element, so play sound
        //    // avoid tiny noise triggering nav SFX (sticks have deadzones)
        //    Vector2 v = ctx.ReadValue<Vector2>();
        //    print("nsv");
        //    if (v.sqrMagnitude > 0.01f)
        //    {
        //        Debug.Log("Navigate performed: " + v);
        //        // Play navigation SFX
        //        uiAudioSource.PlayOneShot(uiMoveSound);
        //    }
        //}

    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (eventSystem.currentSelectedGameObject == null || !eventSystem.enabled) return;
        Debug.Log("Submit performed");
        uiAudioSource.PlayOneShot(uiConfirmSound);
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        Debug.Log("Click performed");
        // src.PlayOneShot(clickClip);
        //
        //if (eventSystem.currentSelectedGameObject == null && currentlySelectedGameobject) //if was deselected
        //{
        //    eventSystem.SetSelectedGameObject(currentlySelectedGameobject);
        //}
    }

}
