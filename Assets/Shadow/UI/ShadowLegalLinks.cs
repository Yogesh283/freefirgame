using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Adds a small Terms & Privacy link to the Shadow main menu.
/// The legal page is hosted separately so its content can be updated without rebuilding the game.
/// </summary>
public static class ShadowLegalLinks
{
    private const string MainMenuScene = "MainMenu";
    private const string RootName = "Shadow Legal Links";
    private const string LegalUrl = "https://shadowiq.fun/legal.html";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AddToScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddToScene(scene);
    }

    private static void AddToScene(Scene scene)
    {
        if (!scene.IsValid() || scene.name != MainMenuScene) return;
        if (GameObject.Find(RootName) != null) return;

        EnsureEventSystem();

        var root = new GameObject(RootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        SceneManager.MoveGameObjectToScene(root, scene);

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var buttonObject = new GameObject("Terms & Privacy", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(root.transform, false);

        var buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 22f);
        buttonRect.sizeDelta = new Vector2(300f, 54f);

        var image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.02f, 0.02f, 0.02f, 0.72f);

        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(OpenLegalPage);

        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.85f, 0.15f, 1f);
        colors.pressedColor = new Color(1f, 0.7f, 0.05f, 1f);
        button.colors = colors;

        var textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        var textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = "TERMS & PRIVACY";
        label.fontSize = 25f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null) return;

        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Object.DontDestroyOnLoad(eventSystem);
    }

    private static void OpenLegalPage()
    {
        Application.OpenURL(LegalUrl);
    }
}
