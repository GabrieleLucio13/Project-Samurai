using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public PlayerInputAction InputActions { get; private set; }
    public event Action<Vector2> Move;
    public event Action Jump;
    public event Action Dodge;
    public event Action Attack;
    public event Action<bool> Run;
    public event Action<bool> Defense;
    public event Action Throw;
    public event Action Pause;


    private void Awake()
    {
        InputActions = new PlayerInputAction();

        // Gameplay
        InputActions.Gameplay.Move.performed += ctx => Move?.Invoke(ctx.ReadValue<Vector2>());
        InputActions.Gameplay.Move.canceled  += _  => Move?.Invoke(Vector2.zero);

        InputActions.Gameplay.Jump.performed   += _ => Jump?.Invoke();
        InputActions.Gameplay.Dodge.performed  += _ => Dodge?.Invoke();
        InputActions.Gameplay.Attack.performed += _ => Attack?.Invoke();
        InputActions.Gameplay.Throw.performed += _ => Throw?.Invoke();

        InputActions.Gameplay.Run.started  += _ => Run?.Invoke(true);
        InputActions.Gameplay.Run.canceled += _ => Run?.Invoke(false);

        InputActions.Gameplay.Defense.started  += _ => Defense?.Invoke(true);
        InputActions.Gameplay.Defense.canceled += _ => Defense?.Invoke(false);

        // System
        InputActions.System.Pause.performed += _ => Pause?.Invoke();

        // Enable default maps
        InputActions.System.Enable();
        EnableGameplay();
    }

    private void OnDestroy()
    {
        InputActions.Dispose();
    }

    public void EnableGameplay()
    {
        InputActions.UI.Disable();
        InputActions.Gameplay.Enable();
    }

    public void EnableUI()
    {
        InputActions.Gameplay.Disable();
        InputActions.UI.Enable();
    }
}



