using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 movement;// { get; private set; }
    public bool jumpHeld;// { get; private set; }
    public bool jumpPressed;// { get; private set; }
    public bool attackPressed;// { get; private set; }
    public bool specialPressed;// { get; private set; }
    public bool pausePressed;




    PlayerActions playerActions;

    // Start is called before the first frame update
    void Awake()
    {
        playerActions = new PlayerActions();
        playerActions.Movement.Move.performed += ctx => StoreMoveInput(ctx.ReadValue<Vector2>());
        playerActions.Movement.Jump.performed += OnJumpPerformed;
        playerActions.Movement.Jump.canceled += OnJumpCanceled;
        playerActions.Movement.Attack.performed += OnAttack;
        playerActions.Movement.SpecialAttack.performed += OnSpecial;
        playerActions.Movement.Pause.performed += OnPause;
    }

    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }

    void StoreMoveInput(Vector2 dir)
    {
        movement = dir;
    }

    //Triggered when button goes down (not when it is held)
    void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        // Button went down
        jumpPressed = true;
        jumpHeld = true;
    }

    void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        // Button was released (if you care)
        jumpHeld = false;
    }

    //Triggered when button goes down (not when it is held)
    void OnAttack(InputAction.CallbackContext ctx)
    {
        // Button went down
        attackPressed = true;
    }

    //Triggered when button goes down (not when it is held)
    void OnSpecial(InputAction.CallbackContext ctx)
    {
        // Button went down
        specialPressed = true;
    }

    //Triggered when button goes down (not when it is held)
    void OnPause(InputAction.CallbackContext ctx)
    {
        // Button went down
        pausePressed = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        jumpPressed = false; //Like a real button, this bool is only true on the frame the button is pressed. So we know when it started
        attackPressed = false;
        specialPressed = false;
        pausePressed = false;
    }


}
