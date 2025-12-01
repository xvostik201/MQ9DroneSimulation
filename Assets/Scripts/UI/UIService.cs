using UnityEngine;

public static class UIService
{
    public static HUDMessages HUD { get; private set; }

    public static void RegisterHUD(HUDMessages hud)
    {
        HUD = hud;
    }

    public static void UnregisterHUD()
    {
        HUD = null;
    }
}