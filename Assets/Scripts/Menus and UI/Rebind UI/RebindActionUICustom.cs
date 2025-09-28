using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// A reusable component with a self-contained UI for rebinding a single action.
/// </summary>
public class RebindActionUICustom : MonoBehaviour
{
    // ---------------------- Fields (moved to top) ----------------------

    [Tooltip("Reference to action that is to be rebound from the UI.")]
    [SerializeField]
    private InputActionReference m_Action;

    [SerializeField]
    private string m_BindingId;

    [SerializeField]
    private InputBinding.DisplayStringOptions m_DisplayStringOptions;

    [SerializeField]
    private Image m_PromptIcon;

    [SerializeField] private Button m_Button;

    [Tooltip("Text label that will receive the name of the action. Optional. Set to None to have the "
        + "rebind UI not show a label for the action.")]
    [SerializeField]
    private TMPro.TextMeshProUGUI m_ActionLabel;

    [Tooltip("Text label that will receive the current, formatted binding string.")]
    [SerializeField]
    private TMPro.TextMeshProUGUI m_BindingText;

    [Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying "
        + "bindings in custom ways, e.g. using images instead of text.")]
    [SerializeField]
    private UpdateBindingUIEvent m_UpdateBindingUIEvent;

    [Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, "
        + "to implement custom UI behavior while a rebind is in progress. It can also be used to further "
        + "customize the rebind.")]
    [SerializeField]
    private InteractiveRebindEvent m_RebindStartEvent;

    [Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
    [SerializeField]
    private InteractiveRebindEvent m_RebindStopEvent;

    private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;

    private static List<RebindActionUICustom> s_RebindActionUIs;

    private string prevOverride;

    private RebindManager rebindManager;

    public Action<RebindActionUICustom, InputAction, string, InputActionRebindingExtensions.RebindingOperation> ReboundAction;

    public Action<RebindActionUICustom, InputAction> RebindInvalid;

    // ---------------------- Properties ----------------------

    /// <summary>
    /// Reference to the action that is to be rebound.
    /// </summary>
    public InputActionReference actionReference
    {
        get => m_Action;
        set
        {
            m_Action = value;
            UpdateActionLabel();
            UpdateBindingDisplay();
        }
    }

    /// <summary>
    /// ID (in string form) of the binding that is to be rebound on the action.
    /// </summary>
    /// <seealso cref="InputBinding.id"/>
    public string bindingId
    {
        get => m_BindingId;
        set
        {
            m_BindingId = value;
            UpdateBindingDisplay();
        }
    }

    public InputBinding.DisplayStringOptions displayStringOptions
    {
        get => m_DisplayStringOptions;
        set
        {
            m_DisplayStringOptions = value;
            UpdateBindingDisplay();
        }
    }

    public Button button
    {
        get => m_Button;
    }

    /// <summary>
    /// Text component that receives the name of the action. Optional.
    /// </summary>
    public TMPro.TextMeshProUGUI actionLabel
    {
        get => m_ActionLabel;
        set
        {
            m_ActionLabel = value;
            UpdateActionLabel();
        }
    }

    /// <summary>
    /// Text component that receives the display string of the binding. Can be <c>null</c> in which
    /// case the component entirely relies on <see cref="updateBindingUIEvent"/>.
    /// </summary>
    public TMPro.TextMeshProUGUI bindingText
    {
        get => m_BindingText;
        set
        {
            m_BindingText = value;
            UpdateBindingDisplay();
        }
    }

    /// <summary>
    /// Event that is triggered every time the UI updates to reflect the current binding.
    /// This can be used to tie custom visualizations to bindings.
    /// </summary>
    public UpdateBindingUIEvent updateBindingUIEvent
    {
        get
        {
            if (m_UpdateBindingUIEvent == null)
                m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
            return m_UpdateBindingUIEvent;
        }
    }

    /// <summary>
    /// Event that is triggered when an interactive rebind is started on the action.
    /// </summary>
    public InteractiveRebindEvent startRebindEvent
    {
        get
        {
            if (m_RebindStartEvent == null)
                m_RebindStartEvent = new InteractiveRebindEvent();
            return m_RebindStartEvent;
        }
    }

    /// <summary>
    /// Event that is triggered when an interactive rebind has been completed or canceled.
    /// </summary>
    public InteractiveRebindEvent stopRebindEvent
    {
        get
        {
            if (m_RebindStopEvent == null)
                m_RebindStopEvent = new InteractiveRebindEvent();
            return m_RebindStopEvent;
        }
    }

    /// <summary>
    /// When an interactive rebind is in progress, this is the rebind operation controller.
    /// Otherwise, it is <c>null</c>.
    /// </summary>
    public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

    // ---------------------- Unity lifecycle ----------------------

    private void Start()
    {
        m_PromptIcon.preserveAspect = true;
        //Get the current rebinds immediately
        ReboundAction?.Invoke(this, actionReference, m_BindingId, m_RebindOperation);
        UpdateBindingDisplay();
    }

    protected void OnEnable()
    {
        if (s_RebindActionUIs == null)
            s_RebindActionUIs = new List<RebindActionUICustom>();
        s_RebindActionUIs.Add(this);
        if (s_RebindActionUIs.Count == 1)
            InputSystem.onActionChange += OnActionChange;
    }

    protected void OnDisable()
    {
        UpdateBindingDisplay();
        m_RebindOperation?.Dispose();
        m_RebindOperation = null;

        s_RebindActionUIs.Remove(this);
        if (s_RebindActionUIs.Count == 0)
        {
            s_RebindActionUIs = null;
            InputSystem.onActionChange -= OnActionChange;
        }
    }

#if UNITY_EDITOR
    // We want the label for the action name to update in edit mode, too, so
    // we kick that off from here.
    protected void OnValidate()
    {
        UpdateActionLabel();
        UpdateBindingDisplay();
    }
#endif

    // ---------------------- Public API ----------------------

    public void BindToManager(RebindManager manager)
    {
        rebindManager = manager;
    }

    public void SetIcon(Sprite sprite)
    {
        // Set sprite
        m_PromptIcon.sprite = sprite;
    }

    /// <summary>
    /// Return the action and binding index for the binding that is targeted by the component
    /// according to <see cref="m_BindingId"/> and <see cref="m_Action"/>.
    /// </summary>
    public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
    {
        bindingIndex = -1;

        action = m_Action?.action;
        if (action == null)
            return false;

        if (string.IsNullOrEmpty(m_BindingId))
            return false;

        // Look up binding index.
        var bindingId = new Guid(m_BindingId);
        bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
        if (bindingIndex == -1)
        {
            Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Trigger a refresh of the currently displayed binding.
    /// </summary>
    public void UpdateBindingDisplay()
    {
        var displayString = string.Empty;
        var deviceLayoutName = default(string);
        var controlPath = default(string);

        // Get display string from action.
        var action = m_Action?.action;
        if (action != null)
        {
            var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);
            if (bindingIndex != -1)
            {
                displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, displayStringOptions);
            }
        }

        // Set on label (if any).
        if (m_BindingText != null)
            m_BindingText.text = displayString;

        // Give listeners a chance to configure UI in response.
        m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
    }

    public string GetDeviceLayout()
    {
        var displayString = string.Empty;
        var deviceLayoutName = default(string);
        var controlPath = default(string);

        // Get display string from action.
        var action = m_Action?.action;
        if (action != null)
        {
            var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);
            if (bindingIndex != -1)
            {
                displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, displayStringOptions);
            }
        }
        return deviceLayoutName;
    }

    /// <summary>
    /// Remove currently applied binding overrides.
    /// </summary>
    public void ResetToDefault()
    {
        if (!ResolveActionAndBinding(out var action, out var bindingIndex))
            return;

        if (action.bindings[bindingIndex].isComposite)
        {
            // It's a composite. Remove overrides from part bindings.
            for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                action.RemoveBindingOverride(i);
        }
        else
        {
            action.RemoveBindingOverride(bindingIndex);
        }
        prevOverride = null; //remove stored last override
        ReboundAction?.Invoke(this, actionReference, m_BindingId, m_RebindOperation);
        UpdateBindingDisplay();
    }

    public void RevertLastBinding()
    {
        if (!ResolveActionAndBinding(out var action, out var bindingIndex))
            return;
        //Revert to last binding
        if (prevOverride != null && prevOverride.Length > 0)
        {
            action.ApplyBindingOverride(bindingIndex, prevOverride);
        }

    }

    /// <summary>
    /// Initiate an interactive rebind that lets the player actuate a control to choose a new binding
    /// for the action.
    /// </summary>
    public void StartInteractiveRebind()
    {
        if (!ResolveActionAndBinding(out var action, out var bindingIndex))
            return;

        // If the binding is a composite, we need to rebind each part in turn.
        if (action.bindings[bindingIndex].isComposite)
        {
            var firstPartIndex = bindingIndex + 1;
            if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
        }
        else
        {
            PerformInteractiveRebind(action, bindingIndex);
        }
    }

    void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
    {
        m_RebindOperation?.Cancel(); // Will null out m_RebindOperation.
        prevOverride = action.bindings[bindingIndex].overridePath;
        void CleanUp()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;
            action.Enable(); // re-enable action
        }

        action.Disable(); // disable action before it rebinds
        // Configure the rebind.
        m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            // make Escape cancel the entire rebind flow (optional)
            .WithCancelingThrough(rebindManager.cancelRebindKey)
            .OnPotentialMatch(op =>
            {
                // iterate candidates and remove those we forbid
                for (int i = op.candidates.Count - 1; i >= 0; --i)
                {
                    var control = op.candidates[i];
                    if (control == null) continue;
                    var path = control.path;
                    string controlName = control.name;
                    bool isAnyControlName = controlName == "anyKey" || controlName == "anyButton" || controlName == "any";
                    //Check if this is the any key candidate. If so, always remove
                    if (isAnyControlName)
                    {
                        op.RemoveCandidate(control);
                    }
                    else
                    {
                        bool removedCandidate = false;
                        // If not, check if the path is in the unbindable keys. 
                        for (int fp = 0; fp < rebindManager.unbindableKeys.paths.Count; ++fp)
                        {
                            string normalisedCandidate = rebindManager.NormalizeToBindingPath(path);
                            string normalisedBadKey = rebindManager.NormalizeToBindingPath(rebindManager.unbindableKeys.paths[fp]);
                            if (string.Equals(normalisedCandidate, normalisedBadKey, StringComparison.OrdinalIgnoreCase))//string.Equals(path, rebindManager.unbindableKeys.paths[fp], StringComparison.OrdinalIgnoreCase))
                            {
                                op.RemoveCandidate(control);
                                removedCandidate = true;
                            }
                        }
                        if (removedCandidate)
                        {
                            //Rebind is invalid, so tell the rebind manager
                            RebindInvalid?.Invoke(this, action);
                        }
                    }
                }
            })
            .OnCancel(
                operation =>
                {
                    m_RebindStopEvent?.Invoke(this, operation);
                    UpdateBindingDisplay();
                    CleanUp();
                })
            .OnComplete(
                operation =>
                {
                    ReboundAction?.Invoke(this, actionReference, m_BindingId, operation);
                    UpdateBindingDisplay();
                    CleanUp();
                    // If there's more composite parts we should bind, initiate a rebind
                    // for the next part.
                    if (allCompositeParts)
                    {
                        var nextBindingIndex = bindingIndex + 1;
                        if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                            PerformInteractiveRebind(action, nextBindingIndex, true);
                    }
                });

        // If it's a part binding, show the name of the part in the UI.
        var partName = default(string);
        if (action.bindings[bindingIndex].isPartOfComposite)
            partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

        // Give listeners a chance to act on the rebind starting.
        m_RebindStartEvent?.Invoke(this, m_RebindOperation);
        m_RebindOperation.Start();
    }

    // When the action system re-resolves bindings, we want to update our UI in response. While this will
    // also trigger from changes we made ourselves, it ensures that we react to changes made elsewhere. If
    // the user changes keyboard layout, for example, we will get a BoundControlsChanged notification and
    // will update our UI to reflect the current keyboard layout.
    private static void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.BoundControlsChanged)
            return;

        var action = obj as InputAction;
        var actionMap = action?.actionMap ?? obj as InputActionMap;
        var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

        for (var i = 0; i < s_RebindActionUIs.Count; ++i)
        {
            var component = s_RebindActionUIs[i];
            var referencedAction = component.actionReference?.action;
            if (referencedAction == null)
                continue;

            if (referencedAction == action ||
                referencedAction.actionMap == actionMap ||
                referencedAction.actionMap?.asset == actionAsset)
                component.UpdateBindingDisplay();
        }
    }

    private void UpdateActionLabel()
    {
        if (m_ActionLabel != null)
        {
            var action = m_Action?.action;
            m_ActionLabel.text = action != null ? action.name : string.Empty;
        }
    }
    // ---------------------- Nested classes ----------------------

    [Serializable]
    public class UpdateBindingUIEvent : UnityEvent<RebindActionUICustom, string, string, string>
    {
    }

    [Serializable]
    public class InteractiveRebindEvent : UnityEvent<RebindActionUICustom, InputActionRebindingExtensions.RebindingOperation>
    {
    }
}
