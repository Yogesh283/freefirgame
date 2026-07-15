using MFPS.Mobile;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Builds the whole touch-controls canvas at runtime (joystick, touch-look pad and action buttons)
/// and wires it to MFPS through bl_TouchHelper / bl_MFPSMobileControl.
/// Created automatically in gameplay scenes on mobile builds, hidden while the local player is dead.
/// </summary>
public class bl_MobileControlsUI : MonoBehaviour
{
    private static bl_MobileControlsUI current;
    private static Sprite circleSprite;

    private bl_TouchHelper helper;
    private MobileUIButton aimButton;

    #region Bootstrap
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!bl_UtilityHelper.isMobile) return;

        // Force landscape (left/right only) — Free Fire style
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        var go = new GameObject("MobileControlsBootstrap");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<MobileControlsBootstrap>();

        bl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;
        bl_EventHandler.onLocalPlayerDeath += OnLocalPlayerDeath;
    }

    private class MobileControlsBootstrap : MonoBehaviour
    {
        private void Update()
        {
            // Create the controls as soon as a gameplay scene is active so that
            // bl_TouchPad.Instance exists before MouseLook/bl_WeaponSway read it.
            if (current == null && bl_GameManager.Instance != null)
            {
                EnsureCreated();
            }
        }
    }

    private static void EnsureCreated()
    {
        if (current != null) return;
        var root = new GameObject("MobileControls");
        current = root.AddComponent<bl_MobileControlsUI>();
        current.Build();
    }

    private static void OnLocalPlayerSpawn()
    {
        EnsureCreated();
        bl_MFPSMobileControl.FireHeld = false;
        bl_MFPSMobileControl.AimActive = false;
        current.RefreshAimVisual();
        current.helper.SetMobileCanvasVisible(true);
    }

    private static void OnLocalPlayerDeath()
    {
        bl_MFPSMobileControl.FireHeld = false;
        bl_MFPSMobileControl.AimActive = false;
        if (current != null)
        {
            current.RefreshAimVisual();
            current.helper.SetMobileCanvasVisible(false);
        }
    }
    #endregion

    #region Build
    private void Build()
    {
        var settings = bl_MobileControlSettings.Instance;
        float scale = settings.controlsScale;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
        helper = gameObject.AddComponent<bl_TouchHelper>();

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            es.transform.SetParent(transform, false);
        }

        // Container so bl_TouchHelper survives while controls get toggled
        var controlsRoot = CreateRect("Root", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        helper.controlsRoot = controlsRoot.gameObject;

        BuildTouchPad(controlsRoot);
        BuildJoystick(controlsRoot, scale);
        BuildButtons(controlsRoot, scale, settings.buttonsAlpha);

        // Hidden until the local player spawns
        controlsRoot.gameObject.SetActive(false);
    }

    private void BuildTouchPad(RectTransform parent)
    {
        // Right ~62% of the screen rotates the camera; UI buttons on top of it take priority
        var pad = CreateRect("TouchPad", parent, new Vector2(0.38f, 0f), Vector2.one, Vector2.zero, Vector2.zero);
        var img = pad.gameObject.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.004f);
        pad.gameObject.AddComponent<bl_TouchPad>();
    }

    private void BuildJoystick(RectTransform parent, float scale)
    {
        float size = 320f * scale;

        var area = CreateRect("Joystick", parent, Vector2.zero, Vector2.zero,
            new Vector2(260f * scale, 250f * scale), new Vector2(size, size));
        var bg = area.gameObject.AddComponent<Image>();
        bg.sprite = GetCircleSprite();
        bg.color = new Color(1f, 1f, 1f, 0.18f);

        var knobRect = CreateRect("Knob", area, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(size * 0.42f, size * 0.42f));
        var knobImg = knobRect.gameObject.AddComponent<Image>();
        knobImg.sprite = GetCircleSprite();
        knobImg.color = new Color(1f, 1f, 1f, 0.45f);
        knobImg.raycastTarget = false;

        var joystick = area.gameObject.AddComponent<bl_Joystick>();
        joystick.joystickId = "movement";
        joystick.knob = knobRect;
        joystick.radius = size * 0.34f;
    }

    private void BuildButtons(RectTransform parent, float scale, float alpha)
    {
        var gunManagerGetter = new Func<bl_GunManager>(() =>
        {
            var refs = bl_MFPS.LocalPlayerReferences;
            return refs != null ? refs.gunManager : null;
        });

        // Bottom-right cluster
        CreateButton(parent, "Fire", "FIRE", new Vector2(1, 0), new Vector2(-300, 300), 230, scale, alpha,
            onDown: () =>
            {
                bl_MFPSMobileControl.FireHeld = true;
                bl_TouchHelper.DispatchButton(FPSMobileButton.Fire);
            },
            onUp: () => bl_MFPSMobileControl.FireHeld = false);

        CreateButton(parent, "Jump", "JUMP", new Vector2(1, 0), new Vector2(-120, 170), 150, scale, alpha,
            onDown: bl_TouchHelper.DispatchJump);

        CreateButton(parent, "Crouch", "CROUCH", new Vector2(1, 0), new Vector2(-120, 440), 125, scale, alpha,
            onDown: bl_TouchHelper.DispatchCrouch);

        aimButton = CreateButton(parent, "Aim", "AIM", new Vector2(1, 0), new Vector2(-140, 620), 125, scale, alpha,
            onDown: () =>
            {
                bl_MFPSMobileControl.AimActive = !bl_MFPSMobileControl.AimActive;
                RefreshAimVisual();
            });

        CreateButton(parent, "Reload", "RELOAD", new Vector2(1, 0), new Vector2(-540, 170), 125, scale, alpha,
            onDown: () => bl_TouchHelper.DispatchButton(FPSMobileButton.Reload));

        CreateButton(parent, "Grenade", "NADE", new Vector2(1, 0), new Vector2(-520, 460), 110, scale, alpha,
            onDown: () => gunManagerGetter()?.DoSingleGrenadeThrow(2));

        CreateButton(parent, "Melee", "MELEE", new Vector2(1, 0), new Vector2(-660, 310), 110, scale, alpha,
            onDown: () => gunManagerGetter()?.DoFastMeleeAttack(3));

        CreateButton(parent, "Weapon", "SWAP", new Vector2(1, 0), new Vector2(-320, 100), 110, scale, alpha,
            onDown: () => gunManagerGetter()?.SwitchNext());

        // Top-right
        CreateButton(parent, "Pause", "| |", new Vector2(1, 1), new Vector2(-70, -70), 95, scale, alpha,
            onDown: bl_TouchHelper.DispatchPause);

        // Push-to-talk, hidden unless voice chat PTT is enabled
        var talk = CreateButton(parent, "Talk", "TALK", new Vector2(0, 0), new Vector2(150, 560), 110, scale, alpha,
            onDown: () => bl_TouchHelper.DispatchTransmit(true),
            onUp: () => bl_TouchHelper.DispatchTransmit(false));
        talk.gameObject.SetActive(false);
        helper.talkButton = talk.gameObject;
    }

    private void RefreshAimVisual()
    {
        if (aimButton != null)
        {
            aimButton.SetHighlighted(bl_MFPSMobileControl.AimActive);
        }
    }
    #endregion

    #region UI helpers
    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPosition, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        if (size != Vector2.zero) rect.sizeDelta = size;
        else
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        return rect;
    }

    private MobileUIButton CreateButton(RectTransform parent, string name, string label, Vector2 anchor,
        Vector2 position, float size, float scale, float alpha, Action onDown, Action onUp = null)
    {
        float s = size * scale;
        var rect = CreateRect(name, parent, anchor, anchor, position * scale, new Vector2(s, s));

        var img = rect.gameObject.AddComponent<Image>();
        img.sprite = GetCircleSprite();
        img.color = new Color(1f, 1f, 1f, alpha);

        var textRect = CreateRect("Label", rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var text = textRect.gameObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;
        text.fontSize = Mathf.RoundToInt(s * 0.24f);
        text.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        text.raycastTarget = false;

        var button = rect.gameObject.AddComponent<MobileUIButton>();
        button.Setup(img, alpha, onDown, onUp);
        return button;
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null) return circleSprite;

        const int res = 128;
        var tex = new Texture2D(res, res, TextureFormat.ARGB32, false);
        float center = (res - 1) / 2f;
        float radius = center - 1f;
        var pixels = new Color[res * res];
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float a = Mathf.Clamp01(radius - dist); // 1px anti-aliased edge
                pixels[y * res + x] = new Color(1f, 1f, 1f, a);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        circleSprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
        return circleSprite;
    }
    #endregion

    /// <summary>
    /// Pressable UI button with press tint, highlight state and down/up callbacks.
    /// </summary>
    public class MobileUIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Image image;
        private float normalAlpha;
        private Action onDown, onUp;
        private bool highlighted;

        public void Setup(Image targetImage, float alpha, Action down, Action up)
        {
            image = targetImage;
            normalAlpha = alpha;
            onDown = down;
            onUp = up;
        }

        public void SetHighlighted(bool value)
        {
            highlighted = value;
            UpdateVisual(false);
        }

        private void UpdateVisual(bool pressed)
        {
            if (image == null) return;
            var color = highlighted ? new Color(1f, 0.85f, 0.2f, 1f) : Color.white;
            color.a = pressed ? Mathf.Min(1f, normalAlpha + 0.3f) : (highlighted ? Mathf.Min(1f, normalAlpha + 0.2f) : normalAlpha);
            image.color = color;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateVisual(true);
            onDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            UpdateVisual(false);
            onUp?.Invoke();
        }

        private void OnDisable()
        {
            UpdateVisual(false);
            onUp?.Invoke();
        }
    }
}
