using UnityEngine;

/// <summary>
/// Settings for the custom mobile touch controls.
/// MFPS core reads Instance.disableRecoil (see bl_Recoil.cs).
/// An asset can optionally be placed at Resources/MobileControlSettings, otherwise defaults are used.
/// </summary>
public class bl_MobileControlSettings : ScriptableObject
{
    [Header("Gameplay")]
    [Tooltip("Disable weapon recoil on mobile for easier aiming.")]
    public bool disableRecoil = false;

    [Header("Look (touch pad)")]
    [Tooltip("Degrees of rotation for a full screen-height swipe, before the player sensitivity setting is applied.")]
    public float lookSwipeDegrees = 40f;

    [Header("UI")]
    [Range(0.6f, 1.6f)]
    public float controlsScale = 1f;
    [Range(0.05f, 0.9f)]
    public float buttonsAlpha = 0.45f;

    private static bl_MobileControlSettings instance;
    public static bl_MobileControlSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<bl_MobileControlSettings>("MobileControlSettings");
                if (instance == null)
                {
                    instance = CreateInstance<bl_MobileControlSettings>();
                }
            }
            return instance;
        }
    }
}
