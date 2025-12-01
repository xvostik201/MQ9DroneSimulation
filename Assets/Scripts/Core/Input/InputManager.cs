using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("Input Contexts")]
    [SerializeField] private DroneInput _droneInput;
    [SerializeField] private UIInput _uiInput;

    private void Start()
    {
        SwitchToDrone();
    }

    public void SwitchToDrone()
    {
        _uiInput.enabled = false;
        _droneInput.enabled = true;
    }

    public void SwitchToUI()
    {
        _droneInput.enabled = false;
        _uiInput.enabled = true;
    }
}