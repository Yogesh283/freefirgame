using MFPS.Mobile;
using System;
using UnityEngine;

/// <summary>
/// Central hub for mobile touch input events.
/// MFPS core subscribes to the static events (bl_FirstPersonController, bl_Gun, bl_RoomMenu,
/// bl_PlayerItemDrop, bl_PlayerVoice, bl_Ladder) and calls the instance methods
/// (bl_RoomCamera.SetMobileCanvasVisible, bl_PlayerVoice.OnPushToTalkChange).
/// </summary>
public class bl_TouchHelper : MonoBehaviour
{
    public static bl_TouchHelper Instance { get; private set; }

    public static Action OnJump;
    public static Action OnCrouch;
    public static Action OnPause;
    public static Action OnKit;
    public static Action<bool> OnTransmit;
    public static Action<FPSMobileButton> onMobileButton;

    /// <summary>Root object that holds all the touch controls (assigned by bl_MobileControlsUI).</summary>
    [HideInInspector] public GameObject controlsRoot;

    /// <summary>Talk (push to talk) button, only visible when the voice chat PTT setting is on.</summary>
    [HideInInspector] public GameObject talkButton;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Called by MFPS (bl_RoomCamera) to show the controls in spectator mode / hide them otherwise,
    /// and by the UI itself on player spawn/death.
    /// </summary>
    public void SetMobileCanvasVisible(bool visible)
    {
        if (controlsRoot != null) controlsRoot.SetActive(visible);
    }

    /// <summary>
    /// Called by MFPS (bl_PlayerVoice) when the push-to-talk setting changes.
    /// </summary>
    public void OnPushToTalkChange(bool pushToTalk)
    {
        if (talkButton != null) talkButton.SetActive(pushToTalk);
    }

    #region Static dispatch helpers (called by the UI buttons)
    public static void DispatchJump() => OnJump?.Invoke();
    public static void DispatchCrouch() => OnCrouch?.Invoke();
    public static void DispatchPause() => OnPause?.Invoke();
    public static void DispatchKit() => OnKit?.Invoke();
    public static void DispatchTransmit(bool transmit) => OnTransmit?.Invoke(transmit);
    public static void DispatchButton(FPSMobileButton button) => onMobileButton?.Invoke(button);
    #endregion
}
