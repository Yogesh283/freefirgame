using System.Linq;
using UnityEditor;
using UnityEditor.Build;

/// <summary>
/// Ensures the MFPSM scripting define is set, which activates all the mobile-control
/// hooks inside the MFPS core scripts (bl_FirstPersonController, MouseLook, bl_Gun, etc.)
/// so they use our custom touch controls implementation (Assets/MobileControls).
/// </summary>
[InitializeOnLoad]
public static class MobileControlDefine
{
    const string Define = "MFPSM";

    static MobileControlDefine()
    {
        EditorApplication.delayCall += () =>
        {
            EnsureDefine(NamedBuildTarget.Android);
            EnsureDefine(NamedBuildTarget.iOS);
            EnsureDefine(NamedBuildTarget.Standalone);
        };
    }

    static void EnsureDefine(NamedBuildTarget target)
    {
        PlayerSettings.GetScriptingDefineSymbols(target, out string[] defines);
        if (defines.Contains(Define)) return;

        var list = defines.ToList();
        list.Add(Define);
        PlayerSettings.SetScriptingDefineSymbols(target, list.ToArray());
        UnityEngine.Debug.Log($"[MobileControls] '{Define}' define added for {target.TargetName}.");
    }
}
