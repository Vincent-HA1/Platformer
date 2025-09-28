#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor window to let designers map controls to icons without typing paths.
/// - Choose layout (Keyboard / Gamepad / Mouse / Joystick)
/// - Choose a control from a dropdown (populated with common controls)
/// - Assign a sprite and optional label
/// - Click Add (writes into selected ControlIconDatabase asset)
/// </summary>
public class ControlIconEditorWindow : EditorWindow
{
    ControlIconDatabase db;
    int layoutTab = 0; // 0=Keyboard,1=Gamepad,2=Mouse,3=Joystick/custom
    int keyboardIndex = 0;
    int gamepadIndex = 0;
    int mouseIndex = 0;
    string customPath = "";

    Sprite spriteToAssign;
    string labelText = "";

    static readonly string[] layoutNames = new[] { "Keyboard", "Gamepad", "Mouse", "Joystick/Custom" };

    // Populate keyboard keys from Key enum
    static readonly string[] keyboardOptions = System.Enum.GetNames(typeof(UnityEngine.InputSystem.Key))
        .Select(n => n.ToLower()) // show lowercase for readability
        .ToArray();

    // Minimal list of common gamepad control names (standard)
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

    [MenuItem("Window/Input/Control Icon Editor")]
    public static void ShowWindow()
    {
        var w = GetWindow<ControlIconEditorWindow>("Control Icon Editor");
        w.minSize = new Vector2(520, 360);
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Select a ControlIconDatabase asset, pick layout -> control -> sprite, then click Add to store the canonical control path.", MessageType.Info);

        EditorGUILayout.Space();
        db = (ControlIconDatabase)EditorGUILayout.ObjectField("Database Asset", db, typeof(ControlIconDatabase), false);

        EditorGUILayout.Space();
        layoutTab = GUILayout.Toolbar(layoutTab, layoutNames);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pick control and icon", EditorStyles.boldLabel);

        switch (layoutTab)
        {
            case 0: // Keyboard
                keyboardIndex = EditorGUILayout.Popup("Key", keyboardIndex, keyboardOptions);
                break;
            case 1: // Gamepad
                gamepadIndex = EditorGUILayout.Popup("Gamepad control", gamepadIndex, gamepadOptions);
                break;
            case 2: // Mouse
                mouseIndex = EditorGUILayout.Popup("Mouse control", mouseIndex, mouseOptions);
                break;
            case 3: // custom
                EditorGUILayout.LabelField("Enter a custom control path (e.g. \"<Gamepad>/buttonSouth\"):");
                customPath = EditorGUILayout.TextField("Control path", customPath);
                break;
        }

        EditorGUILayout.Space();
        spriteToAssign = (Sprite)EditorGUILayout.ObjectField("Icon Sprite", spriteToAssign, typeof(Sprite), false);
        labelText = EditorGUILayout.TextField("Optional Label", labelText);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = db != null && spriteToAssign != null;
        if (GUILayout.Button("Add / Update entry", GUILayout.Height(40)))
        {
            string path = BuildPathFromSelection();
            if (!string.IsNullOrEmpty(path))
            {
                db.AddOrUpdate(path, spriteToAssign, string.IsNullOrEmpty(labelText) ? path : labelText);
                EditorUtility.SetDirty(db);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Saved", $"Added icon for: {path}", "OK");
            }
            else EditorUtility.DisplayDialog("Error", "Could not resolve a control path for the selection.", "OK");
        }
        GUI.enabled = true;

        if (GUILayout.Button("Remove entry", GUILayout.Height(40)))
        {
            string path = BuildPathFromSelection();
            if (!string.IsNullOrEmpty(path))
            {
                db.Remove(path);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Removed", $"Removed entry for: {path}", "OK");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (db != null)
        {
            EditorGUILayout.LabelField("Current Database Entries", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Tip: you can also drag an action's effectivePath (action.bindings[index].effectivePath) into the Custom tab and add an icon.", MessageType.None);
            EditorGUILayout.BeginVertical("box");
            foreach (var e in db.entries)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(e.controlPath, GUILayout.Width(260));
                if (e.icon != null) GUILayout.Label(AssetPreview.GetAssetPreview(e.icon), GUILayout.Width(48), GUILayout.Height(48));
                else GUILayout.Label("(no sprite)", GUILayout.Width(48));
                GUILayout.Label(e.label ?? "", GUILayout.Width(120));
                if (GUILayout.Button("Edit"))
                {
                    // prefill fields for editing
                    spriteToAssign = e.icon;
                    labelText = e.label;
                    // Try to derive layout+index from path (simple heuristics)
                    PrefillFromPath(e.controlPath);
                }
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    db.Remove(e.controlPath);
                    AssetDatabase.SaveAssets();
                    break; // break to avoid modifying list while iterating
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }

    string BuildPathFromSelection()
    {
        switch (layoutTab)
        {
            case 0:
                // keyboardOptions entries are lowercase enum names
                {
                    string keyName = keyboardOptions[keyboardIndex];
                    // unify: Key enum names like "a", "space", "leftShift" -> path <Keyboard>/a
                    return $"<Keyboard>/{keyName}";
                }
            case 1:
                {
                    // gamepadOptions may include slashes (dpad/up) or stick axes
                    string g = gamepadOptions[gamepadIndex];
                    // if string contains '/', keep it as subpath
                    return $"<Gamepad>/{g}";
                }
            case 2:
                {
                    string m = mouseOptions[mouseIndex];
                    return $"<Mouse>/{m}";
                }
            case 3:
                {
                    return string.IsNullOrWhiteSpace(customPath) ? null : customPath.Trim();
                }
        }
        return null;
    }

    void PrefillFromPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        // try simple parsing for known prefixes
        if (path.StartsWith("<Keyboard>/", System.StringComparison.OrdinalIgnoreCase))
        {
            var k = path.Substring("<Keyboard>/".Length).ToLower();
            int idx = System.Array.FindIndex(keyboardOptions, x => x == k);
            if (idx >= 0) { layoutTab = 0; keyboardIndex = idx; return; }
        }
        if (path.StartsWith("<Gamepad>/", System.StringComparison.OrdinalIgnoreCase))
        {
            var g = path.Substring("<Gamepad>/".Length);
            int idx = System.Array.FindIndex(gamepadOptions, x => x == g);
            if (idx >= 0) { layoutTab = 1; gamepadIndex = idx; return; }
        }
        if (path.StartsWith("<Mouse>/", System.StringComparison.OrdinalIgnoreCase))
        {
            var m = path.Substring("<Mouse>/".Length);
            int idx = System.Array.FindIndex(mouseOptions, x => x == m);
            if (idx >= 0) { layoutTab = 2; mouseIndex = idx; return; }
        }
        // fallback: custom
        layoutTab = 3;
        customPath = path;
    }
}
#endif
