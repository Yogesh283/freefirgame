namespace MFPS.Mobile
{
    /// <summary>
    /// Button identifiers dispatched through bl_TouchHelper.onMobileButton.
    /// MFPS core listens for Fire and Reload (see bl_Gun.OnMobileButton).
    /// </summary>
    public enum FPSMobileButton
    {
        Fire = 0,
        Reload = 1,
        Aim = 2,
        Jump = 3,
        Crouch = 4,
        Grenade = 5,
        Melee = 6,
        NextWeapon = 7,
        Pause = 8,
        DropKit = 9,
        Talk = 10,
    }
}
