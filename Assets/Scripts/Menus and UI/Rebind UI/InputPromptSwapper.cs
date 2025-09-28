using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class InputPromptSwapper : MonoBehaviour
{
    [Serializable]
    public class PromptBinding
    {
        [Tooltip("The InputAction in your InputActionAsset")]
        public InputActionReference action;
        [Tooltip("The Image component that will display the glyph for this action")]
        public Image image;
        [Tooltip("Optional: A Text component to display the readable binding (e.g. 'Space' or 'A')")]
        public TMPro.TextMeshProUGUI bindingLabel;
    }

    [Header("References")]
    [SerializeField] ControlIconDatabase db;
    [Tooltip("List of actions and the UI Image that should show their icon")]
    [SerializeField] List<PromptBinding> prompts = new List<PromptBinding>();
    [Tooltip("Name of your keyboard/mouse control scheme as in the Input Actions asset")]
    [SerializeField] string keyboardSchemeName = "Keyboard";
    [Tooltip("Name of your gamepad control scheme as in the Input Actions asset")]
    [SerializeField] string gamepadSchemeName = "Gamepad";

    PlayerInput playerInput;
    PlayerActions playerActions;

    void Start()
    {
        playerInput = FindObjectOfType<PlayerInput>();
        playerActions = FindObjectOfType<PlayerInputScript>().actions;
        RebindManager rebindManager = FindObjectOfType<RebindManager>(true);
        if (rebindManager) rebindManager.inputsRebound += () => UpdatePrompts(playerInput.currentControlScheme);
        if (playerInput == null) Debug.LogError("InputPromptSwapper: No PlayerInput found on object or assigned in inspector.");

        // Subscribe to control scheme changes
        playerInput.onControlsChanged += OnControlsChanged;
        UpdatePrompts(playerInput.currentControlScheme);
    }


    private void OnControlsChanged(PlayerInput pi)
    {
        // Called when PlayerInput detects a different device/scheme is active. Change the button prompts
        UpdatePrompts(pi.currentControlScheme);
    }

    /// <summary>
    /// Update all prompt images and optional labels based on the given control scheme name.
    /// </summary>
    public void UpdatePrompts(string controlScheme)
    {
        foreach (var pb in prompts)
        {
            if (pb.image != null && pb.action != null)
            {
                Sprite sprite = GetSpriteForAction(pb.action, controlScheme);
                if (sprite != null)
                {
                    pb.image.sprite = sprite;
                    pb.image.enabled = true;
                }
                else
                {
                    // No sprite for this scheme - hide the image (or assign a fallback)
                    pb.image.enabled = false;
                    pb.image.sprite = null;
                }
            }


            if (pb.bindingLabel != null)
            {
                // Attempt to display a human readable binding for the current scheme.
                pb.bindingLabel.text = pb.action.action.name;//GetActionName(pb.action, controlScheme);
            }
        }
    }


    Sprite GetSpriteForAction(InputAction action, string controlScheme)
    {
        //Get the icon from the database
        if (playerActions == null) return null;
        if (action == null) return null;
        List<InputBinding> bind = action.bindings.ToList();
        foreach (InputBinding binding in bind)
        {
            string path = binding.overridePath != null ? binding.overridePath : binding.effectivePath; //use override if possible
            if (path.Contains(controlScheme))
            {
                //get the sprite
                Sprite sprite = db.GetIconForPath(path);
                if (sprite != null) return sprite;
            }


        }
        return null;
    }

    void OnEnable() => InputSystem.onDeviceChange += OnDeviceChange;
    void OnDisable() => InputSystem.onDeviceChange -= OnDeviceChange;

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        //Debug.Log($"Device change: {device.displayName} ({device.path}) -> {change}");
        if (change == InputDeviceChange.Added)
        {
            // device plugged in
        }
        else if (change == InputDeviceChange.Removed)
        {
            // device unplugged
        }
    }
}