using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Touch-look area (usually the right half of the screen).
/// MFPS core reads it from MouseLook.UpdateLook (GetInput), bl_WeaponSway (Horizontal/Vertical)
/// and bl_RoomCamera (spectator camera).
/// Returns a normalized per-frame swipe delta; MouseLook applies the player sensitivity on top.
/// </summary>
public class bl_TouchPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public static bl_TouchPad Instance { get; private set; }

    private int pointerId = int.MinValue;
    private Vector2 accumulatedDelta = Vector2.zero;
    private Vector2 frameDelta = Vector2.zero;
    private int lastFrame = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void ConsumeFrame()
    {
        if (Time.frameCount == lastFrame) return;
        lastFrame = Time.frameCount;
        frameDelta = accumulatedDelta;
        accumulatedDelta = Vector2.zero;
    }

    /// <summary>
    /// Normalized swipe delta of the current frame.
    /// A full screen-height swipe over one frame equals lookSwipeDegrees.
    /// </summary>
    private Vector2 NormalizedDelta
    {
        get
        {
            ConsumeFrame();
            float scale = bl_MobileControlSettings.Instance.lookSwipeDegrees / Mathf.Max(1, Screen.height);
            return frameDelta * scale;
        }
    }

    /// <summary>
    /// Called by MouseLook / bl_RoomCamera. The sensitivity parameter is intentionally not applied here:
    /// MouseLook.Move() multiplies by the player sensitivity itself, applying it twice would square it.
    /// </summary>
    public Vector2 GetInput(float sensitivity)
    {
        return NormalizedDelta;
    }

    public float Horizontal => NormalizedDelta.x;
    public float Vertical => NormalizedDelta.y;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (pointerId != int.MinValue) return;
        pointerId = eventData.pointerId;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId) return;
        accumulatedDelta += eventData.delta;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId) return;
        pointerId = int.MinValue;
    }
}
