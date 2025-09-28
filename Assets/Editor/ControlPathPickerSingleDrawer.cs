// Replace the relevant parts of your ControlPathPickerSingleDrawer.cs with this version
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ControlPathPickerAttribute))]
public class ControlPathPickerSingleDrawer : PropertyDrawer
{
    static readonly string[] layoutNames = new[] { "Keyboard", "Gamepad", "Mouse", "Custom" };
    static readonly string[] keyboardOptions = System.Enum.GetNames(typeof(UnityEngine.InputSystem.Key)).Select(n => n.ToLower()).ToArray();
    static readonly string[] gamepadOptions = new[] { "buttonSouth", "buttonNorth", "buttonWest", "buttonEast", "leftShoulder", "rightShoulder", "leftTrigger", "rightTrigger", "dpad/up", "dpad/down", "dpad/left", "dpad/right", "leftStick/x", "leftStick/y", "rightStick/x", "rightStick/y", "start", "select", "leftStickPress", "rightStickPress" };
    static readonly string[] mouseOptions = new[] { "leftButton", "rightButton", "middleButton", "scroll", "position", "delta" };

    class PickerState { public int layoutTab = 0; public int keyboardIndex = 0; public int gamepadIndex = 0; public int mouseIndex = 0; public string customPath = ""; public bool initializedFromProperty = false; }
    static Dictionary<string, PickerState> states = new Dictionary<string, PickerState>();

    PickerState GetState(SerializedProperty property)
    {
        var key = property.serializedObject.targetObject.GetInstanceID() + "/" + property.propertyPath;
        if (!states.TryGetValue(key, out var s))
        {
            s = new PickerState();
            states[key] = s;
        }
        // If not yet initialized from the serialized value, prefill once
        if (!s.initializedFromProperty)
        {
            PrefillFromPath(s, property.stringValue);
            s.initializedFromProperty = true;
        }
        return s;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        return line * 3 + spacing * 4;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var state = GetState(property);

        EditorGUI.BeginProperty(position, label, property);

        float line = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // Row 1: string field (shows current saved value)
        var textRect = new Rect(position.x, position.y, position.width, line);
        // allow the user to edit the raw string too; reflect into state.customPath only when changed
        EditorGUI.BeginChangeCheck();
        string newText = EditorGUI.TextField(textRect, label, property.stringValue);
        if (EditorGUI.EndChangeCheck())
        {
            property.stringValue = newText;
            // also update picker state so UI matches typed value
            PrefillFromPath(state, property.stringValue);
            property.serializedObject.ApplyModifiedProperties();
        }

        position.y += line + spacing;

        // Row 2: toolbar
        var toolbarRect = new Rect(position.x, position.y, position.width, line);
        int newTab = GUI.Toolbar(toolbarRect, state.layoutTab, layoutNames);
        position.y += line + spacing;

        // Row 3: selection controls — only write to property when user explicitly changes selection
        var selRect = new Rect(position.x, position.y, position.width, line);
        EditorGUI.BeginChangeCheck();
        switch (newTab)
        {
            case 0:
                int newKeyboardIndex = EditorGUI.Popup(selRect, "Key", state.keyboardIndex, keyboardOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    state.layoutTab = 0;
                    state.keyboardIndex = newKeyboardIndex;
                    property.stringValue = $"<Keyboard>/{keyboardOptions[Mathf.Clamp(state.keyboardIndex, 0, keyboardOptions.Length - 1)]}";
                    property.serializedObject.ApplyModifiedProperties();
                }
                break;
            case 1:
                int newGamepadIndex = EditorGUI.Popup(selRect, "Gamepad control", state.gamepadIndex, gamepadOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    state.layoutTab = 1;
                    state.gamepadIndex = newGamepadIndex;
                    property.stringValue = $"<Gamepad>/{gamepadOptions[Mathf.Clamp(state.gamepadIndex, 0, gamepadOptions.Length - 1)]}";
                    property.serializedObject.ApplyModifiedProperties();
                }
                break;
            case 2:
                int newMouseIndex = EditorGUI.Popup(selRect, "Mouse control", state.mouseIndex, mouseOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    state.layoutTab = 2;
                    state.mouseIndex = newMouseIndex;
                    property.stringValue = $"<Mouse>/{mouseOptions[Mathf.Clamp(state.mouseIndex, 0, mouseOptions.Length - 1)]}";
                    property.serializedObject.ApplyModifiedProperties();
                }
                break;
            case 3:
                string newCustom = EditorGUI.TextField(selRect, "Control path", state.customPath);
                if (EditorGUI.EndChangeCheck())
                {
                    state.layoutTab = 3;
                    state.customPath = newCustom;
                    property.stringValue = state.customPath;
                    property.serializedObject.ApplyModifiedProperties();
                }
                break;
        }

        // update layoutTab if toolbar was toggled (toolbar changes don't trigger EndChangeCheck above)
        state.layoutTab = newTab;

        EditorGUI.EndProperty();
    }

    static void PrefillFromPath(PickerState s, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            s.layoutTab = 0;
            s.customPath = "";
            return;
        }

        if (path.StartsWith("<Keyboard>/", System.StringComparison.OrdinalIgnoreCase))
        {
            var k = path.Substring("<Keyboard>/".Length).ToLower();
            int idx = System.Array.FindIndex(keyboardOptions, x => x == k);
            if (idx >= 0) { s.layoutTab = 0; s.keyboardIndex = idx; s.customPath = ""; return; }
        }
        if (path.StartsWith("<Gamepad>/", System.StringComparison.OrdinalIgnoreCase))
        {
            var g = path.Substring("<Gamepad>/".Length);
            int idx = System.Array.FindIndex(gamepadOptions, x => x == g);
            if (idx >= 0) { s.layoutTab = 1; s.gamepadIndex = idx; s.customPath = ""; return; }
        }
        if (path.StartsWith("<Mouse>/", System.StringComparison.OrdinalIgnoreCase))
        {
            var m = path.Substring("<Mouse>/".Length);
            int idx = System.Array.FindIndex(mouseOptions, x => x == m);
            if (idx >= 0) { s.layoutTab = 2; s.mouseIndex = idx; s.customPath = ""; return; }
        }

        // fallback: custom
        s.layoutTab = 3;
        s.customPath = path;
    }
}
#endif
