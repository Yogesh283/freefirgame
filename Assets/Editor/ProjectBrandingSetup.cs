using UnityEngine;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;

// Automatically applies the game logo as app icon (all platforms + Android adaptive icons)
// and as splash screen logo. Runs once per editor session, or manually via the menu item.
public static class ProjectBrandingSetup
{
    const string LogoPath = "Assets/MFPS/Content/Art/SplashLogo.jpeg";
    const string SessionKey = "ProjectBrandingSetup_Applied";

    [InitializeOnLoadMethod]
    static void AutoRun()
    {
        if (SessionState.GetBool(SessionKey, false)) return;
        EditorApplication.delayCall += () =>
        {
            if (Apply()) SessionState.SetBool(SessionKey, true);
        };
    }

    [MenuItem("Tools/Apply Logo (Icon + Splash)")]
    static void ApplyMenu()
    {
        if (Apply())
            EditorUtility.DisplayDialog("Branding", "Logo set ho gaya: App Icon + Android Adaptive Icons + Splash Screen.", "OK");
    }

    public static bool ApplyBranding() => Apply();

    static bool Apply()
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(LogoPath);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(LogoPath);

        if (tex == null)
        {
            Debug.LogError($"[Branding] Logo nahi mila: {LogoPath}");
            return false;
        }

        // Default icon for every platform
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { tex }, IconKind.Any);

        // Android: adaptive icons (background + foreground)
        SetAndroidIcons(AndroidPlatformIconKind.Adaptive, tex, twoLayers: true);

        // Keep the splash sequence, remove Unity branding, and show only Shadow.
        // Unity 6 allows this on every license tier, including Personal.
        PlayerSettings.SplashScreen.show = true;
        PlayerSettings.SplashScreen.showUnityLogo = false;

        if (sprite != null)
        {
            PlayerSettings.SplashScreen.logos = new[]
            {
                PlayerSettings.SplashScreenLogo.Create(2f, sprite)
            };
        }
        else
        {
            Debug.LogWarning("[Branding] Sprite load nahi hua, splash logo skip kiya. Texture Type 'Sprite (2D and UI)' hona chahiye.");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Branding] Unity logo removed; Shadow app icon and splash logo applied.");
        return true;
    }

    [MenuItem("Tools/Build Test APK (Legacy)")]
    static void BuildTestApkLegacy()
    {
        EditorApplication.ExecuteMenuItem("Tools/Shadow/5. Build Test APK (Debug)");
    }

    static void SetAndroidIcons(PlatformIconKind kind, Texture2D tex, bool twoLayers)
    {
        var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
        foreach (var icon in icons)
        {
            if (twoLayers)
                icon.SetTextures(tex, tex);
            else
                icon.SetTexture(tex);
        }
        PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
    }
}
