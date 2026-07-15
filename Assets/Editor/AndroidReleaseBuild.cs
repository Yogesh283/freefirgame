using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Clean release builds for Shadow — Play Store AAB + signed release APK.
/// Menu: Tools → Shadow → ...
/// </summary>
public static class AndroidReleaseBuild
{
    const string LogoPath = "Assets/MFPS/Content/Art/SplashLogo.jpeg";
    const string KeystoreDir = "Build/Android/signing";
    const string KeystorePath = KeystoreDir + "/shadow-release.keystore";
    const string KeystoreAlias = "shadow";
    const string KeystorePassword = "Shadow@2026";
    const string KeyPassword = "Shadow@2026";

    static readonly string[] Scenes =
    {
        "Assets/MFPS/Scenes/MainMenu.unity",
        "Assets/MFPS/Scenes/RoomUI.unity",
        "Assets/MFPS/Scenes/ExampleLevel.unity"
    };

    [MenuItem("Tools/Shadow/1. Setup Android Release Settings")]
    static void SetupReleaseSettingsMenu()
    {
        ConfigureReleaseSettings();
        EditorUtility.DisplayDialog("Shadow Release",
            "Android release settings apply ho gayi.\n\n" +
            "Package: com.shadow.game\n" +
            "Version: 1.0.0 (code 1)\n" +
            "IL2CPP + ARM64\n\n" +
            "Ab signing ke liye: Tools → Shadow → 2. Create Signing Keystore",
            "OK");
    }

    [MenuItem("Tools/Shadow/2. Create Signing Keystore")]
    static void CreateKeystoreMenu()
    {
        if (CreateKeystoreIfNeeded())
        {
            ConfigureSigning();
            EditorUtility.DisplayDialog("Shadow Signing",
                "Keystore ban gaya aur project mein set ho gaya.\n\n" +
                $"Path: {KeystorePath}\n" +
                $"Alias: {KeystoreAlias}\n" +
                $"Password: {KeystorePassword}\n\n" +
                "IMPORTANT: Ye password save kar lo! Play Store updates ke liye hamesha chahiye.",
                "OK");
        }
    }

    [MenuItem("Tools/Shadow/3. Build Play Store AAB (Release)")]
    static void BuildPlayStoreAabMenu() => BuildAndroid(release: true, appBundle: true);

    [MenuItem("Tools/Shadow/4. Build Release APK")]
    static void BuildReleaseApkMenu() => BuildAndroid(release: true, appBundle: false);

    [MenuItem("Tools/Shadow/5. Build Test APK (Debug)")]
    static void BuildTestApkMenu() => BuildAndroid(release: false, appBundle: false);

    static void ConfigureReleaseSettings()
    {
        ProjectBrandingSetup.ApplyBranding();

        PlayerSettings.companyName = "Shadow";
        PlayerSettings.productName = "Shadow";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.Android.bundleVersionCode = 1;

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.shadow.game");
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Android, Il2CppCompilerConfiguration.Release);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        PlayerSettings.Android.useAPKExpansionFiles = false;
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Medium);

        // Landscape FPS game
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;

        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.connectProfiler = false;
        EditorUserBuildSettings.allowDebugging = false;

        AssetDatabase.SaveAssets();
        Debug.Log("[Shadow Build] Release settings configured.");
    }

    static void ConfigureSigning()
    {
        var fullPath = Path.GetFullPath(KeystorePath);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"[Shadow Build] Keystore nahi mila: {fullPath}");
            return;
        }

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = fullPath;
        PlayerSettings.Android.keystorePass = KeystorePassword;
        PlayerSettings.Android.keyaliasName = KeystoreAlias;
        PlayerSettings.Android.keyaliasPass = KeyPassword;
        Debug.Log("[Shadow Build] Signing configured.");
    }

    static bool CreateKeystoreIfNeeded()
    {
        var fullPath = Path.GetFullPath(KeystorePath);
        if (File.Exists(fullPath))
        {
            ConfigureSigning();
            Debug.Log("[Shadow Build] Keystore already exists, signing re-applied.");
            return true;
        }

        Directory.CreateDirectory(Path.GetFullPath(KeystoreDir));

        var keytool = FindKeytool();
        if (string.IsNullOrEmpty(keytool))
        {
            EditorUtility.DisplayDialog("Error",
                "keytool nahi mila. Unity Android Build Support install karo.", "OK");
            return false;
        }

        var args =
            $"-genkeypair -v " +
            $"-keystore \"{fullPath}\" " +
            $"-alias {KeystoreAlias} " +
            $"-keyalg RSA -keysize 2048 -validity 10000 " +
            $"-storepass {KeystorePassword} " +
            $"-keypass {KeyPassword} " +
            $"-dname \"CN=Shadow Game, OU=Shadow, O=Shadow, L=India, ST=India, C=IN\"";

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = keytool,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var proc = System.Diagnostics.Process.Start(psi);
        proc.WaitForExit();
        var output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();

        if (proc.ExitCode != 0)
        {
            Debug.LogError($"[Shadow Build] keytool failed:\n{output}");
            EditorUtility.DisplayDialog("Keystore Error", output, "OK");
            return false;
        }

        Debug.Log($"[Shadow Build] Keystore created:\n{output}");
        return true;
    }

    static string FindKeytool()
    {
        var unityKeytool =
            @"C:\Program Files\Unity\Hub\Editor\6000.3.17f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\bin\keytool.exe";
        if (File.Exists(unityKeytool)) return unityKeytool;

        var javaHome = System.Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome))
        {
            var p = Path.Combine(javaHome, "bin", "keytool.exe");
            if (File.Exists(p)) return p;
        }
        return null;
    }

    static void BuildAndroid(bool release, bool appBundle)
    {
        ConfigureReleaseSettings();

        if (!File.Exists(Path.GetFullPath(KeystorePath)))
        {
            if (!EditorUtility.DisplayDialog("Signing Required",
                "Release build ke liye keystore chahiye. Abhi banau?",
                "Haan, Create Keystore", "Cancel"))
                return;
            if (!CreateKeystoreIfNeeded()) return;
        }
        ConfigureSigning();

        Directory.CreateDirectory(Path.GetFullPath("Build/Release"));

        EditorUserBuildSettings.buildAppBundle = appBundle;
        EditorUserBuildSettings.development = !release;
        EditorUserBuildSettings.connectProfiler = !release;
        EditorUserBuildSettings.allowDebugging = !release;

        string outputPath = appBundle
            ? "Build/Release/Shadow.aab"
            : release ? "Build/Release/Shadow.apk" : "Build/Release/Shadow-test.apk";

        var buildOptions = release ? BuildOptions.None : BuildOptions.Development;

        var options = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = buildOptions
        };

        Debug.Log($"[Shadow Build] Building {(appBundle ? "AAB" : "APK")} → {outputPath}");
        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            var sizeMb = report.summary.totalSize / (1024f * 1024f);
            Debug.Log($"[Shadow Build] SUCCESS — {outputPath} ({sizeMb:F1} MB)");
            EditorUtility.RevealInFinder(outputPath);
            EditorUtility.DisplayDialog("Build Complete",
                $"{(appBundle ? "AAB (Play Store)" : "APK")} ready!\n\n" +
                $"File: {outputPath}\n" +
                $"Size: {sizeMb:F1} MB\n\n" +
                (appBundle
                    ? "Play Console → Create app → Upload this AAB"
                    : "Phone pe install karke test karo"),
                "OK");
        }
        else
        {
            Debug.LogError($"[Shadow Build] FAILED — {report.summary.result}");
            EditorUtility.DisplayDialog("Build Failed",
                $"Build fail hui: {report.summary.result}\n\nConsole check karo.", "OK");
        }
    }
}
