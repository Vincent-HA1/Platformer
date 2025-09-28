using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindManager : MonoBehaviour
{
    public Action inputsRebound;


    [SerializeField]
    List<RebindActionUICustom> keyboardRebinds = new List<RebindActionUICustom>();
    [SerializeField]
    List<RebindActionUICustom> controllerRebinds = new List<RebindActionUICustom>();
    [SerializeField] ControlIconDatabase db;
    [Tooltip("Name of your keyboard/mouse control scheme as in the Input Actions asset")]
    [SerializeField] string keyboardSchemeName = "Keyboard";
    [Tooltip("Name of your gamepad control scheme as in the Input Actions asset")]
    [SerializeField] string gamepadSchemeName = "Gamepad";

    [Header("UI References")]
    [SerializeField] Button resetKeyboardBinds;
    [SerializeField] Button resetControllerBinds;
    [SerializeField] GameObject rebindOverlay;
    [SerializeField] TMPro.TextMeshProUGUI rebindText;
    [SerializeField] GameObject menuButtonPrompts;

    [Header("Special")]
    [SerializeField] public ControlPathList unbindableKeys;
    [SerializeField, ControlPathPicker] public string cancelRebindKey;
    PlayerActions playerActions;

    EventSystem eventSystem;
    InputHandler inputHandler;
    GameObject lastSelectedGameobject;

    public string exclusionString { get; set; }

    public static bool rebinding = false;

    // Normalize arbitrary strings and control.path to canonical "<device>/control" lowercased form
    public string NormalizeToBindingPath(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;
        raw = raw.Trim().Trim('"').Replace("\\", "/").ToLowerInvariant();

        if (raw.StartsWith("/")) raw = raw.Substring(1);          // "/keyboard/f2" -> "keyboard/f2"
        if (raw.StartsWith("<")) return raw;                      // already "<keyboard>/f2"
        int slash = raw.IndexOf('/');
        if (slash > 0)
        {
            string device = raw.Substring(0, slash).Trim();
            string rest = raw.Substring(slash);                    // includes the '/'
            return $"<{device}>{rest}";
        }
        // fallback
        return $"<{raw}>";
    }

    // Start is called before the first frame update
    void Start()
    {
        NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
        playerActions = FindObjectOfType<PlayerInputScript>().actions;
        foreach (var rebind in keyboardRebinds.Concat(controllerRebinds))
        {
            //Bind to rebind events
            rebind.startRebindEvent.AddListener(RebindStart);
            rebind.stopRebindEvent.AddListener(RebindStop);
            rebind.RebindInvalid += RebindBlocked;
            rebind.ReboundAction += CheckForValidRebind;
            rebind.BindToManager(this);
        }
        resetKeyboardBinds.onClick.AddListener(ResetAllKeyboardRebinds);
        resetControllerBinds.onClick.AddListener(ResetAllControllerRebinds);
        exclusionString = BuildExclusionString();
        inputHandler = FindObjectOfType<InputHandler>();
    }

    void ResetAllKeyboardRebinds()
    {
        foreach (var rebind in keyboardRebinds)
        {
            rebind.ResetToDefault();
        }
    }

    void ResetAllControllerRebinds()
    {
        foreach (var rebind in controllerRebinds)
        {
            rebind.ResetToDefault();
        }
    }

    // turn the list of forbidden controls into a chain of .WithControlsExcluding calls
    // (RebindingOperation.WithControlsExcluding takes a single control mask or pattern per call,
    // but accepting a single concatenated string is fine here — we call it once with a comma-separated list)
    string BuildExclusionString()
    {
        if (unbindableKeys == null || unbindableKeys.paths.Count == 0) return null;
        // Some versions expect a single path; others accept patterns. If you find excluding multiple
        // at once fails, you can call WithControlsExcluding for each entry — e.g. via a helper overload.
        // We'll return a comma-separated string to attempt to exclude multiple.
        return string.Join(",", unbindableKeys.paths);
    }

    void RebindStart(RebindActionUICustom rebind, InputActionRebindingExtensions.RebindingOperation op)
    {
        if (!eventSystem) eventSystem = EventSystem.current;
        //currentRebindOperation = op;
        lastSelectedGameobject = eventSystem.currentSelectedGameObject;
        eventSystem.enabled = false;
        rebindOverlay?.SetActive(true);
        menuButtonPrompts.SetActive(false);
        var deviceName = rebind.GetDeviceLayout();
        var text = !string.IsNullOrEmpty(deviceName)//op.expectedControlType)
        ? $"Waiting for {deviceName} input..."
        : $"Waiting for input...";
        rebindText.text = text;
        rebinding = true;
        rebindBlocked = false;
        inputHandler.playerActions.Disable();
    }

    void RebindStop(RebindActionUICustom rebind, InputActionRebindingExtensions.RebindingOperation op)
    {
        if (rebindBlocked)
        {
            //Input blocked, so check for rebind again
            StartCoroutine(RestartRebind(rebind, op));
            return;
        }
        if (eventSystem && rebinding) //So if was rebinding previously, reselect the last gameobject so the event system returns to normal
        {
            eventSystem.SetSelectedGameObject(lastSelectedGameobject);
            eventSystem.enabled = true;
        }
        rebinding = false;
        rebindOverlay?.SetActive(false);
        menuButtonPrompts.SetActive(true);
        //Reset all inputs to ensure any bindings dont just trigger anything
        inputHandler.ResetAllBools();
        StartCoroutine(EnableInputAfterRebind());
    }

    IEnumerator EnableInputAfterRebind()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        yield return null;
        inputHandler.playerActions.Enable();
    }

    bool rebindBlocked = false;
    void RebindBlocked(RebindActionUICustom rebindInstance, InputAction action)
    {
        //Current rebind was blocked, so dont end rebind, RESTART IT
        rebindBlocked = true;
    }

    void CheckForValidRebind(RebindActionUICustom rebindInstance, InputAction action, string bindingId, InputActionRebindingExtensions.RebindingOperation currentRebindOperation)
    {
        //Cancel the rebind if the binding is in there
        //if (action == null) return null;
        bool rebindCancelled = false;
        Guid.TryParse(bindingId, out Guid guid); //Get id of this specific binding
        List<InputBinding> bind = action.bindings.ToList();//playerInput.actions.FindAction(actionName).bindings.ToList();
        //See if the control icon database has this key (i.e. it is a valid key). Otherwise, reject. This is a failsafe just in case any bad keys come through
        if (GetSpriteForAction(action, bindingId) != null)
        {
            //This is in the database, so is a valid rebind
        }
        else
        {
            //Check if an operation to be cancelled is actually happening. If not, then there is nothing to cancel
            if (currentRebindOperation != null)
            {
                //Cancel rebind
                rebindCancelled = true;
                rebindInstance.RevertLastBinding();
            }
        }
        if (!rebindCancelled || !rebinding)
        {
            //rebindOverlay?.SetActive(false);
            UpdateIcon(rebindInstance, action, bindingId);
            RebindStop(rebindInstance, currentRebindOperation); //Stop the rebind
            inputsRebound?.Invoke(); //successful rebind
        }
        else
        {
            rebindBlocked = true;
            StartCoroutine(RestartRebind(rebindInstance, currentRebindOperation));

        }
    }

    IEnumerator RestartRebind(RebindActionUICustom rebindInstance, InputActionRebindingExtensions.RebindingOperation currentRebindOperation)
    {
        yield return null;//Wait for frame
        if (currentRebindOperation != null)
        {
            currentRebindOperation.Reset(); //reset the operation
        }
        rebindBlocked = false;
        //Need to restart the rebind.
        rebindInstance.StartInteractiveRebind();
    }

    void UpdateIcon(RebindActionUICustom rebindInstance, InputAction action, string bindingId)
    {
        //Update icon for the rebind prompt
        Sprite newIcon = GetSpriteForAction(action, bindingId);
        rebindInstance.SetIcon(newIcon);
    }
    Sprite GetSpriteForAction(InputAction action, string bindingId)
    {
        if (playerActions == null || action == null) return null;
        Guid.TryParse(bindingId, out Guid guid); //Get id of this specific binding
        List<InputBinding> bind = action.bindings.ToList();
        foreach (InputBinding binding in bind)
        {
            //If this is the correct binding, find its sprite
            if (binding.id == guid)
            {
                string path = binding.overridePath != null ? binding.overridePath : binding.effectivePath; //use override if possible
                Sprite sprite = db.GetIconForPath(path);//get the sprite
                if (sprite != null) return sprite;
            }

        }
        return null;
    }
}
