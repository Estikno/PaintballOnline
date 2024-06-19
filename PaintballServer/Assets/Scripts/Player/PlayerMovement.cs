using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    float playerHeight = 2f;

    [Header("Movement")]
    private float moveSpeed;
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 6f;

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform camHolder;

    private float horizontalInput;
    private float verticalInput;
    private bool[] inputs;

    private Vector3 moveDirection;

    private Rigidbody rb;
    private Player player;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;

    private bool readyToJump;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDistance = 0.2f;

    [Header("Slope Handling")]
    [Range(0f, 90f)]
    [SerializeField] private float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public bool isGrounded { get; private set; }

    public MovementState state { get; private set; }
    public enum MovementState
    {
        walking,
        sprinting,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<Player>();

        readyToJump = true;

        inputs = new bool[6];
    }

    private void Update()
    {
        //true if is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        SpeedControl();
        StateHandler();

        if (isGrounded)
            rb.drag = groundDrag;
        else
            rb.drag = airDrag;
    }

    private void FixedUpdate()
    {
        MyInput();
        MovePlayer();
    }

    private void MyInput()
    {
        if (inputs[0])
            verticalInput = 1f;
        else if (inputs[1])
            verticalInput = -1f;
        else
            verticalInput = 0f;

        if (inputs[2])
            horizontalInput = -1f;
        else if (inputs[3])
            horizontalInput = 1f;   
        else
            horizontalInput = 0f;

        //when to jump
        if (inputs[4] && isGrounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        //calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0f)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        //on ground
        else if (isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        //in air
        else if (!isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        //turn gravity off while on slope
        rb.useGravity = !OnSlope();

        SendMovement();
    }

    private void StateHandler()
    {
        //Mode - Sprinting
        if (isGrounded && inputs[5])
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        //Mode - walking
        else if (isGrounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        //Mode - air
        else
        {
            state = MovementState.air;
        }
    }

    private void SpeedControl()
    {
        //limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        //limiting speed on ground or air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            //limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        //reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private Vector3 FlattenVector3(Vector3 vector)
    {
        vector.y = 0;
        return vector;
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope() //returns true if the player is on a slope
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    public void SetInput(bool[] inputs, Vector3 forward)
    {
        this.inputs = inputs;
        orientation.forward = FlattenVector3(forward);
        camHolder.forward = forward;

        Debug.DrawRay(camHolder.position, camHolder.forward * 2, Color.red);
    }

    private void SendMovement()
    {
        /*if (NetworkManager.Instance.CurrentTick % 2 != 0)
            return;*/

        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.Id);
        message.AddUShort(NetworkManager.Instance.CurrentTick);
        message.AddVector3(transform.position);
        message.AddVector3(camHolder.forward);
        message.AddVector3(orientation.InverseTransformDirection(rb.velocity));

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public void Respawn(Vector3 pos)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.respawn);
        message.AddUShort(player.Id);
        message.AddVector3(pos);

        NetworkManager.Instance.Server.SendToAll(message);
    }
}
