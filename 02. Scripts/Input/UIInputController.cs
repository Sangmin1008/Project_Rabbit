using UnityEngine;

public class UIInputController : MonoBehaviour
{
    public InputSystem_Actions UIInputs { get; private set; }
    public InputSystem_Actions.UIActions UIActions { get; private set; }

    private void Awake()
    {
        UIInputs = new InputSystem_Actions();
        UIActions = UIInputs.UI;
    }

    private void OnEnable()
    {
        UIInputs.Enable();
    }

    private void OnDisable()
    {
        UIInputs.Disable();
    }
}
