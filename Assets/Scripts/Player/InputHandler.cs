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

    [Header("UI Input Bools")]
    public bool confirmPressed = false;
    public bool cancelPressed = false;
    public bool cancelHeld = false;


    public void OnEnable()
    {
        //If already initialised, renable
        if(playerActions != null)
        {
            playerActions.Enable();
        }
    }

    public void OnDisable()
    {
        playerActions.Disable();
    }

    public PlayerActions playerActions;

    // Start is called before the first frame update
    void Start()
    {
        PlayerInputScript playerInput = FindObjectOfType<PlayerInputScript>();
        playerActions = playerInput == null ? new PlayerActions() : playerInput.actions;
        playerActions.Movement.Move.performed += ctx => StoreMoveInput(ctx.ReadValue<Vector2>());
        playerActions.Movement.Jump.performed += OnJumpPerformed;
        playerActions.Movement.Jump.canceled += OnJumpCanceled;
        playerActions.Movement.Attack.performed += OnAttack;
        playerActions.Movement.Dash.performed += OnSpecial;
        playerActions.Movement.Pause.performed += OnPause;
        playerActions.UI.Confirm.performed += OnConfirm;
        playerActions.UI.Back.performed += OnCancel;
        playerActions.UI.Back.canceled += OnCancelCancelled;
        playerActions.Enable();
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
        print("jump");
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

    void OnConfirm(InputAction.CallbackContext ctx)
    {
        // Button was released (if you care)
        confirmPressed = true;
    }

    void OnCancel(InputAction.CallbackContext ctx)
    {
        // Button was released (if you care)
        cancelPressed = true;
        cancelHeld = true;
    }

    void OnCancelCancelled(InputAction.CallbackContext ctx)
    {
        // Button was released (if you care)
        cancelHeld = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ResetAllBools();
    }

    public void ResetAllBools()
    {
        jumpPressed = false; //Like a real button, this bool is only true on the frame the button is pressed. So we know when it started
        attackPressed = false;
        specialPressed = false;
        pausePressed = false;
        confirmPressed = false;
        cancelPressed = false;
    }


}
