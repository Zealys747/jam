using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Horizontal Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float groundDrag;
    [Header("Player Jump")]
    // [SerializeField] private float jumpForce;
    [SerializeField, Range(0, 100)] private float gravity;
    // Jump cooldown in seconds
    // [SerializeField, Range(0.0f, 1.0f)] private float jumpCooldown;
    [SerializeField] private float airMultiplier;

    private float _savedWalkSpeed;
    private float _savedSprintSpeed;



    [Header("Ground Check")]
    [SerializeField] public float playerHeight;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform orientation;

    
    [Header("Slope Handling")]
    public float maxSlopeAngle;
    public RaycastHit slopeHit;
    
    // InputAction for inputs... uhuh.....
    private PlayerInput playerInput;
    private InputAction moveAction;
    float horizontalInput;
    float verticalInput;
    
    Vector3 moveDirection;
    Rigidbody rb;
    private float moveSpeed;
    bool grounded;
    // bool readyToJump;
    // bool exitingSLope;

    // ��� ���� ����������� �� �������
    public float fallForce;



    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        air
    };

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        // readyToJump = true;
    }

    private void Update()
    {
        Vector2 inputVector = moveAction.ReadValue<Vector2>();
        horizontalInput = inputVector.x;
        verticalInput = inputVector.y;

        StateHandler();
        HandleDrag();
    }

    private void FixedUpdate()
    {
        SpeedControl();
        MovePlayer();
        ApplyGravity();
    }
    public void Fall()
    {
        //rb.AddForce(Vector3.down)
    }
    private void StateHandler()
    {
        // State - Sprinting
        if(grounded && Input.GetKey(KeyCode.LeftShift))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        //State - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
    }



    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() )
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if(rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        rb.useGravity = !OnSlope();

    }
    // Ground (and air) friction
    private void HandleDrag()
    {
        grounded = Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0), Vector3.down, 0.2f, groundLayer);
        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    private void SpeedControl()
    {
        if (OnSlope())
        {
            if(rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if(flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
        
    }

    // private void ResetJump()
    // {
    //     readyToJump = true;
    //     exitingSLope = false;
    // }

    // public void OnJump()
    // {
    //     if(readyToJump && grounded)
    //     {
    //         exitingSLope = true;
    //
    //         readyToJump = false;
    //         // grounded = false;
    //
    //         rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    //         rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    //
    //         // rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    //
    //         Invoke(nameof(ResetJump), jumpCooldown);
    //     }
    // }

    public void ApplyGravity()
    {
        if (!grounded)
        {
            // moveDirection.y -= gravity * Time.deltaTime;
            rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        }
    }

    void OnGUI()
    {
        GUI.depth = 0;
        GUIStyle myStyle = new GUIStyle{ fontSize = 34 };
        myStyle.normal.textColor = Color.black;
        GUI.Label(new Rect(10, 10, 300, 50), 
        "Horizontal Speed: " + new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude.ToString() + 
        "\nIs on ground: " + grounded.ToString() + 
        "\nIs on slope: " + OnSlope().ToString());        
    }




    //Slope shenanigans
    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public void CharacterStop()
    {
        _savedWalkSpeed = walkSpeed;
        _savedSprintSpeed = sprintSpeed;

        walkSpeed = 0f;
        sprintSpeed = 0f;
    }

    public void CharacterWalk()
    {
        walkSpeed = _savedWalkSpeed;
        sprintSpeed = _savedSprintSpeed;
    }
    
}