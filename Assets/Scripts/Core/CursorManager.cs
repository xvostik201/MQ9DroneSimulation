using UnityEngine;

public static class CursorManager
{
    private static bool _locked;

    public static void Lock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _locked = true;
    }

    public static void Unlock()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _locked = false;
    }

    public static void Toggle()
    {
        if (_locked) Unlock();
        else Lock();
    }
}