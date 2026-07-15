using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// On-screen movement joystick.
/// MFPS core fetches it with bl_Joystick.Get("movement") (see bl_RoomCamera spectator mode),
/// and bl_MFPSMobileControl.GetMovementAxis() reads Horizontal/Vertical for the player controller.
/// </summary>
public class bl_Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public string joystickId = "movement";

    [Tooltip("Knob travel radius in local (canvas) units.")]
    public float radius = 80f;
    public RectTransform knob;

    private static readonly Dictionary<string, bl_Joystick> registry = new();

    private RectTransform rectTransform;
    private int pointerId = int.MinValue;
    private Vector2 axis = Vector2.zero;

    public float Horizontal => axis.x;
    public float Vertical => axis.y;

    public static bl_Joystick Get(string id)
    {
        registry.TryGetValue(id, out var joystick);
        return joystick;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        registry[joystickId] = this;
    }

    private void OnDestroy()
    {
        if (registry.TryGetValue(joystickId, out var current) && current == this)
        {
            registry.Remove(joystickId);
        }
    }

    private void OnDisable()
    {
        ResetState();
    }

    private void ResetState()
    {
        pointerId = int.MinValue;
        axis = Vector2.zero;
        if (knob != null) knob.anchoredPosition = Vector2.zero;
    }

    private void UpdateAxis(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        Vector2 clamped = Vector2.ClampMagnitude(localPoint, radius);
        axis = clamped / radius;
        if (knob != null) knob.anchoredPosition = clamped;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (pointerId != int.MinValue) return;
        pointerId = eventData.pointerId;
        UpdateAxis(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId) return;
        UpdateAxis(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId) return;
        ResetState();
    }
}
