using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerInputSystem : MonoBehaviour
{
    [Header("Input Values")]
    public Vector2 moveDir;
    public bool isEscape = false;

    [Header("Input Events")]
    public UnityEvent onInteractPressed;
    public UnityEvent onConsumptionPressed;
    public GameObject arrowUI;
    private bool hasMovedOnce = false;

    private void Start()
    {
        arrowUI.SetActive(true);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveDir = ctx.ReadValue<Vector2>();

        if (hasMovedOnce)
            return;

        if (moveDir != Vector2.zero)
        {
            hasMovedOnce = true;
            if (arrowUI != null)
                arrowUI.SetActive(false);
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started || ctx.performed)
            onInteractPressed?.Invoke();
    }

    public void OnConsumption(InputAction.CallbackContext ctx)
    {
        if (ctx.started || ctx.performed)
        {
            onConsumptionPressed?.Invoke();
        }
    }

    public void OnEscape(InputAction.CallbackContext ctx)
    {
        isEscape = ctx.performed;
    }
}
