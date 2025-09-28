// Editor/ControlPathPropertyDrawers_Reorderable_Fixed.cs
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(ControlPathList))]
public class ControlPathListDrawer : PropertyDrawer
{
    static readonly string[] layoutNames = new[] { "Keyboard", "Gamepad", "Mouse", "Custom" };
    static readonly string[] keyboardOptions = System.Enum.GetNames(typeof(UnityEngine.InputSystem.Key))
        .Select(n => n.ToLower()).ToArray();
    static readonly string[] gamepadOptions = new[]
    {
        "buttonSouth", "buttonNorth", "buttonWest", "buttonEast",
        "leftShoulder", "rightShoulder",
        "leftTrigger", "rightTrigger",
        "dpad/up", "dpad/down", "dpad/left", "dpad/right",
        "leftStick/x", "leftStick/y", "rightStick/x", "rightStick/y",
        "start", "select", "leftStickPress", "rightStickPress"
    };
    static readonly string[] mouseOptions = new[]
    {
        "leftButton", "rightButton", "middleButton", "scroll",
        "position", "delta"
    };

    class PickerState { public int layoutTab = 0; public int keyboardIndex = 0; public int gamepadIndex = 0; public int mouseIndex = 0; public string customPath = ""; public ReorderableList list = null; }
    static Dictionary<string, PickerState> states = new Dictionary<string, PickerState>();

    PickerState GetState(SerializedProperty property)
    {
        string key = property.serializedObject.targetObject.GetInstanceID() + "/" + property.propertyPath;
        if (!states.TryGetValue(key, out var s))
        {
            s = new PickerState();
            states[key] = s;
        }

        // recreate list if needed (different serializedObject or null)
        if (s.list == null || s.list.serializedProperty.serializedObject != property.serializedObject)
            s.list = CreateReorderableList(property, s);

        return s;
    }

    ReorderableList CreateReorderableList(SerializedProperty property, PickerState state)
    {
        var listProp = property.FindPropertyRelative("paths");

        // create RL first so closures can reference it
        ReorderableList rl = null;
        rl = new ReorderableList(property.serializedObject, listProp, true, true, true, true);

        rl.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Control paths");
        };

        rl.elementHeightCallback = (int index) =>
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
        };

        rl.drawElementCallback = (Rect rect, int index, bool active, bool focused) =>
        {
            // guard against out-of-range indices (can happen during reordering/deletion)
            if (listProp == null) return;
            if (index < 0 || index >= listProp.arraySize) return;

            var element = listProp.GetArrayElementAtIndex(index);
            if (element == null) return;

            rect.y += 2;
            float line = EditorGUIUtility.singleLineHeight;
            Rect labelRect = new Rect(rect.x + 8, rect.y, rect.width - 160 - 12, line);
            Rect prefillRect = new Rect(rect.x + rect.width - 144, rect.y, 64, line);
            Rect removeRect = new Rect(rect.x + rect.width - 72, rect.y, 64, line);

            EditorGUI.LabelField(labelRect, element.stringValue);

            if (GUI.Button(prefillRect, "Prefill"))
            {
                PrefillFromPath(state, element.stringValue);
            }

            if (GUI.Button(removeRect, "Remove"))
            {
                // delegate deletion to the ReorderableList's remove callback so the list stays consistent
                rl.index = index;
                rl.onRemoveCallback?.Invoke(rl);
            }
        };

        rl.onAddCallback = (ReorderableList l) =>
        {
            int insertIndex = listProp.arraySize;
            listProp.InsertArrayElementAtIndex(insertIndex);
            var newEl = listProp.GetArrayElementAtIndex(insertIndex);
            newEl.stringValue = "";
            property.serializedObject.ApplyModifiedProperties();
        };

        rl.onRemoveCallback = (ReorderableList l) =>
        {
            if (l.index < 0 || l.index >= listProp.arraySize) return;
            if (EditorUtility.DisplayDialog("Remove entry", $"Remove '{listProp.GetArrayElementAtIndex(l.index).stringValue}'?", "Yes", "No"))
            {
                listProp.DeleteArrayElementAtIndex(l.index);
                property.serializedObject.ApplyModifiedProperties();
            }
        };

        rl.onReorderCallbackWithDetails = (ReorderableList l, int oldIndex, int newIndex) =>
        {
            property.serializedObject.ApplyModifiedProperties();
        };

        return rl;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float height = line; // foldout

        if (!property.isExpanded) return height + spacing;

        var state = GetState(property);
        // toolbar (1), selection (1), add/clear buttons (1)
        float toolbarArea = line * 3 + spacing * 4;
        height += spacing + toolbarArea;

        // reorderable list height
        if (state.list != null)
            height += state.list.GetHeight() + spacing;

        height += spacing * 2;
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.serializedObject.Update();
        EditorGUI.BeginProperty(position, label, property);

        float line = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // foldout
        Rect foldRect = new Rect(position.x, position.y, position.width, line);
        property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, label, true);
        position.y += line + spacing;

        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
            return;
        }

        var state = GetState(property);
        var listProp = property.FindPropertyRelative("paths");
        if (listProp == null)
        {
            EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, line * 2), "ControlPathList.paths not found.", MessageType.Warning);
            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
            return;
        }

        // tabs
        Rect toolbarRect = new Rect(position.x, position.y, position.width, line);
        state.layoutTab = GUI.Toolbar(toolbarRect, state.layoutTab, layoutNames);
        position.y += line + spacing;

        // selection row
        Rect selRect = new Rect(position.x, position.y, position.width, line);
        switch (state.layoutTab)
        {
            case 0:
                state.keyboardIndex = EditorGUI.Popup(selRect, "Key", state.keyboardIndex, keyboardOptions);
                break;
            case 1:
                state.gamepadIndex = EditorGUI.Popup(selRect, "Gamepad control", state.gamepadIndex, gamepadOptions);
                break;
            case 2:
                state.mouseIndex = EditorGUI.Popup(selRect, "Mouse control", state.mouseIndex, mouseOptions);
                break;
            case 3:
                state.customPath = EditorGUI.TextField(selRect, "Control path", state.customPath);
                break;
        }
        position.y += line + spacing;

        // Add / Clear buttons
        Rect btnRect = new Rect(position.x, position.y, position.width, line);
        float halfWidth = (btnRect.width - 8f) * 0.5f;
        if (GUI.Button(new Rect(btnRect.x, btnRect.y, halfWidth, btnRect.height), "Add Entry"))
        {
            string path = BuildPathFromState(state);
            if (!string.IsNullOrEmpty(path))
            {
                if (!ContainsPath(listProp, path))
                {
                    int idx = listProp.arraySize;
                    listProp.InsertArrayElementAtIndex(idx);
                    listProp.GetArrayElementAtIndex(idx).stringValue = path;
                    property.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    EditorUtility.DisplayDialog("Duplicate", $"The path '{path}' is already in the list.", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid", "Could not resolve a control path for the current selection.", "OK");
            }
        }
        if (GUI.Button(new Rect(btnRect.x + halfWidth + 8f, btnRect.y, halfWidth, btnRect.height), "Clear List"))
        {
            if (EditorUtility.DisplayDialog("Clear all", "Remove all control paths?", "Yes", "No"))
            {
                listProp.ClearArray();
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        position.y += line + spacing;

        // Draw reorderable list
        if (state.list != null)
        {
            var listRect = new Rect(position.x, position.y, position.width, state.list.GetHeight());
            state.list.DoList(listRect);
            position.y += state.list.GetHeight();
        }

        EditorGUI.EndProperty();
        property.serializedObject.ApplyModifiedProperties();
    }

    static string BuildPathFromState(PickerState s)
    {
        switch (s.layoutTab)
        {
            case 0:
                return $"<Keyboard>/{keyboardOptions[Mathf.Clamp(s.keyboardIndex, 0, keyboardOptions.Length - 1)]}";
            case 1:
                return $"<Gamepad>/{gamepadOptions[Mathf.Clamp(s.gamepadIndex, 0, gamepadOptions.Length - 1)]}";
            case 2:
                return $"<Mouse>/{mouseOptions[Mathf.Clamp(s.mouseIndex, 0, mouseOptions.Length - 1)]}";
            case 3:
                return string.IsNullOrWhiteSpace(s.customPath) ? null : s.customPath.Trim();
        }
        return null;
    }

    static void PrefillFromPath(PickerState s, string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        if (path.StartsWith("<Keyboard>/", System.StringComparison.OrdinalIgnoreCase))
        {
            var k = path.Substring("<Keyboard>/".Length).ToLower();
            int idx = System.Array.FindIndex(keyboardOptions, x => x == k);
            if (idx >= 0) { s.layoutTab = 0; s.keyboardIndex = idx; return; }
        }
        if (path.StartsWith("<Gamepad>/", System.StringComparison.OrdinalIgnoreCase))
        {
            var g = path.Substring("<Gamepad>/".Length);
            int idx = System.Array.FindIndex(gamepadOptions, x => x == g);
            if (idx >= 0) { s.layoutTab = 1; s.gamepadIndex = idx; return; }
        }
        if (path.StartsWith("<Mouse>/", System.StringComparison.OrdinalIgnoreCase))
        {
            var m = path.Substring("<Mouse>/".Length);
            int idx = System.Array.FindIndex(mouseOptions, x => x == m);
            if (idx >= 0) { s.layoutTab = 2; s.mouseIndex = idx; return; }
        }

        s.layoutTab = 3;
        s.customPath = path;
    }

    static bool ContainsPath(SerializedProperty listProp, string path)
    {
        for (int i = 0; i < listProp.arraySize; i++)
        {
            if (string.Equals(listProp.GetArrayElementAtIndex(i).stringValue, path, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
#endif
