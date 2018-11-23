using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InspectorLock
{
    
    [MenuItem("Tools/Toggle Inspector Lock %l")] // Ctrl + L
    static void ToggleInspectorLock()
    {
        ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }
}
