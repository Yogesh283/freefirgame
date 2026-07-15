using UnityEngine;

/// <summary>
/// Static bridge between the touch UI and MFPS core.
/// MFPS core calls GetMovementAxis() (bl_FirstPersonController, bl_Ladder, bl_WeaponSway)
/// and HandleWeaponFireInput() (bl_Gun.InputUpdate) when the MFPSM define is active.
/// </summary>
public static class bl_MFPSMobileControl
{
    /// <summary>True while the fire button is held down.</summary>
    public static bool FireHeld { get; set; }

    /// <summary>Aim state, toggled by the aim button.</summary>
    public static bool AimActive { get; set; }

    /// <summary>
    /// Movement axis from the on-screen joystick, both values in [-1, 1].
    /// The controller multiplies vertical by 1.5 and sprints when it exceeds 1 (full push up).
    /// </summary>
    public static Vector2 GetMovementAxis()
    {
        var joystick = bl_Joystick.Get("movement");
        return joystick == null ? Vector2.zero : new Vector2(joystick.Horizontal, joystick.Vertical);
    }

    /// <summary>
    /// Handles fire/aim for the active weapon on mobile. Returning true makes
    /// bl_Gun.InputUpdate skip the desktop mouse/keyboard input for the frame.
    /// Single shots are dispatched separately by the fire button through
    /// bl_TouchHelper.onMobileButton (handled in bl_Gun.OnMobileButton).
    /// </summary>
    public static bool HandleWeaponFireInput(bl_Gun gun)
    {
        if (!bl_UtilityHelper.isMobile) return false;
        if (gun == null) return true;

        gun.isAiming = AimActive && gun.CanAiming;

        bool isAuto = gun.GetFireType() == bl_WeaponBase.FireType.Auto;
        if (isAuto)
        {
            if (FireHeld && gun.CanFire)
            {
                gun.LoopFire();
                gun.isFiring = true;
            }
            else
            {
                gun.isFiring = false;
            }
        }

        return true;
    }
}
