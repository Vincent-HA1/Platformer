// ControlPathTypes.cs
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable wrapper for a list of control paths so that a PropertyDrawer can target it.
/// Replace your List<string> field with ControlPathList for the in-inspector picker UI.
/// </summary>
[Serializable]
public class ControlPathList
{
    public List<string> paths = new List<string>();
}

/// <summary>
/// Attribute you can put on a single string field to show the control-path picker for single-value fields.
/// Example: [ControlPathPicker] public string myControlPath;
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ControlPathPickerAttribute : PropertyAttribute
{
}
