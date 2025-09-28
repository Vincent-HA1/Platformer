using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Input/Control Icon Database", fileName = "ControlIconDatabase")]
public class ControlIconDatabase : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [Tooltip("Canonical control path (e.g. \"<Keyboard>/a\", \"<Gamepad>/buttonSouth\").")]
        public string controlPath;

        [Tooltip("Optional human label")]
        public string label;

        public Sprite icon;
    }

    public List<Entry> entries = new List<Entry>();

    // Runtime lookup caches
    Dictionary<string, Sprite> _pathToSprite;

    void OnEnable()
    {
        BuildCache();
    }

    void BuildCache()
    {
        _pathToSprite = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in entries)
        {
            if (string.IsNullOrEmpty(e.controlPath)) continue;
            if (!_pathToSprite.ContainsKey(e.controlPath))
                _pathToSprite[e.controlPath] = e.icon;
        }
    }

    /// <summary>
    /// Get icon by exact control path (e.g. "<Gamepad>/buttonSouth").
    /// Returns null if not found.
    /// </summary>
    public Sprite GetIconForPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (_pathToSprite == null) BuildCache();

        // Exact match first
        if (_pathToSprite.TryGetValue(path, out var s)) return s;

        // Fallback: try match by trailing control name ("/buttonSouth")
        int slash = path.LastIndexOf('/');
        if (slash >= 0 && slash < path.Length - 1)
        {
            string name = path.Substring(slash + 1);
            // prefer layout-specific if exists (e.g. "<Gamepad>/buttonSouth" entries)
            foreach (var kv in _pathToSprite)
            {
                if (kv.Key.EndsWith("/" + name, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Convenience: pass an InputControl (eg. ctx.control) to get sprite.
    /// </summary>
    public Sprite GetIconForControl(InputControl control)
    {
        if (control == null) return null;
        return GetIconForPath(control.path);
    }

    /// <summary>
    /// Utility: add or replace an entry by path.
    /// </summary>
    public void AddOrUpdate(string controlPath, Sprite sprite, string label = null)
    {
        var e = entries.Find(x => string.Equals(x.controlPath, controlPath, StringComparison.OrdinalIgnoreCase));
        if (e != null) { e.icon = sprite; e.label = label; }
        else entries.Add(new Entry { controlPath = controlPath, icon = sprite, label = label });
        BuildCache();
        UnityEditor.EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Utility: remove entry
    /// </summary>
    public void Remove(string controlPath)
    {
        entries.RemoveAll(x => string.Equals(x.controlPath, controlPath, StringComparison.OrdinalIgnoreCase));
        BuildCache();
        UnityEditor.EditorUtility.SetDirty(this);
    }
}
